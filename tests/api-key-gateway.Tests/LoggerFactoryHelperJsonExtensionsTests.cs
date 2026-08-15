using ApiKeyGateway.Utilities;
using Xunit;
using static ApiKeyGateway.Utilities.LoggerFactoryHelperJsonExtensions;

namespace api_key_gateway.Tests
{
    public class LoggerFactoryHelperJsonExtensionsTests
    {
        private static LoggerFactoryConfiguration CreateConfig(string? level = "Information", bool debug = false, bool console = true)
        {
            return new LoggerFactoryConfiguration
            {
                DefaultLogLevel = level,
                DebugEnabled = debug,
                ConsoleEnabled = console
            };
        }

        [Fact]
        public void ToJson_SerializesCorrectly()
        {
            var config = CreateConfig("Debug", true, false);
            var json = config.ToJson();
            Assert.Contains(@"""defaultLogLevel"":""Debug""", json);
            Assert.Contains(@"""debugEnabled"":true", json);
            Assert.Contains(@"""consoleEnabled"":false", json);
        }

        [Fact]
        public void ToJson_ThrowsArgumentNullException_WhenNull()
        {
            LoggerFactoryConfiguration? config = null;
            Assert.Throws<ArgumentNullException>(() => config!.ToJson());
        }

        [Fact]
        public void FromJson_DeserializesCorrectly()
        {
            var json = @"{""defaultLogLevel"":""Warning"",""debugEnabled"":true,""consoleEnabled"":true}";
            var config = FromJson(json);
            Assert.NotNull(config);
            Assert.Equal("Warning", config.DefaultLogLevel);
            Assert.True(config.DebugEnabled);
            Assert.True(config.ConsoleEnabled);
        }

        [Fact]
        public void FromJson_ReturnsNull_OnInvalidJson()
        {
            var json = "invalid-json";
            var config = FromJson(json);
            Assert.Null(config);
        }

        [Fact]
        public void FromJson_ThrowsArgumentNullException_OnNull()
        {
            Assert.Throws<ArgumentNullException>(() => FromJson(null!));
        }

        [Fact]
        public void FromJson_ThrowsArgumentException_OnEmpty()
        {
            Assert.Throws<ArgumentException>(() => FromJson(""));
        }

        [Fact]
        public void TryFromJson_ReturnsTrue_OnValidJson()
        {
            var json = @"{""defaultLogLevel"":""Error"",""debugEnabled"":false,""consoleEnabled"":false}";
            var success = TryFromJson(json, out var config);
            Assert.True(success);
            Assert.NotNull(config);
            Assert.Equal("Error", config.DefaultLogLevel);
        }

        [Fact]
        public void TryFromJson_ReturnsFalse_OnInvalidJson()
        {
            var json = "invalid-json";
            var success = TryFromJson(json, out var config);
            Assert.False(success);
            Assert.Null(config);
        }

        [Fact]
        public void TryFromJson_ThrowsArgumentNullException_OnNullInput()
        {
            Assert.Throws<ArgumentNullException>(() => TryFromJson(null!, out _));
        }
    }
}
