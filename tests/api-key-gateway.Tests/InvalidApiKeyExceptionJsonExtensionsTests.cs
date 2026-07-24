using System;
using System.Text.Json;
using ApiKeyGateway.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class InvalidApiKeyExceptionJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var exception = new InvalidApiKeyException("Test message");

        // Act
        var json = InvalidApiKeyExceptionJsonExtensions.ToJson(exception);

        // Assert
        json.Should().NotBeNullOrEmpty();
        var deserialized = JsonSerializer.Deserialize<InvalidApiKeyException>(json);
        deserialized.Should().NotBeNull();
        deserialized.Message.Should().Be("Test message");
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => InvalidApiKeyExceptionJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsDeserializedException()
    {
        // Arrange
        var originalException = new InvalidApiKeyException("Test message");
        var json = InvalidApiKeyExceptionJsonExtensions.ToJson(originalException);

        // Act
        var deserialized = InvalidApiKeyExceptionJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Message.Should().Be("Test message");
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => InvalidApiKeyExceptionJsonExtensions.FromJson(null));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndDeserializedException()
    {
        // Arrange
        var originalException = new InvalidApiKeyException("Test message");
        var json = InvalidApiKeyExceptionJsonExtensions.ToJson(originalException);

        // Act
        var result = InvalidApiKeyExceptionJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        result.Should().BeTrue();
        deserialized.Should().NotBeNull();
        deserialized.Message.Should().Be("Test message");
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var result = InvalidApiKeyExceptionJsonExtensions.TryFromJson(null, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Act
        var result = InvalidApiKeyExceptionJsonExtensions.TryFromJson("Invalid json", out _);

        // Assert
        result.Should().BeFalse();
    }
}
