using System.Text.Json;
using Xunit;
using FluentAssertions;
using ApiKeyGateway.Services;

namespace api_key_gateway.Tests
{
    public class RotationResultJsonExtensionsTests
    {
        [Fact]
        public void ToJson_ValidResult_ReturnsExpectedJson()
        {
            var result = new RotationResult 
            { 
                OldKeyId = "old", 
                NewKeyId = "new", 
                ConsumerId = "consumer" 
            };
            
            var json = RotationResultJsonExtensions.ToJson(result);
            
            json.Should().Contain("\"oldKeyId\":\"old\"");
            json.Should().Contain("\"newKeyId\":\"new\"");
            json.Should().Contain("\"consumerId\":\"consumer\"");
        }

        [Fact]
        public void ToJson_NullResult_ThrowsArgumentNullException()
        {
            Action act = () => RotationResultJsonExtensions.ToJson(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsExpectedResult()
        {
            var json = "{\"oldKeyId\":\"old\",\"newKeyId\":\"new\",\"consumerId\":\"consumer\"}";
            
            var result = RotationResultJsonExtensions.FromJson(json);
            
            result.Should().NotBeNull();
            result!.OldKeyId.Should().Be("old");
            result.NewKeyId.Should().Be("new");
            result.ConsumerId.Should().Be("consumer");
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            var json = "invalid";
            
            Action act = () => RotationResultJsonExtensions.FromJson(json);
            
            act.Should().Throw<JsonException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void FromJson_EmptyOrWhitespaceJson_ThrowsArgumentException(string json)
        {
            Action act = () => RotationResultJsonExtensions.FromJson(json);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrue()
        {
            var json = "{\"oldKeyId\":\"old\",\"newKeyId\":\"new\",\"consumerId\":\"consumer\"}";
            
            var success = RotationResultJsonExtensions.TryFromJson(json, out var result);
            
            success.Should().BeTrue();
            result.Should().NotBeNull();
            result!.OldKeyId.Should().Be("old");
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            var json = "invalid";
            
            var success = RotationResultJsonExtensions.TryFromJson(json, out var result);
            
            success.Should().BeFalse();
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void TryFromJson_EmptyOrWhitespaceJson_ThrowsArgumentException(string json)
        {
            Action act = () => RotationResultJsonExtensions.TryFromJson(json, out _);
            
            act.Should().Throw<ArgumentException>();
        }
    }
}
