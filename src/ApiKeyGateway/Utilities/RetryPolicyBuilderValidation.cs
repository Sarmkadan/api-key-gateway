// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="RetryPolicyBuilder"/> instances.
/// </summary>
public static class RetryPolicyBuilderValidation
{
    /// <summary>
    /// Validates a <see cref="RetryPolicyBuilder"/> instance and returns any validation problems.
    /// </summary>
    /// <param name="value">The retry policy builder to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RetryPolicyBuilder? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate MaxRetries
        if (value.MaxRetries < 0)
        {
            problems.Add("Max retries must be a non-negative integer.");
        }

        // Validate InitialDelay
        if (value.InitialDelayMs <= 0)
        {
            problems.Add("Initial delay must be a positive integer greater than zero.");
        }

        // Validate BackoffMultiplier
        if (value.BackoffMultiplier <= 0)
        {
            problems.Add("Backoff multiplier must be a positive number greater than zero.");
        }

        // Validate MaxDelay
        if (value.MaxDelayMs <= 0)
        {
            problems.Add("Max delay must be a positive integer greater than zero.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="RetryPolicyBuilder"/> instance is valid.
    /// </summary>
    /// <param name="value">The retry policy builder to check.</param>
    /// <returns>True if the builder is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this RetryPolicyBuilder? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="RetryPolicyBuilder"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The retry policy builder to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the builder contains validation problems.</exception>
    public static void EnsureValid(this RetryPolicyBuilder? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"RetryPolicyBuilder is not valid. Problems: {string.Join(" ", problems)}",
                nameof(value));
        }
    }
}