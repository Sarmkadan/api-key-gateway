// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace ApiKeyGateway.Benchmarks;

/// <summary>
/// Validation helpers for <see cref="CryptoBenchmarks"/>.
/// </summary>
public static class CryptoBenchmarksValidation
{
    /// <summary>
    /// Validates a <see cref="CryptoBenchmarks"/> instance by invoking its benchmark methods.
    /// </summary>
    /// <param name="value">The benchmarks instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CryptoBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate GenerateRandom_32 by invoking the benchmark method
        try
        {
            var result32 = value.GenerateRandom_32();
            if (result32 is null)
            {
                errors.Add($"{nameof(CryptoBenchmarks.GenerateRandom_32)}() returned null.");
            }
            else if (result32.Length != 32)
            {
                errors.Add($"{nameof(CryptoBenchmarks.GenerateRandom_32)}() must return a string with length 32, but returned length was {result32.Length}.");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CryptoBenchmarks.GenerateRandom_32)}() threw an exception: {ex.Message}");
        }

        // Validate GenerateRandom_64 by invoking the benchmark method
        try
        {
            var result64 = value.GenerateRandom_64();
            if (result64 is null)
            {
                errors.Add($"{nameof(CryptoBenchmarks.GenerateRandom_64)}() returned null.");
            }
            else if (result64.Length != 64)
            {
                errors.Add($"{nameof(CryptoBenchmarks.GenerateRandom_64)}() must return a string with length 64, but returned length was {result64.Length}.");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CryptoBenchmarks.GenerateRandom_64)}() threw an exception: {ex.Message}");
        }

        // Validate GenerateRandom_128 by invoking the benchmark method
        try
        {
            var result128 = value.GenerateRandom_128();
            if (result128 is null)
            {
                errors.Add($"{nameof(CryptoBenchmarks.GenerateRandom_128)}() returned null.");
            }
            else if (result128.Length != 128)
            {
                errors.Add($"{nameof(CryptoBenchmarks.GenerateRandom_128)}() must return a string with length 128, but returned length was {result128.Length}.");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CryptoBenchmarks.GenerateRandom_128)}() threw an exception: {ex.Message}");
        }

        // Validate ComputeSha256 by invoking the benchmark method
        try
        {
            var resultSha256 = value.ComputeSha256();
            if (resultSha256 is null)
            {
                errors.Add($"{nameof(CryptoBenchmarks.ComputeSha256)}() returned null.");
            }
            else if (resultSha256.Length != 64)
            {
                errors.Add($"{nameof(CryptoBenchmarks.ComputeSha256)}() must return a SHA-256 hex string with length 64, but returned length was {resultSha256.Length}.");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CryptoBenchmarks.ComputeSha256)}() threw an exception: {ex.Message}");
        }

        // Validate ComputeHmac by invoking the benchmark method
        try
        {
            var resultHmac = value.ComputeHmac();
            if (resultHmac is null)
            {
                errors.Add($"{nameof(CryptoBenchmarks.ComputeHmac)}() returned null.");
            }
            else if (resultHmac.Length != 64)
            {
                errors.Add($"{nameof(CryptoBenchmarks.ComputeHmac)}() must return an HMAC-SHA256 hex string with length 64, but returned length was {resultHmac.Length}.");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CryptoBenchmarks.ComputeHmac)}() threw an exception: {ex.Message}");
        }

        // Validate VerifyHash by invoking the benchmark method
        try
        {
            var resultVerify = value.VerifyHash();
            // VerifyHash returns bool, which is always valid
            // We just check that it doesn't throw
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
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"The {nameof(CryptoBenchmarks)} instance is invalid. Errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }
}
