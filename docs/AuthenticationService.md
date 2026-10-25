# AuthenticationService

`AuthenticationService` is the default implementation of `IAuthenticationService`. It authenticates an incoming raw API key, checks its status, expiry and optional source-IP whitelist, records usage on success, and writes every attempt to the audit log. The API key middleware calls it for each request.

The type is defined in `src/ApiKeyGateway/Services/AuthenticationService.cs` together with its interface:

- `IAuthenticationService`: the public contract that consumers should depend on.
- `AuthenticationService`: `public class AuthenticationService : IAuthenticationService`. It is not sealed and holds no state beyond its injected dependencies.

The result type is `AuthenticationResult` (`src/ApiKeyGateway/Domain/Models/AuthenticationResult.cs`), and the failure codes are `AuthenticationFailureReason`.

## Registration

The service is registered as scoped in `src/ApiKeyGateway/Configuration/ServiceCollectionExtensions.cs`:

```csharp
services.AddScoped<IAuthenticationService, AuthenticationService>();
```

## Constructor

```csharp
public AuthenticationService(
    IApiKeyService apiKeyService,
    IAuditLogService auditLogService,
    ILogger<AuthenticationService> logger)
```

| Parameter | Type | Required | Description |
|---|---|---|---|
| `apiKeyService` | `IApiKeyService` | Yes | Looks up and validates the raw key value via `ValidateKeyAsync`. |
| `auditLogService` | `IAuditLogService` | Yes | Receives the audit entries written by `LogAuthenticationAttemptAsync`. |
| `logger` | `ILogger<AuthenticationService>` | Yes | Receives information, warning and error messages. |

Throws `ArgumentNullException` if any parameter is `null`.

## Methods

All methods are asynchronous. Parameter names follow the interface.

