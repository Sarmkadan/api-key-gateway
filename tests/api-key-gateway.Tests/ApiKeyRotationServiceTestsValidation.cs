using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="ApiKeyRotationServiceTests"/> instances.
    /// </summary>
    public static class ApiKeyRotationServiceTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="ApiKeyRotationServiceTests"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ApiKeyRotationServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // No public members to validate based on the provided API
            // All members are methods with no state to validate

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="ApiKeyRotationServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this ApiKeyRotationServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="ApiKeyRotationServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing the list of problems.</exception>
        public static void EnsureValid(this ApiKeyRotationServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"ApiKeyRotationServiceTests instance is not valid. Problems: {string.Join(", ", problems)}");
            }
        }
    }
}