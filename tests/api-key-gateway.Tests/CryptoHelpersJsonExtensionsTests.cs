// SPDX-License-Identifier: MIT
// Tests for CryptoHelpersJsonExtensions
// ------------------------------------------------------------

using System;
using ApiKeyGateway.Utilities;
using Xunit;

namespace api_key_gateway.Tests
{
    public class CryptoHelpersJsonExtensionsTests
    {
        private static readonly CryptoHelpersJsonExtensions.CryptoConfiguration DefaultConfig =
            new CryptoHelpersJsonExtensions.CryptoConfiguration();

        [Fact]
        public void CryptoConfiguration_DefaultValues_AreAsExpected()
        {
            Assert.Equal(32, DefaultConfig.SecureRandomStringLength);
            Assert.Equal(
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",
                DefaultConfig.SecureRandomChars);
        }

        [Fact]
        public void ToJson_NullValue_ThrowsArgumentNullException()
        {
            CryptoHelpersJsonExtensions.CryptoConfiguration? nullConfig = null;
            Assert.Throws<ArgumentNullException>(() => nullConfig!.ToJson());
        }

        [Fact]
        public void ToJson_HappyPath_ReturnsCamelCaseJson()
        {
            var config = new CryptoHelpersJsonExtensions.CryptoConfiguration
            {
                SecureRandomStringLength = 64,
                SecureRandomChars = "ABC123"
            };

            string json = config.ToJson();

            // Property names should be camelCase according to the serializer options
            Assert.Contains("\"secureRandomStringLength\":64", json);
            Assert.Contains("\"secureRandomChars\":\"ABC123\"", json);
        }

        [Fact]
        public void ToJson_Indented_ProducesReadableJson()
        {
            var config = new CryptoHelpersJsonExtensions.CryptoConfiguration
            {
                SecureRandomStringLength = 10,
                SecureRandomChars = "XYZ"
            };

            string json = config.ToJson(indented: true);

            // Indented JSON contains line breaks
            Assert.Contains(Environment.NewLine, json);
            // Still contains the expected values
            Assert.Contains("\"secureRandomStringLength\":10", json);
            Assert.Contains("\"secureRandomChars\":\"XYZ\"", json);
        }

        [Fact]
        public void FromJson_NullOrEmpty_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => CryptoHelpersJsonExtensions.FromJson(null!));
            Assert.Throws<ArgumentException>(() => CryptoHelpersJsonExtensions.FromJson(string.Empty));
        }

        [Fact]
        public void FromJson_MalformedJson_ReturnsNull()
        {
            const string malformed = "{ this is not json }";
            var result = CryptoHelpersJsonExtensions.FromJson(malformed);
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsEquivalentObject()
        {
            var original = new CryptoHelpersJsonExtensions.CryptoConfiguration
            {
                SecureRandomStringLength = 128,
                SecureRandomChars = "0123456789"
            };

            string json = original.ToJson();
            var deserialized = CryptoHelpersJsonExtensions.FromJson(json);

            Assert.NotNull(deserialized);
            Assert.Equal(original, deserialized);
        }

        [Fact]
        public void TryFromJson_NullOrEmpty_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => CryptoHelpersJsonExtensions.TryFromJson(null!, out _));
            Assert.Throws<ArgumentException>(() => CryptoHelpersJsonExtensions.TryFromJson(string.Empty, out _));
        }

        [Fact]
        public void TryFromJson_MalformedJson_ReturnsFalseAndNull()
        {
            const string malformed = "[ not json ]";
            bool success = CryptoHelpersJsonExtensions.TryFromJson(malformed, out var value);
            Assert.False(success);
            Assert.Null(value);
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndObject()
        {
            var original = new CryptoHelpersJsonExtensions.CryptoConfiguration
            {
                SecureRandomStringLength = 5,
                SecureRandomChars = "abcde"
            };

            string json = original.ToJson(indented: true);
            bool success = CryptoHelpersJsonExtensions.TryFromJson(json, out var value);

            Assert.True(success);
            Assert.NotNull(value);
            Assert.Equal(original, value);
        }
    }
}
