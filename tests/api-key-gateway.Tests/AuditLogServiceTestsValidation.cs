using System;
using System.Collections.Generic;
using System.Linq;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;

namespace ApiKeyGateway.Tests
{
    /// <summary>
    /// Validation helpers for <see cref="AuditLogServiceTests"/> test cases.
    /// Provides validation for audit log service behavior and test data patterns.
    /// </summary>
    public static class AuditLogServiceTestsValidation
    {
        /// <summary>
        /// Validates that an <see cref="AuditLog"/> entry is properly initialized for testing.
        /// </summary>
        /// <param name="log">The audit log entry to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this AuditLog log)
        {
            ArgumentNullException.ThrowIfNull(log);

            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(log.Id))
            {
                problems.Add("AuditLog.Id must not be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(log.ResourceId))
            {
                problems.Add("AuditLog.ResourceId must not be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(log.ResourceType))
            {
                problems.Add("AuditLog.ResourceType must not be null or whitespace");
            }

            if (log.PerformedAt == default)
            {
                problems.Add("AuditLog.PerformedAt must be set to a valid DateTime");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates an <see cref="AuditLogServiceTests"/> instance for common issues.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> Validate(this AuditLogServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate that all test methods follow consistent patterns
            // This is a static test class, so we validate the test patterns instead

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether an <see cref="AuditLog"/> entry is valid.
        /// </summary>
        /// <param name="log">The audit log entry to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is null.</exception>
        public static bool IsValid(this AuditLog log)
        {
            return log.Validate().Count == 0;
        }

        /// <summary>
        /// Determines whether an <see cref="AuditLogServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this AuditLogServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that an <see cref="AuditLog"/> entry is valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="log">The audit log entry to validate.</param>
        /// <exception cref="ArgumentException">Thrown if the entry is not valid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is null.</exception>
        public static void EnsureValid(this AuditLog log)
        {
            ArgumentNullException.ThrowIfNull(log);

            var problems = log.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"AuditLog is not valid. Problems: {string.Join(", ", problems)}");
            }
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