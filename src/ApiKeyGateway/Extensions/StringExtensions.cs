// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace ApiKeyGateway.Extensions;

/// <summary>
/// Extension methods for string operations.
/// Provides common string manipulation and validation utilities.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Safely truncates a string to maximum length.
    /// </summary>
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    /// <summary>
    /// Truncates with ellipsis suffix.
    /// </summary>
    public static string TruncateWithEllipsis(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength - 3) + "...";
    }

    /// <summary>
    /// Checks if string contains any of the provided values.
    /// </summary>
    public static bool ContainsAny(this string value, params string[] searches) =>
        searches.Any(s => value.Contains(s, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Checks if string starts with any of the provided values.
    /// </summary>
    public static bool StartsWithAny(this string value, params string[] prefixes) =>
        prefixes.Any(p => value.StartsWith(p, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Converts string to a slug suitable for URLs.
    /// Removes spaces, special chars, converts to lowercase.
    /// </summary>
    public static string ToSlug(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Convert to lowercase
        value = value.ToLowerInvariant();

        // Remove special characters
        var sb = new StringBuilder();
        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c) || c is '-' or '_')
                sb.Append(c);
            else if (char.IsWhiteSpace(c))
                sb.Append('-');
        }

        // Remove consecutive dashes
        var result = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), @"-+", "-");

        // Remove leading/trailing dashes
        return result.Trim('-');
    }

    /// <summary>
    /// Capitalizes first letter of string.
    /// </summary>
    public static string CapitalizeFirst(this string value) =>
        string.IsNullOrEmpty(value)
            ? value
            : char.ToUpper(value[0]) + value.Substring(1);

    /// <summary>
    /// Converts a delimited string to a list.
    /// </summary>
    public static List<string> ToList(this string value, char delimiter = ',') =>
        string.IsNullOrEmpty(value)
            ? new()
            : value.Split(delimiter).Select(s => s.Trim()).ToList();

    /// <summary>
    /// Checks if string is numeric.
    /// </summary>
    public static bool IsNumeric(this string value) =>
        !string.IsNullOrEmpty(value) && value.All(char.IsDigit);

    /// <summary>
    /// Safely parses string to integer.
    /// </summary>
    public static int? TryParseInt(this string value) =>
        int.TryParse(value, out var result) ? result : null;

    /// <summary>
    /// Safely parses string to long.
    /// </summary>
    public static long? TryParseLong(this string value) =>
        long.TryParse(value, out var result) ? result : null;
}
