# AuditLogService

`AuditLogService` records audit events for compliance and security monitoring, and reads them back for queries and XML export. Writes are asynchronous: `LogAsync` only places the entry on an in-memory queue, and a background task writes queued entries to storage in batches. The service is defined in `src/ApiKeyGateway/Services/AuditLogService.cs` together with its contract:

- `IAuditLogService`: the public contract. It extends `IDisposable`. Depend on this interface in consumers such as `AuthenticationService`.
- `AuditLogService`: `public class AuditLogService : IAuditLogService`. It is registered with `AddScoped` in `Configuration/ServiceCollectionExtensions.cs`.
- `AuditLog`: the entry model. See [AuditLog](AuditLog.md).
- `AuditLogRepository` / `IAuditLogRepository`: the storage layer the service delegates to. See [AuditLogRepository](AuditLogRepository.md).

## Constructor

```csharp
public AuditLogService(IAuditLogRepository repository, ILogger<AuditLogService> logger)
```

| Parameter | Type | Required | Description |
|---|---|---|---|
| `repository` | `IAuditLogRepository` | Yes | Persists queued entries and serves read and export requests. |
| `logger` | `ILogger<AuditLogService>` | Yes | Receives flush, cleanup, and disposal diagnostics. |

Throws `ArgumentNullException` if either argument is `null`.

