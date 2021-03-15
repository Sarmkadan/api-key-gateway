// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="KeyStoreUnavailableException"/>.
/// </summary>
public static class KeyStoreUnavailableExceptionValidation
{
    /// <summary>
    /// Validates the <see cref="KeyStoreUnavailableException"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The exception instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this KeyStoreUnavailableException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrEmpty(value.Operation))
        {
            errors.Add("Operation must not be empty.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the <see cref="KeyStoreUnavailableException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception instance to check.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this KeyStoreUnavailableException value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the <see cref="KeyStoreUnavailableException"/> instance is invalid.
    /// </summary>
    /// <param name="value">The exception instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this KeyStoreUnavailableException value)
    {
        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", errors), nameof(value));
        }
    }
}
