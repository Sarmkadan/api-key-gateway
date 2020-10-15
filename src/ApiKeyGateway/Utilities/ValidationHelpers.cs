// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Helper methods for validating input data
/// </summary>
public static class ValidationHelpers
{
    private static readonly Regex EmailRegex = new(
        @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ApiKeyFormatRegex = new(
        @"^sk_[A-Za-z0-9]{32,}$",
        RegexOptions.Compiled);

    private static readonly Regex IpAddressRegex = new(
        @"^(\d{1,3}\.){3}\d{1,3}$",
        RegexOptions.Compiled);

    /// <summary>
    /// Validates an email address format
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex.IsMatch(email) && email.Length <= 255;
    }

    /// <summary>
    /// Validates API key format
    /// </summary>
    public static bool IsValidApiKeyFormat(string keyValue)
    {
        if (string.IsNullOrWhiteSpace(keyValue))
            return false;

        return ApiKeyFormatRegex.IsMatch(keyValue);
    }

    /// <summary>
    /// Validates an IP address format
    /// </summary>
    public static bool IsValidIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        if (!IpAddressRegex.IsMatch(ipAddress))
            return false;

        var parts = ipAddress.Split('.');
        return parts.All(part => int.TryParse(part, out var num) && num >= 0 && num <= 255);
    }

    /// <summary>
    /// Validates a GUID format
    /// </summary>
    public static bool IsValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }

    /// <summary>
    /// Validates a URL format
    /// </summary>
    public static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Sanitizes a string for safe storage
    /// </summary>
    public static string SanitizeInput(string input, int maxLength = 1000)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var trimmed = input.Trim();
        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }
}
