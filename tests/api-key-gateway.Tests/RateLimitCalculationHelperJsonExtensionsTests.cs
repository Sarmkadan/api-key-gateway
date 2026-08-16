using System;
using System.Text.Json;
using ApiKeyGateway.Utilities;
using Xunit;

namespace api_key_gateway.Tests
{
    public class RateLimitCalculationHelperJsonExtensionsTests
    {
        [Fact]
        public void ToJson_ReturnsValidJson_WithMethods()
        {
            // Act
            var json = RateLimitCalculationHelperJsonExtensions.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));

            var metadata = JsonSerializer.Deserialize<RateLimitCalculationHelperJsonExtensions.RateLimitCalculationHelperMetadata>(json);
            Assert.NotNull(metadata);
            Assert.Equal("RateLimitCalculationHelper", metadata!.TypeName);
            Assert.NotNull(metadata.Methods);
            Assert.Contains(nameof(RateLimitCalculationHelper.GetWindowEnd), metadata.Methods);
            Assert.Contains(nameof(RateLimitCalculationHelper.GetWindowStart), metadata.Methods);
            Assert.Contains(nameof(RateLimitCalculationHelper.GetSecondsUntilAllowed), metadata.Methods);
            Assert.Contains(nameof(RateLimitCalculationHelper.CalculateQuotagePercentage), metadata.Methods);
            Assert.Contains(nameof(RateLimitCalculationHelper.ShouldWarnAboutLimit), metadata.Methods);
            Assert.Contains(nameof(RateLimitCalculationHelper.GetReadableResetTime), metadata.Methods);
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsMetadata()
        {
            // Arrange
            var json = RateLimitCalculationHelperJsonExtensions.ToJson(indented: true);

            // Act
            var metadata = RateLimitCalculationHelperJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(metadata);
            Assert.Equal("RateLimitCalculationHelper", metadata!.TypeName);
            Assert.NotEmpty(metadata.Methods);
        }

        [Fact]
        public void FromJson_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RateLimitCalculationHelperJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void FromJson_EmptyString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RateLimitCalculationHelperJsonExtensions.FromJson(string.Empty));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndMetadata()
        {
            // Arrange
            var json = RateLimitCalculationHelperJsonExtensions.ToJson();

            // Act
            var result = RateLimitCalculationHelperJsonExtensions.TryFromJson(json, out var metadata);

            // Assert
            Assert.True(result);
            Assert.NotNull(metadata);
            Assert.Equal("RateLimitCalculationHelper", metadata!.TypeName);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act
            var result = RateLimitCalculationHelperJsonExtensions.TryFromJson(invalidJson, out var metadata);

            // Assert
            Assert.False(result);
            Assert.Null(metadata);
        }

        [Fact]
        public void TryFromJson_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RateLimitCalculationHelperJsonExtensions.TryFromJson(null!, out _));
        }

        [Fact]
        public void TryFromJson_EmptyString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RateLimitCalculationHelperJsonExtensions.TryFromJson(string.Empty, out _));
        }
    }
}
