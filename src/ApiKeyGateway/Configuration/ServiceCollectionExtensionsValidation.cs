// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using ApiKeyGateway.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="ServiceCollectionExtensions"/> configuration.
/// </summary>
public static class ServiceCollectionExtensionsValidation
{
    /// <summary>
    /// Validates the gateway configuration derived from <see cref="ServiceCollectionExtensions"/> extension methods.
    /// </summary>
    /// <param name="value">The configuration instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this GatewayConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate boolean flags - no specific validation needed beyond null check
        // (they're always valid as they're just flags)

        // Validate key length constraints
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

        // Validate expiration period
        if (value.DefaultKeyExpirationDays <= 0)
        {
            errors.Add("DefaultKeyExpirationDays must be greater than zero.");
        }

        // Validate audit log retention
        if (value.AuditLogRetentionDays <= 0)
        {
            errors.Add("AuditLogRetentionDays must be greater than zero.");
        }

        // Validate rate limiting configuration
        if (value.EnableRateLimiting && value.DefaultRateLimitPerHour <= 0)
        {
            errors.Add("DefaultRateLimitPerHour must be greater than zero when rate limiting is enabled.");
        }

        if (value.DefaultRateLimitPerHour < 0)
        {
            errors.Add("DefaultRateLimitPerHour must be non-negative.");
        }

        // Validate concurrent request limit
        if (value.MaxConcurrentRequests <= 0)
        {
            errors.Add("MaxConcurrentRequests must be greater than zero.");
        }

        // Validate clock skew tolerance (should be reasonable value)
        if (value.ClockSkewToleranceSeconds < 0)
        {
            errors.Add("ClockSkewToleranceSeconds must be non-negative.");
        }

        // Validate SSL requirement is consistent with typical deployment scenarios
        // (This is more of a configuration best practice than a hard constraint)

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the gateway configuration is valid.
    /// </summary>
    /// <param name="value">The configuration instance to check.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this GatewayConfiguration value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the gateway configuration is invalid.
    /// </summary>
    /// <param name="value">The configuration instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the configuration is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this GatewayConfiguration value)
    {
        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", errors), nameof(value));
        }
    }
}
