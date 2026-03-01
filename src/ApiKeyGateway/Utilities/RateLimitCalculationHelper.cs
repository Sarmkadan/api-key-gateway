// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Enums;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Helper for rate limit calculations and window management.
/// Encapsulates the logic for determining if requests are within quota
/// and calculating reset times. This is separated from business logic
/// to allow easy testing and reuse across different components.
/// </summary>
public static class RateLimitCalculationHelper
{
    /// <summary>
    /// Calculates the window end time based on the limit unit.
    /// For example, if current time is 14:23:45 and unit is HOUR,
    /// the window ends at 15:00:00.
    /// </summary>
    public static DateTime GetWindowEnd(DateTime currentTime, RateLimitUnit unit)
    {
        return unit switch
        {
            RateLimitUnit.SECOND => currentTime.AddSeconds(1),
            RateLimitUnit.MINUTE => currentTime.AddMinutes(1).AddSeconds(-currentTime.Second).AddMilliseconds(-currentTime.Millisecond),
            RateLimitUnit.HOUR => currentTime.AddHours(1).AddMinutes(-currentTime.Minute).AddSeconds(-currentTime.Second),
            RateLimitUnit.DAY => currentTime.AddDays(1).Date,
            RateLimitUnit.MONTH => currentTime.AddMonths(1).AddDays(-currentTime.Day + 1).Date,
            _ => throw new ArgumentException($"Unknown rate limit unit: {unit}")
        };
    }

    /// <summary>
    /// Calculates the window start time based on the limit unit.
    /// Inverse of GetWindowEnd - determines when the current window started.
    /// </summary>
    public static DateTime GetWindowStart(DateTime currentTime, RateLimitUnit unit)
    {
        return unit switch
        {
            RateLimitUnit.SECOND => currentTime.AddSeconds(-1),
            RateLimitUnit.MINUTE => new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0),
            RateLimitUnit.HOUR => new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 0, 0),
            RateLimitUnit.DAY => currentTime.Date,
            RateLimitUnit.MONTH => new DateTime(currentTime.Year, currentTime.Month, 1),
            _ => throw new ArgumentException($"Unknown rate limit unit: {unit}")
        };
    }

    /// <summary>
    /// Determines if a request is allowed based on current usage and limit.
    /// Returns the number of seconds until the next request can be made,
    /// or 0 if the request is allowed immediately.
    /// </summary>
    public static int GetSecondsUntilAllowed(int currentUsage, int limit, DateTime windowStart, RateLimitUnit unit)
    {
        // If under limit, request is allowed immediately
        if (currentUsage < limit)
            return 0;

        // Calculate when the window resets
        var windowEnd = GetWindowEnd(windowStart, unit);
        var now = DateTime.UtcNow;
        var secondsUntilReset = (int)Math.Ceiling((windowEnd - now).TotalSeconds);

        return Math.Max(0, secondsUntilReset);
    }

    /// <summary>
    /// Calculates the percentage of quota used in the current window.
    /// Useful for warning users when approaching limits.
    /// </summary>
    public static int CalculateQuotagePercentage(int currentUsage, int limit)
    {
        if (limit <= 0)
            return 0;

        var percentage = (currentUsage * 100) / limit;
        return Math.Min(100, percentage);
    }

    /// <summary>
    /// Determines if we should warn the user about approaching their limit.
    /// Warning triggers at 80%, 90%, and 100%.
    /// </summary>
    public static bool ShouldWarnAboutLimit(int percentage) =>
        percentage >= 80;

    /// <summary>
    /// Gets human-readable time until reset for logging/response purposes.
    /// </summary>
    public static string GetReadableResetTime(DateTime windowEnd, DateTime? now = null)
    {
        now ??= DateTime.UtcNow;
        var timespan = windowEnd - now;

        return timespan.TotalSeconds < 0
            ? "immediately"
            : timespan.TotalHours >= 1
                ? $"{Math.Ceiling(timespan.TotalHours)} hours"
                : timespan.TotalMinutes >= 1
                    ? $"{Math.Ceiling(timespan.TotalMinutes)} minutes"
                    : $"{Math.Max(1, (int)Math.Ceiling(timespan.TotalSeconds))} seconds";
    }
}
