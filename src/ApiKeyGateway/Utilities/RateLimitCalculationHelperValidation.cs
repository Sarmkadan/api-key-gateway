using System;
using System.Collections.Generic;
using System.Globalization;

namespace ApiKeyGateway.Utilities
{
    /// <summary>
    /// Provides validation helpers for <see cref="RateLimitCalculationHelper"/> operations.
    /// Validates the results of rate limit calculation methods.
    /// </summary>
    public static class RateLimitCalculationHelperValidation
    {
        /// <summary>
        /// Validates the results of calling <see cref="RateLimitCalculationHelper"/> methods with typical values.
        /// Returns any validation problems found in the calculated values.
        /// </summary>
        /// <returns>A list of validation errors; empty if valid.</returns>
        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();

            // Validate GetWindowStart with a typical DateTime
            var now = DateTime.UtcNow;
            var windowStart = RateLimitCalculationHelper.GetWindowStart(now, Domain.Enums.RateLimitUnit.Second);
            if (windowStart == default)
            {
                errors.Add("GetWindowStart returned default DateTime value.");
            }
            else if (windowStart > now)
            {
                errors.Add("GetWindowStart returned a value that is after the current time.");
            }

            // Validate GetWindowEnd with a typical DateTime
            var windowEnd = RateLimitCalculationHelper.GetWindowEnd(now, Domain.Enums.RateLimitUnit.Second);
            if (windowEnd == default)
            {
                errors.Add("GetWindowEnd returned default DateTime value.");
            }
            else if (windowEnd <= now)
            {
                errors.Add("GetWindowEnd returned a value that is not after the current time.");
            }
            else if (windowEnd <= windowStart)
            {
                errors.Add("GetWindowEnd returned a value that is not after GetWindowStart.");
            }

            // Validate GetSecondsUntilAllowed with typical parameters
            var secondsUntilAllowed = RateLimitCalculationHelper.GetSecondsUntilAllowed(0, 100, now, Domain.Enums.RateLimitUnit.Second);
            if (secondsUntilAllowed < 0)
            {
                errors.Add("GetSecondsUntilAllowed returned a negative value.");
            }

            // Validate CalculateQuotagePercentage with typical parameters
            var quotaPercentage = RateLimitCalculationHelper.CalculateQuotagePercentage(50, 100);
            if (quotaPercentage < 0 || quotaPercentage > 100)
            {
                errors.Add("CalculateQuotagePercentage returned a value outside the range [0, 100].");
            }

            // Validate ShouldWarnAboutLimit with typical parameters
            var shouldWarn = RateLimitCalculationHelper.ShouldWarnAboutLimit(85);
            // Boolean method - no validation needed beyond checking it returns a valid bool

            // Validate GetReadableResetTime with typical parameters
            var readableResetTime = RateLimitCalculationHelper.GetReadableResetTime(windowEnd);
            if (string.IsNullOrWhiteSpace(readableResetTime))
            {
                errors.Add("GetReadableResetTime returned null or whitespace.");
            }
            else if (readableResetTime == "0 seconds" && windowEnd > now)
            {
                errors.Add("GetReadableResetTime returned '0 seconds' when windowEnd is in the future.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the <see cref="RateLimitCalculationHelper"/> methods return valid results.
        /// </summary>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool IsValid()
        {
            return Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the <see cref="RateLimitCalculationHelper"/> methods return valid results.
        /// Throws an exception if validation fails.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if validation fails with a list of validation errors.</exception>
        public static void EnsureValid()
        {
            var errors = Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"RateLimitCalculationHelper validation failed. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
        }
    }
}
