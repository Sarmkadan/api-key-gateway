// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using ApiKeyGateway.Domain.Exceptions;

namespace ApiKeyGateway.Services;

/// <summary>
/// Sliding-window rate limiter that enforces per-key request quotas.
/// Unlike <see cref="IRateLimitingService"/> which is the persistence-backed service,
/// this class operates entirely in-memory for low-latency hot-path checks.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Attempts to acquire a permit for the given key. Returns true if allowed.
    /// </summary>
    bool TryAcquire(string apiKeyId);

    /// <summary>
    /// Returns the number of remaining permits in the current window.
    /// </summary>
    int GetRemainingPermits(string apiKeyId);

    /// <summary>
    /// Returns when the current rate limit window resets for the given key.
    /// </summary>
    DateTime GetWindowResetTime(string apiKeyId);

    /// <summary>
    /// Configures per-key rate limit overrides at runtime.
    /// </summary>
    void SetKeyLimit(string apiKeyId, int maxRequests, TimeSpan window);

    /// <summary>
    /// Removes all tracked state for a key (e.g. after revocation).
    /// </summary>
    void RemoveKey(string apiKeyId);
}

/// <inheritdoc cref="IRateLimiter"/>
public class RateLimiter : IRateLimiter
{
    private readonly ConcurrentDictionary<string, SlidingWindow> _windows = new();
    private readonly int _defaultMaxRequests;
    private readonly TimeSpan _defaultWindow;
    private readonly ILogger<RateLimiter> _logger;

    public RateLimiter(ILogger<RateLimiter> logger, int defaultMaxRequests = 60, TimeSpan? defaultWindow = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defaultMaxRequests = defaultMaxRequests;
        _defaultWindow = defaultWindow ?? TimeSpan.FromMinutes(1);
    }

    /// <inheritdoc/>
    public bool TryAcquire(string apiKeyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKeyId);

        var window = _windows.GetOrAdd(apiKeyId, _ => new SlidingWindow(_defaultMaxRequests, _defaultWindow));

        lock (window)
        {
            window.EvictExpired();

            if (window.Timestamps.Count >= window.MaxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for key {ApiKeyId}: {Count}/{Max}",
                    apiKeyId, window.Timestamps.Count, window.MaxRequests);
                return false;
            }

            window.Timestamps.Enqueue(DateTime.UtcNow);
            return true;
        }
    }

    /// <inheritdoc/>
    public int GetRemainingPermits(string apiKeyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKeyId);

        if (!_windows.TryGetValue(apiKeyId, out var window))
            return _defaultMaxRequests;

        lock (window)
        {
            window.EvictExpired();
            return Math.Max(0, window.MaxRequests - window.Timestamps.Count);
        }
    }

    /// <inheritdoc/>
    public DateTime GetWindowResetTime(string apiKeyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKeyId);

        if (!_windows.TryGetValue(apiKeyId, out var window))
            return DateTime.UtcNow;

        lock (window)
        {
            if (window.Timestamps.Count == 0)
                return DateTime.UtcNow;

            return window.Timestamps.Peek().Add(window.WindowSize);
        }
    }

    /// <inheritdoc/>
    public void SetKeyLimit(string apiKeyId, int maxRequests, TimeSpan window)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKeyId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRequests);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(window, TimeSpan.Zero);

        _windows.AddOrUpdate(
            apiKeyId,
            _ => new SlidingWindow(maxRequests, window),
            (_, existing) =>
            {
                lock (existing)
                {
                    existing.MaxRequests = maxRequests;
                    existing.WindowSize = window;
                    return existing;
                }
            });

        _logger.LogInformation("Updated rate limit for key {ApiKeyId}: {Max} per {Window}",
            apiKeyId, maxRequests, window);
    }

    /// <inheritdoc/>
    public void RemoveKey(string apiKeyId)
    {
        _windows.TryRemove(apiKeyId, out _);
    }

    private sealed class SlidingWindow
    {
        public readonly Queue<DateTime> Timestamps = new();
        public int MaxRequests;
        public TimeSpan WindowSize;

        public SlidingWindow(int maxRequests, TimeSpan windowSize)
        {
            MaxRequests = maxRequests;
            WindowSize = windowSize;
        }

        public void EvictExpired()
        {
            var cutoff = DateTime.UtcNow - WindowSize;
            while (Timestamps.Count > 0 && Timestamps.Peek() < cutoff)
            {
                Timestamps.Dequeue();
            }
        }
    }
}
