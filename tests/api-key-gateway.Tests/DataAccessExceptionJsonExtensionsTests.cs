using Xunit;
using System.Text.Json;
using ApiKeyGateway.Domain.Exceptions;

namespace ApiKeyGateway.Tests
{
    public class DataAccessExceptionJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var exception = new DataAccessException("Test message");

            // Act
            var json = DataAccessExceptionJsonExtensions.ToJson(exception);

            // Assert
            Assert.NotNull(json);
            Assert.StartsWith("{\"message\":\"", json);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => DataAccessExceptionJsonExtensions.ToJson(null));
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsDeserializedException()
        {
            // Arrange
            var json = "{\"message\":\"Test message\"}";
            var expectedException = new DataAccessException("Test message");

            // Act
            var deserializedException = DataAccessExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserializedException);
            Assert.Equal(expectedException.Message, deserializedException.Message);
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => DataAccessExceptionJsonExtensions.FromJson(null));
        }

        [Fact]
        public void FromJson_EmptyJson_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => DataAccessExceptionJsonExtensions.FromJson(""));
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"message\":\"Test message\"}";

            // Act
            var success = DataAccessExceptionJsonExtensions.TryFromJson(json, out var deserializedException);

            // Assert
            Assert.True(success);
            Assert.NotNull(deserializedException);
            Assert.Equal("Test message", deserializedException.Message);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsFalse()
        {
            // Act
            var success = DataAccessExceptionJsonExtensions.TryFromJson(null, out var deserializedException);

            // Assert
            Assert.False(success);
            Assert.Null(deserializedException);
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Act
            var success = DataAccessExceptionJsonExtensions.TryFromJson("", out var deserializedException);

            // Assert
            Assert.False(success);
            Assert.Null(deserializedException);
        }
    }
}