The constructor starts the background flusher with `Task.Run`. Each instance therefore owns one flush loop, so the instance must be disposed when it is no longer needed. See [Lifecycle](#lifecycle).

## Fixed settings

These values are private fields. They cannot be changed through the constructor or configuration.

| Setting | Value | Effect |
|---|---|---|
| Flush interval | 5 seconds | Delay between flush cycles. |
| Batch size | 50 entries | Maximum entries written per flush cycle. |

## Methods

### `IAuditLogService.LogAsync`

```csharp
Task LogAsync(AuditLog log);
```

Queues one audit entry for asynchronous persistence. It does not touch the database.

- Throws `ArgumentNullException` if `log` is `null`. Because the method is `async`, the exception is delivered through the returned task, so it surfaces when the task is awaited.
- Returns once the entry is enqueued. It does not wait for the entry to reach storage.
- Storage failures are not visible to the caller. They are handled by the flusher; see [Notes](#notes).
- The method does not check whether the service has been disposed. Entries queued after disposal are never written; see [Lifecycle](#lifecycle).

### `IAuditLogService.GetLogsAsync`

```csharp
Task<List<AuditLog>> GetLogsAsync(string resourceId, int limit = 100);
```

Returns up to `limit` audit entries whose `ResourceId` matches `resourceId`.

- A `null`, empty, or whitespace `resourceId` returns an empty list. No query is run.
- Throws `ArgumentException` if `limit` is zero or negative. The exception is delivered through the returned task.
- Only entries already flushed to storage are returned. Entries still in the queue are not visible.

### `IAuditLogService.GetLogsForPeriodAsync`

```csharp
Task<List<AuditLog>> GetLogsForPeriodAsync(DateTime startDate, DateTime endDate);
```

Returns every audit entry in the date range, as the repository's `GetByDateRangeAsync` defines it.

- Throws `ArgumentException` if `endDate` is earlier than `startDate`.
- This method has no `limit` parameter. A wide range can return a large list. Narrow the range or use the XML export for bounded results.

### `IAuditLogService.ExportLogsToXmlAsync`

```csharp
Task<string> ExportLogsToXmlAsync(string resourceId, int limit = 100);
```

Returns the entries for one resource as an XML document, produced by `IAuditLogRepository.ExportByResourceIdToXmlAsync`.

- Throws `ArgumentException` if `resourceId` is `null` or empty. Whitespace is not rejected by this method and is passed to the repository as-is.
- `limit` is passed to the repository. It is not validated here.

### `IAuditLogService.ExportLogsForPeriodToXmlAsync`

```csharp
Task<string> ExportLogsForPeriodToXmlAsync(DateTime startDate, DateTime endDate);
```

Returns the entries in a date range as an XML document, produced by `IAuditLogRepository.ExportByDateRangeToXmlAsync`.

- The service does not check that `endDate` is after `startDate`. That check exists only in `GetLogsForPeriodAsync`.
- The service does not pass a limit. The repository's default limit of `100` applies, so the export is capped at 100 entries.

### `IAuditLogService.CleanupOldLogsAsync`

```csharp
Task CleanupOldLogsAsync(int retentionDays);
```

Deletes entries older than `DateTime.UtcNow.AddDays(-retentionDays)` through `IAuditLogRepository.DeleteOlderThanAsync`, then logs the number removed at information level.

- Throws `ArgumentException` if `retentionDays` is zero or negative. The exception is delivered through the returned task.
- Cleanup is best-effort. Repository exceptions are logged at error level and swallowed, so the caller does not see them. The next scheduled run retries.

### `AuditLogService.FlushAsync`

```csharp
public async Task FlushAsync();
```

Writes one batch of up to 50 queued entries immediately. This method is public on the concrete class only. It is not part of `IAuditLogService`, so consumers that depend on the interface cannot call it.

It writes at most one batch per call. If more entries are queued, call it again or wait for the background flusher.

### `IDisposable.Dispose` and `Dispose(bool)`

`Dispose()` stops the background flusher and makes one final flush attempt. See [Lifecycle](#lifecycle). The protected virtual `Dispose(bool disposing)` overload is idempotent: a second call returns immediately.

## Lifecycle

Registration is `AddScoped`, so the DI container creates one `AuditLogService` per scope. When the scope ends, the container disposes it.

Disposal runs in this order:

1. Cancels the flush loop.
2. Waits up to 10 seconds for the flush loop to finish. A timeout is not reported as an error.
3. Disposes the flush task and cancellation source.
4. Calls `FlushAsync` once, which writes one batch of up to 50 entries.

Entries beyond that first batch remain in the queue and are lost when the instance is discarded. Any `LogAsync` call made after disposal is enqueued but never written.

Because each scope has its own service instance, entries queued in one scope are flushed by that instance only. Short-lived scopes with a few entries each therefore flush through the final flush in step 4.

## Usage

### Example 1: Record an audit event from a request handler

Resolve the service from DI and queue an entry. The caller does not wait for the write.

```csharp
public sealed class KeyUsageRecorder
{
    private readonly IAuditLogService _auditLogService;

    public KeyUsageRecorder(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public Task RecordUsageAsync(string keyId, string? sourceIp)
    {
        var entry = new AuditLog
        {
            Id = Guid.NewGuid().ToString("N"),
            ResourceId = keyId,
            ResourceType = "ApiKey",
            Action = AuditAction.KeyUsed,
            PerformedBy = "system",
            SourceIp = sourceIp,
            IsSuccess = true
        };

        return _auditLogService.LogAsync(entry);
    }
}
```

### Example 2: Read and export the history of one key

```csharp
var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();

var recent = await auditLogService.GetLogsAsync(keyId, limit: 25);
Console.WriteLine($"{recent.Count} entries for {keyId}");

var xml = await auditLogService.ExportLogsToXmlAsync(keyId, limit: 500);
await File.WriteAllTextAsync($"audit-{keyId}.xml", xml);
```

### Example 3: Nightly retention cleanup

```csharp
await auditLogService.CleanupOldLogsAsync(retentionDays: 90);
```

## Notes

- **Queued writes are not durable.** Entries live only in process memory until they are written. A crash or forced termination loses every entry still in the queue. Graceful shutdown through DI disposal flushes only the first batch.
- **Throughput is capped.** The flusher writes one batch of 50 entries every 5 seconds, which is about 10 entries per second. If entries arrive faster than that, the queue grows without limit. The service has no back-pressure or queue size limit.
- **Failed batches are retried at the back of the queue.** When a write fails, the whole batch is re-enqueued, including entries that were already written before the failure. Duplicates are possible. An entry that always fails is retried every cycle until it is written or the process stops.
- **Errors are logged, not thrown.** Flush failures are logged at error level with the message "Failed to flush {Count} audit logs to persistent storage". `LogAsync` never reports storage problems to the caller. Monitor the logs if audit completeness matters.
- **Ordering is not guaranteed after a failure.** Requeued entries go to the tail of the queue, so a later batch can contain entries that were logged after them.
- **Reads see only flushed data.** `GetLogsAsync`, `GetLogsForPeriodAsync`, and the export methods read storage directly. Entries queued in the last few seconds may be missing from the results.
- **Validation happens inside the returned task.** All argument checks are in `async` methods, so a bad argument produces a faulted task rather than a synchronous throw. Callers that discard the task will not see the exception.
- **Logging.** Diagnostics use the `ILogger<AuditLogService>` category. Flush batches are logged at debug level, cleanup results at information level, and failures at error level.
