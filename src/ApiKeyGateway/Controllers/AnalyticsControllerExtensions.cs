// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using ApiKeyGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Extension methods for <see cref="AnalyticsController"/> that provide additional analytics functionality
/// by composing existing controller actions.
/// </summary>
public static class AnalyticsControllerExtensions
{
    /// <summary>
    /// Returns a comparison of current vs previous period metrics for an API key.
    /// </summary>
    /// <param name="controller">The analytics controller instance.</param>
    /// <param name="keyId">The API key ID to query.</param>
    /// <param name="period">The period to compare (e.g., "7d", "30d"). Defaults to "30d".</param>
    /// <param name="comparisonPeriod">The period length to compare against (e.g., "7d", "14d"). Defaults to same as period.</param>
    /// <returns>A comparison of metrics between the two periods.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="keyId"/> is null or whitespace.</exception>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<ActionResult<PeriodComparison>> ComparePeriodsAsync(
        this AnalyticsController controller,
        string keyId,
        string period = "30d",
        string comparisonPeriod = null)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyId);

        // Parse period strings like "7d", "30d", "1d"
        if (!TryParsePeriod(period, out var currentDays))
            return controller.BadRequest(new { error = "Invalid period format. Use format like '7d', '30d', '1d'" });

        comparisonPeriod ??= period;
        if (!TryParsePeriod(comparisonPeriod, out var comparisonDays))
            return controller.BadRequest(new { error = "Invalid comparisonPeriod format. Use format like '7d', '30d', '1d'" });

        var now = DateTime.UtcNow;
        var currentStart = now.AddDays(-currentDays);
        var currentEnd = now;

        var comparisonStart = currentStart.AddDays(-comparisonDays);
        var comparisonEnd = currentStart;

        // Get current period summary
        var currentSummary = await controller.GetSummary(keyId, currentStart, currentEnd);
        if (currentSummary.Result is not OkObjectResult currentOkResult)
            return currentSummary.Result;

        var current = (AnalyticsSummary)currentOkResult.Value!;

        // Get comparison period summary
        var comparisonSummary = await controller.GetSummary(keyId, comparisonStart, comparisonEnd);
        if (comparisonSummary.Result is not OkObjectResult comparisonOkResult)
            return comparisonSummary.Result;

        var comparison = (AnalyticsSummary)comparisonOkResult.Value!;

        var comparisonResult = new PeriodComparison
        {
            CurrentPeriod = new PeriodMetrics
            {
                Period = period,
                From = currentStart,
                To = currentEnd,
                TotalRequests = current.TotalRequests,
                SuccessfulRequests = current.SuccessfulRequests,
                FailedRequests = current.FailedRequests,
                SuccessRatePercent = current.SuccessRatePercent,
                ErrorRatePercent = current.ErrorRatePercent,
                AverageResponseTimeMs = current.AverageResponseTimeMs,
                TotalBytesTransferred = current.TotalBytesTransferred
            },
            ComparisonPeriod = new PeriodMetrics
            {
                Period = comparisonPeriod,
                From = comparisonStart,
                To = comparisonEnd,
                TotalRequests = comparison.TotalRequests,
                SuccessfulRequests = comparison.SuccessfulRequests,
                FailedRequests = comparison.FailedRequests,
                SuccessRatePercent = comparison.SuccessRatePercent,
                ErrorRatePercent = comparison.ErrorRatePercent,
                AverageResponseTimeMs = comparison.AverageResponseTimeMs,
                TotalBytesTransferred = comparison.TotalBytesTransferred
            },
            Change = new PeriodChange
            {
                RequestsChangePercent = CalculateChangePercent(comparison.TotalRequests, current.TotalRequests),
                SuccessRateChangePercent = CalculateChangePercent(comparison.SuccessRatePercent, current.SuccessRatePercent),
                ErrorRateChangePercent = CalculateChangePercent(comparison.ErrorRatePercent, current.ErrorRatePercent),
                AvgResponseTimeChangePercent = CalculateChangePercent(comparison.AverageResponseTimeMs, current.AverageResponseTimeMs),
                BytesTransferredChangePercent = CalculateChangePercent(comparison.TotalBytesTransferred, current.TotalBytesTransferred)
            }
        };

        return controller.Ok(comparisonResult);
    }

    /// <summary>
    /// Returns endpoint statistics with error rate filtering for an API key.
    /// </summary>
    /// <param name="controller">The analytics controller instance.</param>
    /// <param name="keyId">The API key ID to query.</param>
    /// <param name="minErrorRate">Minimum error rate percentage to filter by (0-100). Defaults to 0.</param>
    /// <param name="limit">Maximum number of endpoints to return (default: 20, max: 100).</param>
    /// <param name="from">Start of the date range (UTC).</param>
    /// <param name="to">End of the date range (UTC).</param>
    /// <returns>Filtered list of endpoint statistics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="keyId"/> is null or whitespace.</exception>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<ActionResult<IReadOnlyList<EndpointStat>>> GetEndpointsByErrorRateAsync(
        this AnalyticsController controller,
        string keyId,
        double minErrorRate = 0,
        int limit = 20,
        DateTime? from = null,
        DateTime? to = null)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyId);

        if (limit < 1 || limit > 100)
            return controller.BadRequest(new { error = "limit must be between 1 and 100" });

        if (minErrorRate < 0 || minErrorRate > 100)
            return controller.BadRequest(new { error = "minErrorRate must be between 0 and 100" });

        var endpointsResult = await controller.GetTopEndpoints(keyId, limit, from, to);
        if (endpointsResult.Result is not OkObjectResult endpointsOkResult)
            return endpointsResult.Result;

        var endpoints = (List<EndpointStat>)endpointsOkResult.Value!;

        var filtered = endpoints
            .Where(e => e.ErrorRatePercent >= minErrorRate)
            .OrderByDescending(e => e.ErrorRatePercent)
            .ThenByDescending(e => e.RequestCount)
            .ToList()
            .AsReadOnly();

        return controller.Ok(filtered);
    }

    /// <summary>
    /// Returns hourly trend data with error rate threshold filtering.
    /// </summary>
    /// <param name="controller">The analytics controller instance.</param>
    /// <param name="keyId">The API key ID to query.</param>
    /// <param name="minErrorRate">Minimum error rate percentage to include (0-100). Defaults to 0.</param>
    /// <param name="from">Start of the date range (UTC).</param>
    /// <param name="to">End of the date range (UTC).</param>
    /// <returns>Filtered hourly trend data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="keyId"/> is null or whitespace.</exception>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<ActionResult<IReadOnlyList<HourlyBucket>>> GetHourlyTrendWithErrorFilterAsync(
        this AnalyticsController controller,
        string keyId,
        double minErrorRate = 0,
        DateTime? from = null,
        DateTime? to = null)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyId);

        if (minErrorRate < 0 || minErrorRate > 100)
            return controller.BadRequest(new { error = "minErrorRate must be between 0 and 100" });

        var hourlyResult = await controller.GetHourlyTrend(keyId, from, to);
        if (hourlyResult.Result is not OkObjectResult hourlyOkResult)
            return hourlyResult.Result;

        var hourlyBuckets = (List<HourlyBucket>)hourlyOkResult.Value!;

        var filtered = hourlyBuckets
            .Where(b => b.ErrorCount > 0 && (b.RequestCount > 0 ? (double)b.ErrorCount / b.RequestCount * 100 >= minErrorRate : false))
            .ToList()
            .AsReadOnly();

        return controller.Ok(filtered);
    }

    /// <summary>
    /// Returns daily trend data aggregated by week for easier trend analysis.
    /// </summary>
    /// <param name="controller">The analytics controller instance.</param>
    /// <param name="keyId">The API key ID to query.</param>
    /// <param name="weeks">Number of weeks to return (default: 4, max: 12).</param>
    /// <param name="from">Start of the date range (UTC).</param>
    /// <param name="to">End of the date range (UTC).</param>
    /// <returns>Weekly aggregated trend data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="keyId"/> is null or whitespace.</exception>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<ActionResult<IReadOnlyList<WeeklyBucket>>> GetWeeklyTrendAsync(
        this AnalyticsController controller,
        string keyId,
        int weeks = 4,
        DateTime? from = null,
        DateTime? to = null)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyId);

        if (weeks < 1 || weeks > 12)
            return controller.BadRequest(new { error = "weeks must be between 1 and 12" });

        var now = DateTime.UtcNow;
        var startDate = from ?? now.AddDays(-7 * weeks);
        var endDate = to ?? now;

        var dailyResult = await controller.GetDailyTrend(keyId, startDate, endDate);
        if (dailyResult.Result is not OkObjectResult dailyOkResult)
            return dailyResult.Result;

        var dailyBuckets = (List<DailyBucket>)dailyOkResult.Value!;

        var weeklyBuckets = dailyBuckets
            .GroupBy(b => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                b.Date,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday))
            .Select(g => new WeeklyBucket
            {
                WeekNumber = g.Key,
                Year = g.First().Date.Year,
                StartDate = g.First().Date,
                EndDate = g.Last().Date,
                RequestCount = g.Sum(b => b.RequestCount),
                ErrorCount = g.Sum(b => b.ErrorCount),
                AverageResponseTimeMs = g.Average(b => b.AverageResponseTimeMs),
                TotalBytes = g.Sum(b => b.TotalBytes)
            })
            .OrderByDescending(w => w.Year)
            .ThenByDescending(w => w.WeekNumber)
            .ToList()
            .AsReadOnly();

        return controller.Ok(weeklyBuckets);
    }

    private static bool TryParsePeriod(string period, out int days)
    {
        days = 0;
        if (string.IsNullOrWhiteSpace(period) || period.Length < 2)
            return false;

        var suffix = period[^1];
        if (!int.TryParse(period[..^1], CultureInfo.InvariantCulture, out var value))
            return false;

        return suffix switch
        {
            'd' or 'D' when value > 0 => (days = value, true).Item2,
            _ => false
        };
    }

    private static double CalculateChangePercent(double oldValue, double newValue)
    {
        if (oldValue == 0)
            return newValue > 0 ? 100 : 0;

        return Math.Round(((newValue - oldValue) / oldValue) * 100, 2);
    }
}

