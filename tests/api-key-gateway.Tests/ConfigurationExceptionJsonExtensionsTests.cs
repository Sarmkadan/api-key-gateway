// SPDX-License-Identifier: MIT
// Unit tests for ApiKeyGateway.Domain.Exceptions.ConfigurationExceptionJsonExtensions
// ---------------------------------------------------------------

using System;
using ApiKeyGateway.Domain.Exceptions;
using Xunit;

namespace ApiKeyGateway.Tests
{
    public class ConfigurationExceptionJsonExtensionsTests
    {
        #region Helper

        private static ConfigurationException CreateException(
            string message = "Test configuration error",
            string? setting = "TestSetting",
            Exception? inner = null)
        {
            // Use the most specific constructor available
            return inner != null && setting != null
                ? new ConfigurationException(message, setting, inner)
                : setting != null
                    ? new ConfigurationException(message, setting)
                    : new ConfigurationException(message);
        }

        #endregion

        #region ToJson

        [Fact]
        public void ToJson_HappyPath_WithDefaultOptions_ReturnsJsonString()
        {
            // Arrange
            var exception = CreateException();

            // Act
            var json = ConfigurationExceptionJsonExtensions.ToJson(exception);

            // Assert
            Assert.NotNull(json);
            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
            Assert.Contains("Test configuration error", json);
        }

        [Fact]
        public void ToJson_HappyPath_WithIndented_ReturnsFormattedJsonString()
        {
            // Arrange
            var exception = CreateException();

            // Act
            var json = ConfigurationExceptionJsonExtensions.ToJson(exception, indented: true);

            // Assert
            Assert.NotNull(json);
            Assert.StartsWith("{\n", json);
            Assert.Contains("Test configuration error", json);
        }

        [Fact]
        public void ToJson_HappyPath_WithSettingProperty_ReturnsJsonWithSetting()
        {
            // Arrange
            var exception = new ConfigurationException("Missing API key", "ApiKeySettings");

            // Act
            var json = ConfigurationExceptionJsonExtensions.ToJson(exception);

            // Assert
            Assert.Contains("Missing API key", json);
            Assert.Contains("ApiKeySettings", json);
        }

        [Fact]
        public void ToJson_HappyPath_WithInnerException_ReturnsJsonWithInnerException()
        {
            // Arrange
            var inner = new InvalidOperationException("Inner error");
            var exception = new ConfigurationException("Outer error", "OuterSetting", inner);

            // Act
            var json = ConfigurationExceptionJsonExtensions.ToJson(exception);

            // Assert
            Assert.Contains("Outer error", json);
            Assert.Contains("OuterSetting", json);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ConfigurationExceptionJsonExtensions.ToJson(null));
        }

        #endregion

        #region FromJson

        [Fact]
        public void FromJson_HappyPath_WithMessage_ReturnsDeserializedException()
        {
            // Arrange
            var json = "{\"message\":\"Configuration value is missing\"}";
            var expected = new ConfigurationException("Configuration value is missing");

            // Act
            var deserializedException = ConfigurationExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserializedException);
            Assert.Equal("Configuration value is missing", deserializedException.Message);
        }

        [Fact]
        public void FromJson_HappyPath_WithSetting_ReturnsDeserializedExceptionWithSetting()
        {
            // Arrange
            var json = "{\"message\":\"Invalid setting\",\"setting\":\"DatabaseConnection\"}";

            // Act
            var deserializedException = ConfigurationExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserializedException);
            Assert.Equal("Invalid setting", deserializedException.Message);
            Assert.Equal("DatabaseConnection", deserializedException.Setting);
        }

        [Fact]
        public void FromJson_HappyPath_WithOnlyMessage_ReturnsDeserializedException()
        {
            // Arrange
            var json = "{\"message\":\"Error without setting\"}";

            // Act
            var deserializedException = ConfigurationExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserializedException);
            Assert.Equal("Error without setting", deserializedException.Message);
            Assert.Null(deserializedException.Setting);
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ConfigurationExceptionJsonExtensions.FromJson(null));
        }

        [Fact]
        public void FromJson_EmptyString_ReturnsNull()
        {
            // Act
            var deserializedException = ConfigurationExceptionJsonExtensions.FromJson("");

            // Assert
            Assert.Null(deserializedException);
        }

        [Fact]
        public void FromJson_WhitespaceString_ThrowsJsonException()
        {
            // Act and Assert
            Assert.Throws<System.Text.Json.JsonException>(
                () => ConfigurationExceptionJsonExtensions.FromJson("   "));
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidJson = "{invalid json}";

            // Act and Assert
            Assert.Throws<System.Text.Json.JsonException>(
                () => ConfigurationExceptionJsonExtensions.FromJson(invalidJson));
        }

        [Fact]
        public void FromJson_EmptyObject_ReturnsExceptionWithDefaultMessage()
        {
            // Arrange
            var json = "{}";

            // Act
            var deserializedException = ConfigurationExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserializedException);
            Assert.NotEqual(string.Empty, deserializedException.Message);
        }

        #endregion

        #region TryFromJson

        [Fact]
        public void TryFromJson_HappyPath_WithValidJson_ReturnsTrueAndDeserializedException()
        {
            // Arrange
            var json = "{\"message\":\"Valid configuration error\"}";

            // Act
            var success = ConfigurationExceptionJsonExtensions.TryFromJson(json, out var deserializedException);

            // Assert
            Assert.True(success);
            Assert.NotNull(deserializedException);
            Assert.Equal("Valid configuration error", deserializedException.Message);
        }

        [Fact]
        public void TryFromJson_HappyPath_WithSetting_ReturnsTrueAndDeserializedException()
        {
            // Arrange
            var json = "{\"message\":\"Setting error\",\"setting\":\"CacheTimeout\"}";

            // Act
            var success = ConfigurationExceptionJsonExtensions.TryFromJson(json, out var deserializedException);

            // Assert
            Assert.True(success);
            Assert.NotNull(deserializedException);
            Assert.Equal("Setting error", deserializedException.Message);
            Assert.Equal("CacheTimeout", deserializedException.Setting);
        }

        [Fact]
        public void TryFromJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(
                () => ConfigurationExceptionJsonExtensions.TryFromJson(null, out _));
        }

        [Fact]
        public void TryFromJson_EmptyString_ReturnsTrueAndNull()
        {
            // Act
            var success = ConfigurationExceptionJsonExtensions.TryFromJson("", out var deserializedException);

            // Assert
            Assert.True(success);
            Assert.Null(deserializedException);
        }

        [Fact]
        public void TryFromJson_WhitespaceString_ReturnsFalseAndNull()
        {
            // Act
            var success = ConfigurationExceptionJsonExtensions.TryFromJson("   ", out var deserializedException);

            // Assert
            Assert.False(success);
            Assert.Null(deserializedException);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var invalidJson = "{invalid json}";

            // Act
            var success = ConfigurationExceptionJsonExtensions.TryFromJson(invalidJson, out var deserializedException);

            // Assert
            Assert.False(success);
            Assert.Null(deserializedException);
        }

        [Fact]
        public void TryFromJson_EmptyObject_ReturnsTrueAndDeserializedException()
        {
            // Arrange
            var json = "{}";

            // Act
            var success = ConfigurationExceptionJsonExtensions.TryFromJson(json, out var deserializedException);

            // Assert
            Assert.True(success);
            Assert.NotNull(deserializedException);
            Assert.NotEqual(string.Empty, deserializedException.Message);
        }

        #endregion
    }
}