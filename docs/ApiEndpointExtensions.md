# ApiEndpointExtensions

Static extension methods for `ApiEndpoint` (`ApiKeyGateway.Domain.Models`) that combine the endpoint's accessibility check with its payload-size limit and expose its configuration headers as a read-only dictionary. The class lives in `src/ApiKeyGateway/Domain/Models/ApiEndpointExtensions.cs`.

Both methods validate their `endpoint` argument with `ArgumentNullException.ThrowIfNull` and do not modify the endpoint.

## API

### `IsValid(this ApiEndpoint endpoint, long payloadBytes)` → bool

Returns `true` only when both of the following hold:

- `endpoint.IsAccessible()` is `true`: the endpoint is `IsActive` and `TargetUrl` is not empty or whitespace.
- `endpoint.IsPayloadSizeValid(payloadBytes)` is `true`: `payloadBytes` is less than or equal to `MaxPayloadBytes`.

The method delegates to those two `ApiEndpoint` members, so the rules are defined there. The check is inclusive at the limit, so a payload of exactly `MaxPayloadBytes` is valid. Negative `payloadBytes` values are not rejected by this method.

- **Throws** `ArgumentNullException` if `endpoint` is `null`.

```csharp
using ApiKeyGateway.Domain.Models;

var endpoint = new ApiEndpoint
{
    Id = "orders",
    Path = "/api/orders",
    TargetUrl = "https://orders.internal.example",
    MaxPayloadBytes = 1024
};

bool ok = endpoint.IsValid(512);     // true
bool tooBig = endpoint.IsValid(2048); // false - exceeds MaxPayloadBytes

endpoint.IsActive = false;
bool disabled = endpoint.IsValid(512); // false - endpoint is inactive
```

### `GetConfiguration(this ApiEndpoint endpoint)` → `IReadOnlyDictionary<string, string>`

Returns the endpoint's `Headers` as a dictionary. The method copies the collection into a new `Dictionary<string, string>` and returns that copy, so:

- Changes made to `endpoint.Headers` after the call are **not** reflected in the returned dictionary.
- If `Headers` is `null` (possible only if the property was assigned `null` at runtime, since the declared type is non-nullable), an empty dictionary is returned instead.
- An endpoint with no headers returns an empty dictionary, never `null`.

- **Throws** `ArgumentNullException` if `endpoint` is `null`.

```csharp
var endpoint = new ApiEndpoint
{
    Id = "reports",
    Path = "/api/reports",
    TargetUrl = "https://reports.internal.example"
};
endpoint.Headers["X-Upstream-Region"] = "eu-west";

IReadOnlyDictionary<string, string> config = endpoint.GetConfiguration();
string region = config["X-Upstream-Region"]; // "eu-west"

endpoint.Headers["X-Added-Later"] = "1";
bool seen = config.ContainsKey("X-Added-Later"); // false - config is a snapshot
```

## Related Types

- [`ApiEndpoint`](ApiEndpoint.md) - the model these methods extend. Its `IsAccessible()` and `IsPayloadSizeValid(long)` methods hold the rules used by `IsValid`.

## Usage Notes

- Use `IsValid` as a single pre-forwarding gate for an endpoint and request body size. It does not check the HTTP method, the consumer allow-list (`IsConsumerAllowed`), or the API key requirement; those checks must be made separately.
- Because `GetConfiguration` returns a snapshot, call it again after changing `Headers` if the new values are needed.
- Header keys are copied as stored. Lookups on the returned dictionary use the default string comparer, which is case-sensitive.
