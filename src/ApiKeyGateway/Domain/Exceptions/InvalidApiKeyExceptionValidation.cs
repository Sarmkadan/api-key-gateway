// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Provides validation helpers for InvalidApiKeyException instances
// =====================================================================
using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="InvalidApiKeyException"/> instances
/// </summary>
public static class InvalidApiKeyExceptionValidation
{
    /// <summary>
    /// Validates the specified <see cref="InvalidApiKeyException"/> instance
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <returns>A list of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this InvalidApiKeyException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message cannot be null, empty, or whitespace.");
        }

        if (!string.IsNullOrEmpty(value.ApiKeyHash) && value.ApiKeyHash != value.ApiKeyHash.Trim())
        {
            problems.Add("ApiKeyHash cannot contain leading or trailing whitespace.");
        }

        if (value.OccurredAt == default)
        {
            problems.Add("OccurredAt cannot be the default DateTime value.");
        }
        else if (value.OccurredAt.Kind != DateTimeKind.Utc)
        {
            problems.Add("OccurredAt must be in UTC format.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="InvalidApiKeyException"/> is valid
    /// </summary>
    /// <param name="value">The exception to check</param>
    /// <returns>True if valid; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this InvalidApiKeyException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="InvalidApiKeyException"/> is valid
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing the validation problems</exception>
    public static void EnsureValid(this InvalidApiKeyException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"InvalidApiKeyException is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}