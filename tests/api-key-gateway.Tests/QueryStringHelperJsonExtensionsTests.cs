// SPDX-License-Identifier: MIT
// Tests for ApiKeyGateway.Utilities.QueryStringHelperJsonExtensions
// Uses the same namespace style as the existing test files.

using System;
using System.Collections.Generic;
using System.Text.Json;
using ApiKeyGateway.Utilities;
using Xunit;

namespace api_key_gateway.Tests;

public sealed class QueryStringHelperJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithParameters_ReturnsValidJson()
    {
        // Arrange
        var data = new QueryStringHelperJsonExtensions.QueryStringData
        {
            Parameters = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            }
        };

        // Act
        string json = data.ToJson();

        // Assert
        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.True(root.TryGetProperty("key1", out var v1));
        Assert.Equal("value1", v1.GetString());

        Assert.True(root.TryGetProperty("key2", out var v2));
        Assert.Equal("value2", v2.GetString());
    }

    [Fact]
    public void ToJson_WithIndentation_ProducesIndentedJson()
    {
        // Arrange
        var data = new QueryStringHelperJsonExtensions.QueryStringData
        {
            Parameters = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["a"] = "b"
            }
        };

        // Act
        string json = data.ToJson(indented: true);

        // Assert
        // Indented JSON contains line breaks; a simple check is that it contains '\n'
        Assert.Contains('\n', json);
    }

    [Fact]
    public void ToJson_NullArgument_ThrowsArgumentNullException()
    {
        // Arrange
        QueryStringHelperJsonExtensions.QueryStringData? data = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => data!.ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsObject()
    {
        // Arrange
        string json = """{ "keyA": "valueA", "keyB": "valueB" }""";

        // Act
        var result = QueryStringHelperJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result!.Parameters.Count);
        Assert.Equal("valueA", result.Parameters["keyA"]);
        Assert.Equal("valueB", result.Parameters["keyB"]);
    }

    [Fact]
    public void FromJson_NullOrEmpty_ThrowsArgumentException()
    {
        // Null
        Assert.Throws<ArgumentException>(() => QueryStringHelperJsonExtensions.FromJson(null!));

        // Empty
        Assert.Throws<ArgumentException>(() => QueryStringHelperJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrue()
    {
        // Arrange
        string json = """{ "x": "y" }""";

        // Act
        bool success = QueryStringHelperJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Single(result!.Parameters);
        Assert.Equal("y", result.Parameters["x"]);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Arrange
        string malformedJson = """{ "unclosed": "value" """;

        // Act
        bool success = QueryStringHelperJsonExtensions.TryFromJson(malformedJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_EmptyParameters_ReturnsObjectWithEmptyDictionary()
    {
        // Arrange
        string json = """{ }""";

        // Act
        var result = QueryStringHelperJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result!.Parameters);
    }
}
