using System;
using System.Collections.Generic;
using System.Globalization;

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="UsageQuota"/>.
/// </summary>
public static class UsageQuotaValidation
{
    /// <summary>
    /// Validates the <see cref="UsageQuota"/> instance and returns a list of human‑readable problems.
    /// </summary>
    /// <param name="value">The <see cref="UsageQuota"/> instance to validate.</param>
    /// <returns>A read‑only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this UsageQuota value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Id must not be null or empty
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id must not be null or empty.");
        }

        // ApiKeyId must not be null or empty
        if (string.IsNullOrWhiteSpace(value.ApiKeyId))
        {
            errors.Add("ApiKeyId must not be null or empty.");
        }

        // QuotaLimit must be non-negative
        if (value.QuotaLimit < 0)
        {
            errors.Add($"QuotaLimit must be non-negative (found {value.QuotaLimit}).");
        }

        // Period must be a defined enum value
        if (!Enum.IsDefined(typeof(Enums.QuotaPeriod), value.Period))
        {
            errors.Add("Period must be a valid QuotaPeriod value.");
        }

        // CreatedAt must not be the default DateTime
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must not be the default DateTime.");
        }

        // PeriodStartAt must not be the default DateTime
        if (value.PeriodStartAt == default)
        {
            errors.Add("PeriodStartAt must not be the default DateTime.");
        }

        // CurrentUsage must be non-negative
        if (value.CurrentUsage < 0)
        {
            errors.Add($"CurrentUsage must be non-negative (found {value.CurrentUsage}).");
        }

        // If the quota is enabled, CurrentUsage must not exceed QuotaLimit
        if (value.IsEnabled && value.CurrentUsage > value.QuotaLimit)
        {
            errors.Add($"CurrentUsage ({value.CurrentUsage}) exceeds QuotaLimit ({value.QuotaLimit}).");
        }

        // PeriodStartAt must be before the period end
        var periodEnd = value.GetPeriodEndUtc();
        if (periodEnd <= value.PeriodStartAt)
        {
            errors.Add($"PeriodStartAt ({value.PeriodStartAt:o}) must be before the period end ({periodEnd:o}).");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the <see cref="UsageQuota"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="UsageQuota"/> instance to check.</param>
    /// <returns><c>true</c> if valid; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this UsageQuota value) => Validate(value).Count == 0;

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the <see cref="UsageQuota"/> instance is invalid.
    /// </summary>
    /// <param name="value">The <see cref="UsageQuota"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this UsageQuota value)
    {
        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", errors), nameof(value));
        }
    }
}