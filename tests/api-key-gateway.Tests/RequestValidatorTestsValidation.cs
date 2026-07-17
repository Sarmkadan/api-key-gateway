// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Reflection;
using System.Linq;

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

            switch (fieldValue)
            {
                case null:
                    problems.Add($"Field '{field.Name}' is null.");
                    break;
                case string strValue when string.IsNullOrWhiteSpace(strValue):
                    problems.Add($"Field '{field.Name}' is null, empty, or whitespace.");
                    break;
                case int intValue when intValue == 0:
                    problems.Add($"Field '{field.Name}' has default value (0).");
                    break;
                case DateTime dateValue when dateValue == default:
                    problems.Add($"Field '{field.Name}' has default DateTime value.");
                    break;
                case DateOnly dateOnlyValue when dateOnlyValue == default:
                    problems.Add($"Field '{field.Name}' has default DateOnly value.");
                    break;
                case TimeOnly timeOnlyValue when timeOnlyValue == default:
                    problems.Add($"Field '{field.Name}' has default TimeOnly value.");
                    break;
                case Guid guidValue when guidValue == Guid.Empty:
                    problems.Add($"Field '{field.Name}' has empty Guid.");
                    break;
                case decimal decimalValue when decimalValue == 0m:
                    problems.Add($"Field '{field.Name}' has default decimal value (0).");
                    break;
                case double doubleValue when doubleValue == 0d:
                    problems.Add($"Field '{field.Name}' has default double value (0).");
                    break;
                case float floatValue when floatValue == 0f:
                    problems.Add($"Field '{field.Name}' has default float value (0).");
                    break;
                case long longValue when longValue == 0L:
                    problems.Add($"Field '{field.Name}' has default long value (0).");
                    break;
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
                $"RequestValidatorTests instance is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems.Select(p => $"- {p}"))}",
                nameof(value));
        }
    }
}
