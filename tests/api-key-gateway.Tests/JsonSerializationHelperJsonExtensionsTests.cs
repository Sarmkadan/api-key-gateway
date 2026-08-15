using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Xunit;
using ApiKeyGateway.Utilities;

namespace ApiKeyGateway.Tests
{
    public class JsonSerializationHelperJsonExtensionsTests
    {
        [Fact]
        public void JsonSerializationSettings_ShouldHaveCorrectDefaultValues()
        {
            var settings = new JsonSerializationHelperJsonExtensions.JsonSerializationSettings();
            
            settings.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
            settings.DefaultIgnoreCondition.Should().Be(JsonIgnoreCondition.WhenWritingNull);
            settings.WriteIndented.Should().BeFalse();
        }

        [Fact]
        public void ToJson_ValidSettings_ReturnsJsonString()
        {
            var settings = new JsonSerializationHelperJsonExtensions.JsonSerializationSettings();
            
            var json = settings.ToJson();
            
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("\"writeIndented\":false");
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            Action act = () => JsonSerializationHelperJsonExtensions.ToJson(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsSettings()
        {
            // Excluding PropertyNamingPolicy from JSON as it cannot be deserialized automatically
            var json = "{\"writeIndented\":true}";
            
            var settings = JsonSerializationHelperJsonExtensions.FromJson(json);
            
            settings.Should().NotBeNull();
            settings!.WriteIndented.Should().BeTrue();
        }

        [Fact]
        public void FromJson_InvalidJson_ReturnsNull()
        {
            var json = "{invalid}";
            
            var settings = JsonSerializationHelperJsonExtensions.FromJson(json);
            
            settings.Should().BeNull();
        }

        [Fact]
        public void FromJson_EmptyString_ThrowsArgumentException()
        {
            Action act = () => JsonSerializationHelperJsonExtensions.FromJson(string.Empty);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndSettings()
        {
            // Excluding PropertyNamingPolicy from JSON
            var json = "{\"writeIndented\":true}";
            
            var success = JsonSerializationHelperJsonExtensions.TryFromJson(json, out var settings);
            
            success.Should().BeTrue();
            settings.Should().NotBeNull();
            settings!.WriteIndented.Should().BeTrue();
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            var json = "{invalid}";
            
            var success = JsonSerializationHelperJsonExtensions.TryFromJson(json, out var settings);
            
            success.Should().BeFalse();
            settings.Should().BeNull();
        }
    }
}
