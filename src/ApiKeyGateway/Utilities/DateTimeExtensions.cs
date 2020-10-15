// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Extension methods for DateTime operations
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Gets the start of the current day (00:00:00)
    /// </summary>
    public static DateTime StartOfDay(this DateTime date)
    {
        return date.Date;
    }

    /// <summary>
    /// Gets the end of the current day (23:59:59)
    /// </summary>
    public static DateTime EndOfDay(this DateTime date)
    {
        return date.Date.AddDays(1).AddSeconds(-1);
    }

    /// <summary>
    /// Gets the start of the week (Monday)
    /// </summary>
    public static DateTime StartOfWeek(this DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        var diff = date.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;
        return date.AddDays(-diff).Date;
    }

    /// <summary>
    /// Gets the start of the month
    /// </summary>
    public static DateTime StartOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month
    /// </summary>
    public static DateTime EndOfMonth(this DateTime date)
    {
        return date.StartOfMonth().AddMonths(1).AddSeconds(-1);
    }

    /// <summary>
    /// Checks if a date is in the past
    /// </summary>
    public static bool IsInPast(this DateTime date)
    {
        return date < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a date is in the future
    /// </summary>
    public static bool IsInFuture(this DateTime date)
    {
        return date > DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the number of days until a date
    /// </summary>
    public static int DaysUntil(this DateTime date)
    {
        return (int)(date.Date - DateTime.UtcNow.Date).TotalDays;
    }

    /// <summary>
    /// Gets a human-readable time difference
    /// </summary>
    public static string ToHumanReadableTime(this DateTime date)
    {
        var timeSpan = DateTime.UtcNow - date;

        return timeSpan.TotalSeconds < 60 ? "just now" :
               timeSpan.TotalMinutes < 60 ? $"{(int)timeSpan.TotalMinutes}m ago" :
               timeSpan.TotalHours < 24 ? $"{(int)timeSpan.TotalHours}h ago" :
               timeSpan.TotalDays < 30 ? $"{(int)timeSpan.TotalDays}d ago" :
               $"{(int)(timeSpan.TotalDays / 30)}mo ago";
    }
}
