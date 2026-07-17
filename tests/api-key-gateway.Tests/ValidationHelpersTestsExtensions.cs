// =============================================================================
// Author: Automated Extension Generator
// =============================================================================

using System;
using ApiKeyGateway.Utilities;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides reusable assertion helpers for <see cref="ValidationHelpersTests"/>.
/// </summary>
public static class ValidationHelpersTestsExtensions
{
    /// <summary>
    /// Asserts that the result of <see cref="ValidationHelpers.IsValidEmail"/> matches the expected value.
    /// </summary>
    /// <param name="test">The test instance invoking the helper.</param>
    /// <param name="email">The email address to validate.</param>
    /// <param name="expected">The expected validation result.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="email"/> is <c>null</c> or empty.</exception>
    public static void AssertEmailValidity(this ValidationHelpersTests test, string email, bool expected)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentException.ThrowIfNullOrEmpty(email);

        var result = ValidationHelpers.IsValidEmail(email);
        result.Should().Be(expected);
    }

    /// <summary>
    /// Asserts that the result of <see cref="ValidationHelpers.IsValidApiKeyFormat"/> matches the expected value.
    /// </summary>
    /// <param name="test">The test instance invoking the helper.</param>
    /// <param name="apiKey">The API key string to validate.</param>
    /// <param name="expected">The expected validation result.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKey"/> is <c>null</c> or empty.</exception>
    public static void AssertApiKeyFormat(this ValidationHelpersTests test, string apiKey, bool expected)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentException.ThrowIfNullOrEmpty(apiKey);

        var result = ValidationHelpers.IsValidApiKeyFormat(apiKey);
        result.Should().Be(expected);
    }

    /// <summary>
    /// Asserts that the result of <see cref="ValidationHelpers.IsValidIpAddress"/> matches the expected value.
    /// </summary>
    /// <param name="test">The test instance invoking the helper.</param>
    /// <param name="ipAddress">The IP address string to validate.</param>
    /// <param name="expected">The expected validation result.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ipAddress"/> is <c>null</c> or empty.</exception>
    public static void AssertIpAddressValidity(this ValidationHelpersTests test, string ipAddress, bool expected)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentException.ThrowIfNullOrEmpty(ipAddress);

        var result = ValidationHelpers.IsValidIpAddress(ipAddress);
        result.Should().Be(expected);
    }

    /// <summary>
    /// Asserts that the result of <see cref="ValidationHelpers.IsValidGuid"/> matches the expected value.
    /// </summary>
    /// <param name="test">The test instance invoking the helper.</param>
    /// <param name="guidValue">The GUID string to validate.</param>
    /// <param name="expected">The expected validation result.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="guidValue"/> is <c>null</c> or empty.</exception>
    public static void AssertGuidValidity(this ValidationHelpersTests test, string guidValue, bool expected)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentException.ThrowIfNullOrEmpty(guidValue);

        var result = ValidationHelpers.IsValidGuid(guidValue);
        result.Should().Be(expected);
    }

    /// <summary>
    /// Asserts that the result of <see cref="ValidationHelpers.IsValidUrl"/> matches the expected value.
    /// </summary>
    /// <param name="test">The test instance invoking the helper.</param>
    /// <param name="url">The URL string to validate.</param>
    /// <param name="expected">The expected validation result.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> is <c>null</c> or empty.</exception>
    public static void AssertUrlValidity(this ValidationHelpersTests test, string url, bool expected)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentException.ThrowIfNullOrEmpty(url);

        var result = ValidationHelpers.IsValidUrl(url);
        result.Should().Be(expected);
    }

    /// <summary>
    /// Asserts that <see cref="ValidationHelpers.SanitizeInput"/> returns the expected sanitized string.
    /// </summary>
    /// <param name="test">The test instance invoking the helper.</param>
    /// <param name="input">The raw input string.</param>
    /// <param name="maxLength">The maximum allowed length for the sanitized output.</param>
    /// <param name="expected">The expected sanitized string.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="input"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is negative.</exception>
    public static void AssertSanitizedInput(this ValidationHelpersTests test, string input, int maxLength, string expected)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(input);
        if (maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength), "maxLength must be non-negative.");
        }

        var result = ValidationHelpers.SanitizeInput(input, maxLength);
        result.Should().Be(expected);
    }
}