/// <summary>
/// Represents a comparison between two time periods.
/// </summary>
public class PeriodComparison
{
    /// <summary>Metrics for the current period.</summary>
    public required PeriodMetrics CurrentPeriod { get; set; }

    /// <summary>Metrics for the comparison period.</summary>
    public required PeriodMetrics ComparisonPeriod { get; set; }

    /// <summary>Change percentages between periods.</summary>
    public required PeriodChange Change { get; set; }
}

/// <summary>Metrics for a specific time period.</summary>
public class PeriodMetrics
{
    /// <summary>Period descriptor (e.g., "7d", "30d").</summary>
    public required string Period { get; set; }

    /// <summary>Start date of the period (UTC).</summary>
    public required DateTime From { get; set; }

    /// <summary>End date of the period (UTC).</summary>
    public required DateTime To { get; set; }

    /// <summary>Total number of requests.</summary>
    public int TotalRequests { get; set; }

    /// <summary>Number of successful requests.</summary>
    public int SuccessfulRequests { get; set; }

    /// <summary>Number of failed requests.</summary>
    public int FailedRequests { get; set; }

    /// <summary>Success rate percentage (0-100).</summary>
    public double SuccessRatePercent { get; set; }

    /// <summary>Error rate percentage (0-100).</summary>
    public double ErrorRatePercent { get; set; }

