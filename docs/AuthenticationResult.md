# AuthenticationResult

`AuthenticationResult` is the outcome of one authentication attempt. `IAuthenticationService.AuthenticateAsync` returns it, and the API key middleware reads it to decide whether the request continues or gets a problem response. It never carries an exception: a failed attempt is a result with `Success == false` and a reason, not a thrown error.

The type is defined in `src/ApiKeyGateway/Domain/Models/AuthenticationResult.cs` together with its enum:

- `AuthenticationResult`: `public record`. Equality and `ToString()` come from the record. Its properties are `init`-only, so instances are created through the factory methods.
- `AuthenticationFailureReason`: `public enum` in the same file. It lists the reasons a result can fail.

## Properties

| Property | Type | Set when | Description |
|---|---|---|---|
| `Success` | `bool` | Always | `true` if the key was accepted. |
| `FailureReason` | `AuthenticationFailureReason?` | `Success == false` | Why the attempt failed. `null` on success. |
| `ApiKey` | `ApiKey?` | `Success == true` | The validated key. `null` on failure. |

On success `FailureReason` is `null`. On failure `ApiKey` is `null`. The type does not enforce this, because the properties are `init`-settable. Use the factory methods so the two stay consistent.

## Factory methods

| Method | Returns | Result |
|---|---|---|
| `SuccessResult(ApiKey apiKey)` | `AuthenticationResult` | `Success = true`, `ApiKey = apiKey`. |
| `FailureResult(AuthenticationFailureReason failureReason)` | `AuthenticationResult` | `Success = false`, `FailureReason = failureReason`. |

Both methods are `static` and do not check their arguments. `SuccessResult` stores a `null` key as given, so callers must pass a validated `ApiKey`.

## Failure reasons

`AuthenticationFailureReason` has these values:

| Value | Meaning | Returned by `AuthenticationService` |
|---|---|---|
| `MissingApiKey` | No key value was supplied, or it was empty or whitespace. | Yes |
| `InvalidApiKeyFormat` | The key did not resolve to a stored key, or the lookup threw a non-expiry `InvalidApiKeyException`. | Yes |
| `ApiKeyNotFound` | Defined but not returned by the current service. A missing key is reported as `InvalidApiKeyFormat`. | No |
| `ApiKeyExpired` | The key is past its expiry date. | Yes |
| `ApiKeyDisabled` | The key status is not `Active`. | Yes |
| `IpNotWhitelisted` | The request IP is not in the key's whitelist. | Yes |
| `ServiceUnavailable` | The key store or another dependency failed. This is a transient error, not a bad key. | Yes |

`ApiKeyNotFound` is handled by the middleware (it is grouped with `InvalidApiKeyFormat`), but no code path produces it today.

## Usage in the authentication flow

The flow has three steps: the service builds the result, the middleware reads it, and the downstream code uses the key it carries.

1. `AuthenticationService.AuthenticateAsync(apiKey, ipAddress)` runs the checks in order and returns the first failure, or `SuccessResult` if every check passes. See [AuthenticationService](AuthenticationService.md).
2. `ApiKeyAuthenticationMiddleware` checks `result.Success`:
   - On success it stores `result.ApiKey` in `HttpContext.Items["ApiKey"]` and `ApiKey.ConsumerId` in `HttpContext.Items["ConsumerId"]`, then continues the pipeline.
   - On failure it records the reason in metrics, then writes an RFC 7807 problem response chosen by `FailureReason`.
3. Metrics skip `ServiceUnavailable`. A store outage returns `503` and is not counted as an unauthorized attempt.

Example of the success path:

```csharp
var result = await _authenticationService.AuthenticateAsync(apiKey, remoteIp);

if (!result.Success)
{
    // result.FailureReason is non-null here; map it to a response.
    return;
}

var key = result.ApiKey!;
// key.Id and key.ConsumerId identify the caller.
```

Example of building a result in a test or a custom service:

```csharp
var ok = AuthenticationResult.SuccessResult(apiKey);
var expired = AuthenticationResult.FailureResult(AuthenticationFailureReason.ApiKeyExpired);
```

Middleware tests mock `IAuthenticationService` and return these factory results. See `tests/api-key-gateway.Tests/ApiKeyAuthenticationMiddlewareTests.cs`.

## Related

- [AuthenticationService](AuthenticationService.md)
- [ApiKey](ApiKey.md)
