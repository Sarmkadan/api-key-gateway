// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Services;

/// <summary>
/// Aggregated usage analytics for a single API key over a date range
/// </summary>
public class AnalyticsSummary
{
    public string ApiKeyId { get; set; } = string.Empty;
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double SuccessRatePercent { get; set; }
    public double ErrorRatePercent { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public long TotalBytesTransferred { get; set; }
    public int UniqueEndpoints { get; set; }
    public int UniqueSourceIps { get; set; }
}

/// <summary>
/// Usage statistics for a single endpoint
/// </summary>
public class EndpointStat
{
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public int ErrorCount { get; set; }
    public double ErrorRatePercent { get; set; }
}

/// <summary>
/// Usage bucket grouped by hour
/// </summary>
public class HourlyBucket
{
    public DateTime Hour { get; set; }
    public int RequestCount { get; set; }
    public int ErrorCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
}

/// <summary>
/// Usage bucket grouped by calendar day
/// </summary>
public class DailyBucket
{
    public DateTime Date { get; set; }
    public int RequestCount { get; set; }
    public int ErrorCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public long TotalBytes { get; set; }
}

/// <summary>
/// Provides aggregated analytics over raw usage records.
/// All aggregation runs in-process over records returned by
/// <see cref="IUsageTrackingService"/>; no additional storage layer is required.
/// </summary>
public interface IUsageAnalyticsService
{
    /// <summary>
    /// Returns a high-level summary for a key over the given date range.
    /// </summary>
    Task<AnalyticsSummary> GetSummaryAsync(string apiKeyId, DateTime from, DateTime to);

    /// <summary>
    /// Returns the most-called endpoints ordered by request count descending.
    /// </summary>
    Task<List<EndpointStat>> GetTopEndpointsAsync(string apiKeyId, DateTime from, DateTime to, int limit = 10);

    /// <summary>
    /// Returns per-hour request counts and latency for the date range.
    /// </summary>
    Task<List<HourlyBucket>> GetHourlyTrendAsync(string apiKeyId, DateTime from, DateTime to);

    /// <summary>
    /// Returns per-day request counts, error counts, latency and bytes for the date range.
    /// </summary>
    Task<List<DailyBucket>> GetDailyTrendAsync(string apiKeyId, DateTime from, DateTime to);
}

/// <summary>
/// Default implementation of <see cref="IUsageAnalyticsService"/>.
/// </summary>
public class UsageAnalyticsService : IUsageAnalyticsService
{
    private readonly IUsageTrackingService _usageTracking;
    private readonly ILogger<UsageAnalyticsService> _logger;

    public UsageAnalyticsService(
        IUsageTrackingService usageTracking,
        ILogger<UsageAnalyticsService> logger)
    {
        _usageTracking = usageTracking ?? throw new ArgumentNullException(nameof(usageTracking));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<AnalyticsSummary> GetSummaryAsync(string apiKeyId, DateTime from, DateTime to)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));

        if (to < from)
            throw new ArgumentException("End date must be after start date", nameof(to));

        var records = await _usageTracking.GetUsageRecordsAsync(apiKeyId, from, to);

        var total = records.Count;
        var failed = records.Count(r => r.IsError);
        var succeeded = total - failed;

        return new AnalyticsSummary
        {
            ApiKeyId = apiKeyId,
            From = from,
            To = to,
            TotalRequests = total,
            SuccessfulRequests = succeeded,
            FailedRequests = failed,
            SuccessRatePercent = total > 0 ? Math.Round(succeeded * 100.0 / total, 2) : 0,
            ErrorRatePercent = total > 0 ? Math.Round(failed * 100.0 / total, 2) : 0,
            AverageResponseTimeMs = records.Count > 0 ? Math.Round(records.Average(r => r.ResponseTimeMs), 2) : 0,
            TotalBytesTransferred = records.Sum(r => r.TotalBytes),
            UniqueEndpoints = records.Select(r => r.Endpoint).Distinct().Count(),
            UniqueSourceIps = records
                .Select(r => r.SourceIp)
                .Where(ip => !string.IsNullOrWhiteSpace(ip))
                .Distinct()
                .Count()
        };
    }

    /// <inheritdoc/>
    public async Task<List<EndpointStat>> GetTopEndpointsAsync(
        string apiKeyId, DateTime from, DateTime to, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));

        if (to < from)
            throw new ArgumentException("End date must be after start date", nameof(to));

        if (limit <= 0) limit = 10;

        var records = await _usageTracking.GetUsageRecordsAsync(apiKeyId, from, to);

        return records
            .GroupBy(r => new { r.Endpoint, r.Method })
            .Select(g =>
            {
                var count = g.Count();
                var errors = g.Count(r => r.IsError);
                return new EndpointStat
                {
                    Endpoint = g.Key.Endpoint,
                    Method = g.Key.Method,
                    RequestCount = count,
                    AverageResponseTimeMs = Math.Round(g.Average(r => r.ResponseTimeMs), 2),
                    ErrorCount = errors,
                    ErrorRatePercent = count > 0 ? Math.Round(errors * 100.0 / count, 2) : 0
                };
            })
            .OrderByDescending(e => e.RequestCount)
            .Take(limit)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<HourlyBucket>> GetHourlyTrendAsync(
        string apiKeyId, DateTime from, DateTime to)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));

        if (to < from)
            throw new ArgumentException("End date must be after start date", nameof(to));

        var records = await _usageTracking.GetUsageRecordsAsync(apiKeyId, from, to);

        return records
            .GroupBy(r => new DateTime(
                r.RecordedAt.Year, r.RecordedAt.Month, r.RecordedAt.Day,
                r.RecordedAt.Hour, 0, 0, DateTimeKind.Utc))
            .Select(g =>
            {
                var count = g.Count();
                return new HourlyBucket
                {
                    Hour = g.Key,
                    RequestCount = count,
                    ErrorCount = g.Count(r => r.IsError),
                    AverageResponseTimeMs = Math.Round(g.Average(r => r.ResponseTimeMs), 2)
                };
            })
            .OrderBy(b => b.Hour)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<DailyBucket>> GetDailyTrendAsync(
        string apiKeyId, DateTime from, DateTime to)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));

        if (to < from)
            throw new ArgumentException("End date must be after start date", nameof(to));

        var records = await _usageTracking.GetUsageRecordsAsync(apiKeyId, from, to);

        return records
            .GroupBy(r => r.RecordedAt.Date)
            .Select(g =>
            {
                var count = g.Count();
                return new DailyBucket
                {
                    Date = DateTime.SpecifyKind(g.Key, DateTimeKind.Utc),
                    RequestCount = count,
                    ErrorCount = g.Count(r => r.IsError),
                    AverageResponseTimeMs = Math.Round(g.Average(r => r.ResponseTimeMs), 2),
                    TotalBytes = g.Sum(r => r.TotalBytes)
                };
            })
            .OrderBy(b => b.Date)
            .ToList();
    }

    private static void ValidateArguments(string apiKeyId, DateTime from, DateTime to)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));

        if (to < from)
            throw new ArgumentException("End date must be after start date", nameof(to));
    }

    private static void ValidateArguments(string apiKeyId, DateTime from, DateTime to, int limit)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));

        if (to < from)
            throw new ArgumentException("End date must be after start date", nameof(to));

        if (limit <= 0)
            throw new ArgumentException("Limit must be positive", nameof(limit));
    }
}
