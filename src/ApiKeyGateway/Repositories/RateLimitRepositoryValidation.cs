// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Validation helpers for RateLimitRepository and RateLimit instances to ensure data integrity
/// </summary>
public static class RateLimitRepositoryValidation
{
    /// <summary>
    /// Validates the RateLimitRepository instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The repository instance to validate</param>
    /// <returns>List of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this RateLimitRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Repository dependencies are validated in constructor
        // No additional validation needed for the repository itself

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the RateLimitRepository instance is valid
    /// </summary>
    /// <param name="value">The repository instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this RateLimitRepository? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures the RateLimitRepository instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The repository instance to validate</param>
    /// <exception cref="ArgumentException">Thrown when the repository is invalid</exception>
    public static void EnsureValid(this RateLimitRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"RateLimitRepository is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }

    /// <summary>
    /// Validates a RateLimit instance for repository operations and returns any validation errors
    /// </summary>
    /// <param name="value">The rate limit to validate</param>
    /// <returns>A list of validation error messages, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this RateLimit? value)
    {
        return RateLimitValidation.Validate(value);
    }

    /// <summary>
    /// Determines whether a RateLimit instance is valid for repository operations
    /// </summary>
    /// <param name="value">The rate limit to check</param>
    /// <returns>True if the rate limit is valid; otherwise, false</returns>
    public static bool IsValid(this RateLimit? value) => RateLimitValidation.IsValid(value);

    /// <summary>
    /// Ensures that a RateLimit instance is valid for repository operations, throwing an exception if not
    /// </summary>
    /// <param name="value">The rate limit to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when value contains validation errors</exception>
    public static void EnsureValid(this RateLimit? value)
    {
        RateLimitValidation.EnsureValid(value);
    }
}