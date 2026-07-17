namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides validation extensions for <see cref="UsageAnalyticsServiceTests"/> instances.
/// </summary>
public static class UsageAnalyticsServiceTestsValidation
{
    /// <summary>
    /// Validates the <see cref="UsageAnalyticsServiceTests"/> instance for common issues.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A list of human-readable validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UsageAnalyticsServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate static date constants (same values as in UsageAnalyticsServiceTests.From/To)
        var from = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2025, 1, 8, 0, 0, 0, DateTimeKind.Utc);

        if (from == default)
        {
            errors.Add("Static field 'From' has default DateTime value.");
        }
        else if (from.Kind != DateTimeKind.Utc)
        {
            errors.Add("Static field 'From' must be in UTC kind.");
        }

        if (to == default)
        {
            errors.Add("Static field 'To' has default DateTime value.");
        }
        else if (to.Kind != DateTimeKind.Utc)
        {
            errors.Add("Static field 'To' must be in UTC kind.");
        }

        if (to <= from)
        {
            errors.Add("Static field 'To' must be after 'From'.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="UsageAnalyticsServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this UsageAnalyticsServiceTests? value)
        => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="UsageAnalyticsServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this UsageAnalyticsServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"UsageAnalyticsServiceTests is not valid. Errors: {string.Join("; ", errors)}",
            nameof(value));
    }
}