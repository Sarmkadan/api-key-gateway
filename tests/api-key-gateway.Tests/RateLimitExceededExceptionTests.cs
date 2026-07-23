using Xunit;
using ApiKeyGateway.Domain.Exceptions;

namespace ApiKeyGateway.Tests
{
    public class RateLimitExceededExceptionTests
    {
        [Fact]
        public void Constructor_HappyPath_SetsProperties()
        {
            // Arrange
            var apiKeyId = "test-api-key";
            var limit = 10;
            var windowInSeconds = 60;

            // Act
            var exception = new RateLimitExceededException(apiKeyId, limit, windowInSeconds);

            // Assert
            Assert.Equal(apiKeyId, exception.ApiKeyId);
            Assert.Equal(limit, exception.Limit);
            Assert.Equal(windowInSeconds, exception.WindowInSeconds);
            Assert.NotNull(exception.RetryAfter);
        }

        [Fact]
        public void Constructor_NullApiKeyId_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new RateLimitExceededException(null, 10, 60));
        }

        [Fact]
        public void Constructor_NegativeLimit_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => new RateLimitExceededException("test-api-key", -1, 60));
        }

        [Fact]
        public void Constructor_NegativeWindowInSeconds_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => new RateLimitExceededException("test-api-key", 10, -1));
        }

        [Fact]
        public void RetryAfter_HappyPath_ReturnsExpectedValue()
        {
            // Arrange
            var apiKeyId = "test-api-key";
            var limit = 10;
            var windowInSeconds = 60;

            // Act
            var exception = new RateLimitExceededException(apiKeyId, limit, windowInSeconds);

            // Assert
            Assert.NotNull(exception.RetryAfter);
            Assert.True(exception.RetryAfter.Value > DateTime.UtcNow);
        }
    }
}
