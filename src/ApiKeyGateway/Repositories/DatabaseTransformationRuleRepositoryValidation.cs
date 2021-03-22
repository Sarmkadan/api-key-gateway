// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Globalization;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Provides validation helpers for <see cref="DatabaseTransformationRuleRepository"/> instances.
/// </summary>
public static class DatabaseTransformationRuleRepositoryValidation
{
    /// <summary>
    /// Validates the <see cref="DatabaseTransformationRuleRepository"/> instance for logical consistency.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this DatabaseTransformationRuleRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // DatabaseTransformationRuleRepository dependencies are validated in constructor
        // (_dbConnection and _logger are checked in constructor)
        // No additional validation needed for the repository itself

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DatabaseTransformationRuleRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static bool IsValid(this DatabaseTransformationRuleRepository value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="DatabaseTransformationRuleRepository"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with a detailed message if it is not.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the repository is invalid.</exception>
    public static void EnsureValid(this DatabaseTransformationRuleRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"DatabaseTransformationRuleRepository is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }
}