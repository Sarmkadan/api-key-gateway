// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Validation helpers for validating input parameters used with ValidationHelpers methods
/// </summary>
public static class ValidationHelpersValidation
{
    /// <summary>
    /// Validates an email address using ValidationHelpers.IsValidEmail
    /// </summary>
    /// <param name="email">The email address to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when email is null</exception>
    public static IReadOnlyList<string> ValidateEmail(string email)
    {
        ArgumentNullException.ThrowIfNull(email);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(email))
        {
            problems.Add("Email cannot be null or whitespace");
        }
        else if (!ValidationHelpers.IsValidEmail(email))
        {
            problems.Add("Email has invalid format");
        }
        else if (email.Length > 255)
        {
            problems.Add("Email exceeds maximum length of 255 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates an API key using ValidationHelpers.IsValidApiKeyFormat
    /// </summary>
    /// <param name="keyValue">The API key to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when keyValue is null</exception>
    public static IReadOnlyList<string> ValidateApiKey(string keyValue)
    {
        ArgumentNullException.ThrowIfNull(keyValue);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(keyValue))
        {
            problems.Add("API key cannot be null or whitespace");
        }
        else if (!ValidationHelpers.IsValidApiKeyFormat(keyValue))
        {
            problems.Add("API key has invalid format. Expected format: sk_[A-Za-z0-9]{32,}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates an IP address using ValidationHelpers.IsValidIpAddress
    /// </summary>
    /// <param name="ipAddress">The IP address to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when ipAddress is null</exception>
    public static IReadOnlyList<string> ValidateIpAddress(string ipAddress)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            problems.Add("IP address cannot be null or whitespace");
        }
        else if (!ValidationHelpers.IsValidIpAddress(ipAddress))
        {
            problems.Add("IP address has invalid format. Expected IPv4 format");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a GUID using ValidationHelpers.IsValidGuid
    /// </summary>
    /// <param name="value">The GUID to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> ValidateGuid(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (!ValidationHelpers.IsValidGuid(value))
        {
            problems.Add("GUID has invalid format");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a URL using ValidationHelpers.IsValidUrl
    /// </summary>
    /// <param name="url">The URL to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when url is null</exception>
    public static IReadOnlyList<string> ValidateUrl(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(url))
        {
            problems.Add("URL cannot be null or whitespace");
        }
        else if (!ValidationHelpers.IsValidUrl(url))
        {
            problems.Add("URL has invalid format. Expected http:// or https:// URL");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates and sanitizes input using ValidationHelpers.SanitizeInput
    /// </summary>
    /// <param name="input">The input to validate and sanitize</param>
    /// <param name="maxLength">Maximum allowed length</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is less than 1</exception>
    public static IReadOnlyList<string> ValidateSanitizeInput(string input, int maxLength = 1000)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 1);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(input))
        {
            problems.Add("Input cannot be null or whitespace");
        }
        else
        {
            var sanitized = ValidationHelpers.SanitizeInput(input, maxLength);
            if (sanitized.Length == 0 && !string.IsNullOrWhiteSpace(input))
            {
                problems.Add("Input was sanitized to empty string");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an email address is valid using ValidationHelpers.IsValidEmail
    /// </summary>
    /// <param name="email">The email address to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when email is null</exception>
    public static bool IsValidEmail(string email)
    {
        return ValidateEmail(email).Count == 0;
    }

    /// <summary>
    /// Checks if an API key is valid using ValidationHelpers.IsValidApiKeyFormat
    /// </summary>
    /// <param name="keyValue">The API key to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when keyValue is null</exception>
    public static bool IsValidApiKey(string keyValue)
    {
        return ValidateApiKey(keyValue).Count == 0;
    }

    /// <summary>
    /// Checks if an IP address is valid using ValidationHelpers.IsValidIpAddress
    /// </summary>
    /// <param name="ipAddress">The IP address to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when ipAddress is null</exception>
    public static bool IsValidIpAddress(string ipAddress)
    {
        return ValidateIpAddress(ipAddress).Count == 0;
    }

    /// <summary>
    /// Checks if a GUID is valid using ValidationHelpers.IsValidGuid
    /// </summary>
    /// <param name="value">The GUID to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValidGuid(string value)
    {
        return ValidateGuid(value).Count == 0;
    }

    /// <summary>
    /// Checks if a URL is valid using ValidationHelpers.IsValidUrl
    /// </summary>
    /// <param name="url">The URL to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when url is null</exception>
    public static bool IsValidUrl(string url)
    {
        return ValidateUrl(url).Count == 0;
    }

    /// <summary>
    /// Checks if input can be sanitized using ValidationHelpers.SanitizeInput
    /// </summary>
    /// <param name="input">The input to check</param>
    /// <param name="maxLength">Maximum allowed length</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is less than 1</exception>
    public static bool IsValidSanitizeInput(string input, int maxLength = 1000)
    {
        return ValidateSanitizeInput(input, maxLength).Count == 0;
    }

    /// <summary>
    /// Ensures an email address is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="email">The email address to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when email is null</exception>
    /// <exception cref="ArgumentException">Thrown when email is invalid</exception>
    public static void EnsureValidEmail(string email)
    {
        var problems = ValidateEmail(email);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Email validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures an API key is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="keyValue">The API key to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when keyValue is null</exception>
    /// <exception cref="ArgumentException">Thrown when API key is invalid</exception>
    public static void EnsureValidApiKey(string keyValue)
    {
        var problems = ValidateApiKey(keyValue);
        if (problems.Count > 0)
        {
            throw new ArgumentException("API key validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures an IP address is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="ipAddress">The IP address to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when ipAddress is null</exception>
    /// <exception cref="ArgumentException">Thrown when IP address is invalid</exception>
    public static void EnsureValidIpAddress(string ipAddress)
    {
        var problems = ValidateIpAddress(ipAddress);
        if (problems.Count > 0)
        {
            throw new ArgumentException("IP address validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures a GUID is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The GUID to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when GUID is invalid</exception>
    public static void EnsureValidGuid(string value)
    {
        var problems = ValidateGuid(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException("GUID validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures a URL is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="url">The URL to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when url is null</exception>
    /// <exception cref="ArgumentException">Thrown when URL is invalid</exception>
    public static void EnsureValidUrl(string url)
    {
        var problems = ValidateUrl(url);
        if (problems.Count > 0)
        {
            throw new ArgumentException("URL validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures input can be sanitized, throwing ArgumentException if not
    /// </summary>
    /// <param name="input">The input to validate</param>
    /// <param name="maxLength">Maximum allowed length</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is less than 1</exception>
    /// <exception cref="ArgumentException">Thrown when input cannot be sanitized</exception>
    public static void EnsureValidSanitizeInput(string input, int maxLength = 1000)
    {
        var problems = ValidateSanitizeInput(input, maxLength);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Input validation failed: " + string.Join("; ", problems));
        }
    }
}