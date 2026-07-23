// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Text.Json;
using ApiKeyGateway.Events;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class ApiKeyEventJsonExtensionsTests
{
    private static ApiKeyEvent CreateSampleEvent()
    {
        // The ApiKeyEvent type is part of the production code. We create an instance
        // via reflection to avoid depending on its concrete constructor signature.
        // All properties will be left at their default values, which is sufficient
        // for serialization tests.
        return (ApiKeyEvent)Activator.CreateInstance(typeof(ApiKeyEvent))!;
    }

    [Fact]
    public void ToJson_WithDefaultIndentedFalse_ReturnsCompactJson()
    {
        // Arrange
        var evt = CreateSampleEvent();

        // Act
        var json = evt.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().NotMatchRegex(@"\s"); // compact JSON has no whitespace

        // Verify that the JSON can be deserialized back to an ApiKeyEvent
        var deserialized = ApiKeyEventJsonExtensions.FromJson(json);
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<ApiKeyEvent>();
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var evt = CreateSampleEvent();

        // Act
        var json = evt.ToJson(indented: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\n"); // formatted JSON contains newlines
        json.Should().Contain("  "); // formatted JSON contains indentation

        // Verify that the formatted JSON can still be deserialized
        var deserialized = ApiKeyEventJsonExtensions.FromJson(json);
        deserialized.Should().NotBeNull();
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsDeserializedEvent()
    {
        // Arrange
        var original = CreateSampleEvent();
        var json = original.ToJson();

        // Act
        var result = ApiKeyEventJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ApiKeyEvent>();
    }

    [Fact]
    public void FromJson_WithNull_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ApiKeyEventJsonExtensions.FromJson(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_WithEmptyString_ThrowsArgumentException()
    {
        // Act
        Action act = () => ApiKeyEventJsonExtensions.FromJson(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ invalid json";

        // Act
        Action act = () => ApiKeyEventJsonExtensions.FromJson(invalidJson);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializedEvent()
    {
        // Arrange
        var original = CreateSampleEvent();
        var json = original.ToJson();

        // Act
        var success = ApiKeyEventJsonExtensions.TryFromJson(json, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeOfType<ApiKeyEvent>();
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ invalid json";

        // Act
        var success = ApiKeyEventJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithNull_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ApiKeyEventJsonExtensions.TryFromJson(null!, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryFromJson_WithEmptyString_ThrowsArgumentException()
    {
        // Act
        Action act = () => ApiKeyEventJsonExtensions.TryFromJson(string.Empty, out _);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
