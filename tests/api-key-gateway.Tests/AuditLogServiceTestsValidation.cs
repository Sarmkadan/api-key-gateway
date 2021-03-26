using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Tests
{
    /// <summary>
    /// Validation helpers for <see cref="AuditLogServiceTests"/> test cases.
    /// </summary>
    public static class AuditLogServiceTestsValidation
    {
        /// <summary>
        /// Validates an <see cref="AuditLogServiceTests"/> instance for common issues.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> Validate(this AuditLogServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // No instance members to validate based on public API

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether an <see cref="AuditLogServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this AuditLogServiceTests value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that an <see cref="AuditLogServiceTests"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid.</exception>
        public static void EnsureValid(this AuditLogServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"AuditLogServiceTests is not valid. Problems: {string.Join(", ", problems)}");
            }
        }
    }
}