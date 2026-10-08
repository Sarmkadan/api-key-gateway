// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Diagnostics;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Services;

/// <summary>
/// Real-time usage tracker that maintains in-memory counters for active API keys
/// and periodically flushes aggregated metrics to the persistent usage store.
/// </summary>
public interface IUsageTracker
{
    /// <summary>
    /// Increments the request counter for the given API key.
    /// </summary>
    void TrackRequest(string apiKeyId, long bytesTransferred = 0);

    /// <summary>
    /// Returns the current in-memory snapshot of usage for an API key.
    /// </summary>
    UsageSnapshot GetSnapshot(string apiKeyId);

    /// <summary>
    /// Flushes all accumulated counters to the persistent store and resets them.
    /// </summary>
    Task FlushAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total number of requests tracked since the last flush.
    /// </summary>
    long GetPendingRequestCount();
}

/// <summary>
/// Snapshot of in-memory usage counters for a single API key.
/// </summary>
[DebuggerDisplay("UsageSnapshot {ApiKeyId,nq} Requests={RequestCount}")]
public class UsageSnapshot
{
    /// <summary>API key identifier</summary>
    public string ApiKeyId { get; init; } = string.Empty;

    /// <summary>Number of requests since last flush</summary>
    public long RequestCount { get; init; }

    /// <summary>Total bytes transferred since last flush</summary>
    public long BytesTransferred { get; init; }

    /// <summary>Timestamp of the first tracked request in this window</summary>
    public DateTime WindowStart { get; init; }

    /// <summary>Timestamp of the most recent tracked request</summary>
    public DateTime LastRequestAt { get; init; }

    /// <summary>
    /// Returns a concise representation of the snapshot counters.
    /// </summary>
    public override string ToString() =>
        $"UsageSnapshot {{ ApiKeyId = {ApiKeyId}, RequestCount = {RequestCount}, BytesTransferred = {BytesTransferred}, WindowStart = {WindowStart:O}, LastRequestAt = {LastRequestAt:O} }}";
}

/// <inheritdoc cref="IUsageTracker"/>
public class UsageTracker : IUsageTracker
{
    private readonly ConcurrentDictionary<string, UsageCounters> _counters = new();
    private readonly IUsageTrackingService _persistentStore;
    private readonly ILogger<UsageTracker> _logger;

    public UsageTracker(IUsageTrackingService persistentStore, ILogger<UsageTracker> logger)
    {
        _persistentStore = persistentStore ?? throw new ArgumentNullException(nameof(persistentStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void TrackRequest(string apiKeyId, long bytesTransferred = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKeyId);

        var counters = _counters.GetOrAdd(apiKeyId, _ => new UsageCounters(DateTime.UtcNow));
        Interlocked.Increment(ref counters.RequestCount);
        Interlocked.Add(ref counters.BytesTransferred, bytesTransferred);
        counters.LastRequestAt = DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public UsageSnapshot GetSnapshot(string apiKeyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKeyId);

        if (!_counters.TryGetValue(apiKeyId, out var counters))
        {
            return new UsageSnapshot { ApiKeyId = apiKeyId };
        }

        return new UsageSnapshot
        {
            ApiKeyId = apiKeyId,
            RequestCount = Interlocked.Read(ref counters.RequestCount),
            BytesTransferred = Interlocked.Read(ref counters.BytesTransferred),
            WindowStart = counters.WindowStart,
            LastRequestAt = counters.LastRequestAt
        };
    }

    /// <inheritdoc/>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        var keys = _counters.Keys.ToList();
        var flushedCount = 0;

        foreach (var key in keys)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            if (!_counters.TryRemove(key, out var counters))
                continue;

            var requestCount = Interlocked.Read(ref counters.RequestCount);
            if (requestCount == 0)
                continue;

            try
            {
                var record = new UsageRecord
                {
                    ApiKeyId = key,
                    RecordedAt = DateTime.UtcNow,
                    RequestBytes = Interlocked.Read(ref counters.BytesTransferred),
                    ResponseBytes = 0
                };

                await _persistentStore.RecordUsageAsync(record);
                flushedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to flush usage counters for key {ApiKeyId}", key);
                // Re-add counters so they are not lost
                _counters.TryAdd(key, counters);
            }
        }

        if (flushedCount > 0)
        {
            _logger.LogInformation("Flushed usage counters for {Count} API keys", flushedCount);
        }
    }

    /// <inheritdoc/>
    public long GetPendingRequestCount()
    {
        long total = 0;
        foreach (var counters in _counters.Values)
        {
            total += Interlocked.Read(ref counters.RequestCount);
        }
        return total;
    }

    private sealed class UsageCounters
    {
        public long RequestCount;
        public long BytesTransferred;
        public readonly DateTime WindowStart;
        public DateTime LastRequestAt;

        public UsageCounters(DateTime windowStart)
        {
            WindowStart = windowStart;
            LastRequestAt = windowStart;
        }
    }
}
