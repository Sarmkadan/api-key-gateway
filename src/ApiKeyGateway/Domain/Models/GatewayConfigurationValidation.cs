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
    /// <param name="value">The configuration instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this GatewayConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrEmpty(value.Id))
        {
            errors.Add("Id must not be empty.");
        }

        if (string.IsNullOrEmpty(value.JwtSecret))
        {
            errors.Add("JwtSecret must not be empty.");
        }

        if (string.IsNullOrEmpty(value.DatabaseConnectionString))
        {
            errors.Add("DatabaseConnectionString must not be empty.");
        }

        if (value.MinKeyLength <= 0)
        {
            errors.Add("MinKeyLength must be greater than zero.");
        }

        if (value.MaxKeyLength <= 0)
        {
            errors.Add("MaxKeyLength must be greater than zero.");
        }

        if (value.MaxKeyLength < value.MinKeyLength)
        {
            errors.Add("MaxKeyLength must be greater than or equal to MinKeyLength.");
        }

        if (value.DefaultKeyExpirationDays <= 0)
        {
            errors.Add("DefaultKeyExpirationDays must be greater than zero.");
        }

        if (value.AuditLogRetentionDays <= 0)
        {
            errors.Add("AuditLogRetentionDays must be greater than zero.");
        }

        if (value.DefaultRateLimitPerHour <= 0)
        {
            errors.Add("DefaultRateLimitPerHour must be greater than zero.");
        }

        if (value.MaxConcurrentRequests <= 0)
        {
            errors.Add("MaxConcurrentRequests must be greater than zero.");
        }

        if (value.UpdatedAt == DateTime.MinValue)
        {
            errors.Add("UpdatedAt must not be the default value.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the <see cref="GatewayConfiguration"/> instance is valid.
    /// </summary>
    /// <param name="value">The configuration instance to check.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this GatewayConfiguration value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the <see cref="GatewayConfiguration"/> instance is invalid.
    /// </summary>
    /// <param name="value">The configuration instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this GatewayConfiguration value)
    {
        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", errors), nameof(value));
        }
    }
}
