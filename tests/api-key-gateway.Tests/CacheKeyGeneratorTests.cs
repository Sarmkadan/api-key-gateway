using Xunit;
using ApiKeyGateway.Caching;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Tests for the <see cref="CacheKeyGenerator"/> utility class.
/// </summary>
public class CacheKeyGeneratorTests
{
    // -------------------------------------------------------------------------
    // Key format consistency
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="CacheKeyGenerator.GetApiKeyKey(string)"/> returns the expected
    /// key format for a given API key identifier.
    /// </summary>
    [Fact]
    public void GetApiKeyKey_ReturnsExpectedFormat()
    {
        CacheKeyGenerator.GetApiKeyKey("key-001")
            .Should().Be("apigw:apikey:key-001");
    }

    /// <summary>
    /// Verifies that <see cref="CacheKeyGenerator.GetApiKeyMetadataKey(string)"/> returns the
    /// expected key format for a given API key identifier.
    /// </summary>
    [Fact]
    public void GetApiKeyMetadataKey_ReturnsExpectedFormat()
    {
        CacheKeyGenerator.GetApiKeyMetadataKey("key-001")
            .Should().Be("apigw:apikey_meta:key-001");
    }

    /// <summary>
    /// Verifies that <see cref="CacheKeyGenerator.GetRateLimitKey(string)"/> returns a wildcard
    /// endpoint key when no specific endpoint is supplied.
    /// </summary>
    [Fact]
    public void GetRateLimitKey_DefaultEndpoint_ReturnsWildcard()
    {
        CacheKeyGenerator.GetRateLimitKey("key-001")
            .Should().Be("apigw:ratelimit:key-001:*");
    }

    /// <summary>
    /// Verifies that <see cref="CacheKeyGenerator.GetRateLimitKey(string,string)"/> includes the
    /// specified endpoint in the generated key.
    /// </summary>
    [Fact]
    public void GetRateLimitKey_SpecificEndpoint_IncludesEndpoint()
    {
        CacheKeyGenerator.GetRateLimitKey("key-001", "/api/users")
            .Should().Be("apigw:ratelimit:key-001:/api/users");
    }

    /// <summary>
    /// Verifies that <see cref="CacheKeyGenerator.GetUsageStatsKey(string,DateTime)"/> formats
    /// the date component of the key correctly.
    /// </summary>
    [Fact]
    public void GetUsageStatsKey_FormatsDateCorrectly()
    {
        var date = new DateTime(2026, 5, 21);
        CacheKeyGenerator.GetUsageStatsKey("key-001", date)
            .Should().Be("apigw:usage:key-001:2026-05-21");
    }

    /// <summary>
    /// Verifies that <see cref="CacheKeyGenerator.GetQuotaKey(string)"/> returns the expected
    /// quota key format for a given API key identifier.
    /// </summary>
    [Fact]
    public void GetQuotaKey_ReturnsExpectedFormat()
    {
        CacheKeyGenerator.GetQuotaKey("key-001")
            .Should().Be("apigw:quota:key-001");
    }

    /// <summary>
    /// Verifies that <see cref="CacheKeyGenerator.GetWebhookDeliveryKey(Guid)"/> includes the
    /// supplied <see cref="Guid"/> in the generated key.
    /// </summary>
    [Fact]
    public void GetWebhookDeliveryKey_UsesGuidInKey()
    {
        var eventId = Guid.Parse("11223344-5566-7788-99aa-bbccddeeff00");
        CacheKeyGenerator.GetWebhookDeliveryKey(eventId)
            .Should().Be("apigw:webhook:delivery:11223344-5566-7788-99aa-bbccddeeff00");
    }

    // -------------------------------------------------------------------------
    // External API cache key - parameter hash determinism
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="CacheKeyGenerator.GetExternalApiCacheKey(string,string)"/>
    /// omits the hash component when no parameters are supplied.
    /// </summary>
    [Fact]
    public void GetExternalApiCacheKey_NoParameters_OmitsHash()
    {
        var key = CacheKeyGenerator.GetExternalApiCacheKey("stripe", "/charges");
        key.Should().Be("apigw:external:stripe:/charges");
    }

    /// <summary>
    /// Verifies that <see cref="CacheKeyGenerator.GetExternalApiCacheKey(string,string,Dictionary{string,string})"/>
    /// appends a hash derived from the supplied parameters.
    /// </summary>
    [Fact]
    public void GetExternalApiCacheKey_WithParameters_AppendsHash()
    {
        var parameters = new Dictionary<string, string>
        {
            { "currency", "USD" },
            { "amount", "100" }
        };
        var key = CacheKeyGenerator.GetExternalApiCacheKey("stripe", "/charges", parameters);
        key.Should().StartWith("apigw:external:stripe:/charges:");
        key.Split(':').Last().Should().HaveLength(8); // 4 bytes = 8 hex chars
    }

    /// <summary>
    /// Verifies that the order of parameters does not affect the generated hash.
    /// </summary>
    [Fact]
    public void GetExternalApiCacheKey_ParameterOrderDoesNotAffectHash()
    {
        var params1 = new Dictionary<string, string>
        {
            { "a", "1" },
            { "b", "2" },
            { "c", "3" }
        };
        var params2 = new Dictionary<string, string>
        {
            { "c", "3" },
            { "a", "1" },
            { "b", "2" }
        };

        var key1 = CacheKeyGenerator.GetExternalApiCacheKey("api", "/ep", params1);
        var key2 = CacheKeyGenerator.GetExternalApiCacheKey("api", "/ep", params2);

        key1.Should().Be(key2, "parameter order should not affect the cache key hash");
    }

    /// <summary>
    /// Verifies that different parameter values produce distinct cache keys.
    /// </summary>
    [Fact]
    public void GetExternalApiCacheKey_DifferentParameterValues_ProduceDifferentKeys()
    {
        var params1 = new Dictionary<string, string> { { "limit", "10" } };
        var params2 = new Dictionary<string, string> { { "limit", "50" } };

        var key1 = CacheKeyGenerator.GetExternalApiCacheKey("api", "/ep", params1);
        var key2 = CacheKeyGenerator.GetExternalApiCacheKey("api", "/ep", params2);

        key1.Should().NotBe(key2);
    }

    /// <summary>
    /// Verifies that an empty parameter dictionary results in no hash component.
    /// </summary>
    [Fact]
    public void GetExternalApiCacheKey_EmptyParameterDictionary_OmitsHash()
    {
        var key = CacheKeyGenerator.GetExternalApiCacheKey("api", "/ep", new Dictionary<string, string>());
        key.Should().Be("apigw:external:api:/ep");
    }

    // -------------------------------------------------------------------------
    // Invalidation patterns
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="CacheKeyGenerator.GetApiKeyInvalidationPattern(string)"/> returns a pattern
    /// that includes wildcards for all relevant key segments.
    /// </summary>
    [Fact]
    public void GetApiKeyInvalidationPattern_IncludesWildcards()
    {
        CacheKeyGenerator.GetApiKeyInvalidationPattern("key-001")
            .Should().Be("apigw:*:key-001:*");
    }

    /// <summary>
    /// Verifies that <see cref="CacheKeyGenerator.GetRateLimitInvalidationPattern()"/> returns a pattern
    /// that matches all rate limit keys.
    /// </summary>
    [Fact]
    public void GetRateLimitInvalidationPattern_MatchesAllRateLimitKeys()
    {
        CacheKeyGenerator.GetRateLimitInvalidationPattern()
            .Should().Be("apigw:ratelimit:*");
    }
}
