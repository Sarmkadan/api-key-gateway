// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Globalization;
using System.Reflection;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides validation helpers for <see cref="RequestValidatorTests"/> instances.
/// </summary>
public static class RequestValidatorTestsValidation
{
    /// <summary>
    /// Validates the <see cref="RequestValidatorTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RequestValidatorTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate private fields via reflection
        var type = value.GetType();
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            var fieldValue = field.GetValue(value);

            if (fieldValue is null)
            {
                problems.Add($"Field '{field.Name}' is null.");
            }
            else if (fieldValue is string strValue)
            {
                if (string.IsNullOrWhiteSpace(strValue))
                {
                    problems.Add($"Field '{field.Name}' is null, empty, or whitespace.");
                }
            }
            else if (fieldValue is int intValue)
            {
                if (intValue == 0)
                {
                    problems.Add($"Field '{field.Name}' has default value (0).");
                }
            }
            else if (fieldValue is DateTime dateValue)
            {
                if (dateValue == default)
                {
                    problems.Add($"Field '{field.Name}' has default DateTime value.");
                }
            }
            else if (fieldValue is DateOnly dateOnlyValue)
            {
                if (dateOnlyValue == default)
                {
                    problems.Add($"Field '{field.Name}' has default DateOnly value.");
                }
            }
            else if (fieldValue is TimeOnly timeOnlyValue)
            {
                if (timeOnlyValue == default)
                {
                    problems.Add($"Field '{field.Name}' has default TimeOnly value.");
                }
            }
            else if (fieldValue is Guid guidValue)
            {
                if (guidValue == Guid.Empty)
                {
                    problems.Add($"Field '{field.Name}' has empty Guid.");
                }
            }
            else if (fieldValue is decimal decimalValue)
            {
                if (decimalValue == 0m)
                {
                    problems.Add($"Field '{field.Name}' has default decimal value (0).");
                }
            }
            else if (fieldValue is double doubleValue)
            {
                if (doubleValue == 0d)
                {
                    problems.Add($"Field '{field.Name}' has default double value (0).");
                }
            }
            else if (fieldValue is float floatValue)
            {
                if (floatValue == 0f)
                {
                    problems.Add($"Field '{field.Name}' has default float value (0).");
                }
            }
            else if (fieldValue is long longValue)
            {
                if (longValue == 0L)
                {
                    problems.Add($"Field '{field.Name}' has default long value (0).");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="RequestValidatorTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this RequestValidatorTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the <see cref="RequestValidatorTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this RequestValidatorTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"RequestValidatorTests instance is invalid:{Environment.NewLine}- ".Replace("- ", string.Empty) +
                string.Join(Environment.NewLine + "- ", problems),
                nameof(value));
        }
    }
}
