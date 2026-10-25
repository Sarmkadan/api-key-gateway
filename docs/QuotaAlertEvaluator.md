# QuotaAlertEvaluator

`QuotaAlertEvaluator` checks how much of its quota a consumer has used after a request has been recorded. When usage crosses a configured threshold (by default 80% and 100% of the quota limit) it publishes a `QuotaThresholdReachedEvent` through `IEventPublisher`. Each threshold fires at most once per consumer per quota period.

The feature is split across these types, all in `src/ApiKeyGateway/Services/QuotaAlertEvaluator.cs` unless noted:

* `IQuotaAlertEvaluator`: the interface the middleware depends on.
* `QuotaAlertStateCache`: a singleton that remembers which thresholds have already fired.
* `QuotaAlertEvaluator`: the default implementation of `IQuotaAlertEvaluator`.
* `QuotaAlertOptions` (`src/ApiKeyGateway/Configuration/QuotaAlertOptions.cs`): configuration bound from the `QuotaAlerts` section.
* `QuotaThresholdReachedEvent` (`src/ApiKeyGateway/Events/`): the event that is published. See [QuotaThresholdReachedEvent](QuotaThresholdReachedEvent.md).

Registration is in `Configuration/ServiceCollectionExtensions.cs`: `QuotaAlertOptions` is bound with `Configure`, `QuotaAlertStateCache` is added as a singleton, and `IQuotaAlertEvaluator` is added as scoped.

## API

### `public interface IQuotaAlertEvaluator`

Contract for evaluating a consumer's quota usage. The API key authentication middleware calls it for every authenticated request.

#### `Task EvaluateAsync(string consumerId, string apiKeyId)`

Compares the consumer's usage in the current quota period with the API key's quota limit and publishes an event for each threshold that was newly crossed.

- **Parameters**
  - `consumerId` – Identifier of the consumer that made the request.
  - `apiKeyId` – Identifier of the API key used for the request.
- **Return value**
  - A `Task` that completes when evaluation is finished. Nothing is returned.
- **Exceptions**
  - Throws `ArgumentException` if `consumerId` or `apiKeyId` is `null` or empty. This check runs before the best-effort section.
  - Does not throw for downstream failures. Lookup or publish errors are logged at `Error` level and swallowed.

---

### `public sealed class QuotaAlertStateCache`

In-memory record of the highest threshold already alerted for each consumer in each quota period. It is registered as a singleton and is safe for concurrent use. Its state is per process and is lost on restart.

#### `public bool TryMarkAlerted(string consumerId, DateTime periodStart, int threshold)`

Atomically records that `threshold` has been alerted for the consumer in the given period.

- **Parameters**
  - `consumerId` – Consumer identifier.
  - `periodStart` – UTC start of the current quota period.
  - `threshold` – Threshold percentage being alerted.
- **Return value**
  - `true` if this call advanced the state and the caller should publish the event.
  - `false` if the threshold, or a higher one, was already alerted in this period.
- **Behavior**
  - If the stored period differs from `periodStart`, the stored state is replaced and `true` is returned. This is how a new period resets the alerts.
  - Within the same period, `true` is returned only when `threshold` is higher than the stored value. The stored value becomes the higher of the two.
  - Concurrent callers retry with a compare-and-swap, so only one caller wins for a given threshold.
- **Exceptions**
  - Throws `ArgumentException` if `consumerId` is `null` or empty.

#### `public int GetLastAlertedThreshold(string consumerId, DateTime periodStart)`

Returns the highest threshold already alerted for the consumer in the given period.

- **Parameters**
  - `consumerId` – Consumer identifier.
  - `periodStart` – UTC start of the current quota period.
- **Return value**
  - The highest alerted threshold percentage, or `0` if nothing has been alerted in that period. A stored entry for a different period also returns `0`.
- **Exceptions**
  - Throws `ArgumentException` if `consumerId` is `null` or empty.

---

### `public class QuotaAlertEvaluator : IQuotaAlertEvaluator`

Default implementation. It reads the API key's quota, counts the consumer's usage records in the current period, and publishes events for thresholds that were crossed and not yet alerted.

#### `public QuotaAlertEvaluator(IUsageRepository usageRepository, IUsageQuotaRepository quotaRepository, IEventPublisher eventPublisher, QuotaAlertStateCache stateCache, IOptions<QuotaAlertOptions> options, ILogger<QuotaAlertEvaluator> logger)`

- **Parameters**
  - `usageRepository` – Used to count usage records in the period.
  - `quotaRepository` – Used to look up the quota for the API key.
  - `eventPublisher` – Used to publish `QuotaThresholdReachedEvent`.
  - `stateCache` – Shared cache of alerted thresholds.
  - `options` – Supplies `Enabled` and `ThresholdPercentages`. The value is read once, at construction.
  - `logger` – Logger for warnings and errors.
