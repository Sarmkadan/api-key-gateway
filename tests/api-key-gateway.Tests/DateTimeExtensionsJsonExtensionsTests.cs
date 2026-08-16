using System;
using System.Text.Json;
using Xunit;
using ApiKeyGateway.Utilities;

namespace api_key_gateway.Tests
{
    public class DateTimeExtensionsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_ReturnsValidJsonString()
        {
            // Arrange
            var date = new DateTime(2023, 10, 5, 14, 30, 0, DateTimeKind.Utc);

            // Act
            var json = date.ToJson();

            // Assert
            Assert.NotNull(json);
            Assert.Contains("2023-10-05T14:30:00Z", json);
        }

        [Fact]
        public void ToJson_WithIndented_ReturnsFormattedJson()
        {
            // Arrange
            var date = DateTime.Now;

            // Act
            var json = date.ToJson(indented: true);

            // Assert
            Assert.Contains("\n", json);
            Assert.Contains("  ", json);
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsDateTime()
        {
            // Arrange
            var expectedDate = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var json = JsonSerializer.Serialize(expectedDate);

            // Act
            var result = DateTimeExtensionsJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDate, result);
        }

        [Fact]
        public void FromJson_NullOrWhitespace_ReturnsNull()
        {
            // Act & Assert
            var result1 = DateTimeExtensionsJsonExtensions.FromJson(null);
            Assert.Null(result1);

            var result2 = DateTimeExtensionsJsonExtensions.FromJson("   ");
            Assert.Null(result2);
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidJson = "not-a-date";

            // Act & Assert
            Assert.Throws<JsonException>(() => DateTimeExtensionsJsonExtensions.FromJson(invalidJson));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndDate()
        {
            // Arrange
            var expectedDate = DateTime.Now;
            var json = JsonSerializer.Serialize(expectedDate);

            // Act
            var success = DateTimeExtensionsJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            Assert.Equal(expectedDate, result);
        }

        [Fact]
        public void TryFromJson_NullOrWhitespace_ReturnsTrueAndNull()
        {
            // Act
            var success1 = DateTimeExtensionsJsonExtensions.TryFromJson(null, out var result1);
            
            // Assert
            Assert.True(success1);
            Assert.Null(result1);

            // Act
            var success2 = DateTimeExtensionsJsonExtensions.TryFromJson("   ", out var result2);

            // Assert
            Assert.True(success2);
            Assert.Null(result2);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var invalidJson = "invalid-data";

            // Act
            var success = DateTimeExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }
    }
}
