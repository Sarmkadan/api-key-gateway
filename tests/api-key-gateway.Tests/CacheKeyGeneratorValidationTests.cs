using System;
using System.Collections.Generic;
using ApiKeyGateway.Caching;
using Xunit;

namespace api_key_gateway.Tests
{
    public class CacheKeyGeneratorValidationTests
    {
        [Fact]
        public void Validate_ApiKeyId_WithValidValue_ReturnsEmpty()
        {
            // Arrange
            var apiKeyId = "valid-key";

            // Act
            var result = CacheKeyGeneratorValidation.Validate(apiKeyId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_ApiKeyId_WithNull_ThrowsArgumentNullException()
        {
            // Arrange
            string? apiKeyId = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CacheKeyGeneratorValidation.Validate(apiKeyId!));
        }

        [Fact]
        public void Validate_ApiKeyId_WithEmptyString_ReturnsProblem()
        {
            // Arrange
            var apiKeyId = string.Empty;

            // Act
            var result = CacheKeyGeneratorValidation.Validate(apiKeyId);

            // Assert
            Assert.Single(result);
            Assert.Contains("ApiKeyId cannot be null or empty.", result);
        }

        [Fact]
        public void Validate_RateLimit_WithValidValues_ReturnsEmpty()
        {
            // Arrange
            var apiKeyId = "valid-key";
            var endpoint = "/api/test";

            // Act
            var result = CacheKeyGeneratorValidation.Validate(apiKeyId, endpoint);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_RateLimit_WithEmptyEndpoint_ReturnsProblem()
        {
            // Arrange
            var apiKeyId = "valid-key";
            var endpoint = string.Empty;

            // Act
            var result = CacheKeyGeneratorValidation.Validate(apiKeyId, endpoint);

            // Assert
            Assert.Single(result);
            Assert.Contains("Endpoint cannot be null or empty.", result);
        }

        [Fact]
        public void Validate_UsageStats_WithDefaultDate_ReturnsProblem()
        {
            // Arrange
            var apiKeyId = "valid-key";
            var date = default(DateTime);

            // Act
            var result = CacheKeyGeneratorValidation.Validate(apiKeyId, date);

            // Assert
            Assert.Single(result);
            Assert.Contains("Date cannot be default (Unix epoch).", result);
        }

        [Fact]
        public void Validate_EventId_WithEmptyGuid_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CacheKeyGeneratorValidation.Validate(emptyGuid));
        }

        [Fact]
        public void IsValid_ApiKeyId_WithEmptyString_ReturnsFalse()
        {
            // Arrange
            var apiKeyId = string.Empty;

            // Act
            var isValid = CacheKeyGeneratorValidation.IsValid(apiKeyId);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void EnsureValid_ApiKeyId_WithEmptyString_ThrowsArgumentException()
        {
            // Arrange
            var apiKeyId = string.Empty;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => CacheKeyGeneratorValidation.EnsureValid(apiKeyId));
            Assert.Contains("ApiKeyId cannot be null or empty.", ex.Message);
        }
    }
}
