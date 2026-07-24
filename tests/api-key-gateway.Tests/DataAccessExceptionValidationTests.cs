using Xunit;
using ApiKeyGateway.Domain.Exceptions;

namespace ApiKeyGateway.Tests
{
    public class DataAccessExceptionValidationTests
    {
        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var exception = new DataAccessException("Test message");

            // Act
            var problems = DataAccessExceptionValidation.Validate(exception);

            // Assert
            Assert.Empty(problems);
        }

        [Fact]
        public void Validate_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => DataAccessExceptionValidation.Validate(null));
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var exception = new DataAccessException("Test message");

            // Act
            var result = DataAccessExceptionValidation.IsValid(exception);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => DataAccessExceptionValidation.IsValid(null));
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var exception = new DataAccessException("Test message");

            // Act and Assert
            DataAccessExceptionValidation.EnsureValid(exception);
        }

        [Fact]
        public void EnsureValid_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => DataAccessExceptionValidation.EnsureValid(null));
        }

        [Fact]
        public void EnsureValid_InvalidInput_ThrowsArgumentException()
        {
            // Arrange
            var exception = new DataAccessException(null);

            // Act and Assert
            Assert.Throws<ArgumentException>(() => DataAccessExceptionValidation.EnsureValid(exception));
        }
    }
}
