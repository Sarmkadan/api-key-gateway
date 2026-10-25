# ErrorMessages

`src/ApiKeyGateway/Domain/Constants/ErrorMessages.cs` is a static class of `const string` error message templates. Exceptions in the gateway take these strings as their message, so the same wording is used everywhere a given failure is reported. Messages that take arguments are format templates. Callers fill them with `string.Format`.

The class holds 19 constants. Eight are used in code. The other eleven are defined but nothing in `src/` references them yet (see [Unused constants](#unused-constants)).

## Used constants

### ApiKeyNotFound

```text
The specified API key does not exist or has been revoked.
```

Used in `ApiKeyService.ValidateKeyAsync` (`src/ApiKeyGateway/Services/ApiKeyService.cs:244`). The method throws `InvalidApiKeyException` with this message when the hashed key is not found.

### ApiKeyExpired

```text
The API key has expired and is no longer valid.
```

Used in `ApiKeyService.ValidateKeyAsync` (`src/ApiKeyGateway/Services/ApiKeyService.cs:250`). The exception is thrown with `isExpired: true`.

### ApiKeyDisabled

```text
The API key is disabled and cannot be used.
```

Used in `ApiKeyService.ValidateKeyAsync` (`src/ApiKeyGateway/Services/ApiKeyService.cs:256`) when the key status is not active.

### RateLimitExceeded

```text
Rate limit exceeded. Maximum {0} requests per {1} allowed.
```

Placeholders: `{0}` is the request limit, `{1}` is the window as a unit name. `GetTimeUnit` in `RateLimitExceededException` maps 1, 60, 3600 and 86400 seconds to `second`, `minute`, `hour` and `day`. Any other value becomes `"{n} seconds"`.

Used in:
- `RateLimitExceededException` constructors (`src/ApiKeyGateway/Domain/Exceptions/RateLimitExceededException.cs:59` and `:99`).
- `GatewayProblemDetailsFactory` for the RFC 7807 `Detail` field (`src/ApiKeyGateway/Middleware/GatewayProblemDetailsFactory.cs:254`).

### UnauthorizedAccess

```text
Unauthorized access. Valid API key required.
```

Used in `ApiKeyService` (`src/ApiKeyGateway/Services/ApiKeyService.cs:235`). It is thrown as the project's own `ApiKeyGateway.Domain.Exceptions.UnauthorizedAccessException`, not `System.UnauthorizedAccessException`. The file aliases the name to avoid the clash.

### KeyGenerationFailed

```text
Failed to generate API key.
```

Used in `ApiKeyService.CreateKeyAsync` (`src/ApiKeyGateway/Services/ApiKeyService.cs:207`). Any exception from generation, hashing or persistence is wrapped in `InvalidApiKeyException` with this message.

### DataAccessFailed

```text
Failed to access data store.
```

Used as the message of `DataAccessException` in:
- `UsageTrackingService` (`src/ApiKeyGateway/Services/UsageTrackingService.cs:103`, `:144`, `:165`, `:187`, `:208`). The operations are `RecordUsageAsync`, `GetUsageStatisticsAsync`, `GetUsageRecordsAsync`, `GetTotalBytesUsedAsync` and `GetUsageAsync`.
- `ApiKeyService` (`src/ApiKeyGateway/Services/ApiKeyService.cs:386` and `:472`). The operations are `GetConsumerKeysAsync` and `GetIpWhitelistAsync`.

### KeyUpdateFailed

```text
Failed to update API key.
```

Used as the message of `DataAccessException` in `ApiKeyService` for write operations (`src/ApiKeyGateway/Services/ApiKeyService.cs:293`, `:327`, `:361`, `:428` and `:511`). The operations are `DisableKeyAsync`, `EnableKeyAsync`, `RevokeKeyAsync`, `UpdateKeyMetadataAsync` and `SetIpWhitelistAsync`.

## Unused constants

These constants are defined but no code in `src/` references them.

| Constant | Message | Notes |
|---|---|---|
| `InvalidApiKeyFormat` | `API key format is invalid. Expected format: {prefix}_{randomString}` | The name also exists as a member of the `AuthenticationFailureReason` enum, which is a different thing. |
| `InvalidConfiguration` | `Invalid gateway configuration: {0}` | |
| `DatabaseConnectionFailed` | `Failed to establish database connection.` | |
| `ServiceNotInitialized` | `{0} service is not properly initialized.` | |
| `InvalidEndpointConfiguration` | `Invalid endpoint configuration: {0}` | |
| `UsageTrackingFailed` | `Failed to track API usage for key {0}.` | |
| `AuditLogFailed` | `Failed to write audit log entry.` | |
| `UserNotFound` | `API consumer not found with identifier {0}.` | |
| `DuplicateKeyName` | `An API key with name {0} already exists for this consumer.` | |
| `InvalidTimeRange` | `Invalid time range: start time must be before end time.` | |
| `KeyStoreUnavailable` | `The authentication service is temporarily unavailable. Please retry shortly.` | `GatewayProblemDetailsFactory` has its own inline text for this case, `Authentication service temporarily unavailable. Please retry.` (`src/ApiKeyGateway/Middleware/GatewayProblemDetailsFactory.cs:352`). It does not use this constant. |

## Adding a message

1. Add a `public const string` with an XML doc comment to `ErrorMessages.cs`. Use `{0}`, `{1}` and so on for arguments.
2. Use the constant at the throw or response site with `Domain.Constants.ErrorMessages.<Name>`. Pass arguments through `string.Format`, as the rate-limit code does.
3. Update the tables on this page.
