using System;
using System.Collections.Generic;
using System.Globalization;

namespace ApiKeyGateway.Domain.Models
{
    /// <summary>
    /// Provides validation helpers for <see cref="UsageRecord"/> instances.
    /// </summary>
    public static class UsageRecordValidation
    {
        /// <summary>
        /// Validates the specified <see cref="UsageRecord"/> instance.
        /// </summary>
        /// <param name="value">The usage record to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this UsageRecord value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate Id
            if (string.IsNullOrWhiteSpace(value.Id))
            {
                errors.Add("Id must be a non-empty string.");
            }

            // Validate ApiKeyId
            if (string.IsNullOrWhiteSpace(value.ApiKeyId))
            {
                errors.Add("ApiKeyId must be a non-empty string.");
            }

            // Validate ConsumerId
            if (string.IsNullOrWhiteSpace(value.ConsumerId))
            {
                errors.Add("ConsumerId must be a non-empty string.");
            }

            // Validate RecordedAt (must be a valid date, not default)
            if (value.RecordedAt == default)
            {
                errors.Add("RecordedAt must be a valid DateTime.");
            }
            else if (value.RecordedAt > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("RecordedAt cannot be in the future.");
            }

            // Validate Endpoint
            if (string.IsNullOrWhiteSpace(value.Endpoint))
            {
                errors.Add("Endpoint must be a non-empty string.");
            }

            // Validate Method
            if (string.IsNullOrWhiteSpace(value.Method))
            {
                errors.Add("Method must be a non-empty string.");
            }
            else if (!IsValidHttpMethod(value.Method))
            {
                errors.Add($"Method '{value.Method}' is not a valid HTTP method.");
            }

            // Validate ResponseStatusCode
            if (value.ResponseStatusCode < 100 || value.ResponseStatusCode > 999)
            {
                errors.Add("ResponseStatusCode must be a valid HTTP status code (100-999).");
            }

            // Validate RequestBytes
            if (value.RequestBytes < 0)
            {
                errors.Add("RequestBytes must be a non-negative number.");
            }

            // Validate ResponseBytes
            if (value.ResponseBytes < 0)
            {
                errors.Add("ResponseBytes must be a non-negative number.");
            }

            // Validate ResponseTimeMs
            if (value.ResponseTimeMs < 0)
            {
                errors.Add("ResponseTimeMs must be a non-negative number.");
            }

            // Validate ErrorCode (if present, must be non-empty)
            if (value.ErrorCode is not null && string.IsNullOrWhiteSpace(value.ErrorCode))
            {
                errors.Add("ErrorCode must be a non-empty string if provided.");
            }

            // Validate SourceIp (if present, must be valid format)
            if (value.SourceIp is not null)
            {
                if (string.IsNullOrWhiteSpace(value.SourceIp))
                {
                    errors.Add("SourceIp must be a non-empty string if provided.");
                }
                else if (!IsValidIpAddress(value.SourceIp))
                {
                    errors.Add($"SourceIp '{value.SourceIp}' is not a valid IP address.");
                }
            }

            // Validate UserAgent (if present, must be non-empty)
            if (value.UserAgent is not null && string.IsNullOrWhiteSpace(value.UserAgent))
            {
                errors.Add("UserAgent must be a non-empty string if provided.");
            }

            // Validate Tags
            if (value.Tags is null)
            {
                errors.Add("Tags dictionary must be initialized.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="UsageRecord"/> is valid.
        /// </summary>
        /// <param name="value">The usage record to check.</param>
        /// <returns>True if the record is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this UsageRecord value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="UsageRecord"/> is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The usage record to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the record is invalid, containing a list of problems.</exception>
        public static void EnsureValid(this UsageRecord value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"UsageRecord is invalid. Problems:\n  - {
                    string.Join("\n  - ", errors)
                }");
            }
        }

        /// <summary>
        /// Checks if a string is a valid HTTP method.
        /// </summary>
        private static bool IsValidHttpMethod(string method)
        {
            return method is "GET" or "POST" or "PUT" or "DELETE" or "PATCH" or "HEAD" or "OPTIONS";
        }

        /// <summary>
        /// Checks if a string is a valid IP address (IPv4 only for simplicity).
        /// </summary>
        private static bool IsValidIpAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                return false;
            }

            var parts = ip.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
            {
                return false;
            }

            foreach (var part in parts)
            {
                if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var num) || num < 0 || num > 255)
                {
                    return false;
                }
            }

            return true;
        }
    }
}