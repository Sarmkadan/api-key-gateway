using System;
using ApiKeyGateway.Domain.Models;

public static class UsageQuotaExtensions
{
    /// <summary>
    /// Returns the number of remaining requests for the current period.
    /// </summary>
    /// <param name="quota">The usage quota to calculate remaining requests for.</param>
    /// <returns>The number of remaining requests allowed in the current period.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="quota"/> is null.</exception>
    public static long GetRemainingRequests(this UsageQuota quota)
    {
        ArgumentNullException.ThrowIfNull(quota);
        return quota.RemainingRequests;
    }

    /// <summary>
    /// Returns the percentage of remaining requests for the current period.
    /// </summary>
    /// <param name="quota">The usage quota to calculate percentage for.</param>
    /// <returns>The percentage (0-100) of remaining requests in the current period.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="quota"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="UsageQuota.QuotaLimit"/> is zero.</exception>
    public static double GetRemainingRequestsPercentage(this UsageQuota quota)
    {
        ArgumentNullException.ThrowIfNull(quota);

        if (quota.QuotaLimit == 0)
        {
            throw new InvalidOperationException(
                "Cannot calculate percentage when QuotaLimit is zero. The quota is either disabled or not properly configured.");
        }

        return (double)quota.RemainingRequests / quota.QuotaLimit * 100;
    }

    /// <summary>
    /// Resets the counter to zero and advances <see cref="UsageQuota.PeriodStartAt"/>
    /// to the start of the current period.
    /// </summary>
    /// <param name="quota">The usage quota to reset.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="quota"/> is null.</exception>
    public static void ResetPeriodToCurrent(this UsageQuota quota)
    {
        ArgumentNullException.ThrowIfNull(quota);
        quota.ResetPeriod(DateTime.UtcNow);
    }

    /// <summary>
    /// Returns a string representation of the usage quota, including the quota limit,
    /// remaining requests, and percentage of usage.
    /// </summary>
    /// <param name="quota">The usage quota to format as a string.</param>
    /// <returns>A formatted string representing the quota status.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="quota"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="UsageQuota.QuotaLimit"/> is zero.</exception>
    public static string ToUsageQuotaString(this UsageQuota quota)
    {
        ArgumentNullException.ThrowIfNull(quota);

        if (quota.QuotaLimit == 0)
        {
            throw new InvalidOperationException(
                "Cannot format quota string when QuotaLimit is zero. The quota is either disabled or not properly configured.");
        }

        return string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            "Quota: {0} requests, Remaining: {1}, Percentage: {2:F2}%",
            quota.QuotaLimit,
            quota.RemainingRequests,
            quota.GetRemainingRequestsPercentage());
    }
}
