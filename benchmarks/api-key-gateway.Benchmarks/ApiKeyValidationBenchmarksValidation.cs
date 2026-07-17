// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using ApiKeyGateway.Validation;

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
        ValidateBenchmarkResult(
            value.ValidateFormat_32Char_Valid,
            "ValidateFormat_32Char_Valid");

        ValidateBenchmarkResult(
            value.ValidateFormat_64Char_Valid,
            "ValidateFormat_64Char_Valid");

        ValidateBenchmarkResult(
            value.ValidateFormat_WeakEntropy,
            "ValidateFormat_WeakEntropy");

        ValidateBenchmarkResult(
            value.ValidateFormat_TooShort,
            "ValidateFormat_TooShort");

        ValidateBenchmarkResult(
            value.ValidateName_Valid,
            "ValidateName_Valid");

        ValidateBenchmarkResult(
            value.ValidateName_TooLong,
            "ValidateName_TooLong");

        ValidateBenchmarkResult(
            value.ValidateQuota_Valid,
            "ValidateQuota_Valid");

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a benchmark method result and adds any problems to the list.
    /// </summary>
    /// <param name="benchmarkMethod">The benchmark method to invoke.</param>
    /// <param name="methodName">The name of the method for error reporting.</param>
    private static void ValidateBenchmarkResult(
        Func<ValidationResult> benchmarkMethod,
        string methodName)
    {
        ArgumentNullException.ThrowIfNull(benchmarkMethod);

        try
        {
            var result = benchmarkMethod();
            if (result is null)
            {
                throw new InvalidOperationException($"{methodName}() returned null");
            }

            if (!result.IsValid && result.Message is not null)
            {
                throw new InvalidOperationException($"{methodName} validation failed: {result.Message}");
            }
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new InvalidOperationException($"{methodName}() threw an exception: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Determines whether an <see cref="ApiKeyValidationBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmark instance to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ApiKeyValidationBenchmarks value)
        => value.Validate().Count == 0;

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
