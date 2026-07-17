// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Reflection;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides validation helpers for <see cref="IntegrationTests"/> instances.
/// </summary>
public static class IntegrationTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="IntegrationTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this IntegrationTests? value)
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
                case string strValue when string.IsNullOrWhiteSpace(strValue):
                    problems.Add($"Field '{field.Name}' is null, empty, or whitespace.");
                    break;

                case int intValue when intValue < 0:
                    problems.Add($"Field '{field.Name}' has negative value {intValue} which is out of range.");
                    break;

                case DateTime dateTimeValue when dateTimeValue == default:
                    problems.Add($"Field '{field.Name}' has default DateTime value which is invalid.");
                    break;

                case DateTimeOffset dateTimeOffsetValue when dateTimeOffsetValue == default:
                    problems.Add($"Field '{field.Name}' has default DateTimeOffset value which is invalid.");
                    break;

                case System.Collections.IEnumerable enumerable when enumerable is not string:
                    if (enumerable is System.Collections.ICollection collection && collection.Count == 0)
                    {
                        problems.Add($"Field '{field.Name}' is an empty collection.");
                    }
                    break;
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="IntegrationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this IntegrationTests? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="IntegrationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid; the message lists all problems.</exception>
    public static void EnsureValid(this IntegrationTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count != 0)
        {
            throw new ArgumentException(
                $"IntegrationTests instance is invalid:{Environment.NewLine}- ".Replace("- ", string.Empty) +
                string.Join(Environment.NewLine + "- ", problems),
                nameof(value));
        }
    }
}