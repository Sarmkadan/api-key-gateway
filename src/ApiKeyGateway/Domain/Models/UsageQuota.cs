// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Defines a hard usage quota for an API key that resets on a calendar basis
/// (daily or monthly). Unlike rate limits, quotas enforce a total request cap
/// over a billing-style period rather than a rolling window.
/// </summary>
public class UsageQuota
{
    /// <summary>Unique identifier of the quota</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>ID of the API key this quota applies to</summary>
    public string ApiKeyId { get; init; } = string.Empty;

    /// <summary>Maximum number of requests allowed per period</summary>
    public long QuotaLimit { get; init; }

    /// <summary>Whether the quota is enforced; when false requests always pass</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Calendar period on which the counter resets</summary>
    public Enums.QuotaPeriod Period { get; init; } = Enums.QuotaPeriod.Daily;

    /// <summary>When the quota was created (UTC)</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Start of the current period (midnight UTC of day/month)</summary>
    public DateTime PeriodStartAt { get; set; } = DateTime.UtcNow;

    /// <summary>Number of requests counted in the current period</summary>
    public long CurrentUsage { get; set; }

    /// <summary>Number of requests still allowed in the current period</summary>
    public long RemainingRequests => Math.Max(0, QuotaLimit - CurrentUsage);

    /// <summary>Whether the quota is enabled and the usage has reached the limit</summary>
    public bool IsExceeded => IsEnabled && CurrentUsage >= QuotaLimit;

    /// <summary>
    /// Returns the UTC instant at which the current period ends
    /// (i.e. the start of the next period).
    /// </summary>
    public DateTime GetPeriodEndUtc() => Period switch
    {
        Enums.QuotaPeriod.Monthly or Enums.QuotaPeriod.Month => new DateTime(
            PeriodStartAt.Year,
            PeriodStartAt.Month,
            1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1),
        Enums.QuotaPeriod.Hour => PeriodStartAt.AddHours(1),
        Enums.QuotaPeriod.Week => PeriodStartAt.Date.AddDays(7),
        _ => PeriodStartAt.Date.AddDays(1)
    };

    /// <summary>
    /// Returns the UTC start of the period that contains <paramref name="utcNow"/>.
    /// </summary>
    public static DateTime GetPeriodStart(DateTime utcNow, Enums.QuotaPeriod period) => period switch
    {
        Enums.QuotaPeriod.Monthly or Enums.QuotaPeriod.Month => new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
        Enums.QuotaPeriod.Hour => new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0, DateTimeKind.Utc),
        Enums.QuotaPeriod.Week => utcNow.Date.AddDays(-(int)utcNow.DayOfWeek).ToUniversalTime(),
        _ => utcNow.Date.ToUniversalTime()
    };

    /// <summary>
    /// Resets the counter to zero and advances <see cref="PeriodStartAt"/>
    /// to the start of <paramref name="utcNow"/>'s period.
    /// </summary>
    public void ResetPeriod(DateTime utcNow)
    {
        PeriodStartAt = GetPeriodStart(utcNow, Period);
        CurrentUsage = 0;
    }

    /// <summary>Increments the usage counter by one</summary>
    public void RecordRequest()
    {
        if (IsEnabled)
        {
            CurrentUsage++;
        }
    }

    /// <summary>
    /// Returns a short text representation of the quota state.
    /// </summary>
    /// <returns>A string describing whether the quota is enabled, its period start, and current usage.</returns>
    public override string ToString() => $"UsageQuota {{ IsEnabled = {IsEnabled}, PeriodStartAt = {PeriodStartAt}, CurrentUsage = {CurrentUsage} }}";
}
