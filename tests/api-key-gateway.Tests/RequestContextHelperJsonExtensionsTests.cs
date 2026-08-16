using System;
using System.Text.Json;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Utilities;
using Xunit;

namespace api_key_gateway.Tests;

public class RequestContextHelperJsonExtensionsTests
{
    [Fact]
    public void ToJson_ShouldReturnJsonString_WhenValueIsNotNull()
    {
        // Arrange
        var context = new RequestContext();

        // Act
        var json = context.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    [Fact]
    public void ToJson_ShouldThrowArgumentNullException_WhenValueIsNull()
    {
        // Arrange
        RequestContext? context = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context!.ToJson());
    }

    [Fact]
    public void FromJson_ShouldReturnRequestContext_WhenJsonIsValid()
    {
        // Arrange
        var context = new RequestContext();
        var json = context.ToJson();

        // Act
        var result = RequestContextHelperJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_ShouldThrowArgumentNullException_WhenJsonIsNull()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => RequestContextHelperJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void FromJson_ShouldThrowArgumentException_WhenJsonIsEmpty()
    {
        // Arrange
        var json = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RequestContextHelperJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_ShouldThrowJsonException_WhenJsonIsInvalid()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act & Assert
        Assert.Throws<JsonException>(() => RequestContextHelperJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void TryFromJson_ShouldReturnTrueAndValue_WhenJsonIsValid()
    {
        // Arrange
        var context = new RequestContext();
        var json = context.ToJson();

        // Act
        var success = RequestContextHelperJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_ShouldReturnFalse_WhenJsonIsNullOrEmpty()
    {
        // Arrange
        string? nullJson = null;
        string emptyJson = string.Empty;

        // Act
        var successNull = RequestContextHelperJsonExtensions.TryFromJson(nullJson!, out var resultNull);
        var successEmpty = RequestContextHelperJsonExtensions.TryFromJson(emptyJson, out var resultEmpty);

        // Assert
        Assert.False(successNull);
        Assert.Null(resultNull);
        Assert.False(successEmpty);
        Assert.Null(resultEmpty);
    }

    [Fact]
    public void TryFromJson_ShouldReturnFalse_WhenJsonIsInvalid()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act
        var success = RequestContextHelperJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }
}
