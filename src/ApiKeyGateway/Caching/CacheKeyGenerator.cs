// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    /// </summary>
    private static string ComputeParameterHash(Dictionary<string, string> parameters)
    {
        var orderedParams = parameters
            .OrderBy(p => p.Key)
            .Select(p => $"{p.Key}={p.Value}")
            .ToList();

        var combined = string.Join('&', orderedParams);
        var hashBytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(hashBytes)[..8]; // First 8 chars of hash
    }
}
