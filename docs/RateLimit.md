# RateLimit

`RateLimit` models a time-windowed request throttle bound to a specific API key. It tracks the number of requests made within a configurable unit of time, enforces a ceiling on that count, and exposes both inspection and mutation methods for evaluating whether an incoming request should be permitted.

## API

### Properties

#### `public string Id`
Unique identifier for this rate limit entry.

#### `public string ApiKeyId`
The identifier of the API key to which this rate limit applies. Used to correlate the limit with its owning key.

#### `public int RequestsPerUnit`
Maximum number of requests allowed within one window of the configured `Unit`. When `CurrentRequestCount` reaches this value, `CanProcessRequest` returns `false` until the window resets.

#### `public Enums.RateLimitUnit Unit`
The time granularity over which `RequestsPerUnit` is measured. Interpretation is provided by `GetWindowInSeconds`.

#### `public bool IsEnabled`
Indicates whether this rate limit is actively enforced. When `false`, `CanProcessRequest` should always return `true` regardless of the current count.

#### `public DateTime CreatedAt`
Timestamp of when this rate limit record was first persisted.

#### `public DateTime? LastResetAt`
Timestamp of the most recent window reset, or `null` if the window has never been explicitly reset or no requests have been recorded. Set by `ResetWindow` and may be set implicitly by `RecordRequest` when a stale window is detected.

#### `public int CurrentRequestCount`
The number of requests recorded within the current window. Incremented by `RecordRequest` and set to zero by `ResetWindow`.

### Methods

#### `public int GetWindowInSeconds()`
Returns the duration of the rate limit window in seconds based on the value of `Unit`. The mapping from `Enums.RateLimitUnit` to seconds is deterministic (e.g., `Minute` → 60, `Hour` → 3600).

- **Returns**: `int` — positive number of seconds representing one full window.

#### `public bool CanProcessRequest()`
Evaluates whether a request can be accepted under the current rate limit state. Returns `true` if `IsEnabled` is `false`, or if the window has elapsed since `LastResetAt` (based on `GetWindowInSeconds()`), or if `CurrentRequestCount` is less than `RequestsPerUnit`. Returns `false` when the limit is enabled, the window is still active, and the count has reached the ceiling.

- **Returns**: `bool` — `true` if the request should be allowed; `false` if it should be throttled.

#### `public void RecordRequest()`
Records a single request against this rate limit. If the current window has expired (determined by comparing `LastResetAt` plus `GetWindowInSeconds()` to the current time), the window is implicitly reset before incrementing. Otherwise, `CurrentRequestCount` is incremented by one. Callers should typically invoke `CanProcessRequest` before calling this method to avoid exceeding the limit.

#### `public void ResetWindow()`
Explicitly resets the current window by setting `CurrentRequestCount` to zero and updating `LastResetAt` to the current time. This discards all request history for the active window and begins a fresh period.

## Usage

### Example 1: Checking and recording a request

```csharp
RateLimit limit = rateLimitStore.GetLimit(apiKeyId);

if (limit.IsEnabled && !limit.CanProcessRequest())
{
    throw new RateLimitExceededException(
        $"Limit of {limit.RequestsPerUnit} requests per " +
        $"{limit.GetWindowInSeconds()} seconds exceeded.");
}

limit.RecordRequest();
rateLimitStore.Save(limit);
```

### Example 2: Administrative reset of a limit window

```csharp
RateLimit limit = rateLimitStore.GetLimit(apiKeyId);

// Force-reset the window, e.g. after a plan upgrade or manual intervention.
limit.ResetWindow();
rateLimitStore.Save(limit);

Console.WriteLine(
    $"Window reset at {limit.LastResetAt:O}. " +
    $"Current count is now {limit.CurrentRequestCount}.");
```

## Notes

- **Window boundary detection**: `CanProcessRequest` and `RecordRequest` both evaluate whether the window has expired based on `LastResetAt` and `GetWindowInSeconds()`. If `LastResetAt` is `null`, the window is treated as expired, meaning the first call to `RecordRequest` will implicitly reset it.
- **Implicit reset in `RecordRequest`**: When `RecordRequest` detects an expired window, it resets the count to zero and then increments to one. This means a burst that exactly fills a window and then stalls will automatically begin a new window on the next request without requiring an explicit `ResetWindow` call.
- **Disabled limits**: When `IsEnabled` is `false`, `CanProcessRequest` returns `true` unconditionally. `RecordRequest` still increments the counter, which may be useful for passive monitoring even when enforcement is off.
- **Thread safety**: This type does not provide internal synchronisation. In multi-threaded environments (e.g., concurrent HTTP requests hitting the same API key), external locking or an atomic persistence layer is required to avoid race conditions between `CanProcessRequest` and `RecordRequest`, or between multiple concurrent calls to `RecordRequest`.
- **Persistence assumptions**: `LastResetAt` and `CurrentRequestCount` are mutable fields expected to be persisted and reloaded. Stale in-memory instances that have not been refreshed from storage may make throttling decisions based on outdated counts.
