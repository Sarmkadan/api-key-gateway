// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Diagnostics.CodeAnalysis;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Provides validation helpers for <see cref="ApiKeyRepository"/> instances.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ApiKeyRepositoryValidation
{
    /// <summary>
    /// Validates the <see cref="ApiKeyRepository"/> instance for logical consistency.
    /// </summary>
    /// <remarks>
    /// This method performs runtime validation of the repository instance.
    /// Currently, <see cref="ApiKeyRepository"/> is a service class with injected dependencies
    /// that are validated in its constructor. Additional validation logic can be added here
    /// if the repository gains state that requires runtime validation.
    /// </remarks>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ApiKeyRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // ApiKeyRepository is primarily a service class with injected dependencies.
        // The main validation occurs in the constructor via ArgumentNullException.ThrowIfNull.
        // This method provides extensibility for future validation requirements.

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ApiKeyRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid([NotNullWhen(true)] this ApiKeyRepository? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ApiKeyRepository"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with a detailed message if it is not.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the repository is invalid.</exception>
    public static void EnsureValid(this ApiKeyRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count is 0)
            return;

        throw new ArgumentException(
            $"ApiKeyRepository is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }
}