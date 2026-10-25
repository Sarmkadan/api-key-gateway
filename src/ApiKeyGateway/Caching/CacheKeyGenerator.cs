// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace ApiKeyGateway.Caching;

/// <summary>
/// Generates consistent cache keys across the application.
/// Centralizing key generation prevents cache miss issues from inconsistent naming
/// and makes it easy to change key structure globally.
/// </summary>
public static class CacheKeyGenerator
{
    private const string Prefix = "apigw";
    private const char Separator = ':';

    private const string ApiKeyKeyPrefix = "apigw:apikey:";
    private const string ApiKeyMetadataKeyPrefix = "apigw:apikey_meta:";
    private const string RateLimitKeyPrefix = "apigw:ratelimit:";
    private const string UsageStatsKeyPrefix = "apigw:usage:";
    private const string QuotaKeyPrefix = "apigw:quota:";
    private const string WebhookDeliveryKeyPrefix = "apigw:webhook:delivery:";
    private const string ExternalApiKeyPrefix = "apigw:external:";
    private const string AnyNamespaceWildcardPrefix = "apigw:*:";
    private const string UsageStatsDateFormat = "yyyy-MM-dd";

    // Worst-case delimiters per parameter in the hash input: '=' and '&'.
    private const int ParameterDelimiterChars = 2;

    // Number of SHA-256 bytes kept for the parameter hash (4 bytes → 8 hex chars).
    private const int ParameterHashBytes = 4;

    /// <summary>
    /// Generates cache key for an API key entity.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>Cache key string for the API key.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    public static string GetApiKeyKey(string apiKeyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);
        int idLength = apiKeyId.Length;
        return string.Create(ApiKeyKeyPrefix.Length + idLength, apiKeyId, (span, id) =>
        {
            ApiKeyKeyPrefix.AsSpan().CopyTo(span);
            id.AsSpan().CopyTo(span.Slice(ApiKeyKeyPrefix.Length));
        });
    }

    /// <summary>
    /// Generates cache key for API key permissions/metadata.
    /// Separate from actual key for granular invalidation.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>Cache key string for the API key metadata.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    public static string GetApiKeyMetadataKey(string apiKeyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);
        int idLength = apiKeyId.Length;
        return string.Create(ApiKeyMetadataKeyPrefix.Length + idLength, apiKeyId, (span, id) =>
        {
            ApiKeyMetadataKeyPrefix.AsSpan().CopyTo(span);
            id.AsSpan().CopyTo(span.Slice(ApiKeyMetadataKeyPrefix.Length));
        });
    }

    /// <summary>
    /// Generates cache key for rate limit tracking.
    /// Used to store current usage within a time window.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <param name="endpoint">The API endpoint being rate limited (default: "*").</param>
    /// <returns>Cache key string for the rate limit.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> or <paramref name="endpoint"/> is null or empty.</exception>
    public static string GetRateLimitKey(string apiKeyId, string endpoint = "*")
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        return $"{RateLimitKeyPrefix}{apiKeyId}{Separator}{endpoint}";
    }

    /// <summary>
    /// Generates cache key for usage statistics.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <param name="date">The date for the usage statistics.</param>
    /// <returns>Cache key string for the usage statistics.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    public static string GetUsageStatsKey(string apiKeyId, DateTime date)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);
        return $"{UsageStatsKeyPrefix}{apiKeyId}{Separator}{date.ToString(UsageStatsDateFormat)}";
    }

    /// <summary>
    /// Generates cache key for quota limits.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>Cache key string for the quota.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    public static string GetQuotaKey(string apiKeyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);
        return $"{QuotaKeyPrefix}{apiKeyId}";
    }

    /// <summary>
    /// Generates cache key for webhook delivery status.
    /// Prevents duplicate webhook deliveries.
    /// </summary>
    /// <param name="eventId">The event ID for the webhook.</param>
    /// <returns>Cache key string for the webhook delivery status.</returns>
    public static string GetWebhookDeliveryKey(Guid eventId)
    {
        return $"{WebhookDeliveryKeyPrefix}{eventId}";
    }

    /// <summary>
    /// Generates cache key for external API responses.
    /// Used for caching third-party API calls with TTL.
    /// </summary>
    /// <param name="apiName">Name of the external API.</param>
    /// <param name="endpoint">API endpoint path.</param>
    /// <param name="parameters">Optional query parameters for cache key differentiation.</param>
    /// <returns>Cache key string for the external API response.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiName"/> or <paramref name="endpoint"/> is null or empty.</exception>
    public static string GetExternalApiCacheKey(string apiName, string endpoint, Dictionary<string, string>? parameters = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiName);
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        if (parameters?.Count > 0)
        {
            var paramHash = ComputeParameterHash(parameters);
            return $"{ExternalApiKeyPrefix}{apiName}{Separator}{endpoint}{Separator}{paramHash}";
        }
        else
        {
            return $"{ExternalApiKeyPrefix}{apiName}{Separator}{endpoint}";
        }
    }

    /// <summary>
    /// Generates pattern for invalidating all cache entries for an API key.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>Cache key pattern string for invalidation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    public static string GetApiKeyInvalidationPattern(string apiKeyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);
        return $"{AnyNamespaceWildcardPrefix}{apiKeyId}{Separator}*";
    }

    /// <summary>
    /// Generates pattern for invalidating all rate limit entries.
    /// </summary>
    /// <returns>Cache key pattern string for rate limit invalidation.</returns>
    public static string GetRateLimitInvalidationPattern() =>
        $"{RateLimitKeyPrefix}*";

    /// <summary>
    /// Creates a hash of parameters for consistent cache key generation.
    /// Order-independent so different parameter orders hash to same value.
    /// Uses ArrayPool for the char and byte encoding buffers to avoid
    /// creating an intermediate List&lt;string&gt;, a joined string, and a
    /// heap-allocated byte array on every external-API cache lookup.
    /// </summary>
    /// <param name="parameters">Dictionary of parameters to hash.</param>
    /// <returns>8-character hexadecimal hash string.</returns>
    private static string ComputeParameterHash(Dictionary<string, string> parameters)
    {
        var keys = parameters.Keys.ToArray();
        Array.Sort(keys, StringComparer.Ordinal);

        int bufLen = 0;
        foreach (var k in keys)
        {
            bufLen += k.Length + parameters[k].Length + ParameterDelimiterChars;
        }

        char[] charBuf = ArrayPool<char>.Shared.Rent(bufLen);
        byte[]? byteBuf = null;
        try
        {
            int pos = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (i > 0)
                {
                    charBuf[pos++] = '&';
                }

                string k = keys[i], v = parameters[k];
                k.AsSpan().CopyTo(charBuf.AsSpan(pos));
                pos += k.Length;
                charBuf[pos++] = '=';
                v.AsSpan().CopyTo(charBuf.AsSpan(pos));
                pos += v.Length;
            }

            int maxBytes = Encoding.UTF8.GetMaxByteCount(pos);
            byteBuf = ArrayPool<byte>.Shared.Rent(maxBytes);
            int byteCount = Encoding.UTF8.GetBytes(charBuf.AsSpan(0, pos), byteBuf);

            Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
            SHA256.HashData(byteBuf.AsSpan(0, byteCount), hash);
            return Convert.ToHexString(hash[..ParameterHashBytes]);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(charBuf);
            if (byteBuf != null)
            {
                ArrayPool<byte>.Shared.Return(byteBuf);
            }
        }
    }
}
