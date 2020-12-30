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
    /// <param name="date">The date to get the start of day for</param>
    /// <returns>The start of the day (midnight)</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="date"/> is null (though DateTime is a value type)</exception>
    public static DateTime StartOfDay(this DateTime date)
        => date.Date;

    /// <summary>
    /// Gets the end of the current day (23:59:59.999)
    /// </summary>
    /// <param name="date">The date to get the end of day for</param>
    /// <returns>The end of the day (one second before midnight of the next day)</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="date"/> is null (though DateTime is a value type)</exception>
    public static DateTime EndOfDay(this DateTime date)
        => date.Date.AddDays(1).AddTicks(-1);

    /// <summary>
    /// Gets the start of the week (Monday)
    /// </summary>
    /// <param name="date">The date to get the start of week for</param>
    /// <returns>The start of the week (Monday at midnight)</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="date"/> is null (though DateTime is a value type)</exception>
    public static DateTime StartOfWeek(this DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        var diff = date.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;
        return date.AddDays(-diff).Date;
    }

    /// <summary>
    /// Gets the start of the month
    /// </summary>
    /// <param name="date">The date to get the start of month for</param>
    /// <returns>The first day of the month at midnight</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="date"/> is null (though DateTime is a value type)</exception>
    public static DateTime StartOfMonth(this DateTime date)
        => new DateTime(date.Year, date.Month, 1);

    /// <summary>
    /// Gets the end of the month
    /// </summary>
    /// <param name="date">The date to get the end of month for</param>
    /// <returns>The last moment of the last day of the month</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="date"/> is null (though DateTime is a value type)</exception>
    public static DateTime EndOfMonth(this DateTime date)
        => date.StartOfMonth().AddMonths(1).AddTicks(-1);

    /// <summary>
    /// Checks if a date is in the past
    /// </summary>
    /// <param name="date">The date to check</param>
    /// <returns>True if the date is before the current UTC time; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="date"/> is null (though DateTime is a value type)</exception>
    public static bool IsInPast(this DateTime date)
        => date < DateTime.UtcNow;

    /// <summary>
    /// Checks if a date is in the future
    /// </summary>
    /// <param name="date">The date to check</param>
    /// <returns>True if the date is after the current UTC time; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="date"/> is null (though DateTime is a value type)</exception>
    public static bool IsInFuture(this DateTime date)
        => date > DateTime.UtcNow;

    /// <summary>
    /// Gets the number of days until a date
    /// </summary>
    /// <param name="date">The target date</param>
    /// <returns>The number of full days until the target date (positive for future dates, negative for past dates)</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="date"/> is null (though DateTime is a value type)</exception>
    public static int DaysUntil(this DateTime date)
        => (int)(date.Date - DateTime.UtcNow.Date).TotalDays;

    /// <summary>
    /// Gets a human-readable time difference between the provided date and now
    /// </summary>
    /// <param name="date">The date to format</param>
    /// <returns>A human-readable string representing the time difference</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="date"/> is null (though DateTime is a value type)</exception>
    public static string ToHumanReadableTime(this DateTime date)
    {
        ArgumentNullException.ThrowIfNull(date);

        var timeSpan = DateTime.UtcNow - date;
        var totalSeconds = timeSpan.TotalSeconds;

        return totalSeconds switch
        {
            < 60 => "just now",
            < TimeSpan.TicksPerMinute * 60 => $"{(int)timeSpan.TotalMinutes}m ago",
            < TimeSpan.TicksPerHour * 24 => $"{(int)timeSpan.TotalHours}h ago",
            < TimeSpan.TicksPerDay * 30 => $"{(int)timeSpan.TotalDays}d ago",
            _ => $"{(int)(timeSpan.TotalDays / 30)}mo ago"
        };
    }
}
