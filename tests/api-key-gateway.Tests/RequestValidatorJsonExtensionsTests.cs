using Xunit;
using System.Text.Json;
using ApiKeyGateway.Validation;

namespace api_key_gateway.Tests
{
    public class RequestValidatorJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var validationResult = new ValidationResult();

            // Act
            var json = RequestValidatorJsonExtensions.ToJson(validationResult);

            // Assert
            Assert.NotNull(json);
            Assert.True(JsonSerializer.Deserialize<ValidationResult>(json) != null);
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsValidationResult()
        {
            // Arrange
            var json = "{\"IsValid\":true,\"Errors\":[]}";

            // Act
            var validationResult = RequestValidatorJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(validationResult);
            Assert.True(validationResult!.IsValid);
            Assert.Empty(validationResult.Errors);
        }

        [Fact]
        public void FromJson_NullInput_ReturnsNull()
        {
            // Act
            var validationResult = RequestValidatorJsonExtensions.FromJson(null);

            // Assert
            Assert.Null(validationResult);
        }

        [Fact]
        public void FromJson_InvalidJson_ReturnsNull()
        {
            // Act
            var validationResult = RequestValidatorJsonExtensions.FromJson("Invalid json");

            // Assert
            Assert.Null(validationResult);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndValidationResult()
        {
            // Arrange
            var json = "{\"IsValid\":true,\"Errors\":[]}";

            // Act
            var success = RequestValidatorJsonExtensions.TryFromJson(json, out var validationResult);

            // Assert
            Assert.True(success);
            Assert.NotNull(validationResult);
            Assert.True(validationResult!.IsValid);
            Assert.Empty(validationResult.Errors);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsFalseAndNull()
        {
            // Act
            var success = RequestValidatorJsonExtensions.TryFromJson(null, out var validationResult);

            // Assert
            Assert.False(success);
            Assert.Null(validationResult);
        }
    }
}
