// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Enums;

namespace ApiKeyGateway.Extensions;

/// <summary>
/// Extension methods for <see cref="RateLimitUnit"/>.
/// Provides display formatting for rate limit time windows.
/// </summary>
public static class RateLimitUnitExtensions
{
    /// <summary>
    /// Returns a human-readable name for the rate limit unit, suitable for display in dashboards and logs.
    /// </summary>
    /// <param name="unit">The rate limit unit to format.</param>
    /// <returns>The display string for the unit, e.g. "Minute" or "Unlimited".</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="unit"/> is not a defined value.</exception>
    public static string ToDisplayString(this RateLimitUnit unit) => unit switch
    {
        RateLimitUnit.Second => "Second",
        RateLimitUnit.Minute => "Minute",
        RateLimitUnit.Hour => "Hour",
        RateLimitUnit.Day => "Day",
        RateLimitUnit.Month => "Month",
        RateLimitUnit.Unlimited => "Unlimited",
        _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Undefined RateLimitUnit value.")
    };
}
