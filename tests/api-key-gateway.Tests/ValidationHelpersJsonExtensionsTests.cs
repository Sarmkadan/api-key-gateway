using Xunit;
using System.Text.Json;
using ApiKeyGateway.Utilities;

namespace api_key_gateway.Tests
{
    public class ValidationHelpersJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var expected = "{\"typeName\":\"ValidationHelpers\",\"methods\":[\"IsValidEmail\",\"IsValidApiKeyFormat\",\"IsValidIpAddress\",\"IsValidGuid\",\"IsValidUrl\",\"SanitizeInput\"]}";

            // Act
            var result = ValidationHelpersJsonExtensions.ToJson();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsMetadata()
        {
            // Arrange
            var json = "{\"typeName\":\"ValidationHelpers\",\"methods\":[\"IsValidEmail\",\"IsValidApiKeyFormat\",\"IsValidIpAddress\",\"IsValidGuid\",\"IsValidUrl\",\"SanitizeInput\"]}";

            // Act
            var result = ValidationHelpersJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ValidationHelpers", result.TypeName);
            Assert.NotNull(result.Methods);
            Assert.Equal(6, result.Methods.Count);
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => ValidationHelpersJsonExtensions.FromJson(null));
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndMetadata()
        {
            // Arrange
            var json = "{\"typeName\":\"ValidationHelpers\",\"methods\":[\"IsValidEmail\",\"IsValidApiKeyFormat\",\"IsValidIpAddress\",\"IsValidGuid\",\"IsValidUrl\",\"SanitizeInput\"]}";

            // Act
            var result = ValidationHelpersJsonExtensions.TryFromJson(json, out var metadata);

            // Assert
            Assert.True(result);
            Assert.NotNull(metadata);
            Assert.Equal("ValidationHelpers", metadata.TypeName);
            Assert.NotNull(metadata.Methods);
            Assert.Equal(6, metadata.Methods.Count);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsTrueAndNullMetadata()
        {
            // Act
            var result = ValidationHelpersJsonExtensions.TryFromJson(null, out var metadata);

            // Assert
            Assert.True(result);
            Assert.Null(metadata);
        }
    }
}
