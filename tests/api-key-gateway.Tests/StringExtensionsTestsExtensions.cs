// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Extension methods for <see cref="StringExtensionsTests"/> that provide additional test utilities
/// for string manipulation scenarios commonly encountered in API key gateway testing.
/// </summary>
public static class StringExtensionsTestsExtensions
{
    /// <summary>
    /// Determines whether the string contains any of the specified substrings,
    /// ignoring case and culture.
    /// </summary>
    /// <param name="source">The source string to search in.</param>
    /// <param name="values">The values to search for.</param>
    /// <returns>True if any value is found; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    public static bool ContainsAny(this string source, params string[] values)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(values);

        if (values.Length == 0)
        {
            return false;
        }

        foreach (var value in values)
        {
            if (value is not null && source.Contains(value, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the string starts with any of the specified prefixes,
    /// ignoring case and culture.
    /// </summary>
    /// <param name="source">The source string to check.</param>
    /// <param name="prefixes">The prefixes to match against.</param>
    /// <returns>True if any prefix matches; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source or prefixes is null.</exception>
    public static bool StartsWithAny(this string source, params string[] prefixes)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(prefixes);

        if (prefixes.Length == 0)
        {
            return false;
        }

        foreach (var prefix in prefixes)
        {
            if (prefix is not null && source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Converts the string to a URL-safe slug format, removing special characters
    /// and converting spaces to dashes.
    /// </summary>
    /// <param name="source">The source string to convert.</param>
    /// <returns>A URL-safe slug representation of the input.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    public static string ToSlug(this string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrEmpty(source))
        {
            return string.Empty;
        }

        var slug = source
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace('@', '-')
            .Replace('#', '-')
            .Replace('!', '-')
            .Replace('.', '-')
            .Replace('_', '-')
            .Replace('\'', '-')
            .Replace('"', '-')
            .Replace('&', '-')
            .Replace('=', '-')
            .Replace('+', '-')
            .Replace('%', '-')
            .Replace('/', '-')
            .Replace('\\', '-');

        // Remove consecutive dashes
        slug = System.Text.RegularExpressions.Regex.Replace(slug, "[-]+", "-");

        // Remove leading and trailing dashes
        slug = slug.Trim('-');

        return slug;
    }

    /// <summary>
    /// Truncates the string to the specified maximum length, returning null if the input is null.
    /// </summary>
    /// <param name="source">The source string to truncate.</param>
    /// <param name="maxLength">The maximum length of the resulting string.</param>
    /// <returns>The truncated string, or null if source is null.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is negative.</exception>
    public static string? Truncate(this string? source, int maxLength)
    {
        if (maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length cannot be negative.");
        }

        if (source is null)
        {
            return null;
        }

        return source.Length <= maxLength
            ? source
            : source[..maxLength];
    }

    /// <summary>
    /// Truncates the string to the specified maximum length and appends an ellipsis if truncated,
    /// returning null if the input is null.
    /// </summary>
    /// <param name="source">The source string to truncate.</param>
    /// <param name="maxLength">The maximum length including the ellipsis.</param>
    /// <returns>The truncated string with ellipsis if truncated, or null if source is null.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is less than 3.</exception>
    public static string? TruncateWithEllipsis(this string? source, int maxLength)
    {
        if (maxLength < 3)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length must be at least 3 to accommodate ellipsis.");
        }

        if (source is null)
        {
            return null;
        }

        if (source.Length <= maxLength)
        {
            return source;
        }

        var truncated = source[..(maxLength - 3)];
        return $"{truncated}...";
    }
}