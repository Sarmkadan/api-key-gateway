// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using ApiKeyGateway.Validation;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for validating request data using the <see cref="RequestValidator"/> class.
/// </summary>
public class RequestValidatorTests
{
    /// <summary>
    /// Tests the <see cref="RequestValidator.ValidateEmail(string)"/> method with various email inputs.
    /// </summary>
    /// <param name="email">The email string to validate.</param>
    /// <param name="expected">The expected validation result (true if valid, false otherwise).</param>
    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ValidateEmail_VariousInputs_ReturnsExpectedResult(string? email, bool expected)
    {
        var result = RequestValidator.ValidateEmail(email!);
        result.IsValid.Should().Be(expected);
    }

    /// <summary>
    /// Tests the <see cref="RequestValidator.ValidateUrl(string)"/> method with various URL inputs.
    /// </summary>
    /// <param name="url">The URL string to validate.</param>
    /// <param name="expected">The expected validation result (true if valid, false otherwise).</param>
    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://example.com", false)] // defaults to require https
    [InlineData("invalid-url", false)]
    [InlineData("", false)]
    public void ValidateUrl_VariousInputs_ReturnsExpectedResult(string url, bool expected)
    {
        var result = RequestValidator.ValidateUrl(url);
        result.IsValid.Should().Be(expected);
    }

    /// <summary>
    /// Tests the <see cref="RequestValidator.ValidateIpAddress(string)"/> method with various IP address inputs.
    /// </summary>
    /// <param name="ip">The IP address string to validate.</param>
    /// <param name="expected">The expected validation result (true if valid, false otherwise).</param>
    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("invalid-ip", false)]
    [InlineData("", false)]
    public void ValidateIpAddress_VariousInputs_ReturnsExpectedResult(string ip, bool expected)
    {
        var result = RequestValidator.ValidateIpAddress(ip);
        result.IsValid.Should().Be(expected);
    }

    /// <summary>
    /// Tests the <see cref="RequestValidator.ValidateLength(string, int, int)"/> method with valid length input.
    /// </summary>
    [Fact]
    public void ValidateLength_ValidLength_ReturnsTrue()
    {
        var result = RequestValidator.ValidateLength("hello", minLength: 3, maxLength: 10);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests the <see cref="RequestValidator.ValidateLength(string, int, int)"/> method with input that is too short.
    /// </summary>
    [Fact]
    public void ValidateLength_TooShort_ReturnsFalse()
    {
        var result = RequestValidator.ValidateLength("hi", minLength: 3);
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("at least 3");
    }

    /// <summary>
    /// Tests the <see cref="RequestValidator.ValidateRange(int, int, int)"/> method with a value within the valid range.
    /// </summary>
    [Fact]
    public void ValidateRange_ValidValue_ReturnsTrue()
    {
        var result = RequestValidator.ValidateRange(5, minimum: 1, maximum: 10);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests the <see cref="RequestValidator.ValidateRange(int, int, int)"/> method with a value outside the valid range.
    /// </summary>
    [Fact]
    public void ValidateRange_ValueOutOfRange_ReturnsFalse()
    {
        var result = RequestValidator.ValidateRange(15, minimum: 1, maximum: 10);
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("between 1 and 10");
    }

    /// <summary>
    /// Tests the <see cref="RequestValidator.ValidateGuid(Guid)"/> method with a valid GUID.
    /// </summary>
    [Fact]
    public void ValidateGuid_ValidGuid_ReturnsTrue()
    {
        var guid = Guid.NewGuid();
        var result = RequestValidator.ValidateGuid(guid);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests the <see cref="RequestValidator.ValidateGuid(Guid)"/> method with an empty GUID.
    /// </summary>
    [Fact]
    public void ValidateGuid_EmptyGuid_ReturnsFalse()
    {
        var result = RequestValidator.ValidateGuid(Guid.Empty);
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("required");
    }
}
