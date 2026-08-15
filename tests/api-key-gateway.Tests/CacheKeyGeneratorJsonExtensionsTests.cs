using System.Text.Json;
using ApiKeyGateway.Caching;
using Xunit;

namespace ApiKeyGateway.Tests;

public class CacheKeyGeneratorJsonExtensionsTests
{
    [Fact]
    public void CacheKeyGeneratorConfiguration_Properties_InitializedCorrectly()
    {
        var config = new CacheKeyGeneratorJsonExtensions.CacheKeyGeneratorConfiguration();
        Assert.Equal("apigw", config.Prefix);
        Assert.Equal(':', config.Separator);
    }

    [Fact]
    public void FromCacheKeyGenerator_ReturnsDefaultConfiguration()
    {
        var config = CacheKeyGeneratorJsonExtensions.CacheKeyGeneratorConfiguration.FromCacheKeyGenerator();
        Assert.Equal("apigw", config.Prefix);
        Assert.Equal(':', config.Separator);
    }

    [Fact]
    public void ToJson_ValidConfiguration_SerializesCorrectly()
    {
        var config = new CacheKeyGeneratorJsonExtensions.CacheKeyGeneratorConfiguration { Prefix = "test", Separator = '-' };
        var json = config.ToJson();
        Assert.Contains("\"prefix\":\"test\"", json);
        Assert.Contains("\"separator\":\"-\"", json);
    }

    [Fact]
    public void ToJson_NullConfiguration_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((CacheKeyGeneratorJsonExtensions.CacheKeyGeneratorConfiguration)null!).ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_DeserializesCorrectly()
    {
        var json = "{\"prefix\":\"test\",\"separator\":\"-\"}";
        var config = CacheKeyGeneratorJsonExtensions.FromJson(json);
        Assert.NotNull(config);
        Assert.Equal("test", config.Prefix);
        Assert.Equal('-', config.Separator);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromJson_EmptyOrWhitespaceJson_ReturnsNull(string? json)
    {
        var config = CacheKeyGeneratorJsonExtensions.FromJson(json!);
        Assert.Null(config);
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        var json = "invalid json";
        Assert.Throws<JsonException>(() => CacheKeyGeneratorJsonExtensions.FromJson(json));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndValue()
    {
        var json = "{\"prefix\":\"test\",\"separator\":\"-\"}";
        var success = CacheKeyGeneratorJsonExtensions.TryFromJson(json, out var config);
        Assert.True(success);
        Assert.NotNull(config);
        Assert.Equal("test", config.Prefix);
        Assert.Equal('-', config.Separator);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        var json = "invalid json";
        var success = CacheKeyGeneratorJsonExtensions.TryFromJson(json, out var config);
        Assert.False(success);
        Assert.Null(config);
    }

    [Fact]
    public void TryFromJson_NullJson_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => CacheKeyGeneratorJsonExtensions.TryFromJson(null!, out _));
    }
}
