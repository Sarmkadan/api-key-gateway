// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Xunit;
using ApiKeyGateway.Utilities;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="CryptoHelpers"/> cryptography utility class methods.
/// Tests hashing determinism, different inputs producing different outputs, and edge cases.
/// </summary>
public class CryptoHelpersTests
{
    /// <summary>
    /// Tests that ComputeSha256Hash produces deterministic results for the same input.
    /// Ensures that the same input always produces the same hash output.
    /// </summary>
    [Fact]
    public void ComputeSha256Hash_Deterministic_ReturnsSameHashForSameInput()
    {
        // Arrange
        var input = "test input value";

        // Act - compute hash twice
        var hash1 = CryptoHelpers.ComputeSha256Hash(input);
        var hash2 = CryptoHelpers.ComputeSha256Hash(input);

        // Assert - both hashes should be identical
        hash1.Should().Be(hash2);
        hash1.Should().NotBeNullOrEmpty();
        hash2.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that different inputs produce different hash outputs.
    /// Ensures that hash collisions are not produced for different inputs.
    /// </summary>
    [Fact]
    public void ComputeSha256Hash_DifferentInputs_DifferentOutputs()
    {
        // Arrange
        var input1 = "first input";
        var input2 = "second input";

        // Act
        var hash1 = CryptoHelpers.ComputeSha256Hash(input1);
        var hash2 = CryptoHelpers.ComputeSha256Hash(input2);

        // Assert - hashes should be different
        hash1.Should().NotBe(hash2);
        hash1.Should().NotBeNullOrEmpty();
        hash2.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that ComputeSha256Hash handles empty strings correctly.
    /// Validates that empty input throws the expected ArgumentException.
    /// </summary>
    [Fact]
    public void ComputeSha256Hash_EmptyInput_ThrowsArgumentException()
    {
        // Arrange
        var emptyInput = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CryptoHelpers.ComputeSha256Hash(emptyInput));
    }

    /// <summary>
    /// Tests that ComputeSha256Hash handles whitespace-only strings correctly.
    /// Validates that whitespace input throws the expected ArgumentException.
    /// </summary>
    [Fact]
    public void ComputeSha256Hash_WhitespaceInput_ThrowsArgumentException()
    {
        // Arrange
        var whitespaceInput = "   ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CryptoHelpers.ComputeSha256Hash(whitespaceInput));
    }

    /// <summary>
    /// Tests that ComputeSha256Hash handles null input correctly.
    /// Validates that null input throws the expected ArgumentException.
    /// </summary>
    [Fact]
    public void ComputeSha256Hash_NullInput_ThrowsArgumentException()
    {
        // Arrange
        string nullInput = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CryptoHelpers.ComputeSha256Hash(nullInput));
    }

