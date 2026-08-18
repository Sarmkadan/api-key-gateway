namespace ApiKeyGateway.Tests;

using System;
using System.Text.Json;
using Xunit;
using ApiKeyGateway.Domain.Exceptions;
using DomainException = ApiKeyGateway.Domain.Exceptions.UnauthorizedAccessException;

public class UnauthorizedAccessExceptionJsonExtensionsTests
{
    #region ToJson

    [Fact]
    public void ToJson_HappyPath_SerializesReasonAndSourceIp()
    {
        // Arrange
        var exception = new DomainException("Invalid API key", "MissingApiKey", "192.168.1.100");

        // Act
        var json = UnauthorizedAccessExceptionJsonExtensions.ToJson(exception);

        // Assert
        Assert.Contains("\"reason\":\"MissingApiKey\"", json);
        Assert.Contains("\"sourceIp\":\"192.168.1.100\"", json);
        Assert.Contains("\"message\":\"Invalid API key\"", json);
    }

    [Fact]
    public void ToJson_WithIndented_ReturnsFormattedJson()
    {
        // Arrange
        var exception = new DomainException("Invalid API key");

        // Act
        var json = UnauthorizedAccessExceptionJsonExtensions.ToJson(exception, indented: true);

        // Assert
        Assert.StartsWith("{\n", json);
        Assert.Contains("\"message\": \"Invalid API key\"", json);
    }

    [Fact]
    public void ToJson_NullValue_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => UnauthorizedAccessExceptionJsonExtensions.ToJson(null!));
    }

    #endregion

    #region FromJson

    [Fact]
    public void FromJson_ValidJson_DeserializesException()
    {
        // Arrange
        var json = "{\"reason\":\"ExpiredKey\",\"sourceIp\":\"10.0.0.1\"}";

        // Act
        var exception = UnauthorizedAccessExceptionJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("ExpiredKey", exception.Reason);
        Assert.Equal("10.0.0.1", exception.SourceIp);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => UnauthorizedAccessExceptionJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_EmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => UnauthorizedAccessExceptionJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Act & Assert
        Assert.Throws<JsonException>(
            () => UnauthorizedAccessExceptionJsonExtensions.FromJson("{invalid json}"));
    }

    #endregion

    #region TryFromJson

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndDeserializedException()
    {
        // Arrange
        var json = "{\"reason\":\"InvalidKey\",\"sourceIp\":\"192.168.1.50\"}";

        // Act
        var success = UnauthorizedAccessExceptionJsonExtensions.TryFromJson(json, out var exception);

        // Assert
        Assert.True(success);
        Assert.NotNull(exception);
        Assert.Equal("InvalidKey", exception.Reason);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = "{invalid json}";

        // Act
        var success = UnauthorizedAccessExceptionJsonExtensions.TryFromJson(json, out var exception);

        // Assert
        Assert.False(success);
        Assert.Null(exception);
    }

    [Fact]
    public void TryFromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => UnauthorizedAccessExceptionJsonExtensions.TryFromJson(null!, out _));
    }

    #endregion
}