// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="RateLimit"/> to enhance rate limiting capabilities.
/// </summary>
public static class RateLimitExtensions
{
    /// <summary>
    /// Determines if the rate limit has been violated based on the current request count and settings.
    /// </summary>
    /// <param name="rateLimit">The rate limit instance to check.</param>
    /// <returns>True if the rate limit has been violated; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rateLimit"/> is <see langword="null"/>.</exception>
    public static bool IsViolated(this RateLimit rateLimit)
    {
        ArgumentNullException.ThrowIfNull(rateLimit);

        return !QuotaLimit.IsUnlimited(rateLimit.RequestsPerUnit)
            && rateLimit.CurrentRequestCount >= rateLimit.RequestsPerUnit
            && rateLimit.IsEnabled;
    }

    /// <summary>
    /// Calculates the number of remaining requests within the current window.
    /// Returns <see cref="int.MaxValue"/> when the limit is the <see cref="QuotaLimit.Unlimited"/> sentinel.
    /// </summary>
    /// <param name="rateLimit">The rate limit instance to check.</param>
    /// <returns>The number of remaining requests.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rateLimit"/> is <see langword="null"/>.</exception>
    public static int GetRemainingRequests(this RateLimit rateLimit)
    {
        ArgumentNullException.ThrowIfNull(rateLimit);

        return QuotaLimit.IsUnlimited(rateLimit.RequestsPerUnit)
            ? int.MaxValue
            : Math.Max(0, rateLimit.RequestsPerUnit - rateLimit.CurrentRequestCount);
    }

    /// <summary>
    /// Checks if a request should be allowed based on the rate limit settings.
    /// </summary>
    /// <param name="rateLimit">The rate limit instance to check.</param>
    /// <returns>True if the request should be allowed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rateLimit"/> is <see langword="null"/>.</exception>
    public static bool ShouldAllowRequest(this RateLimit rateLimit)
    {
        ArgumentNullException.ThrowIfNull(rateLimit);

        return rateLimit.CanProcessRequest();
    }
}
