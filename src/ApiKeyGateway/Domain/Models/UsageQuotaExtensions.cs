using System;
using System.Globalization;
using ApiKeyGateway.Domain.Models;

public static class UsageQuotaExtensions
{
    /// <summary>
    /// Returns the number of remaining requests for the current period.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="quota"/> is null.</exception>
    public static long GetRemainingRequests(this UsageQuota quota)
    {
        ArgumentNullException.ThrowIfNull(quota);
        return quota.RemainingRequests;
    }

    /// <summary>
    /// Returns the percentage of remaining requests for the current period.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="quota"/> is null.</exception>
    public static double GetRemainingRequestsPercentage(this UsageQuota quota)
    {
        ArgumentNullException.ThrowIfNull(quota);
        return (double)quota.RemainingRequests / quota.QuotaLimit * 100;
    }

    /// <summary>
    /// Resets the counter to zero and advances <see cref="PeriodStartAt"/>
    /// to the start of the current period.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="quota"/> is null.</exception>
    public static void ResetPeriodToCurrent(this UsageQuota quota)
    {
        ArgumentNullException.ThrowIfNull(quota);
        quota.ResetPeriod(DateTime.UtcNow);
    }

    /// <summary>
    /// Returns a string representation of the usage quota, including the remaining requests and percentage.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="quota"/> is null.</exception>
    public static string ToUsageQuotaString(this UsageQuota quota)
    {
        ArgumentNullException.ThrowIfNull(quota);
        return $"Quota: {quota.QuotaLimit} requests, Remaining: {quota.RemainingRequests}, Percentage: {quota.GetRemainingRequestsPercentage():F2}%";
    }
}
