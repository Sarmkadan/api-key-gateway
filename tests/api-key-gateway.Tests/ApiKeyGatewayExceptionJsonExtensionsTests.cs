using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiKeyGateway.Domain.Exceptions;

namespace ApiKeyGateway.Tests
{
    public class ApiKeyGatewayExceptionJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var exception = new ApiKeyGatewayException("Test message");

            // Act
            var json = ApiKeyGatewayExceptionJsonExtensions.ToJson(exception);

            // Assert
            Assert.NotNull(json);
            Assert.StartsWith("{\"Message\":\"", json);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ApiKeyGatewayExceptionJsonExtensions.ToJson(null));
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsDeserializedException()
        {
            // Arrange
            var json = "{\"Message\":\"Test message\"}";
            var expectedException = new ApiKeyGatewayException("Test message");

            // Act
            var deserializedException = ApiKeyGatewayExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserializedException);
            Assert.Equal(expectedException.Message, deserializedException.Message);
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ApiKeyGatewayExceptionJsonExtensions.FromJson(null));
        }

        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Act
            var deserializedException = ApiKeyGatewayExceptionJsonExtensions.FromJson("");

            // Assert
            Assert.Null(deserializedException);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"Message\":\"Test message\"}";

            // Act
            var success = ApiKeyGatewayExceptionJsonExtensions.TryFromJson(json, out var deserializedException);

            // Assert
            Assert.True(success);
            Assert.NotNull(deserializedException);
            Assert.Equal("Test message", deserializedException.Message);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsFalse()
        {
            // Act
            var success = ApiKeyGatewayExceptionJsonExtensions.TryFromJson(null, out var deserializedException);

            // Assert
            Assert.False(success);
            Assert.Null(deserializedException);
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Act
            var success = ApiKeyGatewayExceptionJsonExtensions.TryFromJson("", out var deserializedException);

            // Assert
            Assert.False(success);
            Assert.Null(deserializedException);
        }
    }
}
