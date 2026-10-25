# QuotaThresholdReachedEvent

The `QuotaThresholdReachedEvent` record is published when a consumer's usage in the current quota period crosses a configured alert threshold, such as 80% or 100% of the quota limit. It lets other parts of the gateway, or application code, react to a consumer approaching or exhausting its quota (for example, to send a notification) without polling usage data.

The type is defined in `src/ApiKeyGateway/Events/QuotaThresholdReachedEvent.cs`. It is produced by `QuotaAlertEvaluator` (`src/ApiKeyGateway/Services/QuotaAlertEvaluator.cs`) and delivered through `IEventPublisher`.

## API

All properties are read-only after construction. `EventId` and `Timestamp` are assigned automatically; the rest are set with an object initializer when the event is created.

* `EventId`: A unique identifier for this event instance, generated with `Guid.NewGuid()`.
* `Timestamp`: The UTC time the event object was created, set to `DateTime.UtcNow`.
* `ConsumerId`: The identifier of the consumer whose usage crossed the threshold. Defaults to `string.Empty`.
* `ApiKeyId`: The identifier of the API key whose quota was evaluated. Defaults to `string.Empty`.
* `ThresholdPercentage`: The threshold that was reached, as a whole percentage (for example `80` or `100`).
* `CurrentUsage`: The number of requests counted for the consumer in the current quota period, as a `long`.
* `QuotaLimit`: The total quota limit for the period, as a `long`.
* `PercentageUsed`: The actual usage as a percentage of `QuotaLimit`, as a `double`. This can be higher than `ThresholdPercentage`, for example when a single evaluation finds usage already past 100%.
* `PeriodStart`: The UTC start of the quota period in which usage was measured.
* `PeriodEnd`: The UTC end of the quota period, which is when the usage counter resets.

## When it is published

The event is published by `QuotaAlertEvaluator.EvaluateAsync`, which runs in the API key authentication middleware (`ApiKeyAuthenticationMiddleware`) after the downstream pipeline has completed without throwing. The request's usage is recorded before evaluation, so the current request is included in `CurrentUsage`.

An event is published only when all of the following hold:

* `QuotaAlerts:Enabled` is `true` (the default).
* The API key has an enabled quota with a `QuotaLimit` greater than `0`.
* `PercentageUsed` is at or above the threshold being checked.
* That threshold has not already been alerted for this consumer in this quota period.

Thresholds come from `QuotaAlerts:ThresholdPercentages` (default `[80, 100]`). Values outside `(0, 100]` are ignored, and the remaining values are de-duplicated and sorted ascending. If one request moves usage past several thresholds at once, one event is published per threshold, in ascending order.

Each threshold fires at most once per consumer per quota period. Once a higher threshold has been alerted, lower thresholds are not fired again in the same period.

Evaluation is best-effort. If the quota or usage lookup fails, or publishing throws, the error is logged and nothing is raised to the caller, so the request is not affected.

## Usage

The event is published through `IEventPublisher.PublishAsync`. To handle it, subscribe on the in-memory publisher, which is the implementation registered by default:

```csharp
public sealed class QuotaNotifier
{
    public QuotaNotifier(IEventPublisher publisher)
    {
        // Only the in-memory implementation (registered by default) exposes Subscribe.
        ((InMemoryEventPublisher)publisher).Subscribe<QuotaThresholdReachedEvent>(OnThresholdReachedAsync);
    }

    private Task OnThresholdReachedAsync(QuotaThresholdReachedEvent e)
    {
        if (e.ThresholdPercentage >= 100)
        {
            // Quota exhausted for this period: notify the consumer or ops team.
        }

        return Task.CompletedTask;
    }
}
```

The event can also be constructed directly in tests:

```csharp
var evt = new QuotaThresholdReachedEvent
{
    ConsumerId = "consumer-1",
    ApiKeyId = "key-1",
    ThresholdPercentage = 80,
    CurrentUsage = 800,
    QuotaLimit = 1000,
    PercentageUsed = 80.0,
    PeriodStart = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
    PeriodEnd = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
};
```

Configuration is bound from the `QuotaAlerts` section:

```json
"QuotaAlerts": {
  "Enabled": true,
  "ThresholdPercentages": [50, 80, 100]
}
```

## Notes

* The "at most once per period" guarantee is held in memory by the singleton `QuotaAlertStateCache`. After a process restart, thresholds that were already reached in the current period can fire again. Across multiple gateway instances, each instance tracks its own state, so the same threshold can fire once per instance.
* The state is recorded before the event is published. If publishing fails, that threshold is not retried for the rest of the period.
* `CurrentUsage` counts usage records for the consumer within the period, while `QuotaLimit` comes from the API key's quota. `PercentageUsed` is derived from these two values.
* `PeriodEnd` is computed from the quota's period settings. For monthly quotas it is the first day of the next month; for daily quotas it is the next day.
* `PercentageUsed` is a `double` and `CurrentUsage` is a `long`, so use the same numeric types when comparing them in code.
* `Subscribe` is defined on `InMemoryEventPublisher`, not on the `IEventPublisher` interface. Code that depends only on `IEventPublisher` cannot register handlers through that interface.
