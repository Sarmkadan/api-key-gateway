using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Examples
{
    /// <summary>
    /// Provides validation methods for <see cref="ApiKeyGatewayExample"/> instances.
    /// </summary>
    public static class ApiKeyGatewayExampleValidation
    {
        /// <summary>
        /// Validates an <see cref="ApiKeyGatewayExample"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ApiKeyGatewayExample value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(value.Id))
            {
                errors.Add("Id is required.");
            }

            if (string.IsNullOrWhiteSpace(value.DisplayKey))
            {
                errors.Add("DisplayKey is required.");
            }

            if (string.IsNullOrWhiteSpace(value.ConsumerId))
            {
                errors.Add("ConsumerId is required.");
            }

            if (value.CreatedAt == default)
            {
                errors.Add("CreatedAt must be a valid date.");
            }

            if (string.IsNullOrWhiteSpace(value.Name))
            {
                errors.Add("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(value.Status))
            {
                errors.Add("Status is required.");
            }
            else if (value.Status.Length > 20)
            {
                errors.Add("Status must be 20 characters or less.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether an <see cref="ApiKeyGatewayExample"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>True if valid; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this ApiKeyGatewayExample value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that an <see cref="ApiKeyGatewayExample"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="value"/> is not valid, containing the validation errors.
        /// </exception>
        public static void EnsureValid(this ApiKeyGatewayExample value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"ApiKeyGatewayExample is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
        }
    }
}