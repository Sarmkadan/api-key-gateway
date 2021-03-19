// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Provides validation helpers for <see cref="ApiKeyRepository"/> instances.
/// </summary>
public static class ApiKeyRepositoryValidation
{
    /// <summary>
    /// Validates the <see cref="ApiKeyRepository"/> instance for logical consistency.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this ApiKeyRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // ApiKeyRepository is primarily a service class with injected dependencies
        // The main validation is that the injected services are not null
        // (already validated in constructor)

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ApiKeyRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this ApiKeyRepository value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ApiKeyRepository"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with a detailed message if it is not.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the repository is invalid.</exception>
    public static void EnsureValid(this ApiKeyRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"ApiKeyRepository is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }
}