// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using ApiKeyGateway.Caching;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

public class CacheKeyGeneratorTests
{
    // -------------------------------------------------------------------------
    // Key format consistency
    // -------------------------------------------------------------------------

    [Fact]
    public void GetApiKeyKey_ReturnsExpectedFormat()
    {
        CacheKeyGenerator.GetApiKeyKey("key-001")
            .Should().Be("apigw:apikey:key-001");
    }

    [Fact]
    public void GetApiKeyMetadataKey_ReturnsExpectedFormat()
    {
        CacheKeyGenerator.GetApiKeyMetadataKey("key-001")
            .Should().Be("apigw:apikey_meta:key-001");
    }

    [Fact]
    public void GetRateLimitKey_DefaultEndpoint_ReturnsWildcard()
    {
        CacheKeyGenerator.GetRateLimitKey("key-001")
            .Should().Be("apigw:ratelimit:key-001:*");
    }

    [Fact]
    public void GetRateLimitKey_SpecificEndpoint_IncludesEndpoint()
    {
        CacheKeyGenerator.GetRateLimitKey("key-001", "/api/users")
            .Should().Be("apigw:ratelimit:key-001:/api/users");
    }

    [Fact]
    public void GetUsageStatsKey_FormatsDateCorrectly()
    {
        var date = new DateTime(2026, 5, 21);
        CacheKeyGenerator.GetUsageStatsKey("key-001", date)
            .Should().Be("apigw:usage:key-001:2026-05-21");
    }

    [Fact]
    public void GetQuotaKey_ReturnsExpectedFormat()
    {
        CacheKeyGenerator.GetQuotaKey("key-001")
            .Should().Be("apigw:quota:key-001");
    }

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

    [Fact]
    public void GetExternalApiCacheKey_NoParameters_OmitsHash()
    {
        var key = CacheKeyGenerator.GetExternalApiCacheKey("stripe", "/charges");
        key.Should().Be("apigw:external:stripe:/charges");
    }

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

    [Fact]
    public void GetExternalApiCacheKey_DifferentParameterValues_ProduceDifferentKeys()
    {
        var params1 = new Dictionary<string, string> { { "limit", "10" } };
        var params2 = new Dictionary<string, string> { { "limit", "50" } };

        var key1 = CacheKeyGenerator.GetExternalApiCacheKey("api", "/ep", params1);
        var key2 = CacheKeyGenerator.GetExternalApiCacheKey("api", "/ep", params2);

        key1.Should().NotBe(key2);
    }

    [Fact]
    public void GetExternalApiCacheKey_EmptyParameterDictionary_OmitsHash()
    {
        var key = CacheKeyGenerator.GetExternalApiCacheKey("api", "/ep", new Dictionary<string, string>());
        key.Should().Be("apigw:external:api:/ep");
    }

    // -------------------------------------------------------------------------
    // Invalidation patterns
    // -------------------------------------------------------------------------

    [Fact]
    public void GetApiKeyInvalidationPattern_IncludesWildcards()
    {
        CacheKeyGenerator.GetApiKeyInvalidationPattern("key-001")
            .Should().Be("apigw:*:key-001:*");
    }

    [Fact]
    public void GetRateLimitInvalidationPattern_MatchesAllRateLimitKeys()
    {
        CacheKeyGenerator.GetRateLimitInvalidationPattern()
            .Should().Be("apigw:ratelimit:*");
    }
}
