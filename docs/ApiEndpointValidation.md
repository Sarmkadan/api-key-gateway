# ApiEndpointValidation

Static validation helpers for [`ApiEndpoint`](ApiEndpoint.md) (`ApiKeyGateway.Domain.Models`). The class lives in `src/ApiKeyGateway/Domain/Models/ApiEndpointValidation.cs` and exposes three extension methods: `Validate()`, `IsValid()` and `EnsureValid()`. It also has one private helper, `IsValidHttpMethod`.

Validation is exhaustive: every rule is checked and every failing rule adds its own message, so one call can return several problems. Rules do not short-circuit across fields. Within a single field, a failing "required" check skips the format check for that field.

## API

### `Validate(this ApiEndpoint value)` → `IReadOnlyList<string>`

Runs all rules and returns the problem messages as a read-only list. The list is empty when the endpoint is valid. The method does not modify the endpoint.

- **Throws** `ArgumentNullException` if `value` is `null`.

```csharp
using ApiKeyGateway.Domain.Models;

var endpoint = new ApiEndpoint
{
    Id = "orders-list",
    Path = "orders",          // missing leading slash
    Method = "fetch",         // not an HTTP method
    TargetUrl = "https://orders.internal/api"
};

IReadOnlyList<string> problems = endpoint.Validate();
// [
//   "Path must start with '/'",
//   "Method 'fetch' is not a valid HTTP method (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS)"
// ]
```

### `IsValid(this ApiEndpoint? value)` → `bool`

Returns `true` when `Validate()` returns no problems. Unlike the other two methods, it accepts a nullable receiver: a `null` endpoint returns `false` and does not throw.

```csharp
ApiEndpoint? maybe = null;
bool ok = maybe.IsValid(); // false
```

> The XML doc comment on `IsValid` lists `ArgumentNullException`, but the implementation (`value?.Validate().Count == 0`) never throws for `null`. Rely on the behavior above.

### `EnsureValid(this ApiEndpoint value)` → `void`

Calls `Validate()` and throws if any problem is found. The exception message starts with `ApiEndpoint is invalid. Problems:` and lists each problem on its own line prefixed with `- `.

- **Throws** `ArgumentNullException` if `value` is `null`.
- **Throws** `ArgumentException` if the endpoint has one or more problems.

```csharp
try
{
    endpoint.EnsureValid();
}
catch (ArgumentException ex)
{
    // ex.Message lists every problem found
}
```

Note that `EnsureValid` throws a plain `System.ArgumentException`, not a `Domain.Exceptions.ValidationException`. Code that catches project exceptions for input errors should catch `ArgumentException` here.

## Validation Rules

Each row is checked in the order shown. Messages are copied verbatim from the source.

| Property | Rule | Problem message |
|---|---|---|
| `Id` | Must not be null, empty or whitespace | `Id cannot be null or whitespace` |
| `Path` | Must not be null, empty or whitespace | `Path cannot be null or whitespace` |
| `Path` | Must start with `/` (checked only when non-empty) | `Path must start with '/'` |
| `Method` | Must not be null, empty or whitespace | `Method cannot be null or whitespace` |
| `Method` | Must be one of `GET`, `POST`, `PUT`, `DELETE`, `PATCH`, `HEAD`, `OPTIONS` (checked only when non-empty) | `Method '{Method}' is not a valid HTTP method (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS)` |
| `TargetUrl` | Must not be null, empty or whitespace | `TargetUrl cannot be null or whitespace` |
| `TargetUrl` | Must pass `Uri.IsWellFormedUriString(value, UriKind.Absolute)` (checked only when non-empty) | `TargetUrl '{TargetUrl}' is not a valid absolute URI` |
| `CreatedAt` | Must not equal `default(DateTime)` | `CreatedAt cannot be default(DateTime)` |
| `CreatedAt` | Must not be later than `DateTime.UtcNow` (checked only when not default) | `CreatedAt cannot be in the future` |
| `TimeoutMs` | Must be greater than `0` | `TimeoutMs must be a positive number` |
| `TimeoutMs` | Must not exceed `300000` (5 minutes) (checked only when positive) | `TimeoutMs cannot exceed 300000 (5 minutes)` |
| `MaxPayloadBytes` | Must be greater than `0` | `MaxPayloadBytes must be a positive number` |
| `MaxPayloadBytes` | Must not exceed `104857600` (100 MB) (checked only when positive) | `MaxPayloadBytes cannot exceed 104857600 (100 MB)` |
| `CacheTtlSeconds` | Must not be negative (`0` is allowed) | `CacheTtlSeconds cannot be negative` |
| `CacheTtlSeconds` | Must not exceed `86400` (24 hours) (checked only when not negative) | `CacheTtlSeconds cannot exceed 86400 (24 hours)` |
| `Headers` | Must not be `null` | `Headers dictionary cannot be null` |
| `AllowedConsumers` | Must not be `null` | `AllowedConsumers list cannot be null` |

### Notes on individual rules

- **Method matching is case-sensitive.** `"get"` fails; only the uppercase names listed above pass. The default `ApiEndpoint.Method` value is `"GET"`, so the default passes.
- **Path only checks the leading slash.** It does not check for spaces, query strings or any other characters.
- **TargetUrl is not restricted to `http` or `https`.** Any absolute URI that is well-formed under `Uri.IsWellFormedUriString` passes. Characters that are not escaped, such as spaces, make the string fail the check.
- **Timeout and payload limits are inclusive.** `300000` ms and `104857600` bytes are accepted; one more fails.
- **CacheTtlSeconds of `0` is valid** and means no caching window, even when `CacheEnabled` is `true`.
- **Id has no format rule** beyond being non-blank. Uniqueness is not checked here.
- **Headers and AllowedConsumers are null-checked only.** Their contents are not validated.
- **Description, CacheEnabled, IsActive and RequireApiKey are not validated.**

## Related Types

- [`ApiEndpoint`](ApiEndpoint.md) - the model these helpers validate. Its defaults are `Method = "GET"`, `TimeoutMs = 30000`, `MaxPayloadBytes = 10485760`, `CacheTtlSeconds = 300`, and `CreatedAt = DateTime.UtcNow`.
- [`ApiEndpointExtensions`](ApiEndpointExtensions.md) - companion helpers for `ApiEndpoint`.

## Usage Notes

- Call `Validate()` when you need every problem at once, for example to return a complete list to an admin user. Call `IsValid()` for a simple yes/no check, and `EnsureValid()` at boundaries where invalid input should stop processing.
- `CreatedAt` is compared with `DateTime.UtcNow`, so an endpoint that was created with a timestamp set slightly ahead of the server clock fails validation until that time passes.
- Because the timeout and payload limits are checked for both bounds, an endpoint with `TimeoutMs = 0` reports only the "must be a positive number" message, not the upper-bound message.
