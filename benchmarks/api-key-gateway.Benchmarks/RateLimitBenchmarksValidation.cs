// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Benchmarks;

/// <summary>
/// Provides validation helpers for <see cref="RateLimitBenchmarks"/> instances.
/// </summary>
public static class RateLimitBenchmarksValidation
{
    /// <summary>
    /// Validates a <see cref="RateLimitBenchmarks"/> instance by executing its benchmark methods
    /// and verifying they return valid results according to business logic constraints.
    /// </summary>
    /// <param name="value">The benchmarks instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RateLimitBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate GetWindowEnd_Minute returns a valid future time
        var windowEndMinute = value.GetWindowEnd_Minute();
        if (windowEndMinute <= DateTime.UtcNow)
        {
            errors.Add("GetWindowEnd_Minute must return a time in the future");
        }

        // Validate GetWindowEnd_Hour returns a valid future time
        var windowEndHour = value.GetWindowEnd_Hour();
        if (windowEndHour <= DateTime.UtcNow)
        {
            errors.Add("GetWindowEnd_Hour must return a time in the future");
        }

        // Validate GetWindowStart_Minute returns a valid past time
        var windowStartMinute = value.GetWindowStart_Minute();
        if (windowStartMinute >= DateTime.UtcNow)
        {
            errors.Add("GetWindowStart_Minute must return a time in the past");
        }

        // Validate GetSecondsUntilAllowed_Limited returns non-negative value
        var secondsUntilAllowed = value.GetSecondsUntilAllowed_Limited();
        if (secondsUntilAllowed < 0)
        {
            errors.Add("GetSecondsUntilAllowed_Limited must not be negative");
        }

        // Validate CalculateQuotagePercentage returns value between 0 and 100
        var percentage = value.CalculateQuotagePercentage();
        if (percentage < 0 || percentage > 100)
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