using System;
using ApiKeyGateway.Domain.Exceptions;
using Xunit;

namespace ApiKeyGateway.Tests
{
    public class InvalidApiKeyExceptionTests
    {
        [Fact]
        public void Constructor_MessageOnly_SetsProperties()
        {
            // Arrange
            var before = DateTime.UtcNow;
            var message = "Invalid key";

            // Act
            var ex = new InvalidApiKeyException(message);
            var after = DateTime.UtcNow;

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Null(ex.ApiKeyHash);
            Assert.False(ex.IsExpired);
            Assert.InRange(ex.OccurredAt, before, after);
        }

        [Fact]
        public void Constructor_MessageAndApiKeyHash_SetsHashAndProperties()
        {
            // Arrange
            var before = DateTime.UtcNow;
            var message = "Invalid key";
            var hash = "abc123";

            // Act
            var ex = new InvalidApiKeyException(message, hash);
            var after = DateTime.UtcNow;

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal(hash, ex.ApiKeyHash);
            Assert.False(ex.IsExpired);
            Assert.InRange(ex.OccurredAt, before, after);
        }

        [Fact]
        public void Constructor_MessageAndApiKeyHash_NullHash_LeavesHashNull()
        {
            // Arrange
            var message = "Invalid key";

            // Act
            var ex = new InvalidApiKeyException(message, (string?)null);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Null(ex.ApiKeyHash);
        }

        [Fact]
        public void Constructor_MessageAndInnerException_SetsInnerException()
        {
            // Arrange
            var before = DateTime.UtcNow;
            var message = "Invalid key";
            var inner = new InvalidOperationException("inner");

            // Act
            var ex = new InvalidApiKeyException(message, inner);
            var after = DateTime.UtcNow;

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Same(inner, ex.InnerException);
            Assert.InRange(ex.OccurredAt, before, after);
        }

        [Fact]
        public void Constructor_MessageAndIsExpired_SetsIsExpiredFlag()
        {
            // Arrange
            var before = DateTime.UtcNow;
            var message = "Invalid key";
            var isExpired = true;

            // Act
            var ex = new InvalidApiKeyException(message, isExpired);
            var after = DateTime.UtcNow;

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.True(ex.IsExpired);
            Assert.InRange(ex.OccurredAt, before, after);
        }
    }
}
