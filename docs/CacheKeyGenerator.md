# CacheKeyGenerator

`CacheKeyGenerator` is a static class in `src/ApiKeyGateway/Caching/CacheKeyGenerator.cs` (namespace `ApiKeyGateway.Caching`). It builds every cache key and invalidation pattern used by the gateway, so the key layout lives in one place. All keys share the `apigw:` prefix and use `:` as the separator.

The class does not read or write a cache. It only produces strings. Pass them to an `ICacheProvider` (see [Usage](#usage)).

## Key formats

| Method | Generated string |
|---|---|
| `GetApiKeyKey(apiKeyId)` | `apigw:apikey:{apiKeyId}` |
| `GetApiKeyMetadataKey(apiKeyId)` | `apigw:apikey_meta:{apiKeyId}` |
| `GetRateLimitKey(apiKeyId, endpoint = "*")` | `apigw:ratelimit:{apiKeyId}:{endpoint}` |
| `GetUsageStatsKey(apiKeyId, date)` | `apigw:usage:{apiKeyId}:{yyyy-MM-dd}` |
| `GetQuotaKey(apiKeyId)` | `apigw:quota:{apiKeyId}` |
| `GetWebhookDeliveryKey(eventId)` | `apigw:webhook:delivery:{eventId}` |
| `GetExternalApiCacheKey(apiName, endpoint, parameters = null)` | `apigw:external:{apiName}:{endpoint}` or `apigw:external:{apiName}:{endpoint}:{hash}` |
| `GetApiKeyInvalidationPattern(apiKeyId)` | `apigw:*:{apiKeyId}:*` |
| `GetRateLimitInvalidationPattern()` | `apigw:ratelimit:*` |

The `*` in the two invalidation patterns is a wildcard, not a literal character. See [Invalidation patterns](#invalidation-patterns).

## Public members

All members are `public static`.

### `GetApiKeyKey(string apiKeyId)`

Returns the cache key for an API key entity. Throws `ArgumentNullException` when `apiKeyId` is `null`, and `ArgumentException` when it is empty.

### `GetApiKeyMetadataKey(string apiKeyId)`

Returns the key for an API key's metadata and permissions. It is kept separate from `GetApiKeyKey` so the two can be invalidated independently. Same argument checks as `GetApiKeyKey`.

### `GetRateLimitKey(string apiKeyId, string endpoint = "*")`

Returns the key that tracks rate-limit usage for an API key on an endpoint. The default is the literal string `"*"`, not a wildcard. Throws the same exceptions as above when either `apiKeyId` or `endpoint` is `null` or empty.

### `GetUsageStatsKey(string apiKeyId, DateTime date)`

Returns the key for one day of usage statistics. Only the calendar date is used; the time part is dropped.

The date is formatted with `DateTime.ToString("yyyy-MM-dd")`, which uses the current culture. Pass dates that are already in the intended calendar and time zone, because no UTC conversion is done here.

### `GetQuotaKey(string apiKeyId)`

Returns the quota key for an API key. Same argument checks as `GetApiKeyKey`.

### `GetWebhookDeliveryKey(Guid eventId)`

Returns the key that records delivery status for a webhook event, which prevents duplicate deliveries. The GUID is formatted with its default `ToString()` form (lowercase, hyphenated). No validation is done.

### `GetExternalApiCacheKey(string apiName, string endpoint, Dictionary<string, string>? parameters = null)`

Returns the cache key for a response from a third-party API.

- Without parameters (`null` or empty), the key is `apigw:external:{apiName}:{endpoint}`.
- With parameters, the key gets an 8-character uppercase hex suffix: `apigw:external:{apiName}:{endpoint}:{hash}`.

The hash is built like this:

1. Sort the parameter names with `StringComparer.Ordinal`.
2. Join them into `k1=v1&k2=v2`, with no escaping.
3. Take the UTF-8 bytes and compute SHA-256.
4. Keep the first 4 bytes and hex-encode them.

Because the input is not escaped and the hash is only 32 bits, two different parameter sets can produce the same key. For example, `{"a": "1&b=2"}` and `{"a": "1", "b": "2"}` both hash `a=1&b=2`. Random collisions are also possible: by the birthday bound, the chance of at least one collision among about 30,000 distinct parameter sets for one API and endpoint is roughly 10%. A null parameter value causes a `NullReferenceException`.

Throws `ArgumentNullException` or `ArgumentException` when `apiName` or `endpoint` is `null` or empty.

### `GetApiKeyInvalidationPattern(string apiKeyId)`

Returns a pattern (`apigw:*:{apiKeyId}:*`) that matches the keys for one API key that have a segment after the ID. See [Invalidation patterns](#invalidation-patterns) for what it does and does not match.

### `GetRateLimitInvalidationPattern()`

Returns `apigw:ratelimit:*`, which matches every rate-limit key. `RateLimitResetScheduler` uses it to clear expired rate-limit entries.

## Invalidation patterns

`CacheProvider.RemoveByPatternAsync` (`src/ApiKeyGateway/Caching/CacheProvider.cs`) turns a pattern into an anchored regular expression. It escapes the pattern, then replaces `\*` with `.*` and `\?` with `.`.

The consequences are:

- `GetApiKeyInvalidationPattern("k1")` (`apigw:*:k1:*`) matches `apigw:ratelimit:k1:*`, `apigw:usage:k1:2026-10-25` and similar keys.
- It does **not** match `apigw:apikey:k1`, `apigw:apikey_meta:k1`, `apigw:quota:k1` or webhook keys. These have no segment after the ID, so they must be removed by exact key with `ICacheProvider.RemoveAsync`.
- The `apiKeyId` is not escaped, so a `*` or `?` inside an ID acts as a wildcard. Validate IDs before you build a pattern from them.

## Usage

Build the key with `CacheKeyGenerator` and pass it to an `ICacheProvider`. `ExternalApiClient` does this for external responses:

```csharp
// ExternalApiClient.GetAsync: key from API name and endpoint, no parameters
var cacheKey = CacheKeyGenerator.GetExternalApiCacheKey(_apiName, endpoint);

var cached = await _cache.GetAsync<T>(cacheKey);
if (cached is null)
{
    // ...call the external API, then store the result with a TTL
    await _cache.SetAsync(cacheKey, response, cacheDuration);
}
```

Invalidate an API key's rate-limit and usage entries in one call, and its other entries by exact key:

```csharp
var removed = await cacheProvider.RemoveByPatternAsync(
    CacheKeyGenerator.GetApiKeyInvalidationPattern(apiKeyId));

await cacheProvider.RemoveAsync(CacheKeyGenerator.GetApiKeyKey(apiKeyId));
await cacheProvider.RemoveAsync(CacheKeyGenerator.GetApiKeyMetadataKey(apiKeyId));
await cacheProvider.RemoveAsync(CacheKeyGenerator.GetQuotaKey(apiKeyId));
```

Clear all expired rate-limit entries, as `RateLimitResetScheduler` does:

```csharp
var cleared = await cacheProvider.RemoveByPatternAsync(
    CacheKeyGenerator.GetRateLimitInvalidationPattern());
```

## Validation

`CacheKeyGeneratorValidation` (`src/ApiKeyGateway/Caching/CacheKeyGeneratorValidation.cs`) is a companion static class. It has `Validate`, `IsValid` and `EnsureValid` overloads for each key type. `Validate` returns a list of problems, `IsValid` returns a `bool`, and `EnsureValid` throws when the input is invalid.

## Notes

- Key formats are part of the cache contract. Changing a prefix or the separator makes existing entries unreachable and breaks invalidation patterns, so change them only on purpose.
- Keys are case-sensitive. The webhook GUID is lowercase, and the parameter hash is uppercase hex.
- Benchmarks in `benchmarks/api-key-gateway.Benchmarks/CacheKeyGenerationBenchmarks.cs` cover the parameterized external-key path. Production code (`ExternalApiClient`) calls the unparameterized form.
