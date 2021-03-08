// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace ApiKeyGateway.Benchmarks;

/// <summary>
/// Provides validation helpers for <see cref="RateLimitBenchmarks"/> instances.
/// </summary>
public static class RateLimitBenchmarksValidation
{
    /// <summary>
    /// Validates a <see cref="RateLimitBenchmarks"/> instance.
    /// </summary>
    /// <param name="value">The benchmarks instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RateLimitBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.GetWindowEnd_Minute == default)
        {
            errors.Add("GetWindowEnd_Minute must not be default(DateTime)");
        }

        if (value.GetWindowEnd_Hour == default)
        {
            errors.Add("GetWindowEnd_Hour must not be default(DateTime)");
        }

        if (value.GetWindowStart_Minute == default)
        {
            errors.Add("GetWindowStart_Minute must not be default(DateTime)");
        }

        if (value.GetSecondsUntilAllowed_Limited() < 0)
        {
            errors.Add("GetSecondsUntilAllowed_Limited must not be negative");
        }

        if (value.CalculateQuotagePercentage() < 0 || value.CalculateQuotagePercentage() > 100)
        {
            errors.Add("CalculateQuotagePercentage must be between 0 and 100");
        }

        return errors;
    }

    /// <summary>
    /// Determines whether a <see cref="RateLimitBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmarks instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this RateLimitBenchmarks value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="RateLimitBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmarks instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this RateLimitBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"RateLimitBenchmarks instance is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}