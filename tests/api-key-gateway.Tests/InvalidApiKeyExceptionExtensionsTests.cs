using System;
using ApiKeyGateway.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests
{
    public class InvalidApiKeyExceptionExtensionsTests
    {
        [Fact]
        public void IsKeyExpired_ReturnsTrue_WhenExceptionIsExpired()
        {
            // Arrange
            var exception = new InvalidApiKeyException("Test", true);

            // Act
            var result = exception.IsKeyExpired();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsKeyExpired_ReturnsFalse_WhenExceptionIsNotExpired()
        {
            // Arrange
            var exception = new InvalidApiKeyException("Test", false);

            // Act
            var result = exception.IsKeyExpired();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsKeyExpired_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            InvalidApiKeyException exception = null;

            // Act
            Action act = () => exception.IsKeyExpired();

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("exception");
        }

        [Fact]
        public void GetApiKeyHash_ReturnsHash_WhenExceptionHasHash()
        {
            // Arrange
            var hash = "abc123";
            var exception = new InvalidApiKeyException("Test", hash);

            // Act
            var result = exception.GetApiKeyHash();

            // Assert
            result.Should().Be(hash);
        }

        [Fact]
        public void GetApiKeyHash_ReturnsNull_WhenExceptionHasNoHash()
        {
            // Arrange
            var exception = new InvalidApiKeyException("Test");

            // Act
            var result = exception.GetApiKeyHash();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetApiKeyHash_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            InvalidApiKeyException exception = null;

            // Act
            Action act = () => exception.GetApiKeyHash();

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("exception");
        }

        [Fact]
        public void GetOccurredAt_ReturnsOccurredAt_WhenExceptionIsCreated()
        {
            // Arrange
            var before = DateTime.UtcNow;
            var exception = new InvalidApiKeyException("Test");
            var after = DateTime.UtcNow;

            // Act
            var result = exception.GetOccurredAt();

            // Assert
            result.Should().BeOnOrAfter(before);
            result.Should().BeOnOrBefore(after);
        }

        [Fact]
        public void GetOccurredAt_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            InvalidApiKeyException exception = null;

            // Act
            Action act = () => exception.GetOccurredAt();

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("exception");
        }

        [Fact]
        public void FormatForLogging_ReturnsFormattedString_WhenExceptionHasHash()
        {
            // Arrange
            var hash = "def456";
            var exception = new InvalidApiKeyException("Invalid key", hash)
            {
                IsExpired = true
            };

            // Act
            var result = exception.FormatForLogging();

            // Assert
            result.Should().Contain("InvalidApiKeyException: Invalid key");
            result.Should().Contain($"ApiKeyHash: {hash}");
            result.Should().Contain("IsExpired: True");
            result.Should().Contain("OccurredAt:");
        }

        [Fact]
        public void FormatForLogging_ReturnsFormattedString_WhenExceptionHasNoHash()
        {
            // Arrange
            var exception = new InvalidApiKeyException("Invalid key")
            {
                IsExpired = false
            };

            // Act
            var result = exception.FormatForLogging();

            // Assert
            result.Should().Contain("InvalidApiKeyException: Invalid key");
            result.Should().NotContain("ApiKeyHash:");
            result.Should().Contain("IsExpired: False");
            result.Should().Contain("OccurredAt:");
        }

        [Fact]
        public void FormatForLogging_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            InvalidApiKeyException exception = null;

            // Act
            Action act = () => exception.FormatForLogging();

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("exception");
        }

        [Fact]
        public void IsKeyDisabled_ReturnsTrue_WhenExceptionIsNotExpired()
        {
            // Arrange
            var exception = new InvalidApiKeyException("Test", false);

            // Act
            var result = exception.IsKeyDisabled();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsKeyDisabled_ReturnsFalse_WhenExceptionIsExpired()
        {
            // Arrange
            var exception = new InvalidApiKeyException("Test", true);

            // Act
            var result = exception.IsKeyDisabled();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsKeyDisabled_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            InvalidApiKeyException exception = null;

            // Act
            Action act = () => exception.IsKeyDisabled();

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("exception");
        }
    }
}