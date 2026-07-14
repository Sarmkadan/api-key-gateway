using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace ApiKeyGateway.Utilities
{
    /// <summary>
    /// Provides validation helpers for <see cref="JsonSerializationHelper"/> operations.
    /// Validates the behavior of JSON serialization and deserialization methods.
    /// </summary>
    public static class JsonSerializationHelperValidation
    {
        /// <summary>
        /// Validates the behavior of <see cref="JsonSerializationHelper"/> methods.
        /// Returns any validation problems found in the serialization behavior.
        /// </summary>
        /// <returns>A list of validation errors; empty if valid.</returns>
        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();

            // Test SerializeCompact with a simple object
            var testObject = new { Name = "Test", Value = 42 };
            var serializedCompact = JsonSerializationHelper.SerializeCompact(testObject);
            if (string.IsNullOrWhiteSpace(serializedCompact))
            {
                errors.Add("SerializeCompact returned null or whitespace.");
            }
            else if (serializedCompact == "{}")
            {
                errors.Add("SerializeCompact returned empty object serialization.");
            }
            else
            {
                // Verify it's valid JSON
                if (!JsonSerializationHelper.IsValidJson(serializedCompact))
                {
                    errors.Add("SerializeCompact produced invalid JSON.");
                }
            }

            // Test SerializeFormatted with a simple object
            var serializedFormatted = JsonSerializationHelper.SerializeFormatted(testObject);
            if (string.IsNullOrWhiteSpace(serializedFormatted))
            {
                errors.Add("SerializeFormatted returned null or whitespace.");
            }
            else if (serializedFormatted == "{}")
            {
                errors.Add("SerializeFormatted returned empty object serialization.");
            }
            else
            {
                // Verify it's valid JSON
                if (!JsonSerializationHelper.IsValidJson(serializedFormatted))
                {
                    errors.Add("SerializeFormatted produced invalid JSON.");
                }
            }

            // Test Deserialize with valid JSON
            try
            {
                var deserialized = JsonSerializationHelper.Deserialize<TestDto>(serializedCompact);
                if (deserialized == null)
                {
                    errors.Add("Deserialize returned null for valid JSON input.");
                }
                else if (deserialized.Name != "Test" || deserialized.Value != 42)
                {
                    errors.Add("Deserialize did not correctly deserialize the object.");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Deserialize threw an exception: {ex.Message}");
            }

            // Test SafeDeserialize with valid JSON
            var safeDeserialized = JsonSerializationHelper.SafeDeserialize<TestDto>(serializedCompact);
            if (safeDeserialized == null)
            {
                errors.Add("SafeDeserialize returned null for valid JSON input.");
            }
            else if (safeDeserialized.Name != "Test" || safeDeserialized.Value != 42)
            {
                errors.Add("SafeDeserialize did not correctly deserialize the object.");
            }

            // Test SafeDeserialize with invalid JSON
            var invalidDeserialized = JsonSerializationHelper.SafeDeserialize<TestDto>("invalid json");
            if (invalidDeserialized != null)
            {
                errors.Add("SafeDeserialize did not return null for invalid JSON input.");
            }

            // Test IsValidJson with valid JSON
            if (!JsonSerializationHelper.IsValidJson(serializedCompact))
            {
                errors.Add("IsValidJson returned false for valid JSON.");
            }

            // Test IsValidJson with invalid JSON
            if (JsonSerializationHelper.IsValidJson("invalid json"))
            {
                errors.Add("IsValidJson returned true for invalid JSON.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the <see cref="JsonSerializationHelper"/> methods behave correctly.
        /// </summary>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool IsValid()
        {
            return Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the <see cref="JsonSerializationHelper"/> methods behave correctly.
        /// Throws an exception if validation fails.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if validation fails with a list of validation errors.</exception>
        public static void EnsureValid()
        {
            var errors = Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"JsonSerializationHelper validation failed. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
        }

        private sealed class TestDto
        {
            public string? Name { get; set; }
            public int Value { get; set; }
        }
    }
}
