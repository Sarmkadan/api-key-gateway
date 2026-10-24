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
    /// <summary>
    /// Identifier of the API key the summary covers.
    /// </summary>
    public string ApiKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Start of the analyzed date range.
    /// </summary>
    public DateTime From { get; set; }

    /// <summary>
    /// End of the analyzed date range.
    /// </summary>
    public DateTime To { get; set; }

    /// <summary>
    /// Total number of requests in the range.
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// Number of successful requests in the range.
    /// </summary>
    public int SuccessfulRequests { get; set; }

    /// <summary>
    /// Number of failed requests in the range.
    /// </summary>
    public int FailedRequests { get; set; }

    /// <summary>
    /// Percentage of successful requests.
    /// </summary>
    public double SuccessRatePercent { get; set; }

    /// <summary>
    /// Percentage of failed requests.
    /// </summary>
    public double ErrorRatePercent { get; set; }

    /// <summary>
    /// Average response time in milliseconds.
    /// </summary>
    public double AverageResponseTimeMs { get; set; }

    /// <summary>
    /// Total bytes transferred in the range.
    /// </summary>
    public long TotalBytesTransferred { get; set; }

    /// <summary>
    /// Number of distinct endpoints called.
    /// </summary>
    public int UniqueEndpoints { get; set; }

    /// <summary>
    /// Number of distinct source IP addresses.
    /// </summary>
    public int UniqueSourceIps { get; set; }

    /// <summary>
    /// Returns a short diagnostic string with the key identifier, date range and request counts.
    /// </summary>
    /// <returns>A formatted summary string.</returns>
    public override string ToString() => $"AnalyticsSummary {{ ApiKeyId = {ApiKeyId}, From = {From}, To = {To}, TotalRequests = {TotalRequests}, SuccessfulRequests = {SuccessfulRequests}, FailedRequests = {FailedRequests} }}";
}

/// <summary>
/// Usage statistics for a single endpoint
/// </summary>
public class EndpointStat
{
    /// <summary>
    /// Request path of the endpoint.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method of the endpoint.
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Number of requests to the endpoint.
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// Average response time in milliseconds.
    /// </summary>
    public double AverageResponseTimeMs { get; set; }

    /// <summary>
    /// Number of failed requests to the endpoint.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Percentage of failed requests to the endpoint.
    /// </summary>
    public double ErrorRatePercent { get; set; }
}

/// <summary>
/// Usage bucket grouped by hour
/// </summary>
public class HourlyBucket
{
    /// <summary>
    /// Start of the hour this bucket covers.
    /// </summary>
    public DateTime Hour { get; set; }

    /// <summary>
    /// Number of requests in the hour.
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// Number of failed requests in the hour.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Average response time in milliseconds.
    /// </summary>
    public double AverageResponseTimeMs { get; set; }
}

/// <summary>
/// Usage bucket grouped by calendar day
/// </summary>
public class DailyBucket
{
    /// <summary>
    /// Calendar day this bucket covers.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Number of requests on the day.
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// Number of failed requests on the day.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Average response time in milliseconds.
    /// </summary>
    public double AverageResponseTimeMs { get; set; }

    /// <summary>
    /// Total bytes transferred on the day.
    /// </summary>
    public long TotalBytes { get; set; }
}

/// <summary>
/// Statistics for an API key consumer (top consumers list)
/// </summary>
public class ApiKeyConsumerStat
{
    /// <summary>
    /// Identifier of the API key.
    /// </summary>
    public string ApiKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the consumer that owns the key.
    /// </summary>
    public string ConsumerId { get; set; } = string.Empty;

    /// <summary>
    /// Number of requests made with the key.
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// Total bytes transferred with the key.
    /// </summary>
    public long TotalBytesTransferred { get; set; }

    /// <summary>
    /// Average response time in milliseconds.
    /// </summary>
    public double AverageResponseTimeMs { get; set; }

