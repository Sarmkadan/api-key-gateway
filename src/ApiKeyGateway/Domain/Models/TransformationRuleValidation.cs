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
    /// Validates the <see cref="TransformationRule"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The rule instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this TransformationRule value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrEmpty(value.Id))
        {
            errors.Add("Id must not be empty.");
        }

        if (string.IsNullOrEmpty(value.Name))
        {
            errors.Add("Name must not be empty.");
        }

        if (value.Priority < 0 || value.Priority > 1000)
        {
            errors.Add("Priority must be between 0 and 1000.");
        }

        if (value.CreatedAt == DateTime.MinValue || value.UpdatedAt == DateTime.MinValue)
        {
            errors.Add("CreatedAt and UpdatedAt must not be default dates.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the <see cref="TransformationRule"/> instance is valid.
    /// </summary>
    /// <param name="value">The rule instance to check.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this TransformationRule value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the <see cref="TransformationRule"/> instance is invalid.
    /// </summary>
    /// <param name="value">The rule instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this TransformationRule value)
    {
        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", errors), nameof(value));
        }
    }
}
