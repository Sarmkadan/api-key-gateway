# DateTimeExtensions

Provides a collection of extension methods for `DateTime` values, offering convenient calculations for day/week/month boundaries, temporal comparisons, countdowns, and human-readable formatting. These utilities standardise common date-time operations across the API key gateway codebase, reducing repetitive boilerplate and ensuring consistent handling of time-related logic.

## API

### StartOfDay

```csharp
public static DateTime StartOfDay(this DateTime dateTime)
```

Returns a new `DateTime` representing the start of the same day (midnight, `00:00:00`) with the `DateTimeKind` preserved from the original value. The time component is set to zero while the date portion remains unchanged.

**Parameters:**
- `dateTime` — The `DateTime` value to truncate.

**Returns:**
- A `DateTime` set to `00:00:00.0000000` on the same calendar date.

**Throws:**
- `ArgumentOutOfRangeException` — When the resulting value falls outside the valid `DateTime` range (only possible for extreme boundary values near `MinValue` or `MaxValue`).

---

### EndOfDay

```csharp
public static DateTime EndOfDay(this DateTime dateTime)
```

Returns a new `DateTime` representing the last representable moment of the same day (`23:59:59.9999999`). The `DateTimeKind` is preserved from the original value.

**Parameters:**
- `dateTime` — The `DateTime` value to extend to end-of-day.

**Returns:**
- A `DateTime` set to `23:59:59.9999999` on the same calendar date.

**Throws:**
- `ArgumentOutOfRangeException` — When the resulting value falls outside the valid `DateTime` range (only possible for extreme boundary values near `MinValue` or `MaxValue`).

---

### StartOfWeek

```csharp
public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Monday)
```

Returns a new `DateTime` representing the start of the week containing the given date. The week start day defaults to Monday but can be overridden via the optional parameter. The time component is set to `00:00:00` and `DateTimeKind` is preserved.

**Parameters:**
- `dateTime` — The `DateTime` value whose containing week is calculated.
- `startOfWeek` — The `DayOfWeek` that defines the first day of the week. Defaults to `DayOfWeek.Monday`.

**Returns:**
- A `DateTime` at midnight on the first day of the containing week.

**Throws:**
- `ArgumentOutOfRangeException` — When the resulting value falls outside the valid `DateTime` range.

---

### StartOfMonth

```csharp
public static DateTime StartOfMonth(this DateTime dateTime)
```

Returns a new `DateTime` representing the first day of the month at midnight (`00:00:00`). The `DateTimeKind` is preserved from the original value.

**Parameters:**
- `dateTime` — The `DateTime` value whose month start is calculated.

**Returns:**
- A `DateTime` set to the 1st of the month at `00:00:00`.

**Throws:**
- `ArgumentOutOfRangeException` — When the resulting value falls outside the valid `DateTime` range.

---

### EndOfMonth

```csharp
public static DateTime EndOfMonth(this DateTime dateTime)
```

Returns a new `DateTime` representing the last day of the month at the last representable moment (`23:59:59.9999999`). The method correctly accounts for varying month lengths and leap years. `DateTimeKind` is preserved.

**Parameters:**
- `dateTime` — The `DateTime` value whose month end is calculated.

**Returns:**
- A `DateTime` set to the final tick of the final day of the month.

**Throws:**
- `ArgumentOutOfRangeException` — When the resulting value falls outside the valid `DateTime` range.

---

### IsInPast

```csharp
public static bool IsInPast(this DateTime dateTime)
```

Determines whether the given `DateTime` occurs strictly before the current UTC time. The comparison is performed against `DateTime.UtcNow`, making it timezone-agnostic and suitable for server-side validation.

**Parameters:**
- `dateTime` — The `DateTime` value to evaluate.

**Returns:**
- `true` if `dateTime` is earlier than the current UTC time; otherwise `false`.

**Throws:**
- Does not throw under normal conditions. If `dateTime.Kind` is `Local`, the value is implicitly converted for comparison; this may produce unexpected results if the original local time is ambiguous.

---

### IsInFuture

```csharp
public static bool IsInFuture(this DateTime dateTime)
```

Determines whether the given `DateTime` occurs strictly after the current UTC time. Comparison is performed against `DateTime.UtcNow`.

