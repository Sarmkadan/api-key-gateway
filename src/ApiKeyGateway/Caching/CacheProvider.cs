// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace ApiKeyGateway.Caching;

/// <summary>
/// Interface for cache abstraction. Implementations can use different
/// backends (in-memory, Redis, Memcached) without changing calling code.
/// This is critical for supporting both single-instance and distributed deployments.
/// Methods return ValueTask so that synchronous-completing implementations (e.g.
/// InMemoryCacheProvider) avoid allocating a Task state-machine object per call.
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Retrieves a value from cache by key.
    /// </summary>
    ValueTask<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Stores a value in cache with optional expiration.
    /// </summary>
    ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Removes a value from cache.
    /// </summary>
    ValueTask RemoveAsync(string key);

    /// <summary>
    /// Checks if a key exists in cache.
    /// </summary>
    ValueTask<bool> ExistsAsync(string key);

    /// <summary>
    /// Atomically increments a counter. Used for rate limiting tracking.
    /// </summary>
    ValueTask<long> IncrementAsync(string key, long increment = 1);

    /// <summary>
    /// Removes all entries matching a pattern (use cautiously).
    /// </summary>
    ValueTask<int> RemoveByPatternAsync(string pattern);
}

/// <summary>
/// In-memory cache implementation using MemoryCache.
/// Best for single-instance deployments. Use Redis adapter for clusters.
/// </summary>
public sealed class InMemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InMemoryCacheProvider> _logger;
    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, byte> _trackedKeys = new();

    public InMemoryCacheProvider(IMemoryCache cache, ILogger<InMemoryCacheProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);

        _cache = cache;
        _logger = logger;
    }

    public ValueTask<T?> GetAsync<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Cache HIT for key: {Key}", key);
            return ValueTask.FromResult(value);
        }

        _logger.LogDebug("Cache MISS for key: {Key}", key);
        return ValueTask.FromResult<T?>(null);
    }

    public ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var cacheOptions = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            // Absolute expiration: a TTL passed by callers (API key cache, external
            // API responses) must be a hard deadline. Sliding expiration would let
            // frequently accessed entries live forever, so revoked keys or stale
            // upstream data would keep being served under constant traffic.
            cacheOptions.AbsoluteExpirationRelativeToNow = expiration;
        }

        cacheOptions.RegisterPostEvictionCallback(static (evictedKey, _, reason, state) =>
        {
            // On Replaced the key still holds a live entry, so it must stay tracked.
            if (reason != EvictionReason.Replaced
                && state is ConcurrentDictionary<string, byte> tracked
                && evictedKey is string stringKey)
            {
                tracked.TryRemove(stringKey, out byte _);
            }
        }, _trackedKeys);

        _cache.Set(key, value, cacheOptions);
        _trackedKeys[key] = 0;
        _logger.LogDebug("Cache SET for key: {Key} with expiration: {Expiration}", key, expiration?.TotalSeconds);
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(string key)
    {
        _cache.Remove(key);
        _trackedKeys.TryRemove(key, out _);
        _logger.LogDebug("Cache REMOVE for key: {Key}", key);
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> ExistsAsync(string key) =>
        ValueTask.FromResult(_cache.TryGetValue(key, out _));

    public ValueTask<long> IncrementAsync(string key, long increment = 1)
    {
        var newValue = _counters.AddOrUpdate(key, increment, (_, current) => current + increment);
        _logger.LogDebug("Counter incremented: {Key} = {Value}", key, newValue);
        return ValueTask.FromResult(newValue);
    }

    public ValueTask<int> RemoveByPatternAsync(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return ValueTask.FromResult(0);

        var regex = BuildPatternRegex(pattern);
        var removed = 0;

        foreach (var key in _trackedKeys.Keys)
        {
            if (regex.IsMatch(key) && _trackedKeys.TryRemove(key, out _))
            {
                _cache.Remove(key);
                removed++;
            }
        }

        foreach (var key in _counters.Keys)
        {
            if (regex.IsMatch(key) && _counters.TryRemove(key, out _))
                removed++;
        }

        _logger.LogDebug("Cache REMOVE by pattern {Pattern}: {Count} entries", pattern, removed);
        return ValueTask.FromResult(removed);
    }

    /// <summary>
    /// Translates a glob-style pattern ('*' matches any run of characters,
    /// '?' matches a single character) into an anchored regular expression.
    /// </summary>
    private static Regex BuildPatternRegex(string pattern)
    {
        var escaped = Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".");
        return new Regex($"^{escaped}$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    }
}
