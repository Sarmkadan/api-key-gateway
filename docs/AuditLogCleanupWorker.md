# AuditLogCleanupWorker

`AuditLogCleanupWorker` is a hosted background service that periodically deletes audit log entries older than a fixed retention period (90 days). It keeps recent audit history for compliance while stopping the `AuditLogs` table from growing without bound. The worker only deletes rows; it does not archive them. It is defined in `src/ApiKeyGateway/BackgroundWorkers/AuditLogCleanupWorker.cs` and registered in `Configuration/ServiceRegistrationExtensions.cs` with `services.AddHostedService<AuditLogCleanupWorker>()`.

- `AuditLogCleanupWorker`: `public sealed class AuditLogCleanupWorker : BackgroundServiceBase<AuditLogCleanupWorker>`. It is sealed and is not meant to be subclassed.
- Base class: `BackgroundServiceBase<TWorker>` (`BackgroundWorkers/BackgroundServiceBase.cs`) supplies the timer loop, cancellation handling, error handling, and scope creation. This worker overrides `ExecuteCycleAsync` and `GetErrorDelay`.
- Data access: the actual deletion is done by `IAuditLogRepository.DeleteOlderThanAsync`. See [AuditLogRepository](AuditLogRepository.md).

## Constructor

```csharp
public AuditLogCleanupWorker(IServiceProvider serviceProvider, ILogger<AuditLogCleanupWorker> logger)
```

| Parameter | Type | Required | Description |
|---|---|---|---|
| `serviceProvider` | `IServiceProvider` | Yes | Root provider. The worker creates a new scope on each cycle to resolve the scoped `IAuditLogRepository`. |
| `logger` | `ILogger<AuditLogCleanupWorker>` | Yes | Receives lifecycle, pass-summary, and error logs. |

The constructor passes both arguments to `BackgroundServiceBase<TWorker>`, which throws `ArgumentNullException` if either is `null`.

The worker has no configuration parameters. Its settings are private fields with fixed values:

| Field | Value | Meaning |
|---|---|---|
| `_retentionDays` | `90` | Entries whose timestamp is before `DateTime.UtcNow.AddDays(-90)` are deleted. |
| `_cleanupInterval` | `TimeSpan.FromDays(1)` | Wait between successful cleanup passes. |
| `_totalDeleted` | `0` | Running count of entries deleted by this process. Kept in memory only. |

> **Note:** `docs/ARCHITECTURE.md` lists this worker as purging logs older than `Gateway:AuditLogRetentionDays`. The code does not read that setting. The retention period is always 90 days, regardless of `AuditLogRetentionDays` in `appsettings.json` or `GatewayConfiguration`.

## Methods

### `ExecuteCycleAsync`

```csharp
protected override async Task ExecuteCycleAsync(CancellationToken stoppingToken)
```

Runs one cleanup pass. The steps are:

1. If `_totalDeleted` is `0`, waits 5 minutes with `Task.Delay`. This gives the application time to finish starting up before the first pass.
2. Creates a DI scope and resolves `IAuditLogRepository`.
3. Computes `cutoffDate = DateTime.UtcNow.AddDays(-90)`.
4. Logs the start of the pass with the cutoff date.
5. Calls `DeleteOlderThanAsync(cutoffDate)` and times the call with a `Stopwatch`.
6. Adds the returned count to `_totalDeleted`.
7. Logs the number of entries removed and a summary line with the pass count, the running total, and the duration in milliseconds.
8. Waits 1 day with `Task.Delay(_cleanupInterval, stoppingToken)`.

`DeleteOlderThanAsync` does not take a `CancellationToken`. A delete that has started runs to completion even if shutdown has been requested. Cancellation takes effect at the next `Task.Delay`.

### `GetErrorDelay`

```csharp
protected override TimeSpan GetErrorDelay(CancellationToken stoppingToken)
```

Returns a fixed 5 minutes. This overrides the base default, which is also 5 minutes, so the behavior is the same. The override exists so the delay is explicit in this worker.

### Inherited: `ExecuteAsync`

`BackgroundServiceBase<TWorker>.ExecuteAsync` runs the loop. This worker does not override it. The loop is described under [Lifecycle](#lifecycle).

## Lifecycle

### Startup

When the host starts, `ExecuteAsync` logs `AuditLogCleanupWorker started` and enters its loop. The first call to `ExecuteCycleAsync` happens at once, but because `_totalDeleted` is `0` it first waits 5 minutes. The first deletion therefore occurs about 5 minutes after startup.

### Normal cycle timing

After each successful pass the worker waits 1 day inside `ExecuteCycleAsync`. The base loop then waits another `_defaultDelay` of 1 minute before calling `ExecuteCycleAsync` again. The interval between the start of successive passes is about 1 day and 1 minute.

The 5-minute startup wait applies whenever `_totalDeleted` is still `0`. This happens on every pass until at least one entry has been deleted in the current process. On a fresh deployment, or when no logs are old enough to delete, each pass therefore takes about 5 minutes longer than the 1-day interval. A pass that deletes nothing leaves the total at `0`, so the wait applies to the next pass too. The total is only kept in memory, so it resets to `0` on each process restart and the 5-minute wait applies again after a restart.

### Errors

Any exception other than `OperationCanceledException` thrown inside `ExecuteCycleAsync` is caught by the base loop. The base loop:

1. Logs `Unexpected error in AuditLogCleanupWorker` at `Error` level with the exception.
2. Waits the value returned by `GetErrorDelay` (5 minutes).
3. Calls `ExecuteCycleAsync` again. The pass is retried in full, including the initial wait when `_totalDeleted` is `0`.

The `_defaultDelay` of 1 minute is not used after an error. A failure does not stop the worker. Because `_totalDeleted` is only updated after `DeleteOlderThanAsync` returns, a failed pass adds nothing to the total.

### Shutdown

When the host stops, `stoppingToken` is cancelled. If the worker is inside one of its `Task.Delay` calls, the call throws `OperationCanceledException`. The base loop catches it, logs `AuditLogCleanupWorker shutting down`, and exits the loop. The worker does not restart after shutdown.

If shutdown is requested while `DeleteOlderThanAsync` is running, the delete is not interrupted. The worker exits at the next `Task.Delay` after the delete completes.

## Logging

| Level | Message | When |
|---|---|---|
| Information | `AuditLogCleanupWorker started` | Once, at the start of `ExecuteAsync`. Logged by the base class. |
| Information | `Starting audit log cleanup for logs before {CutoffDate}` | Before each delete. |
| Information | `Cleaned up {Count} audit log entries older than {RetentionDays} days` | After each delete. |
| Information | `Cleanup pass summary: deleted {DeletedThisPass} records, total {TotalDeleted} records, duration {DurationMs} ms` | After each delete. |
| Information | `AuditLogCleanupWorker shutting down` | When the worker stops because of cancellation. Logged by the base class. |
| Error | `Unexpected error in AuditLogCleanupWorker` | When a pass throws an exception other than cancellation. Logged by the base class. |

## Related

- [AuditLogRepository](AuditLogRepository.md): implements `DeleteOlderThanAsync`.
- [AuditLogService](AuditLogService.md): also calls `DeleteOlderThanAsync` (see `Services/AuditLogService.cs`).
