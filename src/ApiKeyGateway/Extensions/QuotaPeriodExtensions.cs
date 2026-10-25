// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Enums;

namespace ApiKeyGateway.Extensions;

/// <summary>
/// Extension methods for <see cref="QuotaPeriod"/>
/// </summary>
public static class QuotaPeriodExtensions
{
    private const int DaysPerWeek = 7;
    private const int DaysPerMonth = 30;

    /// <summary>
    /// Gets the approximate length of the quota period as a <see cref="TimeSpan"/>.
    /// Monthly periods are approximated as 30 days.
    /// </summary>
    /// <param name="period">The quota period</param>
    /// <returns>The length of one period</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="period"/> is not a defined value</exception>
    public static TimeSpan ToTimeSpan(this QuotaPeriod period) => period switch
    {
        QuotaPeriod.Hour => TimeSpan.FromHours(1),
        QuotaPeriod.Daily or QuotaPeriod.Day => TimeSpan.FromDays(1),
        QuotaPeriod.Week => TimeSpan.FromDays(DaysPerWeek),
        QuotaPeriod.Monthly or QuotaPeriod.Month => TimeSpan.FromDays(DaysPerMonth),
        _ => throw new ArgumentOutOfRangeException(nameof(period), period, "Undefined QuotaPeriod value.")
    };

    /// <summary>
    /// Gets a human-readable name of the quota period, suitable for display in dashboards and alerts
    /// </summary>
    /// <param name="period">The quota period</param>
    /// <returns>The display string for the period</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="period"/> is not a defined value</exception>
    public static string ToDisplayString(this QuotaPeriod period) => period switch
    {
        QuotaPeriod.Hour => "Hourly",
        QuotaPeriod.Daily or QuotaPeriod.Day => "Daily",
        QuotaPeriod.Week => "Weekly",
        QuotaPeriod.Monthly or QuotaPeriod.Month => "Monthly",
        _ => throw new ArgumentOutOfRangeException(nameof(period), period, "Undefined QuotaPeriod value.")
    };
}
