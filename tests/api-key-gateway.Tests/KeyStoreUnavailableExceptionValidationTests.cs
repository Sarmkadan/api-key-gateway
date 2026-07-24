using Xunit;
using ApiKeyGateway.Domain.Exceptions;

namespace ApiKeyGateway.Tests
{
    public class KeyStoreUnavailableExceptionValidationTests
    {
        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var exception = new KeyStoreUnavailableException("Test operation");

            // Act
            var result = KeyStoreUnavailableExceptionValidation.Validate(exception);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_NullException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => KeyStoreUnavailableExceptionValidation.Validate(null));
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var exception = new KeyStoreUnavailableException("Test operation");

            // Act
            var result = KeyStoreUnavailableExceptionValidation.IsValid(exception);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_NullException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => KeyStoreUnavailableExceptionValidation.IsValid(null));
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var exception = new KeyStoreUnavailableException("Test operation");

            // Act and Assert
            KeyStoreUnavailableExceptionValidation.EnsureValid(exception);
        }

        [Fact]
        public void EnsureValid_NullException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => KeyStoreUnavailableExceptionValidation.EnsureValid(null));
        }

        [Fact]
        public void EnsureValid_InvalidException_ThrowsArgumentException()
        {
            // Arrange
            var exception = new KeyStoreUnavailableException(string.Empty);

            // Act and Assert
            Assert.Throws<ArgumentException>(() => KeyStoreUnavailableExceptionValidation.EnsureValid(exception));
        }
    }
}
