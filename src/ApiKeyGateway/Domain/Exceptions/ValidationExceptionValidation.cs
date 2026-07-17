// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Provides validation helpers for ValidationException instances
// =====================================================================

using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="ValidationException"/> instances
/// </summary>
public static class ValidationExceptionValidation
{
    /// <summary>
    /// Validates the specified <see cref="ValidationException"/> instance
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ValidationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message cannot be null, empty, or whitespace.");
        }

        if (value.ParameterName is not null && string.IsNullOrWhiteSpace(value.ParameterName))
        {
            problems.Add("ParameterName cannot be whitespace if specified.");
        }

        if (value.AttemptedValue is not null && string.IsNullOrWhiteSpace(value.AttemptedValue.ToString()))
        {
            problems.Add("AttemptedValue cannot be whitespace if specified.");
        }

        if (value.ValidationErrors is not null)
        {
            if (!value.ValidationErrors.Any())
            {
                problems.Add("ValidationErrors collection cannot be empty if specified.");
            }
            else
            {
                foreach (var error in value.ValidationErrors)
                {
                    if (string.IsNullOrWhiteSpace(error))
                    {
                        problems.Add("ValidationErrors collection cannot contain null, empty, or whitespace strings.");
                        break;
                    }
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ValidationException"/> is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ValidationException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ValidationException"/> is valid.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing the validation problems.</exception>
    public static void EnsureValid(this ValidationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ValidationException is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}
