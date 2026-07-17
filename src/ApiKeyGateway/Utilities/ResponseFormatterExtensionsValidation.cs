using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Utilities
{
    /// <summary>
    /// Provides validation extension methods for response types created by <see cref="ResponseFormatterExtensions"/>.
    /// </summary>
    public static class ResponseFormatterExtensionsValidation
    {
        /// <summary>
        /// Validates the specified API response instance.
        /// </summary>
        /// <typeparam name="T">The type of data in the response.</typeparam>
        /// <param name="value">The API response to validate.</param>
        /// <returns>A list of validation problems; empty if the response is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate<T>(this ApiResponse<T> value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            if (value.StatusCode < 100 || value.StatusCode > 999)
            {
                problems.Add($"StatusCode must be between 100 and 999, but was {value.StatusCode}.");
            }

            if (value.Success)
            {
                if (value.StatusCode is < 200 or >= 300)
                {
                    problems.Add("Success is true but StatusCode is not in the 2xx range.");
                }

                if (string.IsNullOrEmpty(value.Message))
                {
                    problems.Add("Success is true but Message is null or empty.");
                }
            }
            else
            {
                if (value.StatusCode is < 400 or >= 600)
                {
                    problems.Add("Success is false but StatusCode is not in the 4xx or 5xx range.");
                }

                if (string.IsNullOrEmpty(value.Message))
                {
                    problems.Add("Success is false but Message is null or empty.");
                }

                if (string.IsNullOrEmpty(value.ErrorCode))
                {
                    problems.Add("Success is false but ErrorCode is null or empty.");
                }
            }

            if (value.Timestamp == default)
            {
                problems.Add("Timestamp must be set to a non-default value.");
            }
            else if (value.Timestamp.Kind != DateTimeKind.Utc)
            {
                problems.Add("Timestamp must be in UTC.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified API response is valid.
        /// </summary>
        /// <typeparam name="T">The type of data in the response.</typeparam>
        /// <param name="value">The API response to check.</param>
        /// <returns><see langword="true"/> if the response is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid<T>(this ApiResponse<T> value)
        {
            return value is not null && value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified API response is valid.
        /// </summary>
        /// <typeparam name="T">The type of data in the response.</typeparam>
        /// <param name="value">The API response to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the response is not valid, containing a list of validation problems.</exception>
        public static void EnsureValid<T>(this ApiResponse<T> value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count == 0)
            {
                return;
            }

            throw new ArgumentException($"ApiResponse is not valid. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }

        /// <summary>
        /// Validates the specified paginated response.
        /// </summary>
        /// <typeparam name="T">The type of items in the paginated response.</typeparam>
        /// <param name="value">The paginated response to validate.</param>
        /// <returns>A list of validation problems; empty if the paginated response is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate<T>(this PaginatedResponse<T> value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            if (value.PageNumber < 1)
            {
                problems.Add($"PageNumber must be positive, but was {value.PageNumber}.");
            }

            if (value.PageSize < 1)
            {
                problems.Add($"PageSize must be positive, but was {value.PageSize}.");
            }

            if (value.TotalCount < 0)
            {
                problems.Add($"TotalCount must be non-negative, but was {value.TotalCount}.");
            }

            if (value.TotalPages < 0)
            {
                problems.Add($"TotalPages must be non-negative, but was {value.TotalPages}.");
            }

            if (value.PageNumber > value.TotalPages && value.TotalPages != 0)
            {
                problems.Add("PageNumber cannot be greater than TotalPages.");
            }

            if (value.Items is null)
            {
                problems.Add("Items collection must not be null.");
            }

            if (value.Timestamp == default)
            {
                problems.Add("Timestamp must be set to a non-default value.");
            }
            else if (value.Timestamp.Kind != DateTimeKind.Utc)
            {
                problems.Add("Timestamp must be in UTC.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified paginated response is valid.
        /// </summary>
        /// <typeparam name="T">The type of items in the paginated response.</typeparam>
        /// <param name="value">The paginated response to check.</param>
        /// <returns><see langword="true"/> if the paginated response is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid<T>(this PaginatedResponse<T> value)
        {
            return value is not null && value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified paginated response is valid.
        /// </summary>
        /// <typeparam name="T">The type of items in the paginated response.</typeparam>
        /// <param name="value">The paginated response to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the paginated response is not valid, containing a list of validation problems.</exception>
        public static void EnsureValid<T>(this PaginatedResponse<T> value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count == 0)
            {
                return;
            }

            throw new ArgumentException($"PaginatedResponse is not valid. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}