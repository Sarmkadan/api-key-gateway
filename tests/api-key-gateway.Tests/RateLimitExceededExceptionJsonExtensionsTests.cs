using System;
using System.Text.Json;
using ApiKeyGateway.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class RateLimitExceededExceptionJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var exception = new RateLimitExceededException("test-api-key", 100, 60);

        // Act
        var json = exception.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("test-api-key");
        json.Should().Contain("100");
        json.Should().Contain("60");
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var exception = new RateLimitExceededException("test-api-key", 100, 60);

        // Act
        var json = exception.ToJson(indented: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("{");
        json.Should().Contain("}");
        json.Should().Contain("apiKeyId");
        json.Should().Contain("limit");
        json.Should().Contain("windowInSeconds");
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        RateLimitExceededException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.ToJson());
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsDeserializedException()
    {
        // Arrange
        var originalException = new RateLimitExceededException("test-api-key", 100, 60);
        var json = originalException.ToJson();

        // Act
        var deserialized = RateLimitExceededExceptionJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.ApiKeyId.Should().Be("test-api-key");
        deserialized.Limit.Should().Be(100);
        deserialized.WindowInSeconds.Should().Be(60);
        deserialized.RetryAfter.Should().NotBeNull();
        deserialized.Message.Should().NotBeNullOrEmpty();
        deserialized.Message.Should().Contain("Rate limit exceeded");
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => RateLimitExceededExceptionJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => RateLimitExceededExceptionJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsNull()
    {
        // Act
        var result = RateLimitExceededExceptionJsonExtensions.FromJson("Invalid json");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndDeserializedException()
    {
        // Arrange
        var originalException = new RateLimitExceededException("test-api-key", 100, 60);
        var json = originalException.ToJson();

        // Act
        var result = RateLimitExceededExceptionJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        result.Should().BeTrue();
        deserialized.Should().NotBeNull();
        deserialized.ApiKeyId.Should().Be("test-api-key");
        deserialized.Limit.Should().Be(100);
        deserialized.WindowInSeconds.Should().Be(60);
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => RateLimitExceededExceptionJsonExtensions.TryFromJson(null, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryFromJson_EmptyInput_ThrowsArgumentException()
    {
        // Act
        Action act = () => RateLimitExceededExceptionJsonExtensions.TryFromJson(string.Empty, out _);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Act
        var result = RateLimitExceededExceptionJsonExtensions.TryFromJson("Invalid json", out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RoundTrip_SerializationDeserialization_PreservesProperties()
    {
        // Arrange
        var originalException = new RateLimitExceededException("round-trip-key", 50, 3600);

        // Act
        var json = originalException.ToJson();
        var deserialized = RateLimitExceededExceptionJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.ApiKeyId.Should().Be(originalException.ApiKeyId);
        deserialized.Limit.Should().Be(originalException.Limit);
        deserialized.WindowInSeconds.Should().Be(originalException.WindowInSeconds);
        deserialized.RetryAfter.Should().BeCloseTo(originalException.RetryAfter.Value, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RoundTrip_WithRetryAfterNull_PreservesProperties()
    {
        // Arrange
        var originalException = new RateLimitExceededException();

        // Act
        var json = originalException.ToJson();
        var deserialized = RateLimitExceededExceptionJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.ApiKeyId.Should().BeEmpty();
        deserialized.Limit.Should().Be(0);
        deserialized.WindowInSeconds.Should().Be(0);
        deserialized.RetryAfter.Should().BeNull();
    }
}