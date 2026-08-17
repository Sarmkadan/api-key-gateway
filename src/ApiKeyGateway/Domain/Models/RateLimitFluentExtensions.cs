// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides fluent extension methods for <see cref="RateLimit"/>.
/// </summary>
public static class RateLimitFluentExtensions
{
    /// <summary>
    /// Checks if the rate limit will be exceeded if a specific number of requests are added.
    /// </summary>
    /// <param name="rateLimit">The rate limit.</param>
    /// <param name="count">Number of requests to add.</param>
    /// <returns>True if exceeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rateLimit"/> is <see langword="null"/>.</exception>
    public static bool IsExceededBy(this RateLimit rateLimit, int count)
    {
        ArgumentNullException.ThrowIfNull(rateLimit);
        
        return (rateLimit.CurrentRequestCount + count) > rateLimit.RequestsPerUnit;
    }

    /// <summary>
    /// Calculates the remaining capacity after subtracting a specific number of requests.
    /// </summary>
    /// <param name="rateLimit">The rate limit.</param>
    /// <param name="count">Number of requests to subtract.</param>
    /// <returns>The remaining capacity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rateLimit"/> is <see langword="null"/>.</exception>
    public static int RemainingCapacity(this RateLimit rateLimit, int count)
    {
        ArgumentNullException.ThrowIfNull(rateLimit);
        
        return Math.Max(0, rateLimit.RequestsPerUnit - rateLimit.CurrentRequestCount - count);
    }

    /// <summary>
    /// Calculates the end time of the current rate limit window based on the start time.
    /// </summary>
    /// <param name="rateLimit">The rate limit.</param>
    /// <param name="start">The start time of the window.</param>
    /// <returns>The end time of the window.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rateLimit"/> is <see langword="null"/>.</exception>
    public static DateTime WindowEnd(this RateLimit rateLimit, DateTime start)
    {
        ArgumentNullException.ThrowIfNull(rateLimit);
        
        return start.AddSeconds(rateLimit.GetWindowInSeconds());
    }

    /// <summary>
    /// Returns a display string for the rate limit.
    /// </summary>
    /// <param name="rateLimit">The rate limit.</param>
    /// <returns>A string representation of the rate limit.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rateLimit"/> is <see langword="null"/>.</exception>
    public static string ToDisplayString(this RateLimit rateLimit)
    {
        ArgumentNullException.ThrowIfNull(rateLimit);
        
        return $"{rateLimit.CurrentRequestCount}/{rateLimit.RequestsPerUnit} {rateLimit.Unit}";
    }
}
