// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Benchmarks;

/// <summary>
/// Provides validation methods for <see cref="CryptoBenchmarks"/> instances.
/// </summary>
public static class CryptoBenchmarksValidation
{
    /// <summary>
    /// Validates a <see cref="CryptoBenchmarks"/> instance by invoking its benchmark methods and
    /// verifying their outputs meet expected criteria.
    /// </summary>
    /// <param name="value">The benchmarks instance to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CryptoBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        ValidateMethod(value, nameof(CryptoBenchmarks.GenerateRandom_32), 32, value.GenerateRandom_32, errors);
        ValidateMethod(value, nameof(CryptoBenchmarks.GenerateRandom_64), 64, value.GenerateRandom_64, errors);
        ValidateMethod(value, nameof(CryptoBenchmarks.GenerateRandom_128), 128, value.GenerateRandom_128, errors);
        ValidateMethod(value, nameof(CryptoBenchmarks.ComputeSha256), 64, value.ComputeSha256, errors);
        ValidateMethod(value, nameof(CryptoBenchmarks.ComputeHmac), 64, value.ComputeHmac, errors);

        // VerifyHash returns bool, which is always valid - we just check that it doesn't throw
        try
        {
            _ = value.VerifyHash();
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CryptoBenchmarks.VerifyHash)}() threw an exception: {ex.Message}");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CryptoBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmarks instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CryptoBenchmarks value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="CryptoBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmarks instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this CryptoBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"The {nameof(CryptoBenchmarks)} instance is invalid. Errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Validates that a benchmark method returns a non-null string of the expected length.
    /// </summary>
    /// <param name="benchmark">The benchmark instance.</param>
    /// <param name="methodName">The name of the benchmark method.</param>
    /// <param name="expectedLength">The expected length of the returned string.</param>
    /// <param name="method">The benchmark method to invoke.</param>
    /// <param name="errors">The list to add validation errors to.</param>
    private static void ValidateMethod(
        CryptoBenchmarks benchmark,
        string methodName,
        int expectedLength,
        Func<string> method,
        List<string> errors)
    {
        try
        {
            var result = method();
            if (result is null)
            {
                errors.Add($"{methodName}() returned null.");
            }
            else if (result.Length != expectedLength)
            {
                errors.Add($"{methodName}() must return a string with length {expectedLength}, but returned length was {result.Length}.");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"{methodName}() threw an exception: {ex.Message}");
        }
    }
}
