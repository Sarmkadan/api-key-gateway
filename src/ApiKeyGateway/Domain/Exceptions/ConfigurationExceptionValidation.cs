// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Validation helpers for ConfigurationException
// =============================================================================

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="ConfigurationException"/> instances
/// </summary>
public static class ConfigurationExceptionValidation
{
    /// <summary>
    /// Validates the specified <see cref="ConfigurationException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <returns>An enumerable of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this ConfigurationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message cannot be null, empty, or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.Setting))
        {
            problems.Add("Setting cannot be null, empty, or whitespace.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ConfigurationException"/> is valid.
    /// </summary>
    /// <param name="value">The exception to check</param>
    /// <returns>true if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static bool IsValid(this ConfigurationException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ConfigurationException"/> is valid.
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the exception is invalid, containing the validation problems</exception>
    public static void EnsureValid(this ConfigurationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ConfigurationException is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}