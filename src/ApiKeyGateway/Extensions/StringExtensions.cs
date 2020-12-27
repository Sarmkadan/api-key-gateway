// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

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
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">Maximum length of the result string.</param>
    /// <returns>The truncated string, or the original string if it's shorter than maxLength.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is negative.</exception>
    public static string Truncate(this string value, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        return string.IsNullOrEmpty(value)
            ? value
            : value.Length <= maxLength
                ? value
                : value[..maxLength];
    }

    /// <summary>
    /// Truncates a string with ellipsis suffix if it exceeds maximum length.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">Maximum length of the result string including ellipsis.</param>
    /// <returns>The truncated string with ellipsis, or the original string if it's shorter than maxLength.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is less than 3.</exception>
    public static string TruncateWithEllipsis(this string value, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 3);

        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value[..(maxLength - 3)] + "...";
    }

    /// <summary>
    /// Checks if the string contains any of the provided values using ordinal ignore case comparison.
    /// </summary>
    /// <param name="value">The string to search within.</param>
    /// <param name="searches">Values to search for.</param>
    /// <returns><see langword="true"/> if any search value is found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="searches"/> is <see langword="null"/>.</exception>
    public static bool ContainsAny(this string value, params string[] searches)
    {
        ArgumentNullException.ThrowIfNull(searches);

        return searches.Any(s => value?.Contains(s, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Checks if the string starts with any of the provided values using ordinal ignore case comparison.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="prefixes">Prefixes to search for.</param>
    /// <returns><see langword="true"/> if the string starts with any prefix; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="prefixes"/> is <see langword="null"/>.</exception>
    public static bool StartsWithAny(this string value, params string[] prefixes)
    {
        ArgumentNullException.ThrowIfNull(prefixes);

        return prefixes.Any(p => value?.StartsWith(p, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Converts a string to a URL-friendly slug.
    /// Removes spaces and special characters, converts to lowercase, and normalizes dashes.
    /// </summary>
    /// <param name="value">The string to convert to a slug.</param>
    /// <returns>A URL-friendly slug, or an empty string if the input is null or empty.</returns>
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
    /// Capitalizes the first character of the string.
    /// </summary>
    /// <param name="value">The string to capitalize.</param>
    /// <returns>The string with the first character capitalized, or the original string if it's null or empty.</returns>
    public static string CapitalizeFirst(this string value) =>
        string.IsNullOrEmpty(value)
            ? value
            : char.ToUpperInvariant(value[0]) + value[1..];

    /// <summary>
    /// Converts a delimited string to a list of strings.
    /// </summary>
    /// <param name="value">The delimited string to convert.</param>
    /// <param name="delimiter">The delimiter character. Defaults to comma.</param>
    /// <returns>A list of strings, or an empty list if the input is null or empty.</returns>
    public static List<string> ToList(this string value, char delimiter = ',')
    {
        return string.IsNullOrEmpty(value)
            ? []
            : value.Split(delimiter).Select(s => s.Trim()).ToList();
    }

    /// <summary>
    /// Checks if string is numeric.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns><see langword="true"/> if the string contains only digits; otherwise, <see langword="false"/>.</returns>
    public static bool IsNumeric(this string value) =>
        !string.IsNullOrEmpty(value) && value.All(char.IsDigit);

    /// <summary>
    /// Safely parses string to integer.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed integer, or <see langword="null"/> if parsing fails.</returns>
    public static int? TryParseInt(this string value) =>
        int.TryParse(value, out var result) ? result : null;

    /// <summary>
    /// Safely parses string to long.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed long, or <see langword="null"/> if parsing fails.</returns>
    public static long? TryParseLong(this string value) =>
        long.TryParse(value, out var result) ? result : null;
}