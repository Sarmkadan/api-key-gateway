# RateLimitStoreFailureEvent

The `RateLimitStoreFailureEvent` record is published when the rate limit store cannot be reached and the gateway applies its configured failure policy instead of a real limit check. Subscribers can use it to alert on a degraded rate limiter, either one that is not enforcing limits (fail-open) or one that is rejecting traffic.

The type is defined in `src/ApiKeyGateway/Events/RateLimitStoreFailureEvent.cs`. It is produced by `RateLimitingService` (`src/ApiKeyGateway/Services/RateLimitingService.cs`) and delivered through `IEventPublisher`.

## API

All properties are read-only after construction. `EventId` and `Timestamp` are assigned automatically; the rest are set with an object initializer when the event is created.

* `EventId`: A unique identifier for this event instance, generated with `Guid.NewGuid()`.
* `Timestamp`: The UTC time the failure was observed, set to `DateTime.UtcNow` when the event object is created.
* `ApiKeyId`: The API key whose request hit the failure. Required.
* `Operation`: The limiter operation that failed. The value is the name of the method that applied the policy: `CheckLimitAsync` or `RecordRequestAsync`. Required.
* `PolicyApplied`: The `RateLimitFailurePolicy` in effect when the failure was handled, as a `RateLimitFailurePolicy` enum value. Required. Because the event is only published under the fail-open policy, this is `FailOpen` in practice.
* `CircuitOpen`: `true` when the circuit breaker was already open and the store was skipped without being called. `false` when the store was called and threw a `DataAccessException`. Defaults to `false`.
* `ErrorMessage`: The message of the underlying `DataAccessException`. `null` when `CircuitOpen` is `true`, because no store call was made.

`RateLimitFailurePolicy` is defined in `src/ApiKeyGateway/Configuration/RateLimitingOptions.cs`. It has two values: `FailOpen` and `FailClosed`.

## When it is published

The event is published from `RateLimitingService.PublishFailureEventAsync`, which is called by `ApplyFailurePolicyAsync`. That method runs in two situations:

* **Circuit breaker open.** `CheckLimitAsync` or `RecordRequestAsync` finds the circuit open, so the store is skipped. `CircuitOpen` is `true` and `ErrorMessage` is `null`.
* **Store failure.** The repository call throws a `DataAccessException`. The failure is recorded toward the circuit breaker, and `CircuitOpen` is `false`.

The event is published only when `RateLimiting:FailurePolicy` is `FailOpen`. Under `FailClosed`, `ApplyFailurePolicyAsync` throws `KeyStoreUnavailableException` before the publish step, so no event is raised and the gateway returns 503 instead. `FailClosed` is the default, so the event is not published unless the policy is changed.

Under `FailOpen`, the request is allowed through, and the event is published after the warning or error log entry is written.

Publishing is best-effort. If `IEventPublisher.PublishAsync` throws, the exception is logged and not raised to the caller, so the request is not affected. If no `IEventPublisher` is supplied to the service, nothing is published.

The circuit breaker opens after `RateLimiting:CircuitBreakerFailureThreshold` consecutive store failures (default `5`) within `RateLimiting:CircuitBreakerFailureWindow` (default 30 seconds). It stays open for `RateLimiting:CircuitBreakerCooldown` (default 30 seconds), during which every limiter call is handled by the failure policy without touching the store.

## Usage

The event is published through `IEventPublisher.PublishAsync`. To handle it, subscribe on the in-memory publisher, which is the implementation registered by default:

```csharp
public sealed class RateLimiterHealthAlerts
{
    public RateLimiterHealthAlerts(IEventPublisher publisher)
    {
        // Only the in-memory implementation (registered by default) exposes Subscribe.
        ((InMemoryEventPublisher)publisher).Subscribe<RateLimitStoreFailureEvent>(OnStoreFailureAsync);
    }

    private Task OnStoreFailureAsync(RateLimitStoreFailureEvent e)
    {
        if (e.CircuitOpen)
        {
            // The store is being skipped: the limiter is not enforcing limits right now.
        }

        return Task.CompletedTask;
    }
}
```

The event can also be constructed directly in tests:

```csharp
var evt = new RateLimitStoreFailureEvent
{
    ApiKeyId = "key-1",
    Operation = "CheckLimitAsync",
    PolicyApplied = RateLimitFailurePolicy.FailOpen,
    CircuitOpen = false,
    ErrorMessage = "Connection timeout"
};
```

Configuration is bound from the `RateLimiting` section:

```json
"RateLimiting": {
  "FailurePolicy": "FailOpen",
  "CircuitBreakerFailureThreshold": 5,
  "CircuitBreakerFailureWindow": "00:00:30",
  "CircuitBreakerCooldown": "00:00:30"
}
```

## Notes

* Under the default `FailClosed` policy this event is never published. To receive it, set `RateLimiting:FailurePolicy` to `FailOpen`, which trades strict enforcement for availability while the store is down.
* `Subscribe` is defined on `InMemoryEventPublisher`, not on the `IEventPublisher` interface. Code that depends only on `IEventPublisher` cannot register handlers through that interface.
* `ErrorMessage` is copied from the data-access exception, which may include store-specific detail. Treat it as internal and avoid returning it to API clients.
* Each failed limiter call publishes its own event. During an outage, many events can be raised for the same key, so subscribers that send notifications should rate-limit or aggregate them.
