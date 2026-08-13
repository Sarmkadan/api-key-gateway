using System;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class CoalescingMetricsJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidMetrics_ReturnsNonEmptyString()
    {
        // Arrange
        var metrics = new CoalescingMetrics();

        // Act
        var json = metrics.ToJson();

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ToJson_WithIndentation_ReturnsIndentedJson()
    {
        // Arrange
        var metrics = new CoalescingMetrics();

        // Act
        var json = metrics.ToJson(indented: true);

        // Assert
        // Indented JSON contains line breaks (or at least a newline character)
        json.Should().Contain("\n");
    }

    [Fact]
    public void ToJson_NullMetrics_ThrowsArgumentNullException()
    {
        // Arrange
        CoalescingMetrics? metrics = null;

        // Act
        Action act = () => metrics!.ToJson();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsMetrics()
    {
        // Arrange
        var original = new CoalescingMetrics();
        var json = original.ToJson();

        // Act
        var deserialized = CoalescingMetricsJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
    }

    [Fact]
    public void FromJson_NullOrEmpty_Throws()
    {
        // Null input
        Action actNull = () => CoalescingMetricsJsonExtensions.FromJson(null!);
        actNull.Should().Throw<ArgumentNullException>();

        // Empty input
        Action actEmpty = () => CoalescingMetricsJsonExtensions.FromJson(string.Empty);
        actEmpty.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndMetrics()
    {
        // Arrange
        var original = new CoalescingMetrics();
        var json = original.ToJson();

        // Act
        var success = CoalescingMetricsJsonExtensions.TryFromJson(json, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act
        var success = CoalescingMetricsJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_EmptyString_ThrowsArgumentException()
    {
        // Act
        Action act = () => CoalescingMetricsJsonExtensions.TryFromJson(string.Empty, out var _);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
