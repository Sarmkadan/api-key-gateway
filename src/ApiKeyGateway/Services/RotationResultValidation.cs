// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace ApiKeyGateway.Services;

/// <summary>
/// Provides validation helpers for <see cref="RotationResult"/>.
/// </summary>
public static class RotationResultValidation
{
    /// <summary>
    /// Validates the <see cref="RotationResult"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The rotation result instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RotationResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrEmpty(value.OldKeyId))
        {
            errors.Add("OldKeyId must not be empty.");
        }

        if (string.IsNullOrEmpty(value.NewKeyId))
        {
            errors.Add("NewKeyId must not be empty.");
        }

        if (string.IsNullOrEmpty(value.ConsumerId))
        {
            errors.Add("ConsumerId must not be empty.");
        }

        if (value.NewKeyExpiresAt.HasValue)
        {
            if (value.NewKeyExpiresAt.Value == default)
            {
                errors.Add("NewKeyExpiresAt must be a valid date if set.");
            }
            else if (value.NewKeyExpiresAt.Value < DateTime.UtcNow.AddMinutes(-5))
            {
                errors.Add("NewKeyExpiresAt cannot be in the past.");
            }
        }

        if (!value.Success && string.IsNullOrEmpty(value.FailureReason))
        {
            errors.Add("FailureReason must be provided when Success is false.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the <see cref="RotationResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The rotation result instance to check.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this RotationResult value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the <see cref="RotationResult"/> instance is invalid.
    /// </summary>
    /// <param name="value">The rotation result instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this RotationResult value)
    {
        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", errors), nameof(value));
        }
    }
}