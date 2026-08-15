// ---------------------------------------------------------------
//  Unit tests for ApiKeyGateway.Utilities.RetryPolicyBuilderJsonExtensions
//  Uses the same namespace and style as existing test files.
// ---------------------------------------------------------------

using System;
using System.Text.Json;
using ApiKeyGateway.Utilities;
using Xunit;

namespace api_key_gateway.Tests
{
    public sealed class RetryPolicyBuilderJsonExtensionsTests
    {
        private static RetryPolicyBuilder CreateSampleBuilder()
        {
            // The builder is expected to have a parameterless constructor.
            // If the real implementation requires configuration, adjust here.
            return new RetryPolicyBuilder();
        }

        [Fact]
        public void ToJson_WithValidBuilder_ReturnsNonEmptyJson()
        {
            // Arrange
            var builder = CreateSampleBuilder();

            // Act
            string json = builder.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            // The JSON should be deserializable back to the same type.
            var deserialized = JsonSerializer.Deserialize<RetryPolicyBuilder>(json);
            Assert.NotNull(deserialized);
        }

        [Fact]
        public void ToJson_WithIndentation_ProducesIndentedJson()
        {
            // Arrange
            var builder = CreateSampleBuilder();

            // Act
            string json = builder.ToJson(indented: true);

            // Assert
            Assert.Contains("\n", json); // Indented JSON contains line breaks.
        }

        [Fact]
        public void ToJson_NullBuilder_ThrowsArgumentNullException()
        {
            // Arrange
            RetryPolicyBuilder? builder = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => builder!.ToJson());
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsBuilder()
        {
            // Arrange
            var original = CreateSampleBuilder();
            string json = original.ToJson();

            // Act
            var result = RetryPolicyBuilderJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RetryPolicyBuilderJsonExtensions.FromJson(json!));
        }

        [Fact]
        public void FromJson_EmptyJson_ThrowsArgumentException()
        {
            // Arrange
            string json = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => RetryPolicyBuilderJsonExtensions.FromJson(json));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndBuilder()
        {
            // Arrange
            var original = CreateSampleBuilder();
            string json = original.ToJson();

            // Act
            bool success = RetryPolicyBuilderJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            string invalidJson = "{ this is not valid json }";

            // Act
            bool success = RetryPolicyBuilderJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RetryPolicyBuilderJsonExtensions.TryFromJson(json!, out _));
        }

        [Fact]
        public void TryFromJson_EmptyJson_ThrowsArgumentException()
        {
            // Arrange
            string json = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => RetryPolicyBuilderJsonExtensions.TryFromJson(json, out _));
        }
    }
}
