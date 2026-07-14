// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace ApiKeyGateway.Benchmarks;

/// <summary>
/// Validation helpers for <see cref="CacheKeyGenerationBenchmarks"/> instances.
/// </summary>
public static class CacheKeyGenerationBenchmarksValidation
{
    /// <summary>
    /// Validates that all cache key strings in the benchmark are non-null, non-empty, and valid.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CacheKeyGenerationBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        ValidateString(problems, nameof(value.RateLimitKey), value.RateLimitKey());
        ValidateString(problems, nameof(value.ApiKeyKey), value.ApiKeyKey());
        ValidateString(problems, nameof(value.ApiKeyMetadataKey), value.ApiKeyMetadataKey());
        ValidateString(problems, nameof(value.QuotaKey), value.QuotaKey());
        ValidateString(problems, nameof(value.ExternalApiKey_NoParams), value.ExternalApiKey_NoParams());
        ValidateString(problems, nameof(value.ExternalApiKey_ThreeParams), value.ExternalApiKey_ThreeParams());
        ValidateString(problems, nameof(value.ExternalApiKey_SixParams), value.ExternalApiKey_SixParams());

        return problems;
    }

    /// <summary>
    /// Determines whether all cache key strings in the benchmark are non-null, non-empty, and valid.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CacheKeyGenerationBenchmarks value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that all cache key strings in the benchmark are non-null, non-empty, and valid.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the benchmark contains invalid values.</exception>
    public static void EnsureValid(this CacheKeyGenerationBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"CacheKeyGenerationBenchmarks is invalid. Problems: {string.Join(", ", problems)}");
        }
    }

    private static void ValidateString(List<string> problems, string memberName, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            problems.Add($"{memberName} is null or empty");
        }
    }
}