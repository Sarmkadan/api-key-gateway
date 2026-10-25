# BackgroundServiceBase

`BackgroundServiceBase<TWorker>` is an abstract base class for the hosted background workers in `ApiKeyGateway.BackgroundWorkers`. It derives from `Microsoft.Extensions.Hosting.BackgroundService` and supplies the common parts every worker needs: a timer loop that runs one cycle at a time, a catch-all error handler with a back-off delay, cancellation handling on shutdown, and a helper for creating DI scopes. A concrete worker only implements `ExecuteCycleAsync` (the work itself) and, optionally, `GetErrorDelay`. It is defined in `src/ApiKeyGateway/BackgroundWorkers/BackgroundServiceBase.cs`.

Subclasses in the project:

| Worker | Notes |
|---|---|
| [AuditLogCleanupWorker](AuditLogCleanupWorker.md) | Deletes audit log entries past retention. Overrides `ExecuteCycleAsync` and `GetErrorDelay`. |
| `KeyRotationScheduler` | Rotates API keys nearing expiration, on a daily cadence. Delegates to `IApiKeyRotationService`. |
| `RateLimitResetScheduler` | Clears expired rate limit counters from the cache so quotas reset when their window ends. |
| `UsageAggregationWorker` | Groups request records into hourly and daily usage summaries for analytics. |

All of these are `sealed`, so the base class is only extended one level deep.

## Declaration

```csharp
public abstract class BackgroundServiceBase<TWorker> : BackgroundService
    where TWorker : BackgroundServiceBase<TWorker>
```

| Type parameter | Constraint | Purpose |
|---|---|---|
| `TWorker` | Must be the derived worker type (the "curiously recurring" pattern) | Gives the logger a typed category (`ILogger<TWorker>`) and supplies the worker name used in log messages. |

## Members

### Protected fields

| Name | Type | Initial value | Description |
|---|---|---|---|
| `_serviceProvider` | `IServiceProvider` | Set by constructor | Root service provider. Used only through `CreateScope()`. |
| `_logger` | `ILogger<TWorker>` | Set by constructor | Logger for lifecycle, error, and subclass messages. |
| `_defaultDelay` | `TimeSpan` | `TimeSpan.FromMinutes(1)` | Pause between the end of one successful cycle and the start of the next. Read-only: a subclass cannot change it. |

### Constructor

```csharp
protected BackgroundServiceBase(IServiceProvider serviceProvider, ILogger<TWorker> logger)
```

Protected, so it is only callable from a derived class. Both arguments are required. `serviceProvider` and `logger` each throw `ArgumentNullException` when `null`.

### `ExecuteAsync`

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
```

The main loop. It is an override of `BackgroundService.ExecuteAsync` and is called by the host when the worker starts. Subclasses do not normally override it. The loop is described in [Lifecycle](#lifecycle).

### `ExecuteCycleAsync`

```csharp
protected abstract Task ExecuteCycleAsync(CancellationToken stoppingToken);
```

The one method every worker must implement. It performs a single pass of the worker's work. The base loop calls it repeatedly. It should:

- Observe `stoppingToken` and stop promptly when it is cancelled.
- Let exceptions propagate. The base loop logs them and applies the error delay. Catching and swallowing errors inside the cycle hides them from the base logging.
- Create its own scope with `CreateScope()` when it needs scoped services. Scopes are not kept between cycles.

### `GetErrorDelay`

```csharp
protected virtual TimeSpan GetErrorDelay(CancellationToken stoppingToken)
```

Returns how long the loop waits after an unhandled exception from `ExecuteCycleAsync`. The default is a fixed 5 minutes. Override it to change the back-off, for example to grow the delay with the number of consecutive failures. The `stoppingToken` argument is passed for overrides that need it; the default implementation ignores it.

### `CreateScope`

```csharp
protected IServiceScope CreateScope()
```

Returns `_serviceProvider.CreateScope()`. Use it to resolve scoped services such as repositories. The caller owns the scope and must dispose it, normally with a `using` declaration:

```csharp
using var scope = CreateScope();
var repository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
```

## Lifecycle

The loop in `ExecuteAsync` runs until `stoppingToken` is cancelled:

1. Logs `{TWorker} started` at Information level.
2. Calls `ExecuteCycleAsync(stoppingToken)`.
3. On success, waits `_defaultDelay` (1 minute) with `Task.Delay`, then returns to step 2.
4. On `OperationCanceledException`, logs `{TWorker} shutting down` and exits the loop. This is the normal shutdown path.
5. On any other exception, logs it at Error level as `Unexpected error in {TWorker}`, waits `GetErrorDelay(stoppingToken)`, then returns to step 2.

Timing is therefore: cycle, then a 1-minute pause on success, or a longer pause after a failure. The pause is part of the loop, not of the cycle, so a worker with `ExecuteCycleAsync` that waits internally (as `AuditLogCleanupWorker` does) adds its own wait on top of the base pause.

### Cancellation

Any `OperationCanceledException` that reaches the loop ends the worker, not only one caused by `stoppingToken`. A subclass should not let unrelated cancellation exceptions escape from `ExecuteCycleAsync`.

If shutdown is requested while the loop is in the error back-off, the `Task.Delay` in the `catch (Exception)` block throws `OperationCanceledException`. That exception is not handled by the `catch` block it came from, so it propagates out of `ExecuteAsync`. The host treats this as a normal stop.

### Errors

Any exception other than `OperationCanceledException` is logged and does not stop the worker. The loop keeps going after `GetErrorDelay`. Because the back-off is applied on every failure, a repeatedly failing cycle runs at the error interval, not at the 1-minute interval.

## Example

A minimal worker built on the base class:

```csharp
public sealed class ExampleWorker : BackgroundServiceBase<ExampleWorker>
{
    public ExampleWorker(IServiceProvider serviceProvider, ILogger<ExampleWorker> logger)
        : base(serviceProvider, logger)
    {
    }

    protected override async Task ExecuteCycleAsync(CancellationToken stoppingToken)
    {
        using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IExampleService>();

        await service.DoWorkAsync(stoppingToken);
    }

    protected override TimeSpan GetErrorDelay(CancellationToken stoppingToken)
    {
        return TimeSpan.FromMinutes(1);
    }
}
```

Register the worker with `services.AddHostedService<ExampleWorker>();`.
