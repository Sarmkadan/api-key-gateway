// =============================================================================
// Author: Automated Test Generation
// =============================================================================

using System;
using System.Text.Json;
using ApiKeyGateway.Domain.Models;
using Xunit;

namespace api_key_gateway.Tests
{
    public class UsageQuotaJsonExtensionsTests
    {
        [Fact]
        public void ToJson_ReturnsNonEmptyString()
        {
            // Arrange
            var quota = Activator.CreateInstance<UsageQuota>();
            // Act
            var json = quota.ToJson();
            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
        }

        [Fact]
        public void ToJson_Indented_IncludesNewLine()
        {
            // Arrange
            var quota = Activator.CreateInstance<UsageQuota>();
            // Act
            var json = quota.ToJson(indented: true);
            // Assert
            Assert.Contains("\n", json);
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsObject()
        {
            // Arrange
            var quota = Activator.CreateInstance<UsageQuota>();
            var json = quota.ToJson();
            // Act
            var result = UsageQuotaJsonExtensions.FromJson(json);
            // Assert
            Assert.NotNull(result);
            // Ensure round‑trip consistency
            var roundTripJson = result.ToJson();
            Assert.Equal(json, roundTripJson);
        }

        [Fact]
        public void FromJson_NullOrEmpty_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => UsageQuotaJsonExtensions.FromJson(null!));
            Assert.Throws<ArgumentException>(() => UsageQuotaJsonExtensions.FromJson(string.Empty));
            Assert.Throws<ArgumentException>(() => UsageQuotaJsonExtensions.FromJson("   "));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndObject()
        {
            // Arrange
            var quota = Activator.CreateInstance<UsageQuota>();
            var json = quota.ToJson();
            // Act
            var success = UsageQuotaJsonExtensions.TryFromJson(json, out var result);
            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            var roundTripJson = result!.ToJson();
            Assert.Equal(json, roundTripJson);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var invalidJson = "{ this is not valid json }";
            // Act
            var success = UsageQuotaJsonExtensions.TryFromJson(invalidJson, out var result);
            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_EmptyOrWhitespace_ReturnsFalse()
        {
            // Arrange
            var empty = "";
            var whitespace = "   ";
            // Act
            var successEmpty = UsageQuotaJsonExtensions.TryFromJson(empty, out var resultEmpty);
            var successWhitespace = UsageQuotaJsonExtensions.TryFromJson(whitespace, out var resultWhitespace);
            // Assert
            Assert.False(successEmpty);
            Assert.Null(resultEmpty);
            Assert.False(successWhitespace);
            Assert.Null(resultWhitespace);
        }
    }
}
