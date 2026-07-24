using Xunit;
using ApiKeyGateway.Domain.Exceptions;

namespace ApiKeyGateway.Tests
{
    public class ValidationExceptionTests
    {
        [Fact]
        public void Constructor_HappyPath_SetsProperties()
        {
            // Arrange
            var message = "Test message";
            var exception = new ValidationException(message);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
        }

        [Fact]
        public void Constructor_ParameterName_HappyPath_SetsProperties()
        {
            // Arrange
            var message = "Test message";
            var parameterName = "Test parameter";
            var attemptedValue = "Test value";
            var exception = new ValidationException(message, parameterName, attemptedValue);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
            Assert.Equal(parameterName, exception.ParameterName);
            Assert.Equal(attemptedValue, exception.AttemptedValue);
        }

        [Fact]
        public void Constructor_ValidationErrors_HappyPath_SetsProperties()
        {
            // Arrange
            var message = "Test message";
            var validationErrors = new[] { "Error 1", "Error 2" };
            var exception = new ValidationException(message, validationErrors);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
            Assert.Equal(validationErrors, exception.ValidationErrors);
        }

        [Fact]
        public void Constructor_InnerException_HappyPath_SetsProperties()
        {
            // Arrange
            var message = "Test message";
            var innerException = new Exception("Inner exception");
            var exception = new ValidationException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
            Assert.Equal(innerException, exception.InnerException);
        }

        [Fact]
        public void ParameterName_Null_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationException(null, "Test parameter", "Test value"));
        }

        [Fact]
        public void AttemptedValue_Null_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationException("Test message", "Test parameter", null));
        }

        [Fact]
        public void ValidationErrors_Null_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationException("Test message", (IEnumerable<string>)null));
        }

        [Fact]
        public void ValidationErrors_Empty_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => new ValidationException("Test message", new List<string>()));
        }
    }
}
