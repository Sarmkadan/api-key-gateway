# AuditLog

`ApiKeyGateway.Domain.Models.AuditLog` records security and administrative actions performed in the gateway (key creation, key use, rate-limit hits, unauthorized attempts, admin operations). Each entry captures who acted, when, on which resource, the outcome, and optionally the field-level changes.

Source: `src/ApiKeyGateway/Domain/Models/AuditLog.cs`

The action is an `ApiKeyGateway.Domain.Enums.AuditAction` value. Entries are created with object initializers and persisted through `AuditLogRepository`.

## Properties

All properties are settable through object initializers unless noted otherwise.

### `public string Id { get; init; }`
Unique identifier of the entry. Defaults to `string.Empty`; the type does not generate it, so callers must assign it.

### `public string ResourceId { get; init; }`
Identifier of the resource acted upon. Defaults to `string.Empty`.

### `public string ResourceType { get; init; }`
Type of the resource, for example `"ApiKey"` or `"Consumer"`. Defaults to `string.Empty`.

### `public Enums.AuditAction Action { get; init; }`
The action that was performed. Defaults to the enum's default value (`0`, which is not a defined member). Defined values:

| Value | Member | Description |
|-------|--------|-------------|
| 1 | `KeyCreated` | API key created |
| 2 | `KeyUsed` | API key used (accessed) |
| 3 | `KeyDisabled` | API key disabled |
| 4 | `KeyEnabled` | API key enabled |
| 5 | `KeyRevoked` | API key revoked |
| 6 | `RateLimitExceeded` | Rate limit exceeded for a key |
| 7 | `ConfigurationUpdated` | Configuration updated |
| 8 | `UnauthorizedAttempt` | Unauthorized access attempt |
| 9 | `StatsViewed` | Admin viewed system statistics |
| 10 | `UsageDataExported` | Admin exported usage data |
| 11 | `ConfigurationViewed` | Admin viewed gateway configuration |
| 12 | `DiagnosticsRun` | Admin ran system diagnostics |
| 13 | `RateLimitsReset` | Admin reset rate limits |
| 14 | `AuditLogsSearched` | Admin searched audit logs |
| 15 | `AuditLogsExportedByResource` | Audit logs exported by resource |
| 16 | `AuditLogsExportedByPeriod` | Audit logs exported by period |
| 17 | `RateLimitOverrideUpdated` | Rate-limit override updated |

### `public string PerformedBy { get; init; }`
The user or system that performed the action. Defaults to `"system"`.

### `public DateTime PerformedAt { get; init; }`
When the action was performed, in UTC. Defaults to `DateTime.UtcNow` at construction time.

### `public int? HttpStatusCode { get; set; }`
HTTP status code returned for the action, if applicable. `null` by default. Settable after construction.

### `public string? SourceIp { get; set; }`
Source IP address of the request, if known. `null` by default.

### `public string? Reason { get; set; }`
Free-text reason or description of the action. `null` by default.

### `public Dictionary<string, object> Changes { get; set; }`
Field-level changes recorded for the action, keyed by field name. Starts as an empty dictionary. Populate it with `RecordChange`; each value is an anonymous object with `Old`, `New` and `ChangedAt` members.

### `public string? ErrorMessage { get; set; }`
Error message if the action failed. `null` by default.

### `public bool IsSuccess { get; set; }`
Whether the action succeeded. Defaults to `true`. It is a plain property: it is not derived from `ErrorMessage`, so set it to `false` explicitly when recording a failure.

## Methods

### `public string GetActionDescription()`
Returns a fixed human-readable description based only on `Action`:

| `Action` | Description |
|----------|-------------|
| `KeyCreated` | `"API key created"` |
| `KeyUsed` | `"API key used"` |
| `KeyDisabled` | `"API key disabled"` |
| `KeyEnabled` | `"API key enabled"` |
| `KeyRevoked` | `"API key revoked"` |
| `RateLimitExceeded` | `"Rate limit exceeded"` |
| `ConfigurationUpdated` | `"Configuration updated"` |
| `UnauthorizedAttempt` | `"Unauthorized access attempt"` |
| any other value | `"Unknown action"` |

The method takes no parameters, does not use `ResourceId`, `Reason` or `ErrorMessage`, and never returns `null`. The other `AuditAction` members (9–17) currently fall through to `"Unknown action"`.

### `public void RecordChange(string fieldName, object? oldValue, object? newValue)`
Stores a change for `fieldName` in `Changes`, replacing any earlier entry with the same name. The stored value is an anonymous object `{ Old = oldValue, New = newValue, ChangedAt = <UtcNow> }`. Both values may be `null`. The method does not validate its arguments: a `null` `fieldName` throws `ArgumentNullException` from the `Dictionary` indexer; there is no explicit guard in the method. It returns `void`.

Example:

```csharp
var entry = new AuditLog
{
    Id = Guid.NewGuid().ToString(),
    ResourceId = "key-123",
    ResourceType = "ApiKey",
    Action = AuditAction.ConfigurationUpdated,
    PerformedBy = "admin@example.com"
};

entry.RecordChange("rateLimit", 100, 500);
// entry.Changes["rateLimit"] => { Old = 100, New = 500, ChangedAt = ... }
```

### `public override string ToString()`
Returns a single-line string with the key fields, for logs and debugging:

```
AuditLog { Id = ..., ResourceId = ..., ResourceType = ..., Action = ..., PerformedBy = ..., PerformedAt = ..., HttpStatusCode = ..., SourceIp = ..., Reason = ..., Changes = ..., ErrorMessage = ..., IsSuccess = ... }
```

`Changes` is rendered with the dictionary's default `ToString()`, which prints the type name rather than its contents. `null` nullable values print as empty.

## Usage

### Recording a successful key creation

```csharp
var entry = new AuditLog
{
    Id = Guid.NewGuid().ToString(),
    ResourceId = apiKey.Id,
    ResourceType = "ApiKey",
    Action = AuditAction.KeyCreated,
    PerformedBy = adminUserId,
    SourceIp = httpContext.Connection.RemoteIpAddress?.ToString(),
    HttpStatusCode = 201
};
```

### Recording a failed action

```csharp
var entry = new AuditLog
{
    Id = Guid.NewGuid().ToString(),
    ResourceId = keyId,
    ResourceType = "ApiKey",
    Action = AuditAction.UnauthorizedAttempt,
    PerformedBy = "anonymous",
    HttpStatusCode = 401,
    IsSuccess = false,
    ErrorMessage = "Invalid API key"
};
```
