// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

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
        var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            var fieldValue = field.GetValue(value);

            if (fieldValue is string strValue)
            {
                if (string.IsNullOrWhiteSpace(strValue))
                {
                    problems.Add($"Field '{field.Name}' is null, empty, or whitespace.");
                }
            }
            else if (fieldValue is int intValue)
            {
                if (intValue < 0)
                {
                    problems.Add($"Field '{field.Name}' has negative value {intValue} which is out of range.");
                }
            }
            else if (fieldValue is DateTime dateTimeValue)
            {
                if (dateTimeValue == default)
                {
                    problems.Add($"Field '{field.Name}' has default DateTime value which is invalid.");
                }
            }
            else if (fieldValue is DateTimeOffset dateTimeOffsetValue)
            {
                if (dateTimeOffsetValue == default)
                {
                    problems.Add($"Field '{field.Name}' has default DateTimeOffset value which is invalid.");
                }
            }
            else if (fieldValue is bool boolValue)
            {
                // Booleans are always valid
            }
            else if (fieldValue is System.Collections.IEnumerable enumerable && fieldValue is not string)
            {
                // Check if it's a collection that might be null or empty
                if (fieldValue is System.Collections.ICollection collection)
                {
                    if (collection.Count == 0)
                    {
                        problems.Add($"Field '{field.Name}' is an empty collection.");
                    }
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="IntegrationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this IntegrationTests? value)
    {
        return Validate(value).Count == 0;
    }

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
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"IntegrationTests instance is invalid:{Environment.NewLine}- ".Replace("- ", string.Empty) +
            string.Join(Environment.NewLine + "- ", problems),
            nameof(value));
    }
}