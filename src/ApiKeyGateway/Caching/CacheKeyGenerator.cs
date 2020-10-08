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
    public static string GetApiKeyKey(string apiKeyId) =>
        $"{Prefix}{Separator}apikey{Separator}{apiKeyId}";

    /// <summary>
    /// Generates cache key for API key permissions/metadata.
    /// Separate from actual key for granular invalidation.
    /// </summary>
    public static string GetApiKeyMetadataKey(string apiKeyId) =>
        $"{Prefix}{Separator}apikey_meta{Separator}{apiKeyId}";

    /// <summary>
    /// Generates cache key for rate limit tracking.
    /// Used to store current usage within a time window.
    /// </summary>
    public static string GetRateLimitKey(string apiKeyId, string endpoint = "*") =>
        $"{Prefix}{Separator}ratelimit{Separator}{apiKeyId}{Separator}{endpoint}";

    /// <summary>
    /// Generates cache key for usage statistics.
    /// </summary>
    public static string GetUsageStatsKey(string apiKeyId, DateTime date) =>
        $"{Prefix}{Separator}usage{Separator}{apiKeyId}{Separator}{date:yyyy-MM-dd}";

    /// <summary>
    /// Generates cache key for quota limits.
    /// </summary>
    public static string GetQuotaKey(string apiKeyId) =>
        $"{Prefix}{Separator}quota{Separator}{apiKeyId}";

    /// <summary>
    /// Generates cache key for webhook delivery status.
    /// Prevents duplicate webhook deliveries.
    /// </summary>
    public static string GetWebhookDeliveryKey(Guid eventId) =>
        $"{Prefix}{Separator}webhook{Separator}delivery{Separator}{eventId}";

    /// <summary>
    /// Generates cache key for external API responses.
    /// Used for caching third-party API calls with TTL.
    /// </summary>
    public static string GetExternalApiCacheKey(string apiName, string endpoint, Dictionary<string, string>? parameters = null)
    {
        var key = $"{Prefix}{Separator}external{Separator}{apiName}{Separator}{endpoint}";

        if (parameters?.Count > 0)
        {
            // Create deterministic parameter hash for consistent cache keys
            var paramHash = ComputeParameterHash(parameters);
            key += $"{Separator}{paramHash}";
        }

        return key;
    }

    /// <summary>
    /// Generates pattern for invalidating all cache entries for an API key.
    /// </summary>
    public static string GetApiKeyInvalidationPattern(string apiKeyId) =>
        $"{Prefix}{Separator}*{Separator}{apiKeyId}{Separator}*";

    /// <summary>
    /// Generates pattern for invalidating all rate limit entries.
    /// </summary>
    public static string GetRateLimitInvalidationPattern() =>
        $"{Prefix}{Separator}ratelimit{Separator}*";

    /// <summary>
    /// Creates a hash of parameters for consistent cache key generation.
    /// Order-independent so different parameter orders hash to same value.
    /// Uses ArrayPool for the char and byte encoding buffers to avoid
    /// creating an intermediate List&lt;string&gt;, a joined string, and a
    /// heap-allocated byte array on every external-API cache lookup.
    /// </summary>
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
                k.AsSpan().CopyTo(charBuf.AsSpan(pos)); pos += k.Length;
                charBuf[pos++] = '=';
                v.AsSpan().CopyTo(charBuf.AsSpan(pos)); pos += v.Length;
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
