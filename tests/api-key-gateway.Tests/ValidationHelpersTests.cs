// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Utilities;
using ApiKeyGateway.Validation;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

public class ValidationHelpersTests
{
    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("name.surname@domain.org", true)]
    [InlineData("userexample.com", false)]
    [InlineData("user@", false)]
    [InlineData("", false)]
    public void IsValidEmail_VariousFormats_ReturnsExpectedResult(string email, bool expected)
    {
        // Act
        var result = ValidationHelpers.IsValidEmail(email);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsValidApiKeyFormat_ValidSkPrefixWith32Chars_ReturnsTrue()
    {
        // Arrange - sk_ prefix followed by exactly 32 alphanumeric characters
        var key = "sk_ABCDEFGHIJKLMNOPQRSTUVWXYZabcdef";

        // Act
        var result = ValidationHelpers.IsValidApiKeyFormat(key);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("sk_short")]
    [InlineData("pk_ABCDEFGHIJKLMNOPQRSTUVWXYZabcdef")]
    [InlineData("")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdef")]
    public void IsValidApiKeyFormat_InvalidFormats_ReturnsFalse(string key)
    {
        // Act
        var result = ValidationHelpers.IsValidApiKeyFormat(key);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("192.168.1.100", true)]
    [InlineData("10.0.0.1", true)]
    [InlineData("0.0.0.0", true)]
    [InlineData("255.255.255.255", true)]
    [InlineData("192.168.1.256", false)]
    [InlineData("999.0.0.1", false)]
    [InlineData("192.168.1", false)]
    public void IsValidIpAddress_VariousAddresses_ReturnsExpectedResult(string ip, bool expected)
    {
        // Act
        var result = ValidationHelpers.IsValidIpAddress(ip);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void SanitizeInput_StringExceedingMaxLength_TruncatesToLimit()
    {
        // Arrange
        var input = new string('x', 2000);

        // Act
        var result = ValidationHelpers.SanitizeInput(input, maxLength: 100);

        // Assert
        result.Length.Should().Be(100);
    }

    [Fact]
    public void SanitizeInput_StringWithLeadingAndTrailingWhitespace_ReturnsTrimmedValue()
    {
        // Arrange
        var input = "  hello world  ";

        // Act
        var result = ValidationHelpers.SanitizeInput(input);

        // Assert
        result.Should().Be("hello world");
    }
}

public class ApiKeyValidatorTests
{
    [Fact]
    public void ValidateKeyFormat_KeyWithSufficientEntropy_ReturnsValid()
    {
        // Arrange - uppercase + lowercase + digits + special chars, 40+ chars
        var key = "Abc123!@#DefGhi456JklMno789Pqr$%^Stu012Vwx";

        // Act
        var result = ApiKeyValidator.ValidateKeyFormat(key);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Message.Should().BeNull();
    }

    [Fact]
    public void ValidateKeyFormat_KeyTooShort_ReturnsInvalidWithMessage()
    {
        // Arrange - below 32-character minimum
        var key = "Short1!";

        // Act
        var result = ApiKeyValidator.ValidateKeyFormat(key);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("32");
    }

    [Fact]
    public void ValidateKeyName_ValidName_ReturnsValid()
    {
        // Arrange
        var name = "Production API Key";

        // Act
        var result = ApiKeyValidator.ValidateKeyName(name);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateKeyName_NameWithSpecialChars_ReturnsInvalid()
    {
        // Arrange - special chars other than space, underscore, hyphen are disallowed
        var name = "My Key <script>";

        // Act
        var result = ApiKeyValidator.ValidateKeyName(name);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(1000, true)]
    [InlineData(1_000_000_000, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1_000_000_001, false)]
    public void ValidateQuotaLimit_VariousLimits_ReturnsExpectedValidity(int limit, bool expected)
    {
        // Act
        var result = ApiKeyValidator.ValidateQuotaLimit(limit);

        // Assert
        result.IsValid.Should().Be(expected);
    }
}
