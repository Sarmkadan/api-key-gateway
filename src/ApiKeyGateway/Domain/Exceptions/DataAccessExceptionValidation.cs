// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="DataAccessException"/> instances
/// </summary>
public static class DataAccessExceptionValidation
{
    /// <summary>
    /// Validates the specified <see cref="DataAccessException"/> instance
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <returns>A list of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this DataAccessException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message cannot be null, empty, or whitespace.");
        }

        if (!string.IsNullOrEmpty(value.Operation) && string.IsNullOrWhiteSpace(value.Operation))
        {
            problems.Add("Operation cannot be whitespace if specified.");
        }

        if (!string.IsNullOrEmpty(value.Entity) && string.IsNullOrWhiteSpace(value.Entity))
        {
            problems.Add("Entity cannot be whitespace if specified.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DataAccessException"/> is valid
    /// </summary>
    /// <param name="value">The exception to check</param>
    /// <returns>True if valid; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this DataAccessException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="DataAccessException"/> is valid
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing the validation problems</exception>
    public static void EnsureValid(this DataAccessException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DataAccessException is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}