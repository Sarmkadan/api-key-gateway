// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="TransformationRule"/>.
/// </summary>
public static class TransformationRuleValidation
{
    /// <summary>
    /// Validates the specified <see cref="TransformationRule"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The rule instance to validate. Must not be <see langword="null"/>.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this TransformationRule value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id must not be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("Name must not be empty or whitespace.");
        }

        if (value.Priority < 0 || value.Priority > 1000)
        {
            errors.Add("Priority must be between 0 and 1000 inclusive.");
        }

        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must not be default.");
        }

        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt must not be default.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="TransformationRule"/> instance is valid.
    /// </summary>
    /// <param name="value">The rule instance to check. Must not be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this TransformationRule value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="TransformationRule"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The rule instance to validate. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors joined by semicolons.</exception>
    public static void EnsureValid(this TransformationRule value)
    {
        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", errors), nameof(value));
        }
    }
}
