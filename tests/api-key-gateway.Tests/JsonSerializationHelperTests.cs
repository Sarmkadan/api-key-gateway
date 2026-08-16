using Xunit;
using System.Text.Json;
using ApiKeyGateway.Utilities;

namespace api_key_gateway.Tests
{
    public class JsonSerializationHelperTests
    {
        [Fact]
        public void SerializeCompact_HappyPath_ReturnsJson()
        {
            // Arrange
            var obj = new { Foo = "bar" };

            // Act
            var json = JsonSerializationHelper.SerializeCompact(obj);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("foo", json);
        }

        [Fact]
        public void SerializeFormatted_HappyPath_ReturnsJson()
        {
            // Arrange
            var obj = new { Foo = "bar" };

            // Act
            var json = JsonSerializationHelper.SerializeFormatted(obj);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("foo", json);
            Assert.Contains("\n", json);
        }

        [Fact]
        public void Deserialize_HappyPath_ReturnsObject()
        {
            // Arrange
            var json = "{\"foo\":\"bar\"}";

            // Act
            var obj = JsonSerializationHelper.Deserialize<dynamic>(json);

            // Assert
            Assert.NotNull(obj);
            Assert.Equal("bar", obj.foo);
        }

        [Fact]
        public void SafeDeserialize_HappyPath_ReturnsObject()
        {
            // Arrange
            var json = "{\"foo\":\"bar\"}";

            // Act
            var obj = JsonSerializationHelper.SafeDeserialize<dynamic>(json);

            // Assert
            Assert.NotNull(obj);
            Assert.Equal("bar", obj.foo);
        }

        [Fact]
        public void IsValidJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"foo\":\"bar\"}";

            // Act
            var result = JsonSerializationHelper.IsValidJson(json);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidJson_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var json = "{\"foo\":\"bar\"";

            // Act
            var result = JsonSerializationHelper.IsValidJson(json);

            // Assert
            Assert.False(result);
        }
    }
}
