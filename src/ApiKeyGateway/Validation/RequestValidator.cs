// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Text.RegularExpressions;

namespace ApiKeyGateway.Validation;

/// <summary>
/// Validator for common request parameters and values.
/// Provides reusable validation methods to prevent code duplication
/// across controllers. Keeps validation logic in one place for easy maintenance.
/// </summary>
public static class RequestValidator
{
    /// <summary>
    /// Validates an email address format.
    /// </summary>
    public static ValidationResult ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return new ValidationResult { IsValid = false, Message = "Email is required" };

        // Simple email validation - in production, use a library
        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(email, emailPattern))
            return new ValidationResult { IsValid = false, Message = "Email format is invalid" };

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Validates a URL is properly formatted and uses HTTPS.
    /// </summary>
    public static ValidationResult ValidateUrl(string url, bool requireHttps = true)
    {
        if (string.IsNullOrWhiteSpace(url))
            return new ValidationResult { IsValid = false, Message = "URL is required" };

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return new ValidationResult { IsValid = false, Message = "URL format is invalid" };

        if (requireHttps && uri.Scheme != "https")
            return new ValidationResult { IsValid = false, Message = "URL must use HTTPS" };

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Validates an IP address or IP range.
    /// </summary>
    public static ValidationResult ValidateIpAddress(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return new ValidationResult { IsValid = false, Message = "IP address is required" };

        if (!IPAddress.TryParse(ip, out _))
            return new ValidationResult { IsValid = false, Message = "IP address format is invalid" };

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Validates a string length is within bounds.
    /// </summary>
    public static ValidationResult ValidateLength(
        string value,
        int minLength = 0,
        int maxLength = int.MaxValue,
        string fieldName = "Value")
    {
        if (string.IsNullOrEmpty(value))
        {
            if (minLength > 0)
                return new ValidationResult { IsValid = false, Message = $"{fieldName} is required" };
            return new ValidationResult { IsValid = true };
        }

        if (value.Length < minLength)
            return new ValidationResult
            {
                IsValid = false,
                Message = $"{fieldName} must be at least {minLength} characters"
            };

        if (value.Length > maxLength)
            return new ValidationResult
            {
                IsValid = false,
                Message = $"{fieldName} cannot exceed {maxLength} characters"
            };

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Validates that a number is within a range.
    /// </summary>
    public static ValidationResult ValidateRange(
        int value,
        int minimum = int.MinValue,
        int maximum = int.MaxValue,
        string fieldName = "Value")
    {
        if (value < minimum || value > maximum)
            return new ValidationResult
            {
                IsValid = false,
                Message = $"{fieldName} must be between {minimum} and {maximum}"
            };

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Validates a GUID is not empty.
    /// </summary>
    public static ValidationResult ValidateGuid(Guid value, string fieldName = "ID")
    {
        if (value == Guid.Empty)
            return new ValidationResult { IsValid = false, Message = $"{fieldName} is required" };

        return new ValidationResult { IsValid = true };
    }
}
