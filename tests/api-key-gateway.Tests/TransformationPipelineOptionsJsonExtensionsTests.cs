using System;
using ApiKeyGateway.Configuration;
using Xunit;

namespace api_key_gateway.Tests
{
    public class TransformationPipelineOptionsJsonExtensionsTests
    {
        private TransformationPipelineOptions CreateSampleOptions()
        {
            // Assuming TransformationPipelineOptions has a parameterless constructor.
            // If it has properties, they can be set here to make the JSON more interesting.
            return new TransformationPipelineOptions();
        }

        [Fact]
        public void ToJson_WithValidObject_ReturnsNonEmptyJson()
        {
            // Arrange
            var options = CreateSampleOptions();

            // Act
            string json = options.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
        }

        [Fact]
        public void ToJson_WithIndent_ProducesIndentedJson()
        {
            // Arrange
            var options = CreateSampleOptions();

            // Act
            string json = options.ToJson(indented: true);

            // Assert
            // Indented JSON contains line breaks; checking for at least one newline character.
            Assert.Contains(Environment.NewLine, json);
        }

        [Fact]
        public void ToJson_NullArgument_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((TransformationPipelineOptions)null!).ToJson());
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsDeserializedObject()
        {
            // Arrange
            var original = CreateSampleOptions();
            string json = original.ToJson();

            // Act
            var deserialized = TransformationPipelineOptionsJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TransformationPipelineOptionsJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void FromJson_EmptyString_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => TransformationPipelineOptionsJsonExtensions.FromJson(string.Empty));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndValue()
        {
            // Arrange
            var original = CreateSampleOptions();
            string json = original.ToJson();

            // Act
            bool result = TransformationPipelineOptionsJsonExtensions.TryFromJson(json, out var value);

            // Assert
            Assert.True(result);
            Assert.NotNull(value);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            string malformedJson = "{ this is not valid json }";

            // Act
            bool result = TransformationPipelineOptionsJsonExtensions.TryFromJson(malformedJson, out var value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        [Fact]
        public void TryFromJson_NullInput_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TransformationPipelineOptionsJsonExtensions.TryFromJson(null!, out _));
        }
    }
}
