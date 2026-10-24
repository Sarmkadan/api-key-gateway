// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace ApiKeyGateway.Services;

/// <summary>
/// Collects and aggregates metrics for monitoring and observability.
/// Tracks request counts, error rates, latencies, and quota usage.
/// These metrics feed into dashboards and alerting systems.
/// </summary>
public interface IMetricsCollectionService
{
    /// <summary>
    /// Records a request metric with API key, endpoint, status code, and latency.
    /// </summary>
    /// <param name="apiKeyId">Identifier of the API key used in the request.</param>
    /// <param name="endpoint">API endpoint that was requested.</param>
    /// <param name="statusCode">HTTP status code of the response.</param>
    /// <param name="latencyMs">Request latency in milliseconds.</param>
    void RecordRequest(string apiKeyId, string endpoint, int statusCode, long latencyMs);

    /// <summary>
    /// Records a rate limit exceeded event for the given API key.
    /// </summary>
    /// <param name="apiKeyId">Identifier of the API key that exceeded rate limits.</param>
    void RecordRateLimitExceeded(string apiKeyId);

    /// <summary>
    /// Records an error metric for the given API key and error code.
    /// </summary>
    /// <param name="apiKeyId">Identifier of the API key associated with the error.</param>
    /// <param name="errorCode">Error code to record.</param>
    void RecordError(string apiKeyId, string errorCode);

    /// <summary>
    /// Returns a snapshot of the current metrics.
    /// </summary>
    /// <returns>A snapshot of the current metrics.</returns>
    MetricsSnapshot GetSnapshot();
}

/// <summary>
/// In-memory metrics collection implementation.
/// For production, integrate with Prometheus, DataDog, or similar.
/// </summary>
public sealed class MetricsCollectionService : IMetricsCollectionService
{
    private readonly ConcurrentDictionary<string, long> _requestCounters = new();
    private readonly ConcurrentDictionary<string, long> _rateLimitCounters = new();
    private readonly ConcurrentDictionary<string, long> _errorCounters = new();
    private readonly ConcurrentBag<long> _latencies = new();
    private readonly ILogger<MetricsCollectionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsCollectionService"/> class.
    /// </summary>
    /// <param name="logger">Logger used for diagnostics</param>
    public MetricsCollectionService(ILogger<MetricsCollectionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Records a request metric with API key, endpoint, status code, and latency.
    /// </summary>
    /// <param name="apiKeyId">Identifier of the API key used in the request</param>
    /// <param name="endpoint">API endpoint that was requested</param>
    /// <param name="statusCode">HTTP status code of the response</param>
    /// <param name="latencyMs">Request latency in milliseconds</param>
    public void RecordRequest(string apiKeyId, string endpoint, int statusCode, long latencyMs)
    {
        var key = $"{apiKeyId}:{endpoint}";
        _requestCounters.AddOrUpdate(key, 1, (_, v) => v + 1);
        _latencies.Add(latencyMs);

        if (statusCode >= 400)
        {
            var errorKey = $"{statusCode}";
            _errorCounters.AddOrUpdate(errorKey, 1, (_, v) => v + 1);
        }
    }

    /// <summary>
    /// Records a rate limit exceeded event for the given API key.
    /// </summary>
    /// <param name="apiKeyId">Identifier of the API key that exceeded rate limits</param>
    public void RecordRateLimitExceeded(string apiKeyId)
    {
        _rateLimitCounters.AddOrUpdate(apiKeyId, 1, (_, v) => v + 1);
    }

    /// <summary>
    /// Records an error metric for the given API key and error code.
    /// </summary>
    /// <param name="apiKeyId">Identifier of the API key associated with the error</param>
    /// <param name="errorCode">Error code to record</param>
    public void RecordError(string apiKeyId, string errorCode)
    {
        _errorCounters.AddOrUpdate(errorCode, 1, (_, v) => v + 1);
    }

    /// <summary>
    /// Returns a snapshot of the current metrics.
    /// </summary>
    /// <returns>A snapshot of the current metrics</returns>
    public MetricsSnapshot GetSnapshot()
    {
        var latenciesArray = _latencies.ToArray();
        var avgLatency = latenciesArray.Length > 0 ? latenciesArray.Average() : 0;
        var p95Latency = latenciesArray.Length > 0
            ? latenciesArray.OrderBy(x => x).ElementAt((int)(latenciesArray.Length * 0.95))
            : 0;

        var totalRequests = _requestCounters.Values.Sum();
        var totalErrors = _errorCounters.Values.Sum();
        var errorRate = totalRequests > 0 ? (totalErrors / (double)totalRequests) * 100 : 0;

        return new MetricsSnapshot
        {
            Timestamp = DateTime.UtcNow,
            TotalRequests = totalRequests,
            TotalErrors = totalErrors,
            ErrorRate = errorRate,
            AverageLatencyMs = avgLatency,
            P95LatencyMs = p95Latency,
            TotalRateLimitExceeded = _rateLimitCounters.Values.Sum(),
            RequestsByEndpoint = _requestCounters
                .GroupBy(x => x.Key.Split(':', 2)[1])
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Value)),
            ErrorsByCode = _errorCounters.ToDictionary(x => x.Key, x => x.Value)
        };
    }
}

/// <summary>
/// Snapshot of current metrics at a point in time.
/// </summary>
public record MetricsSnapshot
{
    /// <summary>UTC time when the snapshot was taken.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Total number of recorded requests.</summary>
    public long TotalRequests { get; set; }

    /// <summary>Total number of recorded error responses.</summary>
    public long TotalErrors { get; set; }

    /// <summary>Percentage of requests that resulted in an error.</summary>
    public double ErrorRate { get; set; }

    /// <summary>Average request latency in milliseconds.</summary>
    public double AverageLatencyMs { get; set; }

    /// <summary>95th percentile request latency in milliseconds.</summary>
    public long P95LatencyMs { get; set; }

    /// <summary>Total number of rate limit exceeded events.</summary>
    public long TotalRateLimitExceeded { get; set; }

    /// <summary>Request counts keyed by endpoint.</summary>
    public Dictionary<string, long> RequestsByEndpoint { get; set; } = new();

    /// <summary>Error counts keyed by error code.</summary>
    public Dictionary<string, long> ErrorsByCode { get; set; } = new();
}
