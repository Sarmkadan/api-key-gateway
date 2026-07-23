namespace ApiKeyGateway.Tests;

using Xunit;
using System.Text.Json;
using ApiKeyGateway.Domain.Exceptions;

public class ValidationExceptionJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var validationException = new ValidationException("Test message");

        // Act
        var json = ValidationExceptionJsonExtensions.ToJson(validationException);

        // Assert
        Assert.NotNull(json);
        Assert.StartsWith("{\"Message\":\"", json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ValidationExceptionJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsValidationException()
    {
        // Arrange
        var json = "{\"Message\":\"Test message\"}";
        var validationException = new ValidationException("Test message");

        // Act
        var result = ValidationExceptionJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(validationException.Message, result.Message);
    }

    [Fact]
    public void FromJson_NullInput_ReturnsNull()
    {
        // Act
        var result = ValidationExceptionJsonExtensions.FromJson(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_EmptyJson_ReturnsNull()
    {
        // Act
        var result = ValidationExceptionJsonExtensions.FromJson("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrue()
    {
        // Arrange
        var json = "{\"Message\":\"Test message\"}";
        var validationException = new ValidationException("Test message");

        // Act
        var result = ValidationExceptionJsonExtensions.TryFromJson(json, out var exception);

        // Assert
        Assert.True(result);
        Assert.Equal(validationException.Message, exception.Message);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var result = ValidationExceptionJsonExtensions.TryFromJson(null, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsFalse()
    {
        // Act
        var result = ValidationExceptionJsonExtensions.TryFromJson("", out _);

        // Assert
        Assert.False(result);
    }
}
