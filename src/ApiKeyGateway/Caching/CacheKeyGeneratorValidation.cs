// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Caching;

/// <summary>
/// Provides validation helpers for <see cref="CacheKeyGenerator"/> cache key generation methods.
/// Ensures that parameters passed to cache key generation methods are valid
/// before they are used to create cache keys.
/// </summary>
public static class CacheKeyGeneratorValidation
{
    /// <summary>
    /// Validates the parameters of <see cref="CacheKeyGenerator.GetApiKeyKey"/>,
    /// <see cref="CacheKeyGenerator.GetApiKeyMetadataKey"/>,
    /// <see cref="CacheKeyGenerator.GetQuotaKey"/>,
    /// and <see cref="CacheKeyGenerator.GetApiKeyInvalidationPattern"/>.
    /// </summary>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiKeyId"/> is null.</exception>
    public static IReadOnlyList<string> Validate(string apiKeyId)
    {
        ArgumentNullException.ThrowIfNull(apiKeyId);

        var problems = new List<string>();
        if (string.IsNullOrEmpty(apiKeyId))
        {
            problems.Add("ApiKeyId cannot be null or empty.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters of <see cref="CacheKeyGenerator.GetRateLimitKey"/>.
    /// </summary>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <param name="endpoint">The endpoint being rate limited; defaults to "*".</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiKeyId"/> is null.</exception>
    public static IReadOnlyList<string> Validate(string apiKeyId, string endpoint = "*")
    {
        ArgumentNullException.ThrowIfNull(apiKeyId);

        var problems = new List<string>();
        if (string.IsNullOrEmpty(apiKeyId))
        {
            problems.Add("ApiKeyId cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(endpoint))
        {
            problems.Add("Endpoint cannot be null or empty.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters of <see cref="CacheKeyGenerator.GetUsageStatsKey"/>.
    /// </summary>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <param name="date">The date for usage statistics; must not be default.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiKeyId"/> is null.</exception>
    public static IReadOnlyList<string> Validate(string apiKeyId, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(apiKeyId);

        var problems = new List<string>();
        if (string.IsNullOrEmpty(apiKeyId))
        {
            problems.Add("ApiKeyId cannot be null or empty.");
        }

        if (date == default)
        {
            problems.Add("Date cannot be default (Unix epoch).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters of <see cref="CacheKeyGenerator.GetWebhookDeliveryKey"/>.
    /// </summary>
    /// <param name="eventId">The event identifier.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="eventId"/> is <see cref="Guid.Empty"/>.</exception>
public static IReadOnlyList<string> Validate(Guid eventId)
{
    ArgumentOutOfRangeException.ThrowIfEqual(eventId, Guid.Empty);
    return Array.Empty<string>();
}

    /// <summary>
    /// Validates the parameters of <see cref="CacheKeyGenerator.GetExternalApiCacheKey"/>.
    /// </summary>
    /// <param name="apiName">The name of the external API.</param>
    /// <param name="endpoint">The API endpoint being called.</param>
    /// <param name="parameters">Optional query parameters for the API call.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiName"/> or <paramref name="endpoint"/> is null.</exception>
    public static IReadOnlyList<string> Validate(string apiName, string endpoint, Dictionary<string, string>? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(apiName);
        ArgumentNullException.ThrowIfNull(endpoint);

        var problems = new List<string>();
        if (string.IsNullOrEmpty(apiName))
        {
            problems.Add("ApiName cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(endpoint))
        {
            problems.Add("Endpoint cannot be null or empty.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters of <see cref="CacheKeyGenerator.GetRateLimitInvalidationPattern"/>.
    /// </summary>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate()
    {
        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if the specified parameters are valid for <see cref="CacheKeyGenerator.GetApiKeyKey"/>,
    /// <see cref="CacheKeyGenerator.GetApiKeyMetadataKey"/>,
    /// <see cref="CacheKeyGenerator.GetQuotaKey"/>,
    /// or <see cref="CacheKeyGenerator.GetApiKeyInvalidationPattern"/>.
    /// </summary>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiKeyId"/> is null.</exception>
    public static bool IsValid(string apiKeyId) => Validate(apiKeyId).Count == 0;

    /// <summary>
    /// Checks if the specified parameters are valid for <see cref="CacheKeyGenerator.GetRateLimitKey"/>.
    /// </summary>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <param name="endpoint">The endpoint being rate limited.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiKeyId"/> is null.</exception>
    public static bool IsValid(string apiKeyId, string endpoint = "*") => Validate(apiKeyId, endpoint).Count == 0;

    /// <summary>
    /// Checks if the specified parameters are valid for <see cref="CacheKeyGenerator.GetUsageStatsKey"/>.
    /// </summary>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <param name="date">The date for usage statistics.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiKeyId"/> is null.</exception>
    public static bool IsValid(string apiKeyId, DateTime date) => Validate(apiKeyId, date).Count == 0;

    /// <summary>
    /// Checks if the specified parameters are valid for <see cref="CacheKeyGenerator.GetWebhookDeliveryKey"/>.
    /// </summary>
    /// <param name="eventId">The event identifier.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="eventId"/> is <see cref="Guid.Empty"/>.</exception>
    public static bool IsValid(Guid eventId) => Validate(eventId).Count == 0;

    /// <summary>
    /// Checks if the specified parameters are valid for <see cref="CacheKeyGenerator.GetExternalApiCacheKey"/>.
    /// </summary>
    /// <param name="apiName">The name of the external API.</param>
    /// <param name="endpoint">The API endpoint being called.</param>
    /// <param name="parameters">Optional query parameters for the API call.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiName"/> or <paramref name="endpoint"/> is null.</exception>
    public static bool IsValid(string apiName, string endpoint, Dictionary<string, string>? parameters = null)
        => Validate(apiName, endpoint, parameters).Count == 0;

    /// <summary>
    /// Checks if the parameters are valid for <see cref="CacheKeyGenerator.GetRateLimitInvalidationPattern"/>.
    /// </summary>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid() => Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified parameters are valid for <see cref="CacheKeyGenerator.GetApiKeyKey"/>,
    /// <see cref="CacheKeyGenerator.GetApiKeyMetadataKey"/>,
    /// <see cref="CacheKeyGenerator.GetQuotaKey"/>,
    /// or <see cref="CacheKeyGenerator.GetApiKeyInvalidationPattern"/>,
    /// throwing an <see cref="ArgumentException"/> if any validation problem is found.
    /// </summary>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiKeyId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public static void EnsureValid(string apiKeyId)
    {
        var problems = Validate(apiKeyId);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that the specified parameters are valid for <see cref="CacheKeyGenerator.GetRateLimitKey"/>,
    /// throwing an <see cref="ArgumentException"/> if any validation problem is found.
    /// </summary>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <param name="endpoint">The endpoint being rate limited.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiKeyId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public static void EnsureValid(string apiKeyId, string endpoint = "*")
    {
        var problems = Validate(apiKeyId, endpoint);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that the specified parameters are valid for <see cref="CacheKeyGenerator.GetUsageStatsKey"/>,
    /// throwing an <see cref="ArgumentException"/> if any validation problem is found.
    /// </summary>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <param name="date">The date for usage statistics.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiKeyId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public static void EnsureValid(string apiKeyId, DateTime date)
    {
        var problems = Validate(apiKeyId, date);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that the specified parameters are valid for <see cref="CacheKeyGenerator.GetWebhookDeliveryKey"/>,
    /// throwing an <see cref="ArgumentException"/> if any validation problem is found.
    /// </summary>
    /// <param name="eventId">The event identifier.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="eventId"/> is <see cref="Guid.Empty"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public static void EnsureValid(Guid eventId)
    {
        var problems = Validate(eventId);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that the specified parameters are valid for <see cref="CacheKeyGenerator.GetExternalApiCacheKey"/>,
    /// throwing an <see cref="ArgumentException"/> if any validation problem is found.
    /// </summary>
    /// <param name="apiName">The name of the external API.</param>
    /// <param name="endpoint">The API endpoint being called.</param>
    /// <param name="parameters">Optional query parameters for the API call.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiName"/> or <paramref name="endpoint"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public static void EnsureValid(string apiName, string endpoint, Dictionary<string, string>? parameters = null)
    {
        var problems = Validate(apiName, endpoint, parameters);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that the parameters are valid for <see cref="CacheKeyGenerator.GetRateLimitInvalidationPattern"/>,
    /// throwing an <see cref="ArgumentException"/> if any validation problem is found.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public static void EnsureValid()
    {
        var problems = Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }
}