# LuaScriptEvent

`LuaScriptEvent` is the abstract base record for events describing the lifecycle of Lua scripts that run inside request transformation rules. Three concrete records derive from it:

* `LuaScriptFailedEvent`: a script execution failed because of a timeout, a runtime error, a syntax error, a memory limit breach, or an unexpected exception.
* `LuaScriptQuarantinedEvent`: a script was skipped because it is quarantined after repeated failures.
* `LuaScriptReenabledEvent`: a quarantined script was re-enabled by an operator.

The types are defined in `src/ApiKeyGateway/Events/LuaScriptEvent.cs`. They are produced by `LuaScriptExecutor` (`src/ApiKeyGateway/Transformation/LuaScriptExecutor.cs`) and delivered through `IEventPublisher`.

## Base properties

These properties are declared on `LuaScriptEvent` and inherited by every derived event.

* `EventId`: A unique identifier for this event instance, generated with `Guid.NewGuid()`. It is read-only after construction.
* `Timestamp`: The UTC time the event object was created, set to `DateTime.UtcNow`. It is read-only after construction.
* `ScriptId`: The identifier of the Lua script involved. The executor uses the transformation rule's `RuleId`, or `"unknown"` when no rule ID is available.
* `ScriptName`: The display name of the Lua script involved. The executor uses the rule's `RuleName`, or `"unknown"` when no name is available.
* `ApiKeyId`: The ID of the API key whose request triggered the execution. Nullable.
* `ConsumerId`: The ID of the consumer whose request triggered the execution. Nullable.
* `ErrorType`: A short category string for the error. Set by the executor to one of the values listed in [Error types](#error-types).
* `ErrorMessage`: A human-readable description of the error.

## LuaScriptFailedEvent

Published when a script execution ends with an error. Besides the base properties, it has:

* `FailureType`: The specific kind of failure. It carries the same value as `ErrorType`.
* `ExecutionTimeMs`: The time the script ran before failing, in milliseconds, as an `int`. For timeouts this is the configured limit, `LuaExecutionOptions.MaxExecutionMs`.
* `MemoryUsedBytes`: The managed memory allocated on the executing thread before the failure, in bytes, as a `long`.

## LuaScriptQuarantinedEvent

Published each time a request reaches a quarantined script and the script is skipped. The request still proceeds. Besides the base properties, it has:

* `ConsecutiveFailures`: The failure count stored for the script's quarantine entry.
* `QuarantineThreshold`: The configured failure count that triggers quarantine, from `LuaExecutionOptions.QuarantineThreshold`.

The executor sets `ErrorType` to `"quarantined"` and `ErrorMessage` to a message in the form `Script quarantined after {n}/{threshold} failures`.

## LuaScriptReenabledEvent

Describes an operator re-enabling a quarantined script.

* `Reason`: Why the script was re-enabled. Defaults to `"Operator re-enabled after quarantine"`.

This record is defined in the codebase, but nothing in `LuaScriptExecutor` or elsewhere in `src/` publishes it. Code that needs this notification has to publish the event itself.

## Error types

`LuaScriptExecutor` sets `ErrorType` (and, for failures, `FailureType`) to one of these values:

| Value | Event | Cause |
|---|---|---|
| `timeout` | `LuaScriptFailedEvent` | The script ran longer than `LuaExecutionOptions.MaxExecutionMs`. |
| `runtime_error` | `LuaScriptFailedEvent` | The Lua runtime raised an unhandled error. |
| `syntax_error` | `LuaScriptFailedEvent` | The script failed to parse. |
| `out_of_memory` | `LuaScriptFailedEvent` | The script allocated more than `LuaExecutionOptions.MaxMemoryBytes`. |
| `unexpected_error` | `LuaScriptFailedEvent` | Any other exception escaped the execution. |
| `quarantined` | `LuaScriptQuarantinedEvent` | The script is in quarantine and was skipped. |

## Relationship to LuaScriptExecutor

`LuaScriptExecutor.ExecuteAsync` is the only publisher of the events in this codebase, and it publishes only on the paths below. Successful runs and scripts that return `"BLOCK"` produce no event.

* **Failures.** Each `catch` block in `ExecuteAsync` publishes a `LuaScriptFailedEvent` before it records the failure and rethrows. The `unexpected_error` branch rethrows the original exception, and the other branches throw a `TransformationScriptException` or an `OperationCanceledException`. The caller sees the exception, and the event is an out-of-band notification.
* **Quarantine skips.** If the script's key and consumer pair is quarantined when `ExecuteAsync` starts, the executor publishes a `LuaScriptQuarantinedEvent` and returns `true`, so the request proceeds without the script running.
* **Quarantine entry.** When a failure brings the count to `LuaExecutionOptions.QuarantineThreshold`, the script is quarantined for `QuarantineDurationSeconds`. No event is published at that moment. The `LuaScriptQuarantinedEvent` appears on the next request that hits the quarantined script.
* **Quarantine is keyed per API key and consumer.** The registry key is `{ApiKeyId}:{ConsumerId}`, so one consumer's failures do not quarantine another consumer's script.

Publishing is best-effort. Each publish helper catches exceptions from `IEventPublisher.PublishAsync`, logs them, and does not rethrow. An event that cannot be delivered therefore never changes the outcome of a request.

## Delivery

Events are published through `IEventPublisher.PublishAsync<T>`, using the concrete event type as `T`. `InMemoryEventPublisher` routes events by exact type, so a subscriber on `LuaScriptEvent` never receives the derived events. Subscribe to each concrete type you need:

```csharp
public sealed class LuaScriptMonitor
{
    public LuaScriptMonitor(IEventPublisher publisher)
    {
        // Only the in-memory implementation (registered by default) exposes Subscribe.
        var inMemory = (InMemoryEventPublisher)publisher;

        inMemory.Subscribe<LuaScriptFailedEvent>(OnScriptFailedAsync);
        inMemory.Subscribe<LuaScriptQuarantinedEvent>(OnScriptQuarantinedAsync);
    }

    private Task OnScriptFailedAsync(LuaScriptFailedEvent e)
    {
        if (e.ErrorType == "out_of_memory")
        {
            // Investigate the rule that exceeded its memory budget.
        }

        return Task.CompletedTask;
    }

    private Task OnScriptQuarantinedAsync(LuaScriptQuarantinedEvent e)
    {
        // Notify operators that the script is being skipped.
        return Task.CompletedTask;
    }
}
```

## Configuration

The thresholds and limits that drive these events come from `LuaExecutionOptions` (`src/ApiKeyGateway/Configuration/TransformationOptions.cs`):

* `MaxExecutionMs`: The wall-clock timeout for one execution. Exceeding it produces a `timeout` failure.
* `MaxMemoryBytes`: The allocation budget for one execution. Exceeding it produces an `out_of_memory` failure.
* `MaxScriptSizeBytes`: The maximum script size. A script over this size is rejected before it runs, and no event is published for that rejection.
* `QuarantineThreshold`: The number of consecutive failures that triggers quarantine. A value of `0` or less disables quarantine, so no `LuaScriptQuarantinedEvent` is published.
* `QuarantineDurationSeconds`: How long a quarantine lasts. After it expires, the script runs again and its failure count is cleared.
* `DisallowedGlobals`: Additional globals removed from the sandbox.

## Related

* `LuaScriptExecutor`: the producer of all three events.
* `ILuaScriptExecutor`: the interface through which scripts are executed.
* `IEventPublisher` and `InMemoryEventPublisher`: the delivery mechanism.
* `QuotaThresholdReachedEvent`: another event type in the same `Events` folder, for comparison.
