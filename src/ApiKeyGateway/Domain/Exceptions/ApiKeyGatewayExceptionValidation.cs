using System;
using System.Collections.Generic;
using System.Globalization;

namespace ApiKeyGateway.Domain.Exceptions;

public static class ApiKeyGatewayExceptionValidation
{
    /// <summary>
    /// Validates an <see cref="ApiKeyGatewayException"/> instance for common problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ApiKeyGatewayException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrEmpty(value.Message))
        {
            problems.Add($"Message is null or empty (actual: {(value.Message is null ? "null" : $"'{value.Message}'")}).");
        }

        if (value.OccurredAt == default)
        {
            problems.Add("OccurredAt is default DateTime (Unix epoch).");
        }

        if (value.ErrorCode is { Length: 0 })
        {
            problems.Add("ErrorCode is an empty string.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether an <see cref="ApiKeyGatewayException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ApiKeyGatewayException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="ApiKeyGatewayException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    public static void EnsureValid(this ApiKeyGatewayException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ApiKeyGatewayException is invalid:{Environment.NewLine}  - {
                    string.Join($"{Environment.NewLine}  - ", problems)
                }");
        }
    }
}