- **Exceptions**
  - Throws `ArgumentNullException` if any argument is `null`.

#### `public async Task EvaluateAsync(string consumerId, string apiKeyId)`

Runs one evaluation. The steps are:

1. Validate both identifiers. Throws `ArgumentException` if either is `null` or empty.
2. Return if `Enabled` is `false`.
3. Return if no effective thresholds are configured.
4. Load the quota by API key ID. Return unless the quota is enabled and `QuotaLimit` is greater than `0`.
5. Compute `periodStart` with `UsageQuota.GetPeriodStart(now, quota.Period)`, where `now` is `DateTime.UtcNow`.
6. Count the consumer's usage records from `periodStart` to `now`. This count is `CurrentUsage`. `PercentageUsed` is `CurrentUsage * 100.0 / QuotaLimit`.
7. Walk the thresholds in ascending order. Stop at the first threshold that `PercentageUsed` is below. For each threshold that was reached, call `TryMarkAlerted`. If it returns `true`, log a warning and publish a `QuotaThresholdReachedEvent`.

Any exception in steps 4 to 7 is logged at `Error` level with the consumer and key IDs and is not rethrown.

---

## Threshold logic

Thresholds come from `QuotaAlertOptions.GetEffectiveThresholds()`:

* Values outside `(0, 100]` are dropped.
* Remaining values are de-duplicated and sorted ascending.
* The default is `[80, 100]`.

Given `percentUsed`, a threshold `t` fires only when all of these hold:

* `percentUsed >= t`. The walk stops at the first `t` above `percentUsed`, so higher thresholds are never checked in that case.
* `TryMarkAlerted(consumerId, periodStart, t)` returns `true`. That is, no threshold `>= t` was already alerted in this period.

Worked examples with the default thresholds and a quota of 1000 requests, evaluated in order for the same consumer and period:

| Evaluation | Usage in period | `PercentageUsed` | Events published by this call |
|---|---|---|---|
| 1 | 799 | 79.9% | none |
| 2 | 800 | 80.0% | `80` |
| 3 | 950 | 95.0% | none (80 already alerted) |
| 4 | 1000 | 100.0% | `100` |
| 5 | 1200 | 120.0% | none (100 already alerted) |

If a single evaluation sees usage jump from below 80% straight to above 100% (for example, if evaluations were skipped for a while), both `80` and `100` fire in ascending order within that one call. Under the middleware, each request is evaluated after it is recorded, so this is uncommon.

A new quota period starts a fresh state. `QuotaPeriod` values are mapped by `UsageQuota.GetPeriodStart`: `Hour` truncates to the hour, `Daily`/`Day` to midnight UTC, `Week` to the most recent Sunday at midnight UTC, and `Monthly`/`Month` to the first day of the month at midnight UTC.

---

## Configuration

Bound from the `QuotaAlerts` section (`QuotaAlertOptions.SectionName`):

```json
{
  "QuotaAlerts": {
    "Enabled": true,
    "ThresholdPercentages": [80, 100]
  }
}
```

* `Enabled` (`bool`, default `true`): set to `false` to turn off evaluation entirely.
* `ThresholdPercentages` (`List<int>`, default `[80, 100]`): percentages of the quota limit at which events are published. Invalid values are ignored as described above.

---

## Usage

The evaluator is called by `ApiKeyAuthenticationMiddleware` after a request has been authenticated and recorded. The middleware passes the consumer ID and the API key ID from `HttpContext.Items`. It catches any exception from the call, so the request is never affected.

Application code normally does not call the evaluator directly. To react to the events, subscribe to `QuotaThresholdReachedEvent` through the event system. To call the evaluator yourself, for example from a background job, resolve `IQuotaAlertEvaluator` from DI:

```csharp
// Inside a scoped service or endpoint
var evaluator = scope.ServiceProvider.GetRequiredService<IQuotaAlertEvaluator>();
await evaluator.EvaluateAsync(consumerId, apiKeyId);
```

Keep in mind that the usage count is taken from the usage repository, and the `QuotaAlertStateCache` is in memory. In a deployment with several instances, each instance tracks its own alerts, so a threshold can fire once per instance in the same period. The cache also keeps one entry per consumer and does not evict entries from past periods. A stale entry is replaced the next time that consumer is evaluated.

## Related

* [QuotaThresholdReachedEvent](QuotaThresholdReachedEvent.md)
* [UsageQuota](UsageQuota.md)
* [UsageQuotaRepository](UsageQuotaRepository.md)