    /// <summary>Average response time in milliseconds.</summary>
    public double AverageResponseTimeMs { get; set; }

    /// <summary>Total bytes transferred.</summary>
    public long TotalBytesTransferred { get; set; }
}

/// <summary>Change percentages between two periods.</summary>
public class PeriodChange
{
    /// <summary>Percentage change in total requests.</summary>
    public double RequestsChangePercent { get; set; }

    /// <summary>Percentage change in success rate.</summary>
    public double SuccessRateChangePercent { get; set; }

    /// <summary>Percentage change in error rate.</summary>
    public double ErrorRateChangePercent { get; set; }

    /// <summary>Percentage change in average response time.</summary>
    public double AvgResponseTimeChangePercent { get; set; }

    /// <summary>Percentage change in bytes transferred.</summary>
    public double BytesTransferredChangePercent { get; set; }
}

/// <summary>Represents aggregated metrics for a week.</summary>
public class WeeklyBucket
{
    /// <summary>ISO week number.</summary>
    public int WeekNumber { get; set; }

    /// <summary>Year.</summary>
    public int Year { get; set; }

    /// <summary>Start date of the week (UTC).</summary>
    public DateTime StartDate { get; set; }

    /// <summary>End date of the week (UTC).</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Total requests in the week.</summary>
    public int RequestCount { get; set; }

    /// <summary>Total errors in the week.</summary>
    public int ErrorCount { get; set; }

    /// <summary>Average response time in milliseconds.</summary>
    public double AverageResponseTimeMs { get; set; }

    /// <summary>Total bytes transferred in the week.</summary>
    public long TotalBytes { get; set; }
}