# RateLimitCalculationHelper

Static helper class that provides calculations for rate‑limiting windows, quota usage, warning thresholds, and human‑readable reset times. The methods are pure functions; they do not retain state and depend only on the arguments supplied.

## API

### GetWindowStart
**Purpose:** Returns the start (inclusive) of the rate‑limit window that contains a given reference timestamp.  
**Parameters:**  
- `referenceUtc` (`DateTime`) – The point in time to anchor the window (typically `DateTime.UtcNow`).  
- `windowSize` (`TimeSpan`) – Length of the window (e.g., one minute). Must be greater than zero.  
**Return value:** `DateTime` representing the window start in UTC.  
**Throws:** `ArgumentException` if `windowSize` is less than or equal to zero.

### GetWindowEnd
**Purpose:** Returns the end (exclusive) of the rate‑limit window that contains a given reference timestamp.  
**Parameters:**  
- `referenceUtc` (`DateTime`) – Reference time for the window calculation now).  
- `windowSize` (`TimeSpan`) – Length of the window; must be greater than zero.  
**Return value:** `DateTime` representing the window end in UTC.  
**Throws:** `ArgumentException` if `windowSize` is less than or equal to zero.

### GetSecondsUntilAllowed
**Purpose:** Calculates how many seconds must elapse before the next request is permitted under the current quota.  
**Parameters:**  
- `currentUtc` (`DateTime`) – Current time (usually `DateTime.UtcNow`).  
- `windowStart` (`DateTime`) – Start of the active window (from `GetWindowStart`).  
- `windowSize` (`TimeSpan`) – Size of the window.  
- `quota` (`int`) – Maximum number of requests allowed in the window.  
- `used` (`int`) – Number of requests already consumed in the window.  
**Return value:** `int` – Seconds to wait before the next request is allowed; returns `0` if the request can be made immediately.  
**Throws:** `ArgumentException` if any of the following hold: `windowSize` ≤ Zero, `quota` ≤ 0, `used` < 0, or `used` > `quota`.

### CalculateQuotagePercentage
**Purpose:** Computes the percentage of the quota that has been used within the current window.  
**Parameters:**  
- `quota` (`int`) – Total allowed requests in the window.  
- `used` (`int`) – Requests already made in the window.  
**Return value:** `int` – Usage percentage from 0 to 100. If `quota` is zero, returns 0 to avoid division by zero.  
**Throws:** `ArgumentException` if `quota` < 0, `used` < 0, or `used` > `quota`.

### ShouldWarnAboutLimit
**Purpose:** Determines whether a warning should be emitted because usage has reached or exceeded a configurable threshold percentage of the quota.  
**Parameters:**  
- `quota` (`int`) – Total allowed requests in the window.  
- `used` (`int`) – Requests already made in the window.  
- `warningThresholdPercent` (`int`) – Threshold at which a warning is triggered (e.g., 80 for 80 %). Must be between 0 and 100 inclusive.  
**Return value:** `true` if `used` ≥ (`quota` * `warningThresholdPercent` / 100); otherwise `false`.  
**Throws:** `ArgumentException` if `quota` ≤ 0 or `warningThresholdPercent` is outside the range 0‑100.

### GetReadableResetTime
**Purpose:** Produces a human‑readable string describing when the current rate‑limit window will reset.  
**Parameters:**  
- `resetUtc` (`DateTime`) – The absolute UTC time at which the window ends (typically the value from `GetWindowEnd`).  
**Return value:** `string` – A relative description such as “in 3 minutes”, “just now”, or an absolute timestamp if the reset is less than a minute away.  
**Throws:** `ArgumentException` if `resetUtc` equals `DateTime.MinValue`.

## Usage

### Example 1: Allowing or delaying a request based on quota
```csharp
var now = DateTime.UtcNow;
var window = TimeSpan.FromMinutes(1);
var quota = 150;

// Assume a repository that tracks usage per API key.
var usageRepo = new UsageRepository();
int used = usageRepo.GetCurrentUsage(apiKey, now, window);

// Determine window boundaries.
var windowStart = RateLimitCalculationHelper.GetWindowStart(now, window);
var windowEnd   = RateLimitCalculationHelper.GetWindowEnd(now, window);

// How long to wait before the next request is allowed?
int secondsWait = RateLimitCalculationHelper.GetSecondsUntilAllowed(
    now, windowStart, window, quota, used);

if (secondsWait == 0)
{
    // Request can be processed.
    usageRepo.Increment(apiKey);
    // …handle request…
}
else
{
    // Signal the caller to retry after the calculated delay.
    throw new RateLimitExceededException($"Try again in {secondsWait} second(s).");
}
```

### Example 2: Emitting a warning when usage approaches the limit
```csharp
var quota = 1000;
var used  = usageRepo.GetUsage(apiKey);
int percent = RateLimitCalculationHelper.GetQuotagePercentage(quota, used);
bool warn   = RateLimitCalculationHelper.ShouldWarnAboutLimit(quota, used, 80);

if (warn)
{
    logger.Warning(
        $"API key '{apiKey}' has reached {percent}% of its hourly quota ({used}/{quota}).");
}
```

## Notes
- All members are **static** and **stateless**; they rely solely on their input arguments, making them inherently thread‑safe for concurrent invocation.
- Input validation is performed; invalid arguments (negative or zero durations, negative quotas, usage outside `[0, quota]`, or threshold percentages outside `[0,100]`) result in `ArgumentException`.
- The methods expect **UTC** `DateTime` values. Supplying local times or mixing UTC with local can produce incorrect window calculations.
- `GetSecondsUntilAllowed` returns `0` when the current usage is below the quota; it does not incorporate any burst allowance beyond the configured quota.
- When `quota` is zero, `CalculateQuotagePercentage` returns `0` to avoid division by zero, and `ShouldWarnAboutLimit` will throw if `quota` ≤ 0.
- The string returned by `GetReadableResetTime` is intended for display purposes (e.g., UI or logs). For programmatic decisions, prefer using the raw `DateTime` from `GetWindowEnd` or `GetSecondsUntilAllowed`.
