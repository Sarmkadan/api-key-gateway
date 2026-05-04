// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Caching;

/// <summary>
/// Interface for cache abstraction. Implementations can use different
/// backends (in-memory, Redis, Memcached) without changing calling code.
/// This is critical for supporting both single-instance and distributed deployments.
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Retrieves a value from cache by key.
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Stores a value in cache with optional expiration.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Removes a value from cache.
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Checks if a key exists in cache.
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Atomically increments a counter. Used for rate limiting tracking.
    /// </summary>
    Task<long> IncrementAsync(string key, long increment = 1);

    /// <summary>
    /// Removes all entries matching a pattern (use cautiously).
    /// </summary>
    Task<int> RemoveByPatternAsync(string pattern);
}

/// <summary>
/// In-memory cache implementation using MemoryCache.
/// Best for single-instance deployments. Use Redis adapter for clusters.
/// </summary>
public sealed class InMemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InMemoryCacheProvider> _logger;
    private static readonly object _lockObj = new();
    private static readonly Dictionary<string, long> _counters = new();

    public InMemoryCacheProvider(IMemoryCache cache, ILogger<InMemoryCacheProvider> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Cache HIT for key: {Key}", key);
            return Task.FromResult(value);
        }

        _logger.LogDebug("Cache MISS for key: {Key}", key);
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var cacheOptions = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            cacheOptions.SlidingExpiration = expiration;
        }

        _cache.Set(key, value, cacheOptions);
        _logger.LogDebug("Cache SET for key: {Key} with expiration: {Expiration}", key, expiration?.TotalSeconds);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        _logger.LogDebug("Cache REMOVE for key: {Key}", key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key) =>
        Task.FromResult(_cache.TryGetValue(key, out _));

    public Task<long> IncrementAsync(string key, long increment = 1)
    {
        // In-memory implementation needs manual locking for counters
        lock (_lockObj)
        {
            if (!_counters.ContainsKey(key))
            {
                _counters[key] = 0;
            }

            _counters[key] += increment;
            var newValue = _counters[key];
            _logger.LogDebug("Counter incremented: {Key} = {Value}", key, newValue);
            return Task.FromResult(newValue);
        }
    }

    public Task<int> RemoveByPatternAsync(string pattern)
    {
        // In-memory cache doesn't have pattern matching built-in
        // This is a simplified implementation
        var keysToRemove = new List<string>();
        _logger.LogWarning("RemoveByPattern not fully implemented for in-memory cache");
        return Task.FromResult(keysToRemove.Count);
    }
}
