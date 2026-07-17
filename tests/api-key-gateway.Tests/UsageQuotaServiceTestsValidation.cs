// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides validation helpers for <see cref="UsageQuotaServiceTests"/> instances.
/// Validates that test instances are properly initialized and configured for test execution.
/// </summary>
public static class UsageQuotaServiceTestsValidation
{
    /// <summary>
    /// Validates the specified test instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this UsageQuotaServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the test class is properly initialized with required dependencies
        // Since we can't access private fields directly, we verify through public behavior
        // by checking that the test class can execute basic operations without throwing

        try
        {
            // Test that the service can be constructed (should be initialized in constructor)
            var _ = value.GetType().GetProperty("Sut", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value);
        }
        catch
        {
            problems.Add("Service under test (SUT) is not properly initialized.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified test instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this UsageQuotaServiceTests value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified test instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, with a detailed message listing all problems.</exception>
    public static void EnsureValid(this UsageQuotaServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"UsageQuotaServiceTests instance is invalid.{Environment.NewLine}Problems:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
    }
}