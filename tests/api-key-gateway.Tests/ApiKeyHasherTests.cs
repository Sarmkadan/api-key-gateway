using System;
using System.Text;
using Xunit;
using ApiKeyGateway.Utilities;

namespace api_key_gateway.Tests
{
    public class ApiKeyHasherTests
    {
        private readonly IApiKeyHasher _hasher = ApiKeyHasherFactory.Create();

        [Fact]
        public void Hash_ProducesDifferentOutputForSameInput_RandomSaltYetVerifySucceeds()
        {
            // Arrange
            string apiKey = "test-api-key-123!@#";

            // Act
            string hash1 = _hasher.Hash(apiKey);
            string hash2 = _hasher.Hash(apiKey);

            // Assert
            Assert.NotEqual(hash1, hash2); // Different salts
            Assert.True(_hasher.Verify(apiKey, hash1));
            Assert.True(_hasher.Verify(apiKey, hash2));
        }

        [Fact]
        public void Verify_ReturnsFalseForWrongKeyAndTamperedHash()
        {
            // Arrange
            string apiKey = "test-api-key-123!@#";
            string wrongKey = "wrong-key-456$%^";
            string correctHash = _hasher.Hash(apiKey);

            // Act & Assert
            // Wrong key
            Assert.False(_hasher.Verify(wrongKey, correctHash));

            // Tampered hash
            string tamperedHash = correctHash.Substring(0, correctHash.Length - 1) + "x";
            Assert.False(_hasher.Verify(apiKey, tamperedHash));
        }

        [Fact]
        public void Hash_ThrowsArgumentException_OnNullOrEmptyInput()
        {
            // Arrange
            IApiKeyHasher hasher = ApiKeyHasherFactory.Create();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => hasher.Hash(null!));
            Assert.Throws<ArgumentException>(() => hasher.Hash(string.Empty));
        }

        [Fact]
        public void Verify_ThrowsArgumentException_OnNullOrEmptyInputs()
        {
            // Arrange
            IApiKeyHasher hasher = ApiKeyHasherFactory.Create();
            string validHash = _hasher.Hash("valid-key");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => hasher.Verify(null!, validHash));
            Assert.Throws<ArgumentException>(() => hasher.Verify(string.Empty, validHash));
            Assert.Throws<ArgumentException>(() => hasher.Verify("valid-key", null!));
            Assert.Throws<ArgumentException>(() => hasher.Verify("valid-key", string.Empty));
        }

        [Fact]
        public void GetHashVersion_ThrowsArgumentException_OnNullOrEmptyInput()
        {
            // Arrange
            IApiKeyHasher hasher = ApiKeyHasherFactory.Create();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => hasher.GetHashVersion(null!));
            Assert.Throws<ArgumentException>(() => hasher.GetHashVersion(string.Empty));
        }

        [Fact]
        public void GetHashVersion_ReturnsVersionPrefixForVersionedHash_AndEmptyStringForUnversioned()
        {
            // Arrange
            IApiKeyHasher hasher = ApiKeyHasherFactory.Create();
            string versionedHash = _hasher.HashWithVersion("test-key", "v2");
            string unversionedHash = "legacyhashwithoutversion"; // Simulate legacy format

            // Act
            string versionFromVersioned = hasher.GetHashVersion(versionedHash);
            string versionFromUnversioned = hasher.GetHashVersion(unversionedHash);

            // Assert
            Assert.Equal("v2", versionFromVersioned);
            Assert.Empty(versionFromUnversioned);
        }

        [Fact]
        public void HashWithVersion_ThrowsArgumentException_OnNullOrEmptyInputs()
        {
            // Arrange
            IApiKeyHasher hasher = ApiKeyHasherFactory.Create();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => hasher.HashWithVersion(null!, "v1"));
            Assert.Throws<ArgumentException>(() => hasher.HashWithVersion(string.Empty, "v1"));
            Assert.Throws<ArgumentException>(() => hasher.HashWithVersion("test-key", null!));
            Assert.Throws<ArgumentException>(() => hasher.HashWithVersion("test-key", string.Empty));
        }

        [Fact]
        public void HashWithVersion_RoundTripsThroughVerify()
        {
            // Arrange
            string apiKey = "test-api-key-123!@#";
            string version = "v3";

            // Act
            string hashed = _hasher.HashWithVersion(apiKey, version);
            bool verifyResult = _hasher.Verify(apiKey, hashed);

            // Assert
            Assert.True(verifyResult);
            Assert.Equal(version, _hasher.GetHashVersion(hashed));
        }
    }
}