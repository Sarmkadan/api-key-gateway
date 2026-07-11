# UsageQuota

Represents a quota configuration and tracking mechanism for API key usage within a specified period. Tracks consumption against a defined limit and provides methods to reset or query period boundaries.

## API

### `Id`
Gets the unique identifier for this quota record. Used for persistence and reference.

### `ApiKeyId`
Gets the identifier of the API key to which this quota applies. Establishes the relationship between quota and key.

### `QuotaLimit`
Gets the maximum allowed number of requests within the quota period. Exceeding this value triggers rate limiting.

### `IsEnabled`
Gets or sets whether the quota is active. Disabled quotas do not enforce limits or track usage.

### `Period`
Gets the time period over which the quota is enforced. Defines the rolling window for usage tracking.

### `CreatedAt`
Gets the timestamp when the quota record was created. Immutable after initialization.

### `PeriodStartAt`
Gets the start of the current quota period. Updated when the period resets.

### `CurrentUsage`
Gets the number of requests recorded in the current period. Reflects real-time consumption.

### `GetPeriodEndUtc()`
Calculates the UTC timestamp marking the end of the current quota period.

Returns: `DateTime` representing the period boundary in UTC.

### `GetPeriodStart(DateTime utcNow)`
Static method to compute the start of a quota period given a reference time.

Parameter: `utcNow` – Current UTC time used to determine period boundaries.

Returns: `DateTime` representing the start of the relevant period.

### `ResetPeriod()`
Advances the period to the next interval and resets usage tracking. Called automatically when the period boundary is crossed or manually to force a reset.

### `RecordRequest()`
Increments the current usage counter by one. Should be called for each processed request.

## Usage

```csharp
// Example 1: Checking quota before processing a request
var quota = await _quotaStore.GetByApiKeyAsync(apiKeyId);
if (quota.IsEnabled && quota.CurrentUsage >= quota.QuotaLimit)
{
    throw new RateLimitExceededException(quota);
}
quota.RecordRequest();
await _quotaStore.UpdateAsync(quota);

// Example 2: Resetting quota at period boundary
var now = DateTime.UtcNow;
var start = UsageQuota.GetPeriodStart(now);
var end = quota.GetPeriodEndUtc();
if (now >= end)
{
    quota.ResetPeriod();
    await _quotaStore.UpdateAsync(quota);
}
```

## Notes

- Thread safety: All public members are safe for concurrent access. Internal state changes (e.g., `CurrentUsage`, `PeriodStartAt`) are guarded by locks to prevent race conditions during `RecordRequest` and `ResetPeriod`.
- `ResetPeriod` recalculates `PeriodStartAt` based on the current UTC time and the `Period` enum, ensuring alignment with period boundaries.
- `CurrentUsage` may temporarily exceed `QuotaLimit` during high-concurrency scenarios if requests are processed between the limit check and the `RecordRequest` call. Implementations should re-validate after recording.
- `PeriodStartAt` is updated atomically with usage reset to maintain consistency across distributed instances if applicable.
- `GetPeriodStart` uses integer division on ticks to ensure deterministic period boundaries regardless of system clock skew.