    /// <summary>
    /// Number of failed requests made with the key.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Percentage of failed requests made with the key.
    /// </summary>
    public double ErrorRatePercent { get; set; }
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

    /// <summary>
    /// Returns the top N API keys by request count over the given date range.
    /// </summary>
    Task<List<ApiKeyConsumerStat>> GetTopConsumersAsync(DateTime from, DateTime to, int topN = 10);
}

/// <summary>
/// Default implementation of <see cref="IUsageAnalyticsService"/>.
/// </summary>
public class UsageAnalyticsService : IUsageAnalyticsService
{
    private readonly IUsageTrackingService _usageTracking;
    private readonly ILogger<UsageAnalyticsService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsageAnalyticsService"/> class.
    /// </summary>
    /// <param name="usageTracking">Source of raw usage records.</param>
    /// <param name="logger">Logger for analytics operations.</param>
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
        {
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));
        }

        if (to < from)
        {
            throw new ArgumentException("End date must be after start date", nameof(to));
        }

        _logger.LogInformation("Getting summary for {ApiKeyId} from {From} to {To}", apiKeyId, from, to);
        var records = await _usageTracking.GetUsageRecordsAsync(apiKeyId, from, to);

        var total = records.Count;
        var failed = records.Count(r => r.IsError);
        var succeeded = total - failed;

        var summary = new AnalyticsSummary
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
        _logger.LogInformation("Finished getting summary for {ApiKeyId}", apiKeyId);
        return summary;
    }

    /// <inheritdoc/>
    public async Task<List<EndpointStat>> GetTopEndpointsAsync(
        string apiKeyId, DateTime from, DateTime to, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
        {
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));
        }

        if (to < from)
        {
            throw new ArgumentException("End date must be after start date", nameof(to));
        }

        if (limit <= 0)
        {
            limit = 10;
        }

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
        {
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));
        }

        if (to < from)
        {
            throw new ArgumentException("End date must be after start date", nameof(to));
        }

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
        {
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));
        }

        if (to < from)
        {
            throw new ArgumentException("End date must be after start date", nameof(to));
        }

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
        {
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));
        }

        if (to < from)
        {
            throw new ArgumentException("End date must be after start date", nameof(to));
        }
    }

    private static void ValidateArguments(string apiKeyId, DateTime from, DateTime to, int limit)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
        {
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));
        }

        if (to < from)
        {
            throw new ArgumentException("End date must be after start date", nameof(to));
        }

        if (limit <= 0)
        {
            throw new ArgumentException("Limit must be positive", nameof(limit));
        }
    }

    /// <summary>
    /// Returns the top N API keys by request count over the given date range.
    /// </summary>
    public async Task<List<ApiKeyConsumerStat>> GetTopConsumersAsync(DateTime from, DateTime to, int topN = 10)
    {
        if (to < from)
        {
            throw new ArgumentException("End date must be after start date", nameof(to));
        }

        if (topN <= 0)
        {
            topN = 10;
        }

        // Get all usage records across all API keys in the date range
        var allRecords = await _usageTracking.GetUsageAsync(from, to);

        // Group by API key and consumer, then aggregate statistics
        var consumerStats = allRecords
            .GroupBy(r => new { r.ApiKeyId, r.ConsumerId })
            .Select(g => new ApiKeyConsumerStat
            {
                ApiKeyId = g.Key.ApiKeyId,
                ConsumerId = g.Key.ConsumerId,
                RequestCount = g.Count(),
                TotalBytesTransferred = g.Sum(r => r.TotalBytes),
                AverageResponseTimeMs = g.Count() > 0 ? Math.Round(g.Average(r => r.ResponseTimeMs), 2) : 0,
                ErrorCount = g.Count(r => r.IsError),
                ErrorRatePercent = g.Count() > 0 ? Math.Round(g.Count(r => r.IsError) * 100.0 / g.Count(), 2) : 0
            })
            .OrderByDescending(s => s.RequestCount)
            .Take(topN)
            .ToList();

        return consumerStats;
    }
}
