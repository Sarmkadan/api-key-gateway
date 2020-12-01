# RateLimitBenchmarks

The `RateLimitBenchmarks` class provides utility methods for computing rate‑limit window boundaries and quota status. It is designed to support performance measurement and validation of rate‑limiting logic in the `api-key-gateway` project. Each method operates on the current time or on an internal state representing a rate‑limit context, returning values that can be used to determine when a new window starts, how long a request must wait, or what percentage of the allowed quota has been consumed.

## API

### `GetWindowEnd_Minute()`
- **Purpose**: Returns the end time of the current one‑minute rate‑limit window.
- **Parameters**: None.
- **Return value**: `DateTime` – the exact end of the minute window (typically the start of the next minute).
- **Throws**: Not documented; no exceptions are expected under normal operation.

### `GetWindowEnd_Hour()`
- **Purpose**: Returns the end time of the current one‑hour rate‑limit window.
- **Parameters**: None.
- **Return value**: `DateTime` – the exact end of the hour window (typically the start of the next hour).
- **Throws**: Not documented; no exceptions are expected under normal operation.

### `GetWindowStart_Minute()`
- **Purpose**: Returns the start time of the current one‑minute rate‑limit window.
- **Parameters**: None.
- **Return value**: `DateTime` – the exact start of the minute window (typically the beginning of the current minute).
- **Throws**: Not documented; no exceptions are expected under normal operation.

### `GetSecondsUntilAllowed_Limited()`
- **Purpose**: Returns the number of seconds that must elapse before the next request is allowed under the current rate‑limit policy.
- **Parameters**: None.
- **Return value**: `int` – a non‑negative number of seconds. A value of `0` indicates that a request is currently allowed.
- **Throws**: Not documented; no exceptions are expected under normal operation.

### `CalculateQuotagePercentage()`
- **Purpose**: Returns the percentage of the allowed quota that has been consumed in the current window.
- **Parameters**: None.
- **Return value**: `int` – an integer between `0` and `100` (inclusive) representing the percentage of quota used.
- **Throws**: Not documented; no exceptions are expected under normal operation.

## Usage

The following examples demonstrate typical usage of `RateLimitBenchmarks` in a rate‑limiting context.

```csharp
// Example 1: Checking window boundaries and wait time
var benchmarks = new RateLimitBenchmarks();

DateTime minuteStart = benchmarks.GetWindowStart_Minute();
DateTime minuteEnd   = benchmarks.GetWindowEnd_Minute();
DateTime hourEnd     = benchmarks.GetWindowEnd_Hour();

int secondsUntilAllowed = benchmarks.GetSecondsUntilAllowed_Limited();

Console.WriteLine($"Minute window: {minuteStart} – {minuteEnd}");
Console.WriteLine($"Hour window ends at: {hourEnd}");
Console.WriteLine($"Seconds until allowed: {secondsUntilAllowed}");
```

```csharp
// Example 2: Monitoring quota consumption
var benchmarks = new RateLimitBenchmarks();

int quotaUsed = benchmarks.CalculateQuotagePercentage();
if (quotaUsed >= 90)
{
    Console.WriteLine($"Warning: {quotaUsed}% of quota consumed. Consider throttling.");
}
else
{
    Console.WriteLine($"Quota usage: {quotaUsed}%");
}
```

## Notes

- **Edge cases**:  
  - `GetSecondsUntilAllowed_Limited` returns `0` when no delay is required.  
  - `CalculateQuotagePercentage` returns `0` when no quota has been consumed and `100` when the quota is exhausted.  
  - Window boundaries are computed relative to the system clock at the time of the call; if the system clock is adjusted, results may be inconsistent.

- **Thread safety**:  
  The `RateLimitBenchmarks` class is **not thread‑safe**. Its methods may read or write internal state that is not synchronized. If the same instance is accessed concurrently from multiple threads, the returned values may be incorrect or inconsistent. For concurrent scenarios, either use a separate instance per thread or synchronize access externally.
