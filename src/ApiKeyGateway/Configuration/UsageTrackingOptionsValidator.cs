// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Startup validator for <see cref="UsageTrackingOptions"/>. Registered with
/// <c>ValidateOnStart</c> so a missing connection string, unsupported store provider or
/// inconsistent flush/retention settings fail the host before it accepts traffic.
/// </summary>
public sealed class UsageTrackingOptionsValidator : IValidateOptions<UsageTrackingOptions>
{
    private readonly string? _connectionString;

    /// <summary>
    /// Initializes a new instance of <see cref="UsageTrackingOptionsValidator"/>.
    /// </summary>
    /// <param name="connectionString">The connection string the usage store is opened with.</param>
    public UsageTrackingOptionsValidator(string? connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, UsageTrackingOptions options)
    {
        var errors = UsageTrackingOptionsValidation.Validate(options, _connectionString);

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
