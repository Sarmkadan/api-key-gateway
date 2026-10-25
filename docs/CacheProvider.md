# CacheProvider

`src/ApiKeyGateway/Caching/CacheProvider.cs` contains the cache abstraction used across the gateway and its default implementation:

- `ICacheProvider` - the interface that calling code depends on.
- `InMemoryCacheProvider` - a sealed implementation backed by `IMemoryCache`, intended for single-instance deployments.

Callers such as `ExternalApiClient` (response caching), `HealthController` (cache round-trip check), and `RateLimitResetScheduler` (pattern-based cleanup) take an `ICacheProvider` through constructor injection, so a distributed backend (for example Redis) can be added later without changing them.

All operations return `ValueTask`. `InMemoryCacheProvider` completes synchronously, so no `Task` state machine is allocated per call.

## Registration

`Configuration/CachingConfiguration.cs` registers the provider:

```csharp
services.AddMemoryCache(options =>
{
    options.SizeLimit = 100 * 1024 * 1024; // 100 MB max
});

services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();
```

`InMemoryCacheProvider` is a singleton, so one instance serves the whole process. Its counters and tracked keys live for the lifetime of that instance.

## ICacheProvider

### GetAsync\<T>

```csharp
ValueTask<T?> GetAsync<T>(string key) where T : class
```

Returns the value stored under `key`, or `null` if there is no entry. Logs `Cache HIT` or `Cache MISS` at Debug level.

Values set through `SetAsync` are read here. Counters written by `IncrementAsync` are not (see [Counters and cache entries are separate](#counters-and-cache-entries-are-separate)).

### SetAsync\<T>

```csharp
ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
```

Stores `value` under `key`, replacing any existing entry.

- If `expiration` has a value, it is an **absolute** expiration relative to now. Sliding expiration is not used, so a frequently read entry still expires at the deadline. This matters for revoked API keys and stale upstream responses.
- If `expiration` is `null`, no expiration is set on the entry. It stays until it is removed, replaced, or evicted under memory pressure.

### RemoveAsync

```csharp
ValueTask RemoveAsync(string key)
```

Removes the entry and its tracking record. Removing a key that does not exist is a no-op.

### ExistsAsync

```csharp
ValueTask<bool> ExistsAsync(string key)
```

Returns `true` if an entry (set through `SetAsync`) exists for `key`. Counter keys from `IncrementAsync` are not checked.

### IncrementAsync

```csharp
ValueTask<long> IncrementAsync(string key, long increment = 1)
```

Atomically adds `increment` to the counter for `key` and returns the new value. A missing counter starts at `0`. Negative values decrement. The operation uses `ConcurrentDictionary.AddOrUpdate`, so concurrent callers never lose an update.

Counters have no expiration. Use `RemoveAsync` or `RemoveByPatternAsync` to reset one.

### RemoveByPatternAsync

```csharp
ValueTask<int> RemoveByPatternAsync(string pattern)
```

Removes every tracked cache entry and every counter whose key matches `pattern`, and returns how many were removed. A null, empty, or whitespace pattern removes nothing and returns `0`.

Patterns are glob-style:

| Token | Meaning |
|-------|---------|
| `*`   | any run of characters (including none) |
| `?`   | exactly one character |

All other characters are literal. The whole key must match (the pattern is anchored at both ends). Matching uses a regular expression with a 1-second timeout; a pattern that exceeds it throws `RegexMatchTimeoutException`. The provider scans all tracked keys on every call, so the cost grows with the number of entries. Use it sparingly.

## InMemoryCacheProvider

```csharp
public sealed class InMemoryCacheProvider : ICacheProvider, IDisposable
```

### Constructor

```csharp
public InMemoryCacheProvider(IMemoryCache cache, ILogger<InMemoryCacheProvider> logger)
```

- `cache` - the `IMemoryCache` that stores values. Registered by `AddMemoryCache`.
- `logger` - used for Debug-level diagnostics.

Both parameters are guarded with `ArgumentNullException.ThrowIfNull`.

### Tracking

The provider records every key written through `SetAsync` in an internal set. A post-eviction callback removes a key from that set when the entry expires or is removed. Replacing an entry keeps the key tracked, because the key still holds a live value. `RemoveByPatternAsync` only scans this set, so it never has to enumerate `IMemoryCache` directly.

### Dispose

```csharp
public void Dispose()
```

Clears the internal counters and tracked-key set. It does **not** dispose the `IMemoryCache` passed to the constructor, which belongs to the container. After `Dispose`, counters start again from zero and `RemoveByPatternAsync` no longer finds previously set entries (their values remain in `IMemoryCache` until they expire).

## Counters and cache entries are separate

`IncrementAsync` writes to a separate counter dictionary, not to `IMemoryCache`. As a result:

- `GetAsync` and `ExistsAsync` do not see counter keys.
- `SetAsync` on a key that is also a counter does not change the counter.
- `RemoveAsync` removes only the cache entry, not the counter. Use `RemoveByPatternAsync` with the exact key to remove both.

Keep counter keys and value keys apart in naming.

## Usage

### Cache an expensive lookup with a TTL

```csharp
public async Task<ApiKey?> GetKeyAsync(string keyId, CancellationToken cancellationToken)
{
    var cacheKey = CacheKeyGenerator.GetApiKeyKey(keyId);

    var cached = await _cache.GetAsync<ApiKey>(cacheKey);
    if (cached is not null)
    {
        return cached;
    }

    // Load from the database (placeholder for the caller's own data access)
    var key = await LoadFromDatabaseAsync(keyId, cancellationToken);
    if (key is not null)
    {
        await _cache.SetAsync(cacheKey, key, TimeSpan.FromMinutes(5));
    }

    return key;
}
```

Use a short absolute TTL for data that can change (revocations, upstream responses) so a stale entry expires without relying on eviction.

### Count events in a window

```csharp
// Illustrative key; use the CacheKeyGenerator helpers for real keys
var counterKey = $"ratelimit:{apiKeyId}:{windowStart:yyyyMMddHHmm}";
var count = await _cache.IncrementAsync(counterKey);
```

Read the count from the return value of `IncrementAsync`. Do not call `GetAsync` on the same key.

### Clear a group of keys

```csharp
var removed = await _cache.RemoveByPatternAsync(
    CacheKeyGenerator.GetRateLimitInvalidationPattern());
```

`RateLimitResetScheduler` uses this to remove rate-limit entries and counters that match a pattern. Build patterns with the `CacheKeyGenerator` helpers so the key format stays in one place.

## Behavior summary

| Member | Stores in | Expiration | Visible to `GetAsync` / `ExistsAsync` | Affected by `RemoveByPatternAsync` |
|--------|-----------|------------|----------------------------------------|-------------------------------------|
| `SetAsync` | `IMemoryCache` | absolute TTL or none | yes | yes (while tracked) |
| `IncrementAsync` | counter dictionary | none | no | yes |

## Related

- `ICacheProvider` and `InMemoryCacheProvider` - `Caching/CacheProvider.cs`
- `CachingConfiguration` - `Configuration/CachingConfiguration.cs`
- `CacheKeyGenerator` - `Caching/CacheKeyGenerator.cs`
- Architecture overview - [architecture.md](architecture.md)
