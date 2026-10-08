// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Validation rules for <see cref="UsageTrackingOptions"/>, combined with the connection
/// string the usage store is opened with. Shared by the startup validator
/// (<see cref="UsageTrackingOptionsValidator"/>) so every error is reported at once.
/// </summary>
public static class UsageTrackingOptionsValidation
{
    /// <summary>
    /// Validates <paramref name="options"/> and <paramref name="connectionString"/>.
    /// </summary>
    /// <param name="options">The usage tracking options to validate.</param>
    /// <param name="connectionString">The connection string the usage store is opened with.</param>
    /// <returns>A read-only list of error messages; empty when the options are valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static IReadOnlyList<string> Validate(UsageTrackingOptions options, string? connectionString)
    {
        ArgumentNullException.ThrowIfNull(options);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            errors.Add("Connection string 'DefaultConnection' must be configured for usage tracking.");
        }

        if (string.IsNullOrWhiteSpace(options.StoreProvider))
        {
            errors.Add("UsageTracking:StoreProvider must be set.");
        }
        else if (!UsageTrackingOptions.SupportedStoreProviders.Contains(options.StoreProvider, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"UsageTracking:StoreProvider '{options.StoreProvider}' is not supported. " +
                       $"Supported providers: {string.Join(", ", UsageTrackingOptions.SupportedStoreProviders)}.");
        }

        if (options.FlushInterval <= TimeSpan.Zero)
        {
            errors.Add("UsageTracking:FlushInterval must be greater than zero.");
        }

        if (options.RetentionDays <= 0)
        {
            errors.Add("UsageTracking:RetentionDays must be greater than zero.");
        }
        else if (options.FlushInterval > TimeSpan.Zero && TimeSpan.FromDays(options.RetentionDays) < options.FlushInterval)
        {
            errors.Add("UsageTracking:RetentionDays must cover at least one UsageTracking:FlushInterval.");
        }

        return errors;
    }
}
