# ICacheProvider

The `ICacheProvider` interface abstracts a key‑value cache used by the API‑Key Gateway to store transient data such as API keys, rate‑limit counters, and transformation results. Implementations provide asynchronous operations for reading, writing, and removing cached entries, as well as utility methods for existence checks, atomic increments, and pattern‑based bulk removal.

## API

### InMemoryCacheProvider
**Type:** `InMemoryCacheProvider` (property)  
**Purpose:** Exposes the underlying in‑memory cache instance that backs the provider. This can be used for advanced scenarios requiring direct access to the cache’s internal diagnostics or configuration.  
**Return Value:** The concrete `InMemoryCacheProvider` implementation associated with this interface.  
**Exceptions:** None.

### GetAsync\<T>
**Signature:** `ValueTask<T?> GetAsync<T>(string key)`  
**Purpose:** Retrieves the value associated with *key* from the cache, if present.  
**Parameters:**  
- `key`: The cache key to look up. Must not be `null`.  
**Return Value:** A `ValueTask<T?>` that completes with the cached value cast to `T`, or `null` if the key does not exist or the value cannot be cast to `T`.  
**Exceptions:**  
- `ArgumentNullException` if *key* is `null`.  
- May propagate `OperationCanceledException` if a cancellation token is supplied via overloads (not shown in the base signature).

### SetAsync\<T>
**Signature:** `ValueTask SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null)`  
**Purpose:** Inserts or updates an entry in the cache.  
**Parameters:**  
- `key`: The cache key. Must not be `null`.  
- `value`: The object to store. May be `null` to effectively remove the entry.  
- `absoluteExpirationRelativeToNow` (optional): Relative time after which the entry should be considered expired. If `null`, the entry uses the provider’s default expiration policy.  
**Return Value:** A `ValueTask` that completes when the operation has been persisted to the cache.  
**Exceptions:**  
- `ArgumentNullException` if *key* is `null`.  
- May throw `ArgumentOutOfRangeException` if the supplied expiration is negative.

### RemoveAsync
**Signature:** `ValueTask RemoveAsync(string key)`  
**Purpose:** Removes the entry identified by *key* from the cache, if it exists.  
**Parameters:**  
- `key`: The cache key to remove. Must not be `null`.  
**Return Value:** A `ValueTask` that completes when the removal operation has been processed.  
**Exceptions:**  
- `ArgumentNullException` if *key* is `null`.

### ExistsAsync
**Signature:** `ValueTask<bool> ExistsAsync(string key)`  
**Purpose:** Determines whether an entry with *key* is present in the cache.  
**Parameters:**  
- `key`: The cache key to test. Must not be `null`.  
**Return Value:** A `ValueTask<bool>` that yields `true` if the key exists and its value has not expired; otherwise `false`.  
**Exceptions:**  
- `ArgumentNullException` if *key* is `null`.

### IncrementAsync
**Signature:** `ValueTask<long> IncrementAsync(string key, long delta = 1)`  
**Purpose:** Atomically increments a numeric cache entry by *delta* and returns the updated value. If the entry does not exist, it is treated as zero before applying the increment.  
**Parameters:**  
- `key`: The cache key holding a numeric value. Must not be `null`.  
- `delta`: The amount to add (can be negative to decrement).  
**Return Value:** A `ValueTask<long>` that completes with the new value after the increment operation.  
**Exceptions:**  
- `ArgumentNullException` if *key* is `null`.  
- `InvalidCastException` if the existing value cannot be interpreted as a signed 64‑bit integer.  
- `OverflowException` if the resulting value lies outside the range of `Int64`.

### RemoveByPatternAsync
**Signature:** `ValueTask<int> RemoveByPatternAsync(string pattern)`  
**Purpose:** Removes all cache entries whose keys match the supplied *pattern*. The pattern follows the provider’s wildcard semantics (typically `*` for zero or more characters and `?` for a single character).  
**Parameters:**  
- `pattern`: The wildcard pattern to match keys against. Must not be `null` or empty.  
**Return Value:** A `ValueTask<int>` that completes with the number of entries removed.  
**Exceptions:**  
- `ArgumentNullException` if *pattern* is `null`.  
- `ArgumentException` if *pattern* is empty.

## Usage

### Basic get/set workflow
```csharp
ICacheProvider cache = GetCacheProvider(); // obtained via DI or factory

// Store a value with a sliding expiration of 5 minutes
await cache.SetAsync("apiKey:12345", apiKeyObject, TimeSpan.FromMinutes(5));

// Retrieve the value later
ValueTask<ApiKey?> getTask = cache.GetAsync<ApiKey>("apiKey:12345");
if (await getTask is ApiKey key && key != null)
{
    // Use the key
}
else
{
    // Handle miss (e.g., reload from source)
}
```

### Atomic counter and bulk cleanup
```csharp
// Increment a request counter for a given client
long currentCount = await cache.IncrementAsync("client:42:requests", 1);

// Periodically remove stale cache entries matching a pattern
int removed = await cache.RemoveByPatternAsync("client:*:requests:*");
Logger.LogInformation("Cleared {Removed} stale request counters", removed);
```

## Notes
- All methods are designed to be safe for concurrent use; implementations should guarantee thread‑safety for simultaneous calls.  
- Passing `null` for any string parameter results in an `ArgumentNullException`.  
- The `GetAsync<T>` method returns `null` both when a key is absent and when the cached value cannot be cast to `T`; callers should verify the result type if distinguishing these cases is required.  
- Expiration policies are implementation‑specific; if no explicit expiration is supplied to `SetAsync<T>`, the provider’s default policy (often sliding or absolute) applies.  
- `IncrementAsync` treats a missing key as zero; therefore the first increment on a new key yields the value of `delta`.  
- Pattern‑based removal (`RemoveByPatternAsync`) may be more expensive than individual removals because it requires scanning the internal key set; use it judiciously in performance‑sensitive paths.  
- Implementations may throw `OperationCanceledException` if a cancellation token is supplied via overloads not shown in the base interface definition. Consumers should observe cancellation semantics when applicable.