**Parameters:**
- `dateTime` — The `DateTime` value to evaluate.

**Returns:**
- `true` if `dateTime` is later than the current UTC time; otherwise `false`.

**Throws:**
- Does not throw under normal conditions. The same `DateTimeKind` caveat as `IsInPast` applies.

---

### DaysUntil

```csharp
public static int DaysUntil(this DateTime from, DateTime to)
```

Calculates the number of whole calendar days between two `DateTime` values. The result is the floor of the absolute difference in days, ignoring the time components of both values. The order of the arguments does not affect the magnitude.

**Parameters:**
- `from` — The first `DateTime` value.
- `to` — The second `DateTime` value.

**Returns:**
- A non-negative `int` representing the number of whole days between the two dates.

**Throws:**
- `ArgumentOutOfRangeException` — When the absolute difference in days exceeds `Int32.MaxValue`.

---

### ToHumanReadableTime

```csharp
public static string ToHumanReadableTime(this DateTime dateTime)
```

Converts a `DateTime` value into a human-friendly string describing how long ago or how far in the future it is relative to the current UTC time. The output uses the largest appropriate unit (e.g., "3 days ago", "in 2 hours", "just now") and is intended for display purposes rather than precision.

**Parameters:**
- `dateTime` — The `DateTime` value to describe.

**Returns:**
- A `string` such as `"5 minutes ago"`, `"in 2 days"`, or `"just now"` for very small differences.

**Throws:**
- Does not throw. Values extremely far in the past or future produce strings with large unit counts (e.g., "1825 days ago").

## Usage

### Example 1: Validating API key expiration windows

```csharp
public bool IsApiKeyValid(ApiKey key)
{
    var now = DateTime.UtcNow;

    // Key must not be expired and must not be valid only in the future
    if (key.ExpiresAt.IsInPast() || key.NotBefore.IsInFuture())
    {
        return false;
    }

    // Key is within its validity window
    return true;
}
```

### Example 2: Generating usage report date ranges

```csharp
public UsageReport GenerateMonthlyReport(DateTime referenceDate)
{
    var monthStart = referenceDate.StartOfMonth();
    var monthEnd   = referenceDate.EndOfMonth();

    var daysRemaining = monthStart.DaysUntil(monthEnd);

    var report = new UsageReport
    {
        PeriodStart = monthStart,
        PeriodEnd   = monthEnd,
        Title       = $"Report for {monthStart:MMMM yyyy}",
        Summary     = $"Period spans {daysRemaining} days. Generated {DateTime.UtcNow.ToHumanReadableTime()}."
    };

    return report;
}
```

## Notes

- **`DateTimeKind` preservation:** Methods that produce a new `DateTime` (`StartOfDay`, `EndOfDay`, `StartOfWeek`, `StartOfMonth`, `EndOfMonth`) retain the `Kind` of the input value. Callers should ensure the input `Kind` is correct for their context; mixing `Local` and `Utc` values can lead to ambiguous boundary calculations around daylight saving transitions.
- **`IsInPast` / `IsInFuture` comparison basis:** Both methods compare against `DateTime.UtcNow`. Passing a `Local` `DateTime` will implicitly convert it to UTC for the comparison. If the local time is ambiguous (e.g., during a DST fall-back transition), the result may not reflect the intended wall-clock time.
- **`DaysUntil` magnitude:** The method returns the absolute difference in whole days. Negative intervals are treated identically to positive ones; the return value is always non-negative. Time components are stripped before calculation, so two timestamps on the same calendar date return `0`.
- **`ToHumanReadableTime` precision:** This method is designed for human-facing display, not for exact measurement. Thresholds between units (seconds, minutes, hours, days, months, years) are approximate and may change across versions. Do not parse its output for programmatic decisions.
- **Thread safety:** All methods are pure static extension methods operating on immutable `DateTime` structs. They hold no mutable state and are safe to call concurrently from multiple threads without synchronisation.
- **Edge cases with `MinValue` and `MaxValue`:** Boundary methods applied to `DateTime.MinValue` or `DateTime.MaxValue` may produce results that exceed the representable range, throwing `ArgumentOutOfRangeException`. Callers dealing with unbounded or sentinel date-time values should guard against these extremes.
