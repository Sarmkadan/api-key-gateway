using System;
using System.Text.Json;
using ApiKeyGateway.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class KeyStoreUnavailableExceptionJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var exception = new KeyStoreUnavailableException("Test message");

        // Act
        var json = exception.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        var deserialized = JsonSerializer.Deserialize<KeyStoreUnavailableException>(json);
        deserialized.Should().NotBeNull();
        deserialized.Message.Should().Be("Test message");
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsDeserializedException()
    {
        // Arrange
        var exception = new KeyStoreUnavailableException("Test message");
        var json = exception.ToJson();

        // Act
        var deserialized = KeyStoreUnavailableExceptionJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Message.Should().Be("Test message");
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => KeyStoreUnavailableExceptionJsonExtensions.FromJson(null));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndDeserializedException()
    {
        // Arrange
        var exception = new KeyStoreUnavailableException("Test message");
        var json = exception.ToJson();

        // Act
        var result = KeyStoreUnavailableExceptionJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        result.Should().BeTrue();
        deserialized.Should().NotBeNull();
        deserialized.Message.Should().Be("Test message");
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var result = KeyStoreUnavailableExceptionJsonExtensions.TryFromJson(null, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Act
        var result = KeyStoreUnavailableExceptionJsonExtensions.TryFromJson("Invalid json", out _);

        // Assert
        result.Should().BeFalse();
    }
}
