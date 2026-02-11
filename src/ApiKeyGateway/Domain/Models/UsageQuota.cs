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
    public string Id { get; init; } = string.Empty;
    public string ApiKeyId { get; init; } = string.Empty;

    /// <summary>Maximum number of requests allowed per period</summary>
    public long QuotaLimit { get; init; }

    /// <summary>Whether the quota is enforced; when false requests always pass</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Calendar period on which the counter resets</summary>
    public Enums.QuotaPeriod Period { get; init; } = Enums.QuotaPeriod.Daily;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Start of the current period (midnight UTC of day/month)</summary>
    public DateTime PeriodStartAt { get; set; } = DateTime.UtcNow;

    /// <summary>Number of requests counted in the current period</summary>
    public long CurrentUsage { get; set; }

    public long RemainingRequests => Math.Max(0, QuotaLimit - CurrentUsage);
    public bool IsExceeded => IsEnabled && CurrentUsage >= QuotaLimit;

    /// <summary>
    /// Returns the UTC instant at which the current period ends
    /// (i.e. the start of the next period).
    /// </summary>
    public DateTime GetPeriodEndUtc() => Period switch
    {
        Enums.QuotaPeriod.Monthly => new DateTime(
            PeriodStartAt.Year,
            PeriodStartAt.Month,
            1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1),
        _ => PeriodStartAt.Date.AddDays(1)
    };

    /// <summary>
    /// Returns the UTC start of the period that contains <paramref name="utcNow"/>.
    /// </summary>
    public static DateTime GetPeriodStart(DateTime utcNow, Enums.QuotaPeriod period) => period switch
    {
        Enums.QuotaPeriod.Monthly => new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
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
            CurrentUsage++;
    }
}