    /// <summary>
    /// Tests that VerifyHash returns false when comparing different inputs.
    /// Validates that hash verification correctly identifies mismatched inputs.
    /// </summary>
    [Fact]
    public void VerifyHash_DifferentInputs_ReturnsFalse()
    {
        // Arrange
        var input1 = "correct input";
        var input2 = "wrong input";
        var hash = CryptoHelpers.ComputeSha256Hash(input1);

        // Act
        var result = CryptoHelpers.VerifyHash(input2, hash);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that VerifyHash returns true when comparing matching inputs.
    /// Validates that hash verification correctly identifies matching inputs.
    /// </summary>
    [Fact]
    public void VerifyHash_MatchingInputs_ReturnsTrue()
    {
        // Arrange
        var input = "test input for verification";
        var hash = CryptoHelpers.ComputeSha256Hash(input);

        // Act
        var result = CryptoHelpers.VerifyHash(input, hash);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that VerifyHash handles empty input correctly.
    /// Validates that empty input returns false.
    /// </summary>
    [Fact]
    public void VerifyHash_EmptyInput_ReturnsFalse()
    {
        // Arrange
        var emptyInput = string.Empty;
        var validHash = "somehash";

        // Act
        var result = CryptoHelpers.VerifyHash(emptyInput, validHash);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that VerifyHash handles null input correctly.
    /// Validates that null input returns false.
    /// </summary>
    [Fact]
    public void VerifyHash_NullInput_ReturnsFalse()
    {
        // Arrange
        string nullInput = null!;
        var validHash = "somehash";

        // Act
        var result = CryptoHelpers.VerifyHash(nullInput, validHash);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that ComputeHmacSha256 produces different outputs for different messages.
    /// Validates that HMAC signatures are unique per message.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_DifferentMessages_DifferentSignatures()
    {
        // Arrange
        var message1 = "message one";
        var message2 = "message two";
        var secret = "shared secret";

        // Act
        var signature1 = CryptoHelpers.ComputeHmacSha256(message1, secret);
        var signature2 = CryptoHelpers.ComputeHmacSha256(message2, secret);

        // Assert
        signature1.Should().NotBe(signature2);
        signature1.Should().NotBeNullOrEmpty();
        signature2.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that ComputeHmacSha256 produces different outputs for different secrets.
    /// Validates that HMAC signatures depend on both message and secret.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_DifferentSecrets_DifferentSignatures()
    {
        // Arrange
        var message = "shared message";
        var secret1 = "secret one";
        var secret2 = "secret two";

        // Act
        var signature1 = CryptoHelpers.ComputeHmacSha256(message, secret1);
        var signature2 = CryptoHelpers.ComputeHmacSha256(message, secret2);

        // Assert
        signature1.Should().NotBe(signature2);
        signature1.Should().NotBeNullOrEmpty();
        signature2.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that ComputeHmacSha256 is deterministic for the same message and secret.
    /// Validates that HMAC computation is consistent.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_Deterministic_ReturnsSameSignature()
    {
        // Arrange
        var message = "consistent message";
        var secret = "consistent secret";

        // Act - compute twice
        var signature1 = CryptoHelpers.ComputeHmacSha256(message, secret);
        var signature2 = CryptoHelpers.ComputeHmacSha256(message, secret);

        // Assert
        signature1.Should().Be(signature2);
        signature1.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that ComputeHmacSha256 handles empty message correctly.
    /// Validates that empty message throws the expected ArgumentException.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_EmptyMessage_ThrowsArgumentException()
    {
        // Arrange
        var emptyMessage = string.Empty;
        var secret = "valid secret";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CryptoHelpers.ComputeHmacSha256(emptyMessage, secret));
    }

    /// <summary>
    /// Tests that ComputeHmacSha256 handles null message correctly.
    /// Validates that null message throws the expected ArgumentException.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_NullMessage_ThrowsArgumentException()
    {
        // Arrange
        string nullMessage = null!;
        var secret = "valid secret";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CryptoHelpers.ComputeHmacSha256(nullMessage, secret));
    }

    /// <summary>
    /// Tests that ComputeHmacSha256 handles empty secret correctly.
    /// Validates that empty secret throws the expected ArgumentException.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_EmptySecret_ThrowsArgumentException()
    {
        // Arrange
        var message = "valid message";
        var emptySecret = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CryptoHelpers.ComputeHmacSha256(message, emptySecret));
    }

    /// <summary>
    /// Tests that ComputeHmacSha256 handles null secret correctly.
    /// Validates that null secret throws the expected ArgumentException.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_NullSecret_ThrowsArgumentException()
    {
        // Arrange
        var message = "valid message";
        string nullSecret = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CryptoHelpers.ComputeHmacSha256(message, nullSecret));
    }

    /// <summary>
    /// Tests that GenerateSecureRandomString produces strings of correct length.
    /// Validates that the random string generator works correctly.
    /// </summary>
    /// <param name="length">The length of random string to generate.</param>
    [Theory]
    [InlineData(10)]
    [InlineData(32)]
    [InlineData(64)]
    public void GenerateSecureRandomString_ValidLength_ReturnsCorrectLength(int length)
    {
        // Act
        var randomString = CryptoHelpers.GenerateSecureRandomString(length);

        // Assert
        randomString.Should().NotBeNullOrEmpty();
        randomString.Length.Should().Be(length);

        // Verify it contains only valid characters (A-Z, a-z, 0-9)
        randomString.Should().MatchRegex("^[A-Za-z0-9]*$");
    }

    /// <summary>
    /// Tests that GenerateSecureRandomString throws ArgumentException for invalid lengths.
    /// Validates that the method properly validates input.
    /// </summary>
    /// <param name="length">The invalid length to test.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void GenerateSecureRandomString_InvalidLength_ThrowsArgumentException(int length)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CryptoHelpers.GenerateSecureRandomString(length));
    }

    /// <summary>
    /// Tests that GenerateSecureRandomString produces different strings on multiple calls.
    /// Validates that the random string generator produces varied output.
    /// </summary>
    [Fact]
    public void GenerateSecureRandomString_DifferentCalls_ProducesDifferentResults()
    {
        // Arrange
        var length = 32;

        // Act - generate multiple strings
        var string1 = CryptoHelpers.GenerateSecureRandomString(length);
        var string2 = CryptoHelpers.GenerateSecureRandomString(length);
        var string3 = CryptoHelpers.GenerateSecureRandomString(length);

        // Assert - all should be different (extremely high probability for cryptographic RNG)
        string1.Should().NotBe(string2);
        string2.Should().NotBe(string3);
        string1.Should().NotBe(string3);
    }
}