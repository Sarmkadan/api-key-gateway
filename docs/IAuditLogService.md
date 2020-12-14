# IAuditLogService

`IAuditLogService` provides a contract for recording, querying, and managing audit trail entries within the API key gateway. It captures security-relevant events—such as key creation, rotation, revocation, and administrative actions—into a persistent log, and exposes methods to retrieve those entries by time range or perform retention-based cleanup.

## API

### AuditLogService

A concrete implementation of the interface. Instantiated via dependency injection, it encapsulates the underlying storage mechanism and exposes the asynchronous operations defined below.

### async Task LogAsync

Records a single `AuditLog` entry into the persistent store.

- **Parameters**: An `AuditLog` object containing the event timestamp, actor, action category, target resource identifier, and an optional detail payload.
- **Returns**: A `Task` that completes when the entry has been durably written.
- **Throws**: `ArgumentNullException` when the supplied `AuditLog` is `null`. `InvalidOperationException` when the underlying storage connection is unavailable or the log sink has been disposed.

### async Task\<List\<AuditLog\>\> GetLogsAsync

Retrieves all audit log entries currently stored, ordered by timestamp descending.

- **Parameters**: None.
- **Returns**: A `List<AuditLog>` containing every persisted entry. Returns an empty list when no logs exist.
- **Throws**: `InvalidOperationException` when the storage backend cannot be reached.

### async Task\<List\<AuditLog\>\> GetLogsForPeriodAsync

Retrieves audit log entries whose timestamps fall within a specified inclusive window.

- **Parameters**:
  - `DateTime from`: The inclusive start of the time window (UTC).
  - `DateTime to`: The inclusive end of the time window (UTC).
- **Returns**: A `List<AuditLog>` containing matching entries ordered by timestamp descending. Returns an empty list when no entries match the period.
- **Throws**: `ArgumentException` when `from` is later than `to`. `InvalidOperationException` when the storage backend is unreachable.

### async Task CleanupOldLogsAsync

Removes audit log entries older than a specified retention threshold. This supports compliance with data retention policies and prevents unbounded storage growth.

- **Parameters**:
  - `DateTime olderThan`: The cutoff timestamp (UTC). Entries with a timestamp strictly before this value are purged.
- **Returns**: A `Task` that completes when the deletion operation finishes.
- **Throws**: `InvalidOperationException` when the storage backend is unreachable or the cleanup operation conflicts with an in-progress write transaction.

## Usage

### Example 1: Recording an API key revocation event

```csharp
public class KeyRevocationHandler
{
    private readonly IAuditLogService _auditLog;

    public KeyRevocationHandler(IAuditLogService auditLog)
    {
        _auditLog = auditLog;
    }

    public async Task RevokeKeyAsync(string keyId, string revokedBy)
    {
        // ... perform revocation ...

        var entry = new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            Actor = revokedBy,
            Action = "KeyRevoked",
            TargetId = keyId,
            Details = "Revoked due to security policy violation"
        };

        await _auditLog.LogAsync(entry);
    }
}
```

### Example 2: Generating a weekly security report and enforcing retention

```csharp
public class WeeklyAuditReporter
{
    private readonly IAuditLogService _auditLog;

    public WeeklyAuditReporter(IAuditLogService auditLog)
    {
        _auditLog = auditLog;
    }

    public async Task<WeeklyReport> GenerateReportAsync()
    {
        var weekStart = DateTime.UtcNow.AddDays(-7).Date;
        var weekEnd = weekStart.AddDays(7).AddTicks(-1);

        List<AuditLog> weeklyLogs = await _auditLog.GetLogsForPeriodAsync(weekStart, weekEnd);

        // Retain only the last 90 days of logs
        await _auditLog.CleanupOldLogsAsync(DateTime.UtcNow.AddDays(-90));

        return new WeeklyReport(weeklyLogs);
    }
}
```

## Notes

- All `DateTime` parameters are expected in UTC. Providing local or unspecified kinds may lead to incorrect range filtering or premature cleanup.
- `GetLogsAsync` and `GetLogsForPeriodAsync` return materialized lists; callers should be mindful of memory pressure when the log volume is high. For large datasets, prefer `GetLogsForPeriodAsync` with narrow windows.
- `CleanupOldLogsAsync` is not guaranteed to be instantaneous. On busy systems, consider scheduling it during low-traffic maintenance windows to avoid storage contention.
- Implementations are expected to be thread-safe. Multiple concurrent calls to `LogAsync` must serialize writes without data corruption, and reads during a `CleanupOldLogsAsync` must reflect a consistent snapshot.
- The service does not expose update or single-entry delete operations; the audit trail is append-only from the caller’s perspective, with only time-based bulk removal available.
