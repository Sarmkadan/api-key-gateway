# CacheKeyGenerator
The `CacheKeyGenerator` type provides a set of static methods for generating cache keys and invalidation patterns for various entities in the `api-key-gateway` project, such as API keys, rate limits, usage statistics, quotas, webhooks, and external API cache. These methods enable consistent and efficient caching and invalidation of cached data across the application.

## API
The `CacheKeyGenerator` type exposes the following public static members:
* `GetApiKeyKey`: Returns a cache key for an API key. Parameters: none. Return value: a string representing the cache key. Throws: none.
* `GetApiKeyMetadataKey`: Returns a cache key for API key metadata. Parameters: none. Return value: a string representing the cache key. Throws: none.
* `GetRateLimitKey`: Returns a cache key for a rate limit. Parameters: none. Return value: a string representing the cache key. Throws: none.
* `GetUsageStatsKey`: Returns a cache key for usage statistics. Parameters: none. Return value: a string representing the cache key. Throws: none.
* `GetQuotaKey`: Returns a cache key for a quota. Parameters: none. Return value: a string representing the cache key. Throws: none.
* `GetWebhookDeliveryKey`: Returns a cache key for a webhook delivery. Parameters: none. Return value: a string representing the cache key. Throws: none.
* `GetExternalApiCacheKey`: Returns a cache key for an external API cache. Parameters: none. Return value: a string representing the cache key. Throws: none.
* `GetApiKeyInvalidationPattern`: Returns an invalidation pattern for API keys. Parameters: none. Return value: a string representing the invalidation pattern. Throws: none.
* `GetRateLimitInvalidationPattern`: Returns an invalidation pattern for rate limits. Parameters: none. Return value: a string representing the invalidation pattern. Throws: none.

## Usage
The following examples demonstrate how to use the `CacheKeyGenerator` type:
```csharp
// Example 1: Generate cache keys for API key and rate limit
string apiKeyKey = CacheKeyGenerator.GetApiKeyKey;
string rateLimitKey = CacheKeyGenerator.GetRateLimitKey;

// Example 2: Use cache keys to store and retrieve cached data
IDbConnection connection = /* obtain a database connection */;
string apiKey = /* retrieve an API key */;
string rateLimit = /* retrieve a rate limit */;

// Store cached data
connection.Execute("INSERT INTO Cache (Key, Value) VALUES (@Key, @Value)", new { Key = apiKeyKey, Value = apiKey });
connection.Execute("INSERT INTO Cache (Key, Value) VALUES (@Key, @Value)", new { Key = rateLimitKey, Value = rateLimit });

// Retrieve cached data
string cachedApiKey = connection.Query<string>("SELECT Value FROM Cache WHERE Key = @Key", new { Key = apiKeyKey }).FirstOrDefault();
string cachedRateLimit = connection.Query<string>("SELECT Value FROM Cache WHERE Key = @Key", new { Key = rateLimitKey }).FirstOrDefault();
```

## Notes
The `CacheKeyGenerator` type is designed to provide a centralized and consistent way of generating cache keys and invalidation patterns. Since all methods are static, they can be safely accessed from multiple threads without fear of concurrency issues. However, it is essential to note that the generated cache keys and invalidation patterns are based on the internal implementation of the `CacheKeyGenerator` type and may change in future versions. Therefore, it is recommended to use the provided methods to generate cache keys and invalidation patterns instead of hardcoding them. Additionally, the `CacheKeyGenerator` type does not provide any caching mechanism itself; it only generates cache keys and invalidation patterns that can be used with a caching library or framework.
