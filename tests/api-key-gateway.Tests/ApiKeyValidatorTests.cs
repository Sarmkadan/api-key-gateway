using Xunit;
using ApiKeyGateway.Validation;

namespace api_key_gateway.Tests
{
    public class ApiKeyValidatorTests
    {
        [Fact]
        public void ValidateKeyFormat_ValidKey_ReturnsSuccess()
        {
            // Arrange
            string validKey = "Abcdefghijklmnopqrstuvwxyz123!@#"; // 32 chars, mixed types

            // Act
            var result = ApiKeyValidator.ValidateKeyFormat(validKey);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateKeyFormat_NullKey_ReturnsFailure()
        {
            // Act
            var result = ApiKeyValidator.ValidateKeyFormat(null!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("cannot be empty", result.Message);
        }

        [Fact]
        public void ValidateKeyFormat_ShortKey_ReturnsFailure()
        {
            // Arrange
            string shortKey = "Short1!"; // Less than 32 chars

            // Act
            var result = ApiKeyValidator.ValidateKeyFormat(shortKey);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("at least 32", result.Message);
        }

        [Fact]
        public void ValidateKeyFormat_InsufficientEntropy_ReturnsFailure()
        {
            // Arrange
            string weakKey = "abcdefghijklmnopqrstuvwxyz123456"; // 32 chars, only lower/digit

            // Act
            var result = ApiKeyValidator.ValidateKeyFormat(weakKey);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("mix of uppercase", result.Message);
        }

        [Fact]
        public void ValidateKeyName_ValidName_ReturnsSuccess()
        {
            // Arrange
            string validName = "Valid_Name-123";

            // Act
            var result = ApiKeyValidator.ValidateKeyName(validName);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateKeyName_InvalidCharacters_ReturnsFailure()
        {
            // Arrange
            string invalidName = "Invalid@Name";

            // Act
            var result = ApiKeyValidator.ValidateKeyName(invalidName);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("only contain letters", result.Message);
        }

        [Fact]
        public void ValidateQuotaLimit_Unlimited_ReturnsSuccess()
        {
            // Act
            var result = ApiKeyValidator.ValidateQuotaLimit(-1);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateQuotaLimit_Zero_ReturnsFailure()
        {
            // Act
            var result = ApiKeyValidator.ValidateQuotaLimit(0);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("greater than 0", result.Message);
        }
    }
}
