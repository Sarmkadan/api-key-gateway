// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Provides validation helpers for UnauthorizedAccessException instances
// =====================================================================

using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="UnauthorizedAccessException"/> instances
/// </summary>
public static class UnauthorizedAccessExceptionValidation
{
    /// <summary>
    /// Validates the specified <see cref="UnauthorizedAccessException"/> instance
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <returns>A list of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this UnauthorizedAccessException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message cannot be null, empty, or whitespace.");
        }

        if (value.Reason is not null && string.IsNullOrWhiteSpace(value.Reason))
        {
            problems.Add("Reason cannot be whitespace if specified.");
        }

        if (value.SourceIp is not null && string.IsNullOrWhiteSpace(value.SourceIp))
        {
            problems.Add("SourceIp cannot be whitespace if specified.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="UnauthorizedAccessException"/> is valid
    /// </summary>
    /// <param name="value">The exception to check</param>
    /// <returns>True if valid; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this UnauthorizedAccessException value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="UnauthorizedAccessException"/> is valid
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing the validation problems</exception>
    public static void EnsureValid(this UnauthorizedAccessException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"UnauthorizedAccessException is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}",
                nameof(value));
        }
    }
}