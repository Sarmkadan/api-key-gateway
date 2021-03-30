// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace ApiKeyGateway.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="StringExtensionsTests"/> test class.
    /// </summary>
    public static class StringExtensionsTestsValidation
    {
        /// <summary>
        /// Validates the test class instance and returns a list of validation problems.
        /// </summary>
        /// <param name="value">The test class instance to validate.</param>
        /// <returns>Collection of human-readable validation problems.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this StringExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate test method attributes and naming conventions
            var testMethods = value.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name.StartsWith("Truncate") || 
                            m.Name.StartsWith("TruncateWithEllipsis") ||
                            m.Name.StartsWith("ContainsAny") ||
                            m.Name.StartsWith("StartsWithAny") ||
                            m.Name.StartsWith("ToSlug"));

            foreach (var method in testMethods)
            {
                // Check for null/empty test names
                if (string.IsNullOrEmpty(method.Name))
                {
                    problems.Add($"Test method has null/empty name: {method.DeclaringType?.Name}");
                }

                // Check for test method attributes
                if (!method.GetCustomAttributes(typeof(FactAttribute), false).Any() &&
                    !method.GetCustomAttributes(typeof(TheoryAttribute), false).Any())
                {
                    problems.Add($"Test method {method.Name} is missing [Fact] or [Theory] attribute");
                }

                // Check for Should().Be() usage in test names
                if (method.Name.Contains("Should"))
                {
                    problems.Add($"Test method {method.Name} uses 'Should' in name - prefer descriptive naming");
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
        public static bool IsValid(this StringExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return !value.Validate().Any();
        }

        /// <summary>
        /// Validates the test class instance and throws if invalid.
        /// </summary>
        /// <param name="value">The test class instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when validation fails with list of problems.</exception>
        public static void EnsureValid(this StringExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Any())
            {
                throw new ArgumentException(string.Join("\n", problems), nameof(value));
            }
        }
    }
}
