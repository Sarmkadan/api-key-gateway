// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using ApiKeyGateway.Caching;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Extension methods for <see cref="CacheKeyGeneratorTests"/> that provide
/// reusable assertions and helper methods for testing cache key generation.
/// </summary>
public static class CacheKeyGeneratorTestsExtensions
{
    /// <summary>
    /// Asserts that a cache key follows the expected format pattern for API keys.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="apiKey">The API key being tested.</param>
    /// <param name="expectedKey">The expected cache key string.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
    public static void ShouldHaveApiKeyFormat(this CacheKeyGeneratorTests test, string apiKey, string expectedKey)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(apiKey);
        ArgumentNullException.ThrowIfNull(expectedKey);

        var actualKey = CacheKeyGenerator.GetApiKeyKey(apiKey);
        actualKey.Should().Be(expectedKey, "API key cache key should follow expected format");
    }

    /// <summary>
    /// Asserts that a cache key follows the expected format pattern for API key metadata.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="apiKey">The API key being tested.</param>
    /// <param name="expectedKey">The expected cache key string.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
    public static void ShouldHaveApiKeyMetadataFormat(this CacheKeyGeneratorTests test, string apiKey, string expectedKey)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(apiKey);
        ArgumentNullException.ThrowIfNull(expectedKey);

        var actualKey = CacheKeyGenerator.GetApiKeyMetadataKey(apiKey);
        actualKey.Should().Be(expectedKey, "API key metadata cache key should follow expected format");
    }

    /// <summary>
    /// Asserts that a rate limit cache key includes the expected components.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="apiKey">The API key being tested.</param>
    /// <param name="endpoint">The endpoint path, or null for wildcard.</param>
    /// <param name="expectedKey">The expected cache key string.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> or <paramref name="apiKey"/> is null.</exception>
    public static void ShouldHaveRateLimitKey(this CacheKeyGeneratorTests test, string apiKey, string? endpoint, string expectedKey)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(apiKey);

        var actualKey = endpoint is null
            ? CacheKeyGenerator.GetRateLimitKey(apiKey)
            : CacheKeyGenerator.GetRateLimitKey(apiKey, endpoint);

        actualKey.Should().Be(expectedKey, "rate limit cache key should include expected components");
    }

    /// <summary>
    /// Asserts that a usage statistics cache key formats the date correctly.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="apiKey">The API key being tested.</param>
    /// <param name="date">The date to format.</param>
    /// <param name="expectedKey">The expected cache key string.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
    public static void ShouldHaveUsageStatsKey(this CacheKeyGeneratorTests test, string apiKey, DateTime date, string expectedKey)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(apiKey);

        var actualKey = CacheKeyGenerator.GetUsageStatsKey(apiKey, date);
        actualKey.Should().Be(expectedKey, "usage stats cache key should format date correctly");
    }

    /// <summary>
    /// Asserts that a quota cache key follows the expected format pattern.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="apiKey">The API key being tested.</param>
    /// <param name="expectedKey">The expected cache key string.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> or <paramref name="apiKey"/> is null.</exception>
    public static void ShouldHaveQuotaKey(this CacheKeyGeneratorTests test, string apiKey, string expectedKey)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(apiKey);
        ArgumentNullException.ThrowIfNull(expectedKey);

        var actualKey = CacheKeyGenerator.GetQuotaKey(apiKey);
        actualKey.Should().Be(expectedKey, "quota cache key should follow expected format");
    }

    /// <summary>
    /// Asserts that a webhook delivery cache key uses the expected GUID format.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="eventId">The event GUID.</param>
    /// <param name="expectedKey">The expected cache key string.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
    public static void ShouldHaveWebhookDeliveryKey(this CacheKeyGeneratorTests test, Guid eventId, string expectedKey)
    {
        ArgumentNullException.ThrowIfNull(test);

        var actualKey = CacheKeyGenerator.GetWebhookDeliveryKey(eventId);
        actualKey.Should().Be(expectedKey, "webhook delivery cache key should use GUID in key");
    }

    /// <summary>
    /// Asserts that an external API cache key follows the expected format.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="provider">The external API provider name.</param>
    /// <param name="endpoint">The API endpoint path.</param>
    /// <param name="parameters">Optional query parameters.</param>
    /// <param name="expectedKey">The expected cache key string.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/>, <paramref name="provider"/>, or <paramref name="endpoint"/> is null.</exception>
    public static void ShouldHaveExternalApiCacheKey(
        this CacheKeyGeneratorTests test,
        string provider,
        string endpoint,
        Dictionary<string, string>? parameters,
        string expectedKey)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(endpoint);

        var actualKey = parameters is null
            ? CacheKeyGenerator.GetExternalApiCacheKey(provider, endpoint)
            : CacheKeyGenerator.GetExternalApiCacheKey(provider, endpoint, parameters);

        actualKey.Should().Be(expectedKey, "external API cache key should follow expected format");
    }

    /// <summary>
    /// Asserts that an external API cache key includes a hash when parameters are provided.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="key">The actual cache key to verify.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> or <paramref name="key"/> is null.</exception>
    public static void ShouldIncludeHash(this CacheKeyGeneratorTests test, string key)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(key);

        key.Should().Contain(":", "cache key should contain segments separated by colons");
        var segments = key.Split(':');
        segments.Should().HaveCountGreaterThan(4, "cache key with parameters should have hash segment");
        segments[^1].Should().Match("*[0-9a-f]*", "last segment should be a hexadecimal hash");
    }

    /// <summary>
    /// Asserts that two cache keys are identical regardless of parameter dictionary order.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="key1">The first cache key.</param>
    /// <param name="key2">The second cache key.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/>, <paramref name="key1"/>, or <paramref name="key2"/> is null.</exception>
    public static void ShouldBeHashOrderInvariant(this CacheKeyGeneratorTests test, string key1, string key2)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(key1);
        ArgumentNullException.ThrowIfNull(key2);

        key1.Should().Be(key2, "cache keys should be identical regardless of parameter dictionary order");
    }

    /// <summary>
    /// Asserts that a cache key follows the expected format pattern for API key invalidation.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="apiKey">The API key being tested.</param>
    /// <param name="expectedPattern">The expected cache key pattern with wildcards.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> or <paramref name="apiKey"/> is null.</exception>
    public static void ShouldHaveApiKeyInvalidationPattern(this CacheKeyGeneratorTests test, string apiKey, string expectedPattern)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(apiKey);
        ArgumentNullException.ThrowIfNull(expectedPattern);

        var actualPattern = CacheKeyGenerator.GetApiKeyInvalidationPattern(apiKey);
        actualPattern.Should().Be(expectedPattern, "API key invalidation pattern should include wildcards");
    }

    /// <summary>
    /// Asserts that a rate limit invalidation pattern matches all rate limit keys.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="expectedPattern">The expected cache key pattern.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
    public static void ShouldHaveRateLimitInvalidationPattern(this CacheKeyGeneratorTests test, string expectedPattern)
    {
        ArgumentNullException.ThrowIfNull(test);

        var actualPattern = CacheKeyGenerator.GetRateLimitInvalidationPattern();
        actualPattern.Should().Be(expectedPattern, "rate limit invalidation pattern should match all rate limit keys");
    }

    /// <summary>
    /// Creates a dictionary of query parameters for testing external API cache keys.
    /// </summary>
    /// <param name="parameters">Key-value pairs to include in the dictionary.</param>
    /// <returns>An <see cref="IReadOnlyDictionary{TKey,TValue}"/> for testing.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameters"/> is null.</exception>
    public static IReadOnlyDictionary<string, string> CreateParameterDictionary(this CacheKeyGeneratorTests test, params (string Key, string Value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(parameters);

        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in parameters)
        {
            dict[key] = value;
        }

        return dict.AsReadOnly();
    }

    /// <summary>
    /// Creates a date for testing usage statistics cache keys.
    /// </summary>
    /// <param name="year">The year component.</param>
    /// <param name="month">The month component (1-12).</param>
    /// <param name="day">The day component (1-31).</param>
    /// <returns>A <see cref="DateTime"/> for testing.</returns>
    public static DateTime CreateDate(this CacheKeyGeneratorTests test, int year, int month, int day)
    {
        ArgumentNullException.ThrowIfNull(test);
        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
    }
}