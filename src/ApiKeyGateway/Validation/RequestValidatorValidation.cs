// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace ApiKeyGateway.Validation;

/// <summary>
/// Validation helpers for RequestValidator static methods.
/// Provides instance-based validation that wraps the static methods of RequestValidator.
/// </summary>
public static class RequestValidatorValidation
{
    /// <summary>
    /// Validates an email address using RequestValidator.ValidateEmail.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>An IReadOnlyList of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if email is null.</exception>
    public static IReadOnlyList<string> ValidateEmail(string email)
    {
        ArgumentNullException.ThrowIfNull(email);

        var result = RequestValidator.ValidateEmail(email);
        return result.IsValid ? Array.Empty<string>() : new[] { result.Message ?? "Email validation failed" };
    }

    /// <summary>
    /// Validates a URL using RequestValidator.ValidateUrl.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <param name="requireHttps">Whether HTTPS is required.</param>
    /// <returns>An IReadOnlyList of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if url is null.</exception>
    public static IReadOnlyList<string> ValidateUrl(string url, bool requireHttps)
    {
        ArgumentNullException.ThrowIfNull(url);

        var result = RequestValidator.ValidateUrl(url, requireHttps);
        return result.IsValid ? Array.Empty<string>() : new[] { result.Message ?? "URL validation failed" };
    }

    /// <summary>
    /// Validates an IP address using RequestValidator.ValidateIpAddress.
    /// </summary>
    /// <param name="ip">The IP address to validate.</param>
    /// <returns>An IReadOnlyList of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if ip is null.</exception>
    public static IReadOnlyList<string> ValidateIpAddress(string ip)
    {
        ArgumentNullException.ThrowIfNull(ip);

        var result = RequestValidator.ValidateIpAddress(ip);
        return result.IsValid ? Array.Empty<string>() : new[] { result.Message ?? "IP address validation failed" };
    }

    /// <summary>
    /// Validates a string length using RequestValidator.ValidateLength.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="minLength">Minimum length requirement.</param>
    /// <param name="maxLength">Maximum length requirement.</param>
    /// <param name="fieldName">Name of the field for error messages.</param>
    /// <returns>An IReadOnlyList of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> ValidateLength(
        string value,
        int minLength = 0,
        int maxLength = int.MaxValue,
        string fieldName = "Value")
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = RequestValidator.ValidateLength(value, minLength, maxLength, fieldName);
        return result.IsValid ? Array.Empty<string>() : new[] { result.Message ?? $"{fieldName} validation failed" };
    }

    /// <summary>
    /// Validates a number range using RequestValidator.ValidateRange.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="minimum">Minimum allowed value.</param>
    /// <param name="maximum">Maximum allowed value.</param>
    /// <param name="fieldName">Name of the field for error messages.</param>
    /// <returns>An IReadOnlyList of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateRange(
        int value,
        int minimum = int.MinValue,
        int maximum = int.MaxValue,
        string fieldName = "Value")
    {
        var result = RequestValidator.ValidateRange(value, minimum, maximum, fieldName);
        return result.IsValid ? Array.Empty<string>() : new[] { result.Message ?? $"{fieldName} validation failed" };
    }

    /// <summary>
    /// Validates a GUID using RequestValidator.ValidateGuid.
    /// </summary>
    /// <param name="value">The GUID to validate.</param>
    /// <param name="fieldName">Name of the field for error messages.</param>
    /// <returns>An IReadOnlyList of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGuid(Guid value, string fieldName = "ID")
    {
        var result = RequestValidator.ValidateGuid(value, fieldName);
        return result.IsValid ? Array.Empty<string>() : new[] { result.Message ?? $"{fieldName} validation failed" };
    }

    /// <summary>
    /// Determines whether an email address is valid.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if email is null.</exception>
    public static bool IsValidEmail(string email) => ValidateEmail(email).Count == 0;

    /// <summary>
    /// Determines whether a URL is valid.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <param name="requireHttps">Whether HTTPS is required.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if url is null.</exception>
    public static bool IsValidUrl(string url, bool requireHttps) => ValidateUrl(url, requireHttps).Count == 0;

    /// <summary>
    /// Determines whether an IP address is valid.
    /// </summary>
    /// <param name="ip">The IP address to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if ip is null.</exception>
    public static bool IsValidIpAddress(string ip) => ValidateIpAddress(ip).Count == 0;

    /// <summary>
    /// Determines whether a string length is valid.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="minLength">Minimum length requirement.</param>
    /// <param name="maxLength">Maximum length requirement.</param>
    /// <param name="fieldName">Name of the field for error messages.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidLength(
        string value,
        int minLength = 0,
        int maxLength = int.MaxValue,
        string fieldName = "Value")
        => ValidateLength(value, minLength, maxLength, fieldName).Count == 0;

    /// <summary>
    /// Determines whether a number is within range.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="minimum">Minimum allowed value.</param>
    /// <param name="maximum">Maximum allowed value.</param>
    /// <param name="fieldName">Name of the field for error messages.</param>
    /// <returns>True if valid; otherwise, false.</returns>
