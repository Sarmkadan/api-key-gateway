using Xunit;
using ApiKeyGateway.Domain.Exceptions;

namespace ApiKeyGateway.Tests
{
    public class ApiKeyGatewayExceptionExtensionsTests
    {
        [Fact]
        public void WithMessage_HappyPath_ReturnsNewException()
        {
            // Arrange
            var exception = new ApiKeyGatewayException("Test message");

            // Act
            var newException = ApiKeyGatewayExceptionExtensions.WithMessage(exception, "New message");

            // Assert
            Assert.NotNull(newException);
            Assert.Equal("New message", newException.Message);
        }

        [Fact]
        public void WithMessage_NullException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ApiKeyGatewayExceptionExtensions.WithMessage(null, "New message"));
        }

        [Fact]
        public void WithMessage_EmptyMessage_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => ApiKeyGatewayExceptionExtensions.WithMessage(new ApiKeyGatewayException("Test message"), string.Empty));
        }

        [Fact]
        public void WithErrorCode_HappyPath_ReturnsNewException()
        {
            // Arrange
            var exception = new ApiKeyGatewayException("Test message");

            // Act
            var newException = ApiKeyGatewayExceptionExtensions.WithErrorCode(exception, "New error code");

            // Assert
            Assert.NotNull(newException);
            Assert.Equal("New error code", newException.ErrorCode);
        }

        [Fact]
        public void WithErrorCode_NullException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ApiKeyGatewayExceptionExtensions.WithErrorCode(null, "New error code"));
        }

        [Fact]
        public void WithErrorCode_EmptyErrorCode_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => ApiKeyGatewayExceptionExtensions.WithErrorCode(new ApiKeyGatewayException("Test message"), string.Empty));
        }

        [Fact]
        public void WithInnerException_HappyPath_ReturnsNewException()
        {
            // Arrange
            var exception = new ApiKeyGatewayException("Test message");
            var innerException = new Exception("Inner exception");

            // Act
            var newException = ApiKeyGatewayExceptionExtensions.WithInnerException(exception, innerException);

            // Assert
            Assert.NotNull(newException);
            Assert.Equal(exception.Message, newException.Message);
            Assert.Equal(innerException, newException.InnerException);
        }

        [Fact]
        public void WithInnerException_NullException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ApiKeyGatewayExceptionExtensions.WithInnerException(null, new Exception("Inner exception")));
        }

        [Fact]
        public void WithInnerException_NullInnerException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ApiKeyGatewayExceptionExtensions.WithInnerException(new ApiKeyGatewayException("Test message"), null));
        }

        [Fact]
        public void HasErrorCode_HappyPath_ReturnsTrue()
        {
            // Arrange
            var exception = new ApiKeyGatewayException("Test message", "Test error code");

            // Act
            var result = ApiKeyGatewayExceptionExtensions.HasErrorCode(exception, "Test error code");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasErrorCode_NullException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ApiKeyGatewayExceptionExtensions.HasErrorCode(null, "Test error code"));
        }

        [Fact]
        public void HasErrorCode_EmptyErrorCode_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => ApiKeyGatewayExceptionExtensions.HasErrorCode(new ApiKeyGatewayException("Test message"), string.Empty));
        }
    }
}
