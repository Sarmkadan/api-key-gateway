// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Validation extension methods for RateLimitExceededException.
/// Provides validation helpers to ensure RateLimitExceededException instances are valid.
/// </summary>
public static class RateLimitExceededExceptionValidation
{
    /// <summary>
    /// Validates the RateLimitExceededException instance.
    /// </summary>
    /// <param name="value">The RateLimitExceededException instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this RateLimitExceededException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.ApiKeyId))
        {
            problems.Add("ApiKeyId cannot be null, empty, or whitespace.");
        }

        if (value.Limit <= 0)
        {
            problems.Add("Limit must be greater than 0.");
        }
        else if (value.Limit > 1000000)
        {
            problems.Add("Limit must be a reasonable value (maximum 1000000).");
        }

        if (value.WindowInSeconds <= 0)
        {
            problems.Add("WindowInSeconds must be greater than 0.");
        }
        else if (value.WindowInSeconds > 86400 * 365) // More than 1 year
        {
            problems.Add("WindowInSeconds must be a reasonable value (maximum 1 year).");
        }

        if (value.RetryAfter.HasValue)
        {
            var retryAfter = value.RetryAfter.Value;
            if (retryAfter.Kind != DateTimeKind.Utc)
            {
                problems.Add("RetryAfter must be in UTC format.");
            }
            else if (retryAfter == default)
            {
                problems.Add("RetryAfter cannot be the default DateTime value.");
            }
            else if (retryAfter < DateTime.UtcNow)
            {
                problems.Add("RetryAfter cannot be in the past.");
            }
        }

        return problems;
    }

    /// <summary>
    /// Checks if the RateLimitExceededException instance is valid.
    /// </summary>
    /// <param name="value">The RateLimitExceededException instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this RateLimitExceededException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures the RateLimitExceededException instance is valid, throwing if not.
    /// </summary>
    /// <param name="value">The RateLimitExceededException instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with a list of problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static void EnsureValid(this RateLimitExceededException value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }
}
