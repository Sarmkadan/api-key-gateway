using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ApiKeyGateway.Controllers
{
    /// <summary>
    /// Validation helpers for the <see cref="StatsController"/> class.
    /// </summary>
    public static class StatsControllerValidation
    {
        /// <summary>
        /// Validates the specified <see cref="StatsController"/> instance.
        /// </summary>
        /// <param name="value">The <see cref="StatsController"/> instance to validate.</param>
        /// <returns>A list of human-readable problems with the instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this StatsController? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // StatsController has no properties to validate

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="StatsController"/> instance is valid.
        /// </summary>
        /// <param name="value">The <see cref="StatsController"/> instance to validate.</param>
        /// <returns>True if the instance is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this StatsController? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures the specified <see cref="StatsController"/> instance is valid.
        /// </summary>
        /// <param name="value">The <see cref="StatsController"/> instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static void EnsureValid(this StatsController? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);

            if (problems.Count > 0)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, problems), nameof(value));
            }
        }
    }
}
