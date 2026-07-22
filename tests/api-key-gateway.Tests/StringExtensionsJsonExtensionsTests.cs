// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;
using ApiKeyGateway.Extensions;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class StringExtensionsJsonExtensionsTests
{
    public class ToJson
    {
        [Fact]
        public void ToJson_WithDefaultIndentedFalse_ReturnsCompactJson()
        {
            // Act
            var result = StringExtensionsJsonExtensions.ToJson();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().NotMatchRegex("\\s"); // Compact JSON has no whitespace

            // Verify it's valid JSON
            var parseResult = JsonSerializer.Deserialize<StringExtensionsJsonExtensions.StringExtensionsMetadata>(
                result,
                new JsonSerializerOptions(JsonSerializerDefaults.Web) { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );
            parseResult.Should().NotBeNull();
            parseResult!.TypeName.Should().Be("StringExtensions");
            parseResult.Methods.Should().NotBeNull();
            parseResult.Methods.Should().NotBeEmpty();
        }

        [Fact]
        public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
        {
            // Act
            var result = StringExtensionsJsonExtensions.ToJson(indented: true);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("\n"); // Formatted JSON has newlines
            result.Should().Contain("  "); // Formatted JSON has indentation

            // Verify it's valid JSON
            var parseResult = JsonSerializer.Deserialize<StringExtensionsJsonExtensions.StringExtensionsMetadata>(
                result,
                new JsonSerializerOptions(JsonSerializerDefaults.Web) { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );
            parseResult.Should().NotBeNull();
            parseResult!.TypeName.Should().Be("StringExtensions");
            parseResult.Methods.Should().NotBeNull();
            parseResult.Methods.Should().NotBeEmpty();
        }
    }

    public class FromJson
    {
        [Fact]
        public void FromJson_WithValidJson_ReturnsDeserializedMetadata()
        {
            // Arrange
            var json = "{\"typeName\":\"StringExtensions\",\"methods\":[\"CapitalizeFirst\",\"ContainsAny\"]}";

            // Act
            var result = StringExtensionsJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result!.TypeName.Should().Be("StringExtensions");
            result.Methods.Should().NotBeNull();
            result.Methods.Should().Contain("CapitalizeFirst");
            result.Methods.Should().Contain("ContainsAny");
        }

        [Fact]
        public void FromJson_WithCamelCaseJson_ReturnsDeserializedMetadata()
        {
            // Arrange - using camelCase as per the actual serialization
            var json = "{\"typeName\":\"StringExtensions\",\"methods\":[\"truncate\",\"truncateWithEllipsis\"]}";

            // Act
            var result = StringExtensionsJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result!.TypeName.Should().Be("StringExtensions");
            result.Methods.Should().NotBeNull();
            result.Methods.Should().Contain("truncate");
            result.Methods.Should().Contain("truncateWithEllipsis");
        }

        [Fact]
        public void FromJson_WithNull_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => StringExtensionsJsonExtensions.FromJson(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJson_WithEmptyString_ReturnsNull()
        {
            // Act
            var result = StringExtensionsJsonExtensions.FromJson("");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void FromJson_WithWhitespaceString_ReturnsNull()
        {
            // Act
            var result = StringExtensionsJsonExtensions.FromJson("   ");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void FromJson_WithInvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidJson = "{invalid json";

            // Act
            Action act = () => StringExtensionsJsonExtensions.FromJson(invalidJson);

            // Assert
            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void FromJson_WithEmptyObject_ReturnsMetadataWithNullValues()
        {
            // Arrange
            var json = "{}";

            // Act
            var result = StringExtensionsJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result.TypeName.Should().BeNull();
            result.Methods.Should().BeNull();
        }
    }

    public class TryFromJson
    {
        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializedMetadata()
        {
            // Arrange
            var json = "{\"typeName\":\"StringExtensions\",\"methods\":[\"ToSlug\",\"IsNumeric\"]}";

            // Act
            var success = StringExtensionsJsonExtensions.TryFromJson(json, out var result);

            // Assert
            success.Should().BeTrue();
            result.Should().NotBeNull();
            result!.TypeName.Should().Be("StringExtensions");
            result.Methods.Should().NotBeNull();
            result.Methods.Should().Contain("ToSlug");
            result.Methods.Should().Contain("IsNumeric");
        }

        [Fact]
        public void TryFromJson_WithNull_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => StringExtensionsJsonExtensions.TryFromJson(null!, out _);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void TryFromJson_WithEmptyString_ReturnsTrueAndNull()
        {
            // Act
            var success = StringExtensionsJsonExtensions.TryFromJson("", out var result);

            // Assert
            success.Should().BeTrue();
            result.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_WithWhitespaceString_ReturnsTrueAndNull()
        {
            // Act
            var success = StringExtensionsJsonExtensions.TryFromJson("   ", out var result);

            // Assert
            success.Should().BeTrue();
            result.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var invalidJson = "{invalid json";

            // Act
            var success = StringExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }
    }

    public class StringExtensionsMetadataProperties
    {
        [Fact]
        public void TypeName_Property_ReturnsCorrectTypeName()
        {
            // Arrange
            var json = StringExtensionsJsonExtensions.ToJson();
            var metadata = StringExtensionsJsonExtensions.FromJson(json);

            // Act & Assert
            metadata!.TypeName.Should().Be("StringExtensions");
        }

        [Fact]
        public void Methods_Property_ReturnsListOfExtensionMethods()
        {
            // Arrange
            var json = StringExtensionsJsonExtensions.ToJson();
            var metadata = StringExtensionsJsonExtensions.FromJson(json);

            // Act & Assert
            metadata!.Methods.Should().NotBeNull();
            metadata.Methods.Should().NotBeEmpty();

            // Verify all methods are actual extension methods from StringExtensions
            var expectedMethods = new[]
            {
                "CapitalizeFirst",
                "ContainsAny",
                "IsNumeric",
                "ToList",
                "ToSlug",
                "Truncate",
                "TruncateWithEllipsis",
                "TryParseInt",
                "TryParseLong",
                "StartsWithAny"
            };

            foreach (var method in expectedMethods)
            {
                metadata.Methods.Should().Contain(method);
            }
        }

        [Fact]
        public void Methods_Property_ReturnsReadOnlyList()
        {
            // Arrange
            var json = StringExtensionsJsonExtensions.ToJson();
            var metadata = StringExtensionsJsonExtensions.FromJson(json);

            // Act & Assert
            metadata.Methods.Should().BeAssignableTo<IReadOnlyList<string>>();
        }
    }
}