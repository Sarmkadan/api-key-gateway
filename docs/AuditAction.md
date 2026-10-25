# AuditAction

`src/ApiKeyGateway/Domain/Enums/AuditAction.cs` is the set of actions that the gateway writes to the audit trail. Each audit entry is an [AuditLog](AuditLog.md) whose `Action` property holds one of these values. The enum is persisted as its integer value (`AuditLogRepository` writes `Action` as `int`, and `SearchAsync(AuditAction, ...)` filters on `(int)action`), so the numeric values are part of the storage format and must not be renumbered.

```csharp
namespace ApiKeyGateway.Domain.Enums;

public enum AuditAction
{
    KeyCreated = 1,
    KeyUsed = 2,
    KeyDisabled = 3,
    KeyEnabled = 4,
    KeyRevoked = 5,
    RateLimitExceeded = 6,
    ConfigurationUpdated = 7,
    UnauthorizedAttempt = 8,
    StatsViewed = 9,
    UsageDataExported = 10,
    ConfigurationViewed = 11,
    DiagnosticsRun = 12,
    RateLimitsReset = 13,
    AuditLogsSearched = 14,
    AuditLogsExportedByResource = 15,
    AuditLogsExportedByPeriod = 16,
    RateLimitOverrideUpdated = 17
}
```

## Values

The "Recorded when" column describes the code that writes the entry today. Entries whose producer is not reached by any code path are marked **not currently recorded**; see [Values that are not recorded today](#values-that-are-not-recorded-today).

| Value | Int | Recorded when |
|-------|-----|---------------|
| `KeyCreated` | 1 | `AuditLogEventHandler.HandleApiKeyCreatedAsync` handles an `ApiKeyCreatedEvent`. Not currently recorded. |
| `KeyUsed` | 2 | `AuthenticationService.AuthenticateAsync` accepts a key: the key is active, not expired, and the caller IP is allowed. Written through `LogAuthenticationAttemptAsync(keyId, true)`. |
| `KeyDisabled` | 3 | `AuditLogEventHandler.HandleApiKeyDisabledAsync` handles an `ApiKeyDisabledEvent`. Not currently recorded. |
| `KeyEnabled` | 4 | Never written. Not currently recorded. |
| `KeyRevoked` | 5 | `AuditLogEventHandler.HandleApiKeyRotatedAsync` handles an `ApiKeyRotatedEvent`, with the reason "previous secret revoked and replaced". Not currently recorded. |
| `RateLimitExceeded` | 6 | `AuditLogEventHandler.HandleQuotaExhaustedAsync` handles a `QuotaExhaustedEvent`, with `IsSuccess = false`. Not currently recorded. |
| `ConfigurationUpdated` | 7 | Never written. Not currently recorded. |
| `UnauthorizedAttempt` | 8 | `AuthenticationService.LogAuthenticationAttemptAsync(..., false, reason)` for a failed authentication. See the list below. |
| `StatsViewed` | 9 | `AdminController` statistics endpoint (`GET api/admin/stats`). |
| `UsageDataExported` | 10 | `AdminController` usage export endpoint (`GET api/admin/export/usage`). |
| `ConfigurationViewed` | 11 | `AdminController` configuration endpoint (`GET api/admin/config`). |
| `DiagnosticsRun` | 12 | `AdminController` diagnostics endpoint (`POST api/admin/diagnose`). |
| `RateLimitsReset` | 13 | `AdminController` reset endpoint (`POST api/admin/reset-limits`). |
| `AuditLogsSearched` | 14 | `AdminController` audit search endpoint (`GET api/admin/audit/search`). |
| `AuditLogsExportedByResource` | 15 | `AdminController` audit export by resource (`GET api/admin/audit/export/resource/{resourceId}`). |
| `AuditLogsExportedByPeriod` | 16 | `AdminController` audit export by period (`GET api/admin/audit/export/period`). |
| `RateLimitOverrideUpdated` | 17 | `AdminController` per-key rate limit override endpoint (`PUT api/admin/rate-limits/{apiKeyId}/override`). |

The admin entries (9 to 17) are written by `AdminController` with `IsSuccess = true`. `ResourceType` is `AdminController`, and `ResourceId` is a fixed placeholder for the operation (for example `system` for stats or `audit-search` for audit search). `PerformedBy` holds the caller's identity.

### UnauthorizedAttempt reasons

`AuthenticationService` writes `UnauthorizedAttempt` with `IsSuccess = false` and one of these `Reason` strings:

- `Missing API key`. The `ResourceId` is `unknown`.
- `Invalid API key`. The `ResourceId` is `unknown`.
- `API key expired`.
- `API key disabled`.
- `IP not whitelisted: {ip}`.
- `Key store unavailable`. The `ResourceId` is `unknown`.
- `Authentication service error`. The `ResourceId` is `unknown`.

A request that presents an unknown key is recorded under the placeholder resource ID `unknown`, so these entries cannot be tied to a stored key.

## Values that are not recorded today

Some values have a handler or a helper, but nothing in `src/` constructs the event that would trigger it, or nothing writes the value at all:

- `KeyCreated`, `KeyDisabled`, `KeyRevoked` (rotation) and `RateLimitExceeded` (quota exhaustion) depend on `ApiKeyCreatedEvent`, `ApiKeyDisabledEvent`, `ApiKeyRotatedEvent` and `QuotaExhaustedEvent`. The handlers are subscribed in `Configuration/EventConfiguration.cs`, but no code publishes these events. Today the audit trail does not contain entries for key creation, disable, rotation or quota exhaustion.
- `KeyEnabled` is never written. `ApiKeyService.EnableKeyAsync` changes the key and logs to the application log only.
- `ConfigurationUpdated` is never written.

`RateLimitExceeded` also does not mean a per-request rate limit rejection. Those rejections throw `RateLimitExceededException` from `RateLimitingService` and are not written to the audit log. The enum value is used only for the quota-exhaustion handler described above.

## Human-readable descriptions

`AuditLog.GetActionDescription()` maps eight values to text: `KeyCreated`, `KeyUsed`, `KeyDisabled`, `KeyEnabled`, `KeyRevoked`, `RateLimitExceeded`, `ConfigurationUpdated` and `UnauthorizedAttempt`. The admin values (9 to 17) return `Unknown action`. The mapping is not exhaustive, so a new value needs a case there as well.

## Related

- [AuditLog](AuditLog.md): the entity that stores `Action`, `IsSuccess`, `Reason` and `ResourceId`.
- [AuditLogService](AuditLogService.md): the service that writes entries from the admin controller and authentication.
- [AuditLogEventHandler](AuditLogEventHandler.md): the event handler for the key lifecycle and quota events.
- [AuditLogRepository](AuditLogRepository.md): persists `Action` as an integer and filters on it.
- [AuthenticationService](AuthenticationService.md): writes `KeyUsed` and `UnauthorizedAttempt`.
- [AdminController](AdminController.md): writes the admin actions 9 to 17.
