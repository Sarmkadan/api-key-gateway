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
    void RecordRequest(string apiKeyId, string endpoint, int statusCode, long latencyMs);
    void RecordRateLimitExceeded(string apiKeyId);
    void RecordError(string apiKeyId, string errorCode);
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

    public MetricsCollectionService(ILogger<MetricsCollectionService> logger)
    {
        _logger = logger;
    }

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

    public void RecordRateLimitExceeded(string apiKeyId)
    {
        _rateLimitCounters.AddOrUpdate(apiKeyId, 1, (_, v) => v + 1);
    }

    public void RecordError(string apiKeyId, string errorCode)
    {
        _errorCounters.AddOrUpdate(errorCode, 1, (_, v) => v + 1);
    }

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
                .GroupBy(x => x.Key.Split(':')[1])
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
    public DateTime Timestamp { get; set; }
    public long TotalRequests { get; set; }
    public long TotalErrors { get; set; }
    public double ErrorRate { get; set; }
    public double AverageLatencyMs { get; set; }
    public long P95LatencyMs { get; set; }
    public long TotalRateLimitExceeded { get; set; }
    public Dictionary<string, long> RequestsByEndpoint { get; set; } = new();
    public Dictionary<string, long> ErrorsByCode { get; set; } = new();
}
