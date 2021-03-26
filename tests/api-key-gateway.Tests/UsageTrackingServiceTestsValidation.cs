namespace ApiKeyGateway.Tests;

public static class UsageTrackingServiceTestsValidation
{
    /// <summary>
    /// Validates the <see cref="UsageTrackingServiceTests"/> instance for common issues.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A list of human-readable validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UsageTrackingServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // No instance members to validate in UsageTrackingServiceTests

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="UsageTrackingServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this UsageTrackingServiceTests value)
        => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="UsageTrackingServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this UsageTrackingServiceTests value)
    {
        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"UsageTrackingServiceTests is not valid. Errors: {string.Join(" ", errors)}",
            nameof(value));
    }
}