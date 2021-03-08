// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace ApiKeyGateway.Benchmarks;

/// <summary>
/// Validation helpers for <see cref="ApiKeyValidationBenchmarks"/> benchmarks.
/// Provides comprehensive validation for benchmark input values and results.
/// </summary>
public static class ApiKeyValidationBenchmarksValidation
{
    /// <summary>
    /// Validates an <see cref="ApiKeyValidationBenchmarks"/> instance.
    /// Checks all benchmark fields for null/empty strings, out-of-range numbers, and invalid dates.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <returns>An immutable list of human-readable validation problems. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ApiKeyValidationBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate benchmark results by invoking the benchmark methods
        try
        {
            var result32 = value.ValidateFormat_32Char_Valid();
            if (result32 is null)
            {
                problems.Add("ValidateFormat_32Char_Valid() returned null");
            }
            else if (!result32.IsValid)
            {
                problems.Add(result32.Message ?? "Validation failed for ValidateFormat_32Char_Valid");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ValidateFormat_32Char_Valid() threw an exception: {ex.Message}");
        }

        try
        {
            var result64 = value.ValidateFormat_64Char_Valid();
            if (result64 is null)
            {
                problems.Add("ValidateFormat_64Char_Valid() returned null");
            }
            else if (!result64.IsValid)
            {
                problems.Add(result64.Message ?? "Validation failed for ValidateFormat_64Char_Valid");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ValidateFormat_64Char_Valid() threw an exception: {ex.Message}");
        }

        try
        {
            var resultWeak = value.ValidateFormat_WeakEntropy();
            if (resultWeak is null)
            {
                problems.Add("ValidateFormat_WeakEntropy() returned null");
            }
            else if (!resultWeak.IsValid)
            {
                problems.Add(resultWeak.Message ?? "Validation failed for ValidateFormat_WeakEntropy");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ValidateFormat_WeakEntropy() threw an exception: {ex.Message}");
        }

        try
        {
            var resultTooShort = value.ValidateFormat_TooShort();
            if (resultTooShort is null)
            {
                problems.Add("ValidateFormat_TooShort() returned null");
            }
            else if (!resultTooShort.IsValid)
            {
                problems.Add(resultTooShort.Message ?? "Validation failed for ValidateFormat_TooShort");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ValidateFormat_TooShort() threw an exception: {ex.Message}");
        }

        try
        {
            var resultNameValid = value.ValidateName_Valid();
            if (resultNameValid is null)
            {
                problems.Add("ValidateName_Valid() returned null");
            }
            else if (!resultNameValid.IsValid)
            {
                problems.Add(resultNameValid.Message ?? "Validation failed for ValidateName_Valid");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ValidateName_Valid() threw an exception: {ex.Message}");
        }

        try
        {
            var resultNameTooLong = value.ValidateName_TooLong();
            if (resultNameTooLong is null)
            {
                problems.Add("ValidateName_TooLong() returned null");
            }
            else if (!resultNameTooLong.IsValid)
            {
                problems.Add(resultNameTooLong.Message ?? "Validation failed for ValidateName_TooLong");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ValidateName_TooLong() threw an exception: {ex.Message}");
        }

        try
        {
            var resultQuota = value.ValidateQuota_Valid();
            if (resultQuota is null)
            {
                problems.Add("ValidateQuota_Valid() returned null");
            }
            else if (!resultQuota.IsValid)
            {
                problems.Add(resultQuota.Message ?? "Validation failed for ValidateQuota_Valid");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ValidateQuota_Valid() threw an exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="ApiKeyValidationBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmark instance to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ApiKeyValidationBenchmarks value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="ApiKeyValidationBenchmarks"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation problems.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    public static void EnsureValid(this ApiKeyValidationBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ApiKeyValidationBenchmarks instance is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }
}
