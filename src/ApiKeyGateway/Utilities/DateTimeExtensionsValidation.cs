// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Validation helpers for DateTimeExtensions operations
/// </summary>
public static class DateTimeExtensionsValidation
{
    /// <summary>
    /// Validates a DateTime value against common issues
    /// </summary>
    /// <param name="value">The DateTime value to validate</param>
    /// <returns>A list of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this DateTime value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Check for default/minimum DateTime value
        if (value == default)
        {
            problems.Add("DateTime cannot be the default value (DateTime.MinValue)");
        }

        // Check for dates in the past when future is expected
        if (value.IsInPast())
        {
            problems.Add("DateTime cannot be in the past");
        }

        // Check for dates that are too far in the future (arbitrary reasonable limit)
        if (value > DateTime.UtcNow.AddYears(10))
        {
            problems.Add("DateTime cannot be more than 10 years in the future");
        }

        // Validate StartOfDay specific constraints
        if (value.TimeOfDay != TimeSpan.Zero)
        {
            problems.Add("StartOfDay result must be at midnight (00:00:00)");
        }

        // Validate EndOfDay specific constraints
        if (value.TimeOfDay != new TimeSpan(23, 59, 59, 999))
        {
            problems.Add("EndOfDay result must be at end of day (23:59:59.999)");
        }

        // Validate StartOfWeek specific constraints
        if (value.TimeOfDay != TimeSpan.Zero)
        {
            problems.Add("StartOfWeek result must be at midnight (00:00:00)");
        }

        // Validate StartOfMonth specific constraints
        if (value.Day != 1 || value.TimeOfDay != TimeSpan.Zero)
        {
            problems.Add("StartOfMonth result must be the first day of month at midnight (00:00:00)");
        }

        // Validate EndOfMonth specific constraints
        var expectedEndOfMonth = value.StartOfMonth().AddMonths(1).AddTicks(-1);
        if (value != expectedEndOfMonth)
        {
            problems.Add("EndOfMonth result must be the last moment of the last day of the month");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a DateTime value is valid
    /// </summary>
    /// <param name="value">The DateTime value to check</param>
    /// <returns>True if the DateTime is valid; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this DateTime value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a DateTime value is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The DateTime value to validate</param>
    /// <exception cref="ArgumentException">Thrown if the DateTime is not valid, containing a list of problems</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static void EnsureValid(this DateTime value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DateTime validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems.Select(p => $"  - {p}"))}");
        }
    }
}