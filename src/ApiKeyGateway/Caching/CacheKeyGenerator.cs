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

    /// <summary>
    /// Generates cache key for an API key entity.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>Cache key string for the API key.</returns>
    public static string GetApiKeyKey(string apiKeyId)
    {
        int idLength = apiKeyId.Length;
        return string.Create(13 + idLength, apiKeyId, (span, id) =>
        {
            "apigw:apikey:".AsSpan().CopyTo(span);
            id.AsSpan().CopyTo(span.Slice(13));
        });
    }

    /// <summary>
    /// Generates cache key for API key permissions/metadata.
    /// Separate from actual key for granular invalidation.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>Cache key string for the API key metadata.</returns>
    public static string GetApiKeyMetadataKey(string apiKeyId)
    {
        int idLength = apiKeyId.Length;
        return string.Create(18 + idLength, apiKeyId, (span, id) =>
        {
            "apigw:apikey_meta:".AsSpan().CopyTo(span);
            id.AsSpan().CopyTo(span.Slice(18));
        });
    }

    /// <summary>
    /// Generates cache key for rate limit tracking.
    /// Used to store current usage within a time window.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <param name="endpoint">The API endpoint being rate limited (default: "*").</param>
    /// <returns>Cache key string for the rate limit.</returns>
    public static string GetRateLimitKey(string apiKeyId, string endpoint = "*")
    {
        int idLength = apiKeyId.Length;
        int endpointLength = endpoint.Length;
        int totalLength = 17 + idLength + 1 + endpointLength; // "apigw:ratelimit:" + id + ":" + endpoint
        return string.Create(totalLength, (apiKeyId, endpoint), (span, state) =>
        {
            var (id, ep) = state;
            "apigw:ratelimit:".AsSpan().CopyTo(span);
            id.AsSpan().CopyTo(span.Slice(17));
            span[17 + idLength] = ':';
            ep.AsSpan().CopyTo(span.Slice(18 + idLength));
        });
    }

    /// <summary>
    /// Generates cache key for usage statistics.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <param name="date">The date for the usage statistics.</param>
    /// <returns>Cache key string for the usage statistics.</returns>
    public static string GetUsageStatsKey(string apiKeyId, DateTime date)
    {
        int idLength = apiKeyId.Length;
        string dateStr = date.ToString("yyyy-MM-dd");
        int dateLength = dateStr.Length;
        int totalLength = 14 + idLength + 1 + dateLength; // "apigw:usage:" + id + ":" + date
        return string.Create(totalLength, (apiKeyId, dateStr), (span, state) =>
        {
            var (id, dt) = state;
            "apigw:usage:".AsSpan().CopyTo(span);
            id.AsSpan().CopyTo(span.Slice(14));
            span[14 + idLength] = ':';
            dt.AsSpan().CopyTo(span.Slice(15 + idLength));
        });
    }

    /// <summary>
    /// Generates cache key for quota limits.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>Cache key string for the quota.</returns>
    public static string GetQuotaKey(string apiKeyId)
    {
        int idLength = apiKeyId.Length;
        return string.Create(13 + idLength, apiKeyId, (span, id) =>
        {
            "apigw:quota:".AsSpan().CopyTo(span);
            id.AsSpan().CopyTo(span.Slice(13));
        });
    }

    /// <summary>
    /// Generates cache key for webhook delivery status.
    /// Prevents duplicate webhook deliveries.
    /// </summary>
    /// <param name="eventId">The event ID for the webhook.</param>
    /// <returns>Cache key string for the webhook delivery status.</returns>
    public static string GetWebhookDeliveryKey(Guid eventId)
    {
        string guidStr = eventId.ToString();
        int guidLength = guidStr.Length;
        return string.Create(27 + guidLength, guidStr, (span, id) =>
        {
            "apigw:webhook:delivery:".AsSpan().CopyTo(span);
            id.AsSpan().CopyTo(span.Slice(27));
        });
    }

    /// <summary>
    /// Generates cache key for external API responses.
    /// Used for caching third-party API calls with TTL.
    /// </summary>
    /// <param name="apiName">Name of the external API.</param>
    /// <param name="endpoint">API endpoint path.</param>
    /// <param name="parameters">Optional query parameters for cache key differentiation.</param>
    /// <returns>Cache key string for the external API response.</returns>
    public static string GetExternalApiCacheKey(string apiName, string endpoint, Dictionary<string, string>? parameters = null)
    {
        if (parameters?.Count > 0)
        {
            var paramHash = ComputeParameterHash(parameters);
            int apiLength = apiName.Length;
            int endpointLength = endpoint.Length;
            int hashLength = paramHash.Length;
            int totalLength = 18 + apiLength + 1 + endpointLength + 1 + hashLength; // "apigw:external:" + apiName + ":" + endpoint + ":" + paramHash
            return string.Create(totalLength, (apiName, endpoint, paramHash), (span, state) =>
            {
                var (api, ep, hash) = state;
                "apigw:external:".AsSpan().CopyTo(span);
                api.AsSpan().CopyTo(span.Slice(18));
                span[18 + apiLength] = ':';
                ep.AsSpan().CopyTo(span.Slice(19 + apiLength));
                span[19 + apiLength + endpointLength] = ':';
                hash.AsSpan().CopyTo(span.Slice(20 + apiLength + endpointLength));
            });
        }
        else
        {
            int apiLength = apiName.Length;
            int endpointLength = endpoint.Length;
            int totalLength = 18 + apiLength + 1 + endpointLength; // "apigw:external:" + apiName + ":" + endpoint
            return string.Create(totalLength, (apiName, endpoint), (span, state) =>
            {
                var (api, ep) = state;
                "apigw:external:".AsSpan().CopyTo(span);
                api.AsSpan().CopyTo(span.Slice(18));
                span[18 + apiLength] = ':';
                ep.AsSpan().CopyTo(span.Slice(19 + apiLength));
            });
        }
    }

    /// <summary>
    /// Generates pattern for invalidating all cache entries for an API key.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>Cache key pattern string for invalidation.</returns>
    public static string GetApiKeyInvalidationPattern(string apiKeyId)
    {
        int idLength = apiKeyId.Length;
        return string.Create(12 + idLength + 2, apiKeyId, (span, id) =>
        {
            "apigw:*:".AsSpan().CopyTo(span);
            id.AsSpan().CopyTo(span.Slice(12));
            span[12 + idLength] = ':';
            "*".AsSpan().CopyTo(span.Slice(13 + idLength));
        });
    }

    /// <summary>
    /// Generates pattern for invalidating all rate limit entries.
    /// </summary>
    /// <returns>Cache key pattern string for rate limit invalidation.</returns>
    public static string GetRateLimitInvalidationPattern() =>
        "apigw:ratelimit:*";

    /// <summary>
    /// Creates a hash of parameters for consistent cache key generation.
    /// Order-independent so different parameter orders hash to same value.
    /// Uses ArrayPool for the char and byte encoding buffers to avoid
    /// creating an intermediate List<string>, a joined string, and a
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
            bufLen += k.Length + parameters[k].Length + 2; // "k=v&" worst case

        char[] charBuf = ArrayPool<char>.Shared.Rent(bufLen);
        byte[]? byteBuf = null;
        try
        {
            int pos = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (i > 0) charBuf[pos++] = '&';
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
            return Convert.ToHexString(hash[..4]); // 4 bytes → 8 uppercase hex chars
        }
        finally
        {
            ArrayPool<char>.Shared.Return(charBuf);
            if (byteBuf != null) ArrayPool<byte>.Shared.Return(byteBuf);
        }
    }
}