/// <exception cref="ArgumentNullException">Thrown if fieldName is null.</exception>
    public static bool IsValidRange(
        int value,
        int minimum = int.MinValue,
        int maximum = int.MaxValue,
        string fieldName = "Value")
        => ValidateRange(value, minimum, maximum, fieldName).Count == 0;

    /// <summary>
    /// Determines whether a GUID is valid.
    /// </summary>
    /// <param name="value">The GUID to validate.</param>
    /// <param name="fieldName">Name of the field for error messages.</param>
/// <exception cref="ArgumentNullException">Thrown if fieldName is null.</exception>
/// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidGuid(Guid value, string fieldName = "ID")
        => ValidateGuid(value, fieldName).Count == 0;

    /// <summary>
    /// Ensures that an email address is valid, throwing if not.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void EnsureValidEmail(string email)
    {
        var errors = ValidateEmail(email);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", errors));
        }
    }

    /// <summary>
    /// Ensures that a URL is valid, throwing if not.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <param name="requireHttps">Whether HTTPS is required.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void EnsureValidUrl(string url, bool requireHttps)
    {
        var errors = ValidateUrl(url, requireHttps);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", errors));
        }
    }

    /// <summary>
    /// Ensures that an IP address is valid, throwing if not.
    /// </summary>
    /// <param name="ip">The IP address to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void EnsureValidIpAddress(string ip)
    {
        var errors = ValidateIpAddress(ip);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", errors));
        }
    }

    /// <summary>
    /// Ensures that a string length is valid, throwing if not.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="minLength">Minimum length requirement.</param>
    /// <param name="maxLength">Maximum length requirement.</param>
    /// <param name="fieldName">Name of the field for error messages.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
  /// <exception cref="ArgumentNullException">Thrown if fieldName is null.</exception>
    public static void EnsureValidLength(
        string value,
        int minLength = 0,
        int maxLength = int.MaxValue,
        string fieldName = "Value")
    {
        var errors = ValidateLength(value, minLength, maxLength, fieldName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", errors));
        }
    }

    /// <summary>
    /// Ensures that a number is within range, throwing if not.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="minimum">Minimum allowed value.</param>
    /// <param name="maximum">Maximum allowed value.</param>
    /// <param name="fieldName">Name of the field for error messages.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
  /// <exception cref="ArgumentNullException">Thrown if fieldName is null.</exception>
    public static void EnsureValidRange(
        int value,
        int minimum = int.MinValue,
        int maximum = int.MaxValue,
        string fieldName = "Value")
    {
        var errors = ValidateRange(value, minimum, maximum, fieldName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", errors));
        }
    }

    /// <summary>
    /// Ensures that a GUID is valid, throwing if not.
    /// </summary>
    /// <param name="value">The GUID to validate.</param>
    /// <param name="fieldName">Name of the field for error messages.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
  /// <exception cref="ArgumentNullException">Thrown if fieldName is null.</exception>
    public static void EnsureValidGuid(Guid value, string fieldName = "ID")
    {
        var errors = ValidateGuid(value, fieldName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", errors));
        }
    }
}
