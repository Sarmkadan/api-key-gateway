using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using ApiKeyGateway.Domain.Exceptions;
using Xunit;

namespace ApiKeyGateway.Tests
{
    public class RateLimitExceededExceptionExtensionsTests
    {
        private static RateLimitExceededException CreateException(
            string apiKeyId = "test-key",
            int limit = 10,
            int windowInSeconds = 60)
        {
            // The constructor used in the existing tests sets RetryAfter to a future time.
            return new RateLimitExceededException(apiKeyId, limit, windowInSeconds);
        }

        private static void SetRetryAfter(RateLimitExceededException ex, DateTime? value)
        {
            // The RetryAfter property is likely read‑only; use reflection to force a value for edge‑case tests.
            var prop = typeof(RateLimitExceededException).GetProperty(
                "RetryAfter",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (prop == null)
                throw new InvalidOperationException("RetryAfter property not found on RateLimitExceededException.");

            prop.SetValue(ex, value);
        }

        [Fact]
        public void GetRateLimitExceededMessage_HappyPath_ReturnsFormattedMessage()
        {
            // Arrange
            var ex = CreateException();

            var expectedRetryAfter = ex.RetryAfter!.Value
                .ToString("O", CultureInfo.InvariantCulture);

            var expected = $"Rate limit exceeded for API key '{ex.ApiKeyId}'. " +
                           $"Limit: {ex.Limit} per {ex.WindowInSeconds} seconds. " +
                           $"Retry after: {expectedRetryAfter}.";

            // Act
            var message = ex.GetRateLimitExceededMessage();

            // Assert
            Assert.Equal(expected, message);
        }

        [Fact]
        public void GetRateLimitExceededMessage_NullException_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((RateLimitExceededException)null!).GetRateLimitExceededMessage());
        }

        [Fact]
        public void IsRetryAfterExpired_WhenRetryAfterInFuture_ReturnsFalse()
        {
            // Arrange
            var ex = CreateException();

            // Act
            var result = ex.IsRetryAfterExpired();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsRetryAfterExpired_WhenRetryAfterIsNull_ReturnsFalse()
        {
            // Arrange
            var ex = CreateException();
            SetRetryAfter(ex, null);

            // Act
            var result = ex.IsRetryAfterExpired();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsRetryAfterExpired_NullException_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((RateLimitExceededException)null!).IsRetryAfterExpired());
        }

        [Fact]
        public void GetRateLimitHeaderValues_HappyPath_IncludesAllHeaders()
        {
            // Arrange
            var ex = CreateException();

            // Act
            var headers = ex.GetRateLimitHeaderValues();

            // Assert
            Assert.NotNull(headers);
            Assert.Equal(3, headers.Count); // Limit, Window, Retry-After

            Assert.Equal(ex.Limit.ToString(CultureInfo.InvariantCulture), headers["X-RateLimit-Limit"]);
            Assert.Equal(ex.WindowInSeconds.ToString(CultureInfo.InvariantCulture), headers["X-RateLimit-Window"]);
            Assert.Equal(
                ex.RetryAfter!.Value.ToString("O", CultureInfo.InvariantCulture),
                headers["Retry-After"]);
        }

        [Fact]
        public void GetRateLimitHeaderValues_WhenRetryAfterIsNull_OmitsRetryAfterHeader()
        {
            // Arrange
            var ex = CreateException();
            SetRetryAfter(ex, null);

            // Act
            var headers = ex.GetRateLimitHeaderValues();

            // Assert
            Assert.NotNull(headers);
            Assert.Equal(2, headers.Count); // Only Limit and Window

            Assert.Equal(ex.Limit.ToString(CultureInfo.InvariantCulture), headers["X-RateLimit-Limit"]);
            Assert.Equal(ex.WindowInSeconds.ToString(CultureInfo.InvariantCulture), headers["X-RateLimit-Window"]);
            Assert.DoesNotContain("Retry-After", headers);
        }

        [Fact]
        public void GetRateLimitHeaderValues_NullException_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((RateLimitExceededException)null!).GetRateLimitHeaderValues());
        }

        [Fact]
        public void GetRetryAfterMessage_WithRetryAfter_ReturnsFormattedMessage()
        {
            // Arrange
            var ex = CreateException();

            // Act
            var message = ex.GetRetryAfterMessage();

            // Assert
            var expected = $"Please retry after {ex.RetryAfter!.Value.ToString("O", CultureInfo.InvariantCulture)}.";
            Assert.Equal(expected, message);
        }

        [Fact]
        public void GetRetryAfterMessage_WithoutRetryAfter_ReturnsDefaultMessage()
        {
            // Arrange
            var ex = CreateException();
            SetRetryAfter(ex, null);

            // Act
            var message = ex.GetRetryAfterMessage();

            // Assert
            Assert.Equal("Please retry after the rate limit window has passed.", message);
        }

        [Fact]
        public void GetRetryAfterMessage_NullException_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((RateLimitExceededException)null!).GetRetryAfterMessage());
        }
    }
}
