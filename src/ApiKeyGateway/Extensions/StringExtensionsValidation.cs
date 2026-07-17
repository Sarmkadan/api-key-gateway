// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Extensions;

/// <summary>
/// Validation extension methods for string operations from StringExtensions.
/// Provides validation helpers to ensure string operation parameters are valid.
/// </summary>
public static class StringExtensionsValidation
{
    /// <summary>
    /// Validates parameters for Truncate method.
    /// </summary>
    /// <param name="maxLength">The maximum length parameter.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is negative.</exception>
    public static IReadOnlyList<string> ValidateTruncateParameters(int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        if (maxLength == 0)
        {
            return new[] { "maxLength must be greater than 0 for Truncate method." };
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for TruncateWithEllipsis method.
    /// </summary>
    /// <param name="maxLength">The maximum length parameter including ellipsis.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is less than 3.</exception>
    public static IReadOnlyList<string> ValidateTruncateWithEllipsisParameters(int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 3);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for ContainsAny method.
    /// </summary>
    /// <param name="searches">The array of search strings.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="searches"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateContainsAnyParameters(params string[] searches)
    {
        ArgumentNullException.ThrowIfNull(searches);

        foreach (var search in searches)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return new[] { "Search strings cannot be null, empty, or whitespace." };
            }
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for StartsWithAny method.
    /// </summary>
    /// <param name="prefixes">The array of prefix strings.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="prefixes"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateStartsWithAnyParameters(params string[] prefixes)
    {
        ArgumentNullException.ThrowIfNull(prefixes);

        foreach (var prefix in prefixes)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return new[] { "Prefix strings cannot be null, empty, or whitespace." };
            }
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for ToList method.
    /// </summary>
    /// <param name="delimiter">The delimiter character.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="delimiter"/> is a control character.</exception>
    public static IReadOnlyList<string> ValidateToListParameters(char delimiter)
    {
        if (char.IsControl(delimiter))
        {
            return new[] { "Delimiter character cannot be a control character." };
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if Truncate parameters are valid.
    /// </summary>
    /// <param name="maxLength">The maximum length parameter.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidTruncateParameters(int maxLength) => ValidateTruncateParameters(maxLength).Count == 0;

    /// <summary>
    /// Checks if TruncateWithEllipsis parameters are valid.
    /// </summary>
    /// <param name="maxLength">The maximum length parameter including ellipsis.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidTruncateWithEllipsisParameters(int maxLength) => ValidateTruncateWithEllipsisParameters(maxLength).Count == 0;

    /// <summary>
    /// Checks if ContainsAny parameters are valid.
    /// </summary>
    /// <param name="searches">The array of search strings.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidContainsAnyParameters(params string[] searches) => ValidateContainsAnyParameters(searches).Count == 0;

    /// <summary>
    /// Checks if StartsWithAny parameters are valid.
    /// </summary>
    /// <param name="prefixes">The array of prefix strings.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidStartsWithAnyParameters(params string[] prefixes) => ValidateStartsWithAnyParameters(prefixes).Count == 0;

    /// <summary>
    /// Checks if ToList parameters are valid.
    /// </summary>
    /// <param name="delimiter">The delimiter character.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidToListParameters(char delimiter) => ValidateToListParameters(delimiter).Count == 0;

    /// <summary>
    /// Ensures Truncate parameters are valid, throwing if not.
    /// </summary>
    /// <param name="maxLength">The maximum length parameter.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void EnsureValidTruncateParameters(int maxLength)
    {
        var problems = ValidateTruncateParameters(maxLength);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures TruncateWithEllipsis parameters are valid, throwing if not.
    /// </summary>
    /// <param name="maxLength">The maximum length parameter including ellipsis.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void EnsureValidTruncateWithEllipsisParameters(int maxLength)
    {
        var problems = ValidateTruncateWithEllipsisParameters(maxLength);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures ContainsAny parameters are valid, throwing if not.
    /// </summary>
    /// <param name="searches">The array of search strings.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void EnsureValidContainsAnyParameters(params string[] searches)
    {
        var problems = ValidateContainsAnyParameters(searches);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures StartsWithAny parameters are valid, throwing if not.
    /// </summary>
    /// <param name="prefixes">The array of prefix strings.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void EnsureValidStartsWithAnyParameters(params string[] prefixes)
    {
        var problems = ValidateStartsWithAnyParameters(prefixes);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures ToList parameters are valid, throwing if not.
    /// </summary>
    /// <param name="delimiter">The delimiter character.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="delimiter"/> is a control character.</exception>
    public static void EnsureValidToListParameters(char delimiter)
    {
        if (char.IsControl(delimiter))
        {
            throw new ArgumentException("Delimiter character cannot be a control character.");
        }
    }
}