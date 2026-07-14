// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using ApiKeyGateway.Validation;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Extension methods for <see cref="RequestValidatorTests"/> that provide additional test scenarios
/// and helper methods for validating request validator behavior.
/// </summary>
public static class RequestValidatorTestsExtensions
{
    /// <summary>
    /// Creates a collection of test cases for email validation covering common scenarios.
    /// </summary>
    /// <returns>An enumerable of tuples containing (email, expectedResult) pairs.</returns>
    public static IEnumerable<(string? Email, bool Expected)> CreateEmailValidationTestCases()
    {
        yield return ("test@example.com", true);
        yield return ("user.name+tag@example.co.uk", true);
        yield return ("user@sub.domain.com", true);
        yield return ("invalid-email", false);
        yield return ("@example.com", false);
        yield return ("user@", false);
        yield return ("", false);
        yield return (null, false);
    }

    /// <summary>
    /// Creates a collection of test cases for URL validation covering common scenarios.
    /// </summary>
    /// <returns>An enumerable of tuples containing (url, expectedResult) pairs.</returns>
    public static IEnumerable<(string Url, bool Expected)> CreateUrlValidationTestCases()
    {
        yield return ("https://example.com", true);
        yield return ("https://sub.domain.com/path?query=value#fragment", true);
        yield return ("https://localhost:5000", true);
        yield return ("http://example.com", false); // defaults to require https
        yield return ("ftp://example.com", false);
        yield return ("not-a-url", false);
        yield return ("", false);
    }

    /// <summary>
    /// Creates a collection of test cases for IP address validation covering common scenarios.
    /// </summary>
    /// <returns>An enumerable of tuples containing (ipAddress, expectedResult) pairs.</returns>
    public static IEnumerable<(string IpAddress, bool Expected)> CreateIpAddressValidationTestCases()
    {
        yield return ("192.168.1.1", true);
        yield return ("10.0.0.1", true);
        yield return ("172.16.0.1", true);
        yield return ("255.255.255.255", true);
        yield return ("0.0.0.0", true);
        yield return ("2001:0db8:85a3:0000:0000:8a2e:0370:7334", false); // IPv6 not supported
        yield return ("256.1.1.1", false);
        yield return ("192.168.1", false);
        yield return ("192.168.1.1.1", false);
        yield return ("", false);
    }

    /// <summary>
    /// Creates a collection of test cases for length validation covering common scenarios.
    /// </summary>
    /// <returns>An enumerable of tuples containing (value, minLength, maxLength, expectedResult) quadruples.</returns>
    public static IEnumerable<(string Value, int MinLength, int MaxLength, bool Expected)> CreateLengthValidationTestCases()
    {
        yield return ("hello", 3, 10, true);
        yield return ("hi", 3, 10, false);
        yield return ("hello world", 3, 10, false);
        yield return ("a", 1, 1, true);
        yield return ("", 0, 5, true);
        yield return ("toolong", 1, 5, false);
    }

    /// <summary>
    /// Creates a collection of test cases for range validation covering common scenarios.
    /// </summary>
    /// <returns>An enumerable of tuples containing (value, minimum, maximum, expectedResult) quadruples.</returns>
    public static IEnumerable<(int Value, int Minimum, int Maximum, bool Expected)> CreateRangeValidationTestCases()
    {
        yield return (5, 1, 10, true);
        yield return (1, 1, 10, true);
        yield return (10, 1, 10, true);
        yield return (0, 1, 10, false);
        yield return (11, 1, 10, false);
        yield return (int.MaxValue, int.MinValue, int.MaxValue, true);
        yield return (int.MinValue, int.MinValue, int.MaxValue, true);
    }

    /// <summary>
    /// Creates a collection of test cases for GUID validation covering common scenarios.
    /// </summary>
    /// <returns>An enumerable of tuples containing (guid, expectedResult) pairs.</returns>
    public static IEnumerable<(Guid Guid, bool Expected)> CreateGuidValidationTestCases()
    {
        yield return (Guid.NewGuid(), true);
        yield return (Guid.Empty, false);
        yield return (new Guid("00000000-0000-0000-0000-000000000000"), false);
    }

    /// <summary>
    /// Asserts that the validation result is valid and contains no error message.
    /// </summary>
    /// <param name="result">The validation result to assert.</param>
    /// <exception cref="ArgumentNullException">Thrown if result is null.</exception>
    public static void ShouldBeValid(this ValidationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        result.IsValid.Should().BeTrue("validation should succeed");
        result.Message.Should().BeNullOrEmpty("valid result should have no error message");
    }

    /// <summary>
    /// Asserts that the validation result is invalid and contains an error message.
    /// </summary>
    /// <param name="result">The validation result to assert.</param>
    /// <param name="expectedMessageFragment">Optional fragment of the expected error message.</param>
    /// <exception cref="ArgumentNullException">Thrown if result is null.</exception>
    public static void ShouldBeInvalid(this ValidationResult result, string? expectedMessageFragment = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        result.IsValid.Should().BeFalse("validation should fail");
        result.Message.Should().NotBeNullOrEmpty("invalid result should have an error message");

        if (expectedMessageFragment is not null)
        {
            result.Message.Should().Contain(expectedMessageFragment,
                $"error message should contain '{expectedMessageFragment}'");
        }
    }

    /// <summary>
    /// Creates a validation result with the specified validity and message.
    /// </summary>
    /// <param name="isValid">Whether the validation should succeed.</param>
    /// <param name="message">Optional error message.</param>
    /// <returns>A new validation result.</returns>
    public static ValidationResult CreateValidationResult(bool isValid, string? message = null)
    {
        return new ValidationResult
        {
            IsValid = isValid,
            Message = message
        };
    }
}