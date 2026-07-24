// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="RateLimitExceededException"/> instances
/// </summary>
public static class RateLimitExceededExceptionValidation
{
    /// <summary>
    /// Validates the specified <see cref="RateLimitExceededException"/> instance
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <returns>A list of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this RateLimitExceededException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message cannot be null, empty, or whitespace.");
        }

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

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="RateLimitExceededException"/> is valid
    /// </summary>
    /// <param name="value">The exception to check</param>
    /// <returns>True if valid; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this RateLimitExceededException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="RateLimitExceededException"/> is valid
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing the validation problems</exception>
    public static void EnsureValid(this RateLimitExceededException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"RateLimitExceededException is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}