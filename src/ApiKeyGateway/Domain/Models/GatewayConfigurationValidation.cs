// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="GatewayConfiguration"/>.
/// </summary>
public static class GatewayConfigurationValidation
{
    /// <summary>
    /// Validates the <see cref="GatewayConfiguration"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="configuration">The configuration instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configuration"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this GatewayConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(configuration.Id))
        {
            errors.Add("Id must not be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(configuration.JwtSecret))
        {
            errors.Add("JwtSecret must not be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(configuration.DatabaseConnectionString))
        {
            errors.Add("DatabaseConnectionString must not be empty or whitespace.");
        }

        if (configuration.MinKeyLength <= 0)
        {
            errors.Add("MinKeyLength must be greater than zero.");
        }

        if (configuration.MaxKeyLength <= 0)
        {
            errors.Add("MaxKeyLength must be greater than zero.");
        }

        if (configuration.MaxKeyLength < configuration.MinKeyLength)
        {
            errors.Add("MaxKeyLength must be greater than or equal to MinKeyLength.");
        }

        if (configuration.DefaultKeyExpirationDays <= 0)
        {
            errors.Add("DefaultKeyExpirationDays must be greater than zero.");
        }

        if (configuration.AuditLogRetentionDays <= 0)
        {
            errors.Add("AuditLogRetentionDays must be greater than zero.");
        }

        if (configuration.DefaultRateLimitPerHour <= 0)
        {
            errors.Add("DefaultRateLimitPerHour must be greater than zero.");
        }

        if (configuration.MaxConcurrentRequests <= 0)
        {
            errors.Add("MaxConcurrentRequests must be greater than zero.");
        }

        if (configuration.UpdatedAt == DateTime.MinValue)
        {
            errors.Add("UpdatedAt must not be the default value.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the <see cref="GatewayConfiguration"/> instance is valid.
    /// </summary>
    /// <param name="configuration">The configuration instance to check.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configuration"/> is null.</exception>
    public static bool IsValid(this GatewayConfiguration configuration) => Validate(configuration).Count == 0;

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the <see cref="GatewayConfiguration"/> instance is invalid.
    /// </summary>
    /// <param name="configuration">The configuration instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configuration"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this GatewayConfiguration configuration)
    {
        var errors = Validate(configuration);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", errors), nameof(configuration));
        }
    }
}
