// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

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
            RateLimitUnit.Second => currentTime.AddSeconds(1),
            RateLimitUnit.Minute => currentTime.AddMinutes(1).AddSeconds(-currentTime.Second).AddMilliseconds(-currentTime.Millisecond),
            RateLimitUnit.Hour => currentTime.AddHours(1).AddMinutes(-currentTime.Minute).AddSeconds(-currentTime.Second),
            RateLimitUnit.Day => currentTime.AddDays(1).Date,
            RateLimitUnit.Month => currentTime.AddMonths(1).AddDays(-currentTime.Day + 1).Date,
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
            RateLimitUnit.Second => currentTime.AddSeconds(-1),
            RateLimitUnit.Minute => new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0),
            RateLimitUnit.Hour => new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 0, 0),
            RateLimitUnit.Day => currentTime.Date,
            RateLimitUnit.Month => new DateTime(currentTime.Year, currentTime.Month, 1),
            _ => throw new ArgumentException($"Unknown rate limit unit: {unit}")
        };
    }

    /// <summary>
    /// Determines if a request is allowed based on current usage and limit.
    /// Returns the number of seconds until the next request can be made,
    /// or 0 if the request is allowed immediately.
    /// When the request would exceed the limit (currentUsage >= limit), returns
    /// <see cref="int.MaxValue"/> to indicate the request should be rejected immediately rather than delayed.
    /// </summary>
    /// <param name="currentUsage">Current number of requests in the window.</param>
    /// <param name="limit">Maximum allowed requests per window.</param>
    /// <param name="windowStart">Start time of the current rate limit window.</param>
    /// <param name="unit">Time unit for the rate limit window.</param>
    /// <returns>
    /// 0 if the request is allowed immediately (currentUsage < limit),
    /// <see cref="int.MaxValue"/> if the request exceeds the limit (currentUsage >= limit),
    /// or the number of seconds until the window resets (0 < result < int.MaxValue).
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="currentUsage"/> is negative or <paramref name="limit"/> is not positive.
    /// </exception>
    public static int GetSecondsUntilAllowed(int currentUsage, int limit, DateTime windowStart, RateLimitUnit unit)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(currentUsage);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        // If at or over the limit, reject immediately (consistent with RateLimit.CanProcessRequest())
        if (currentUsage >= limit)
            return int.MaxValue;

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
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="currentUsage"/> is negative or <paramref name="limit"/> is not positive.
    /// </exception>
    public static int CalculateQuotagePercentage(int currentUsage, int limit)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(currentUsage);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        var percentage = (currentUsage * 100) / limit;
        return Math.Min(100, percentage);
    }

    /// <summary>
    /// Determines if we should warn the user about approaching their limit.
    /// Warning triggers at 80%, 90%, and 100%.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="percentage"/> is negative.
    /// </exception>
    public static bool ShouldWarnAboutLimit(int percentage)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(percentage);

        return percentage >= 80;
    }

    /// <summary>
    /// Gets human-readable time until reset for logging/response purposes.
    /// </summary>
    public static string GetReadableResetTime(DateTime windowEnd, DateTime? now = null)
    {
        now ??= DateTime.UtcNow;
        var timespan = windowEnd - now.Value;

        return timespan.TotalSeconds < 0
            ? "immediately"
            : timespan.TotalHours >= 1
                ? $"{Math.Ceiling(timespan.TotalHours)} hours"
                : timespan.TotalMinutes >= 1
                    ? $"{Math.Ceiling(timespan.TotalMinutes)} minutes"
                    : $"{Math.Max(1, (int)Math.Ceiling(timespan.TotalSeconds))} seconds";
    }
}
