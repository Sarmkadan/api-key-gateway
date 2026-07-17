// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Reflection;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides validation helpers for <see cref="ApiKeyModelTests"/> and <see cref="RateLimitModelTests"/> instances.
/// Validates that test classes are properly initialized and configured for test execution
/// by ensuring all required test methods exist via reflection.
/// </summary>
public static class ApiKeyModelTestsValidation
{
    /// <summary>
    /// Validates the specified test instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ApiKeyModelTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();
        ValidateTestMethods(value, problems, typeof(ApiKeyModelTests));
        ValidateTestMethods(value, problems, typeof(RateLimitModelTests));

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that all expected test methods exist on the specified test type.
    /// </summary>
    /// <param name="instance">The test instance to validate.</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    /// <param name="testType">The test class type to validate.</param>
    private static void ValidateTestMethods(ApiKeyModelTests instance, List<string> problems, Type testType)
    {
        var expectedMethods = GetExpectedTestMethods(testType);

        foreach (var methodName in expectedMethods)
        {
            var method = testType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method is null)
            {
                problems.Add($"Test method '{methodName}' is not defined in {testType.Name}.");
            }
        }
    }

    /// <summary>
    /// Gets the expected test method names for a given test class type.
    /// </summary>
    /// <param name="testType">The test class type.</param>
    /// <returns>An enumerable of expected test method names.</returns>
    private static IEnumerable<string> GetExpectedTestMethods(Type testType)
    {
        if (testType == typeof(ApiKeyModelTests))
        {
            return new[]
            {
                nameof(ApiKeyModelTests.CanBeUsed_ActiveNonExpiredKey_ReturnsTrue),
                nameof(ApiKeyModelTests.CanBeUsed_InactiveStatus_ReturnsFalse),
                nameof(ApiKeyModelTests.CanBeUsed_ExpiredKey_ReturnsFalse),
                nameof(ApiKeyModelTests.RecordUsage_Called_IncrementsRequestCountAndUpdatesLastUsed),
                nameof(ApiKeyModelTests.Disable_ActiveKey_SetsDisabledStatusAndTimestamp),
                nameof(ApiKeyModelTests.Enable_DisabledKey_RestoresActiveStatusAndClearsTimestamp),
                nameof(ApiKeyModelTests.IsIpAllowed_NullWhitelist_AllowsAnyIp),
                nameof(ApiKeyModelTests.IsIpAllowed_IpInCommaDelimitedWhitelist_ReturnsTrue),
                nameof(ApiKeyModelTests.IsIpAllowed_IpNotInWhitelist_ReturnsFalse)
            };
        }

        if (testType == typeof(RateLimitModelTests))
        {
            return new[]
            {
                nameof(RateLimitModelTests.CanProcessRequest_CountBelowLimit_ReturnsTrue),
                nameof(RateLimitModelTests.CanProcessRequest_CountAtLimit_ReturnsFalse),
                nameof(RateLimitModelTests.CanProcessRequest_UnlimitedUnit_AlwaysReturnsTrue),
                nameof(RateLimitModelTests.RecordRequest_Called_IncrementsCurrentRequestCount),
                nameof(RateLimitModelTests.ResetWindow_Called_ZeroesCounterAndUpdatesLastResetAt),
                nameof(RateLimitModelTests.GetWindowInSeconds_KnownUnits_ReturnsCorrectDuration)
            };
        }

        return Enumerable.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified test instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ApiKeyModelTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified test instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, with a detailed message listing all problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static void EnsureValid(this ApiKeyModelTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException($"ApiKeyModelTests instance is invalid.{Environment.NewLine}Problems:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
    }
}