# AuditLogRepository

`AuditLogRepository` implements `IAuditLogRepository` and provides data persistence for audit log entries using raw ADO.NET (`DbCommand` / `DbDataReader`) against the `AuditLogs` table.

## Namespace

`ApiKeyGateway.Repositories`

## Dependencies

- `IDbConnection` - database connection abstraction (from `ApiKeyGateway.Data`).
- `ILogger<AuditLogRepository>` - structured logging.

Both are injected via the constructor and must not be null.

## Public Methods

### `CreateAsync(AuditLog log)`
Inserts a new audit log entry into the `AuditLogs` table.
- **Parameters:** `log` - the `AuditLog` entity to persist (must not be null).
- **Throws:** `ArgumentNullException` if `log` is null; `DataAccessException` on database failure.
- **Columns written:** `Id`, `ResourceId`, `ResourceType`, `Action`, `PerformedBy`, `PerformedAt`, `HttpStatusCode`, `SourceIp`, `Reason`, `IsSuccess`.

### `GetByResourceIdAsync(string resourceId, int limit = 100)`
Retrieves audit logs for a specific resource, ordered by `PerformedAt` descending.
- **Parameters:**
  - `resourceId` - the resource identifier (must not be null/empty).
  - `limit` - maximum number of rows (default 100, must be positive).
- **Returns:** `List<AuditLog>` of matching entries.
- **Throws:** `ArgumentException` if `resourceId` is empty or `limit` is not positive; `DataAccessException` on database failure.

### `GetByDateRangeAsync(DateTime startDate, DateTime endDate)`
Retrieves all audit logs within a time range, ordered by `PerformedAt` descending.
- **Parameters:**
  - `startDate` - inclusive lower bound.
  - `endDate` - inclusive upper bound.
- **Returns:** `List<AuditLog>` of matching entries.
- **Throws:** `DataAccessException` on database failure.

### `SearchAsync(AuditAction action, DateTime fromUtc, DateTime toUtc, int limit = 100)`
Searches audit logs by action type and time range.
- **Parameters:**
  - `action` - the `AuditAction` enum value to filter by.
  - `fromUtc` - inclusive start (UTC).
  - `toUtc` - inclusive end (UTC).
  - `limit` - maximum number of rows (default 100, must be positive).
- **Returns:** `List<AuditLog>` of matching entries ordered by `PerformedAt` descending.
- **Throws:** `ArgumentException` if `limit` is not positive; `DataAccessException` on database failure.

### `DeleteOlderThanAsync(DateTime cutoffDate)`
Deletes all audit log entries with `PerformedAt` before the cutoff date.
- **Parameters:** `cutoffDate` - entries older than this are deleted.
- **Returns:** `int` - the number of deleted rows.
- **Throws:** `DataAccessException` on database failure.

### `ExportByResourceIdToXmlAsync(string resourceId, int limit = 100)`
Exports audit logs for a resource as an XML string via `XmlExportHelper.ToXml`.
- **Parameters:**
  - `resourceId` - the resource identifier (must not be null/empty).
  - `limit` - maximum number of rows (default 100, must be positive).
- **Returns:** XML string containing the matching audit log entries.
- **Throws:** `ArgumentException` if `resourceId` is empty or `limit` is not positive; `DataAccessException` on database failure.

### `ExportByDateRangeToXmlAsync(DateTime startDate, DateTime endDate, int limit = 100)`
Exports audit logs for a time range as an XML string.
- **Parameters:**
  - `startDate` - inclusive lower bound.
  - `endDate` - inclusive upper bound (must not be before `startDate`).
  - `limit` - maximum number of rows (default 100, must be positive).
- **Returns:** XML string containing the matching audit log entries.
- **Throws:** `ArgumentException` if `endDate < startDate` or `limit` is not positive; `DataAccessException` on database failure.

## Error Handling

All methods wrap database exceptions in `DataAccessException`, which carries the operation name (`CREATE`, `SELECT`, `DELETE`) and table name (`AuditLog`). The original exception is preserved as the inner exception.

## Usage

```csharp
// Inject via DI
var repo = serviceProvider.GetRequiredService<IAuditLogRepository>();

// Create an entry
await repo.CreateAsync(new AuditLog
{
    Id = Guid.NewGuid().ToString(),
    ResourceId = apiKeyId,
    ResourceType = "ApiKey",
    Action = AuditAction.Created,
    PerformedBy = "admin@example.com",
    PerformedAt = DateTime.UtcNow,
    IsSuccess = true
});

// Query by resource
var logs = await repo.GetByResourceIdAsync(apiKeyId, limit: 50);

// Cleanup old entries
int deleted = await repo.DeleteOlderThanAsync(DateTime.UtcNow.AddDays(-90));
```
