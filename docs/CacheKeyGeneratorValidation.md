# CacheKeyGeneratorValidation

Static validation helpers for the parameters of [`CacheKeyGenerator`](CacheKeyGenerator.md) (`ApiKeyGateway.Caching`). The class lives in `src/ApiKeyGateway/Caching/CacheKeyGeneratorValidation.cs` and has three method groups:

- `Validate(...)` returns an `IReadOnlyList<string>` of problem messages. An empty list means the input is valid.
- `IsValid(...)` returns a `bool`.
- `EnsureValid(...)` throws an `ArgumentException` when the input has problems.

Each group has one overload per key type. The overloads map to the `CacheKeyGenerator` methods as shown in the [Overload map](#overload-map) below.

Validation is exhaustive within a call: every rule for every parameter is checked, and each failing rule adds its own message. The one exception is the null check, which throws before any rule runs (see [Null handling](#null-handling)).

## Overload map

| Validation overload | Key generator method(s) it covers |
|---|---|
| `(string apiKeyId)` | `GetApiKeyKey`, `GetApiKeyMetadataKey`, `GetQuotaKey`, `GetApiKeyInvalidationPattern` |
| `(string apiKeyId, string endpoint = "*")` | `GetRateLimitKey` |
| `(string apiKeyId, DateTime date)` | `GetUsageStatsKey` |
| `(Guid eventId)` | `GetWebhookDeliveryKey` |
| `(string apiName, string endpoint, Dictionary<string, string>? parameters = null)` | `GetExternalApiCacheKey` |
| `()` | `GetRateLimitInvalidationPattern` |

The `IsValid` and `EnsureValid` groups have the same overload shapes.

## Validation rules

Rules are listed per parameter, in the order the code checks them. Messages are copied verbatim from the source.

| Overload | Parameter | Rule | Problem message |
|---|---|---|---|
| `(string apiKeyId)` and `(string apiKeyId, string endpoint)` | `apiKeyId` | Must not be `null` or empty. Whitespace-only values pass. | `ApiKeyId cannot be null or empty.` |
| `(string apiKeyId, string endpoint)` | `endpoint` | Must not be `null` or empty. Whitespace-only values pass. | `Endpoint cannot be null or empty.` |
| `(string apiKeyId, DateTime date)` | `apiKeyId` | Same as above. | `ApiKeyId cannot be null or empty.` |
| `(string apiKeyId, DateTime date)` | `date` | Must not equal `default(DateTime)`. | `Date cannot be default (Unix epoch).` |
| `(Guid eventId)` | `eventId` | Must not equal `Guid.Empty`. This rule throws instead of returning a message (see [Guid handling](#guid-handling)). | none (throws) |
| `(string apiName, string endpoint, ...)` | `apiName` | Must not be `null` or empty. Whitespace-only values pass. | `ApiName cannot be null or empty.` |
| `(string apiName, string endpoint, ...)` | `endpoint` | Must not be `null` or empty. Whitespace-only values pass. | `Endpoint cannot be null or empty.` |
| `(string apiName, string endpoint, ...)` | `parameters` | Not validated. Any value, including `null`, is accepted. | none |
| `()` | none | No parameters to check. Always valid. | none |

### Overload-specific notes

- **`Validate(string apiKeyId)`** has no endpoint or date rule. Its only rule is the `apiKeyId` check.
- **`Validate(string apiKeyId, DateTime date)`** does not check the year or range. Only the exact `default(DateTime)` value (`0001-01-01T00:00:00`) fails. The message says "Unix epoch", but the default `DateTime` is not the Unix epoch (`1970-01-01`). Any other date, including past or future dates, passes.
- **`Validate(string apiName, string endpoint, Dictionary<string, string>? parameters = null)`** does not inspect `parameters`. Entries with `null` or empty keys or values are not reported.

## Null handling

Null arguments do not produce problem messages. They throw `ArgumentNullException` before any rule runs.

| Overload | Null parameter that throws |
|---|---|
| `(string apiKeyId)`, `(string apiKeyId, string endpoint)`, `(string apiKeyId, DateTime date)` | `apiKeyId` |
| `(string apiName, string endpoint, ...)` | `apiName`, `endpoint` |

The `endpoint` in `(string apiKeyId, string endpoint = "*")` is not null-checked. A null `endpoint` is reported as the problem `Endpoint cannot be null or empty.` instead of throwing. The same applies to `apiName` and `endpoint` in the external API overload, which do throw.

```csharp
using ApiKeyGateway.Caching;

IReadOnlyList<string> problems = CacheKeyGeneratorValidation.Validate("key-1", null!);
// [ "Endpoint cannot be null or empty." ]

CacheKeyGeneratorValidation.Validate(null!);
// throws ArgumentNullException (paramName: apiKeyId)
```

## Guid handling

`Validate(Guid eventId)` does not return a problem for `Guid.Empty`. It calls `ArgumentOutOfRangeException.ThrowIfEqual` and throws. A valid `Guid` returns an empty list.

As a result:

- `Validate(Guid.Empty)` throws `ArgumentOutOfRangeException`.
- `IsValid(Guid.Empty)` throws `ArgumentOutOfRangeException`. It does not return `false`.
- `EnsureValid(Guid.Empty)` throws `ArgumentOutOfRangeException`, not `ArgumentException`. The Guid check runs inside `Validate` before `EnsureValid` inspects the list.

Code that wants a `bool` for an empty `Guid` must catch the exception, or check `eventId != Guid.Empty` itself.

## `IsValid`

Each `IsValid` overload returns `Validate(...).Count == 0`. It returns `true` when no problems are reported and `false` otherwise. Null-argument and `Guid.Empty` exceptions propagate unchanged, as described above.

## `EnsureValid`

Each `EnsureValid` overload calls `Validate` and, if any problem is reported, throws:

```csharp
new ArgumentException(string.Join(" ", problems));
```

- The exception message joins all problem messages with a single space. There is no `ParamName`.
- The exception type is `System.ArgumentException`. It is not `Domain.Exceptions.ValidationException`. Code that catches project exceptions for input errors must catch `ArgumentException` here.
- Null arguments still throw `ArgumentNullException`, which derives from `ArgumentException`. A `catch (ArgumentException)` block catches both cases, so check the type first if the distinction matters.

```csharp
try
{
    CacheKeyGeneratorValidation.EnsureValid(apiKeyId, endpoint);
}
catch (ArgumentNullException)
{
    // apiKeyId was null
}
catch (ArgumentException ex)
{
    // ex.Message: "Endpoint cannot be null or empty." (for example)
}
```

## Usage

### Collecting every problem

```csharp
IReadOnlyList<string> problems = CacheKeyGeneratorValidation.Validate("", DateTime.MinValue);
// [
//   "ApiKeyId cannot be null or empty.",
//   "Date cannot be default (Unix epoch)."
// ]
```

### Boolean check before generating a key

```csharp
if (CacheKeyGeneratorValidation.IsValid(apiKeyId, endpoint))
{
    string key = CacheKeyGenerator.GetRateLimitKey(apiKeyId, endpoint);
}
```

### Validating an external API cache key

```csharp
var query = new Dictionary<string, string> { ["page"] = "1" };

CacheKeyGeneratorValidation.EnsureValid("billing-api", "/invoices", query);
string key = CacheKeyGenerator.GetExternalApiCacheKey("billing-api", "/invoices", query);
```

`parameters` is not checked by the validator, so `EnsureValid` does not reject a bad dictionary. Only `apiName` and `endpoint` are checked.

## Known gaps

- The `IsValid(string apiKeyId, string endpoint = "*")` XML comment does not mention that a null `endpoint` is reported as a problem rather than thrown.
- Whitespace-only `apiKeyId`, `endpoint`, `apiName` values pass validation. The rules only test for `null` and the empty string.
- `Validate(string apiKeyId, DateTime date)` does not reject non-default but out-of-range dates.
