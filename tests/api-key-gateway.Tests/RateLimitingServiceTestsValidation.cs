// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides validation helpers for <see cref="RateLimitingServiceTests"/> test class.
/// Validates test method naming conventions, attributes, and structure.
/// </summary>
public static class RateLimitingServiceTestsValidation
{
    /// <summary>
    /// Validates the test class instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The test class instance to validate.</param>
    /// <returns>Collection of human-readable validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RateLimitingServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();
        var testClassType = value.GetType();

        // Validate test method attributes and naming conventions
        var testMethods = testClassType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name.StartsWith("Constructor_") ||
                        m.Name.StartsWith("CheckLimitAsync_") ||
                        m.Name.StartsWith("RecordRequestAsync_") ||
                        m.Name.StartsWith("UpdateLimitAsync_") ||
                        m.Name.StartsWith("ResetWindowAsync_"))
            .ToList();

        if (testMethods.Count == 0)
        {
            problems.Add("Test class has no recognized test methods (Constructor_, CheckLimitAsync_, RecordRequestAsync_, UpdateLimitAsync_, or ResetWindowAsync_ prefixes)");
        }

        foreach (var method in testMethods)
        {
            // Method names from reflection are never null, but we check for empty as defensive coding
            if (string.IsNullOrEmpty(method.Name))
            {
                problems.Add($"Test method has null/empty name: {method.DeclaringType?.Name}");
            }

            // Check for test method attributes using modern API
            var hasFactAttribute = method.GetCustomAttributes<FactAttribute>().Any();
            var hasTheoryAttribute = method.GetCustomAttributes<TheoryAttribute>().Any();

            if (!hasFactAttribute && !hasTheoryAttribute)
            {
                problems.Add($"Test method {method.Name} is missing [Fact] or [Theory] attribute");
            }

            // Check for Should() usage in test names - prefer descriptive naming like "ReturnsTrue" over "ShouldReturnTrue"
            if (method.Name.Contains("Should", StringComparison.Ordinal))
            {
                problems.Add($"Test method {method.Name} uses 'Should' in name - prefer descriptive naming without 'Should'");
            }

            // Validate method signatures for async Task patterns using pattern matching
            switch (method.Name)
            {
                case var _ when method.Name.StartsWith("Constructor_"):
                    if (method.ReturnType != typeof(void))
                    {
                        problems.Add($"Constructor test method {method.Name} should return void, found {method.ReturnType.Name}");
                    }
                    break;

                case var _ when method.Name.StartsWith("CheckLimitAsync_") ||
                             method.Name.StartsWith("RecordRequestAsync_") ||
                             method.Name.StartsWith("UpdateLimitAsync_") ||
                             method.Name.StartsWith("ResetWindowAsync_"):
                    if (method.ReturnType != typeof(Task))
                    {
                        problems.Add($"Async test method {method.Name} should return Task, found {method.ReturnType.Name}");
                    }
                    break;
            }
        }

        return problems;
    }

    /// <summary>
    /// Checks if the test class instance is valid according to validation rules.
    /// </summary>
    /// <param name="value">The test class instance to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this RateLimitingServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Validates the test class instance and throws if invalid.
    /// </summary>
    /// <param name="value">The test class instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems.</exception>
    public static void EnsureValid(this RateLimitingServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(Environment.NewLine, problems), nameof(value));
        }
    }
}