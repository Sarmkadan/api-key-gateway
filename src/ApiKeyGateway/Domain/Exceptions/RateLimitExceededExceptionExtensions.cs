using System;
using System.Collections.Generic;
using System.Globalization;

namespace ApiKeyGateway.Domain.Exceptions
{
    /// <summary>
    /// Extension methods for <see cref="RateLimitExceededException"/>.
    /// </summary>
    public static class RateLimitExceededExceptionExtensions
    {
        /// <summary>
        /// Returns a user‑friendly message describing the rate‑limit violation.
        /// </summary>
        /// <param name="ex">The exception instance.</param>
        /// <returns>A formatted message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is <c>null</c>.</exception>
        public static string GetRateLimitExceededMessage(this RateLimitExceededException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);

            var retryAfter = ex.RetryAfter.HasValue
                ? ex.RetryAfter.Value.ToString("O", CultureInfo.InvariantCulture)
                : "unknown";

            return $"Rate limit exceeded for API key '{ex.ApiKeyId}'. Limit: {ex.Limit} per {ex.WindowInSeconds} seconds. Retry after: {retryAfter}.";
        }

        /// <summary>
        /// Indicates whether the retry‑after time has already elapsed.
        /// </summary>
        /// <param name="ex">The exception instance.</param>
        /// <returns><c>true</c> if <see cref="RateLimitExceededException.RetryAfter"/> is set and has passed; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is <c>null</c>.</exception>
        public static bool IsRetryAfterExpired(this RateLimitExceededException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);

            return ex.RetryAfter.HasValue && ex.RetryAfter.Value <= DateTime.UtcNow;
        }

        /// <summary>
        /// Builds a dictionary of HTTP header names and values that can be used to inform the client about the rate‑limit status.
        /// </summary>
        /// <param name="ex">The exception instance.</param>
        /// <returns>A read‑only dictionary of header names to values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is <c>null</c>.</exception>
        public static IReadOnlyDictionary<string, string> GetRateLimitHeaderValues(this RateLimitExceededException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);

            var headers = new Dictionary<string, string>
            {
                ["X-RateLimit-Limit"] = ex.Limit.ToString(CultureInfo.InvariantCulture),
                ["X-RateLimit-Window"] = ex.WindowInSeconds.ToString(CultureInfo.InvariantCulture)
            };

            if (ex.RetryAfter.HasValue)
            {
                headers["Retry-After"] = ex.RetryAfter.Value.ToString("O", CultureInfo.InvariantCulture);
            }

            return headers;
        }

        /// <summary>
        /// Provides a concise string that can be logged or displayed to indicate when the client may retry.
        /// </summary>
        /// <param name="ex">The exception instance.</param>
        /// <returns>A string describing the retry‑after period.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is <c>null</c>.</exception>
        public static string GetRetryAfterMessage(this RateLimitExceededException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);

            return ex.RetryAfter.HasValue
                ? $"Please retry after {ex.RetryAfter.Value.ToString("O", CultureInfo.InvariantCulture)}."
                : "Please retry after the rate limit window has passed.";
        }
    }
}