| Method | Returns | Description |
|---|---|---|
| `AuthenticateAsync(string apiKey, string? ipAddress = null)` | `Task<AuthenticationResult>` | Authenticates a raw key value. Never throws for authentication problems; they are returned as a failed result. See [AuthenticateAsync](#authenticateasync). |
| `ValidateIpAsync(ApiKey key, string ipAddress)` | `Task<bool>` | Checks an IP address against an already-loaded key's whitelist. See [ValidateIpAsync](#validateipasync). |
| `LogAuthenticationAttemptAsync(string apiKeyId, bool success, string? reason = null)` | `Task` | Writes one authentication entry to the audit log. Failures are logged and swallowed. See [LogAuthenticationAttemptAsync](#logauthenticationattemptasync). |

### AuthenticateAsync

Parameters:

- `apiKey`: the raw key value from the request. Empty or whitespace-only values are rejected without calling the key service.
- `ipAddress`: the caller's IP address. Optional. The whitelist is checked only when this value is not empty or whitespace. Pass `null` to skip the IP check.

The method evaluates the checks in this order. The first failing check decides the result.

| Step | Condition | Failure reason returned | Audit entry |
|---|---|---|---|
| 1 | `apiKey` is null, empty or whitespace | `MissingApiKey` | Failure, key ID `"unknown"`, reason `Missing API key` |
| 2 | `IApiKeyService.ValidateKeyAsync` returns `null` | `InvalidApiKeyFormat` | Failure, `"unknown"`, `Invalid API key` |
| 3 | `ApiKey.IsExpired` is `true` | `ApiKeyExpired` | Failure, key ID, `API key expired` |
| 4 | `ApiKey.Status` is not `ApiKeyStatus.Active` | `ApiKeyDisabled` | Failure, key ID, `API key disabled` |
| 5 | `ipAddress` is set and `ApiKey.IsIpAllowed(ipAddress)` is `false` | `IpNotWhitelisted` | Failure, key ID, `IP not whitelisted: {ipAddress}` |
| 6 | All checks pass | none (success) | Success, key ID, default success reason |

On success the method also calls `ApiKey.RecordUsage()`, writes an information log line with the key ID and IP address (or `unknown`), and returns `AuthenticationResult.SuccessResult(validKey)`.

Exception handling inside the method:

| Exception | Result | Notes |
|---|---|---|
| `InvalidApiKeyException` with `IsExpired == true` | `ApiKeyExpired` | Logged as a warning. |
| `InvalidApiKeyException` (other) | `InvalidApiKeyFormat` | Logged as a warning. |
| `DataAccessException` | `ServiceUnavailable` | Logged as an error. The key store is treated as unavailable. |
| Any other `Exception` | `ServiceUnavailable` | Logged as an error. |

The method does not rethrow. Callers receive `ServiceUnavailable` for infrastructure failures and should treat it as a transient error, not as a bad key.

Usage:

```csharp
var result = await _authenticationService.AuthenticateAsync(apiKey, remoteIp);

if (!result.Success)
{
    // result.FailureReason says why; map it to a response.
    return;
}

var key = result.ApiKey!;
// key.Id and key.ConsumerId identify the caller.
```

The gateway's `ApiKeyAuthenticationMiddleware` calls this method only when a key was supplied. It then handles the result by `FailureReason`: each failure reason writes a different RFC 7807 problem response (invalid key, expired, disabled, IP not whitelisted, or service unavailable). On success it also checks the key's route scope, rate limit and usage quota, and stores the key in `HttpContext.Items["ApiKey"]` and the consumer ID in `HttpContext.Items["ConsumerId"]`. `ServiceUnavailable` is not counted as an unauthorized attempt in metrics.

### ValidateIpAsync

Parameters:

- `key`: an `ApiKey` that is already loaded. Must not be `null` (`ArgumentNullException`).
- `ipAddress`: the IP address to check.

Behavior:

- If `ipAddress` is empty or whitespace, the method returns `true` without checking the whitelist. Callers that need to block requests without an address must handle that case themselves.
- Otherwise it returns `key.IsIpAllowed(ipAddress)`.
- When the address is not allowed, it writes a failure audit entry with reason `IP not whitelisted: {ipAddress}` before returning `false`.

This method does not check expiry or status and does not call `RecordUsage()`. It is useful for re-checking an IP address on a key that has already been authenticated.

### LogAuthenticationAttemptAsync

Parameters:

- `apiKeyId`: the key ID to record. Use `"unknown"` when no key is known.
- `success`: `true` for a successful authentication, `false` otherwise.
- `reason`: optional text stored in the audit entry. When omitted it defaults to `Successful authentication` or `Failed authentication`.

Behavior:

- Creates an `AuditLog` with `ResourceType = "ApiKey"` and `ResourceId = apiKeyId`.
- Uses `AuditAction.KeyUsed` for success and `AuditAction.UnauthorizedAttempt` for failure, and sets `IsSuccess` accordingly.
- Calls `IAuditLogService.LogAsync`. Any exception is caught and logged as an error (`Failed to log authentication attempt`). The method never throws, so an audit outage does not fail authentication.

## Failure reasons

| Value | Meaning |
|---|---|
| `MissingApiKey` | No key value was supplied. |
| `InvalidApiKeyFormat` | The key did not resolve to a stored key, or the key service threw a non-expiry `InvalidApiKeyException`. |
| `ApiKeyNotFound` | Defined in the enum but not returned by this service. A missing key is reported as `InvalidApiKeyFormat`. |
| `ApiKeyExpired` | The key has passed its expiry date. |
| `ApiKeyDisabled` | The key status is not `Active`. |
| `IpNotWhitelisted` | The request IP address is not in the key's whitelist. |
| `ServiceUnavailable` | The key store or another dependency failed. Treat as a transient error. |

## Logging and audit

- Information: a successful authentication, with key ID and IP address.
- Warning: expired or invalid keys caught as `InvalidApiKeyException`.
- Error: `DataAccessException`, other unexpected exceptions, and failures to write the audit entry.
- Audit: every call to `AuthenticateAsync` that reaches a decision writes one entry through `LogAuthenticationAttemptAsync`. Early failures that have no key ID are recorded under `"unknown"`, so audit queries by key ID will not see them.

## Related

- [IAuthenticationService](IAuthenticationService.md)
- [ApiKeyService](ApiKeyService.md)
- [IAuditLogService](IAuditLogService.md)
