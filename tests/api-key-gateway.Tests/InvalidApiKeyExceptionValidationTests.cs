using Xunit;
using ApiKeyGateway.Domain.Exceptions;

namespace ApiKeyGateway.Tests
{
    public class InvalidApiKeyExceptionValidationTests
    {
        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var exception = new InvalidApiKeyException("Test message");

            // Act
            var problems = InvalidApiKeyExceptionValidation.Validate(exception);

            // Assert
            Assert.Empty(problems);
        }

        [Fact]
        public void Validate_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => InvalidApiKeyExceptionValidation.Validate(null));
        }

        [Fact]
        public void Validate_EmptyMessage_ReturnsProblem()
        {
            // Arrange
            var exception = new InvalidApiKeyException(string.Empty);

            // Act
            var problems = InvalidApiKeyExceptionValidation.Validate(exception);

            // Assert
            Assert.Single(problems);
            Assert.Equal("Message cannot be null, empty, or whitespace.", problems[0]);
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var exception = new InvalidApiKeyException("Test message");

            // Act
            var isValid = InvalidApiKeyExceptionValidation.IsValid(exception);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => InvalidApiKeyExceptionValidation.IsValid(null));
        }

        [Fact]
        public void IsValid_InvalidMessage_ReturnsFalse()
        {
            // Arrange
            var exception = new InvalidApiKeyException(string.Empty);

            // Act
            var isValid = InvalidApiKeyExceptionValidation.IsValid(exception);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var exception = new InvalidApiKeyException("Test message");

            // Act and Assert
            InvalidApiKeyExceptionValidation.EnsureValid(exception);
        }

        [Fact]
        public void EnsureValid_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => InvalidApiKeyExceptionValidation.EnsureValid(null));
        }

        [Fact]
        public void EnsureValid_InvalidMessage_ThrowsArgumentException()
        {
            // Arrange
            var exception = new InvalidApiKeyException(string.Empty);

            // Act and Assert
            Assert.Throws<ArgumentException>(() => InvalidApiKeyExceptionValidation.EnsureValid(exception));
        }
    }
}
