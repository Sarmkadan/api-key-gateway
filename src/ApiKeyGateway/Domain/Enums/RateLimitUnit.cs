// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Enums;

/// <summary>
/// Defines the time window for rate limit calculations
/// </summary>
public enum RateLimitUnit
{
    /// <summary>
    /// Rate limit is per second
    /// </summary>
    Second = 1,

    /// <summary>
    /// Rate limit is per minute
    /// </summary>
    Minute = 2,

    /// <summary>
    /// Rate limit is per hour
    /// </summary>
    Hour = 3,

    /// <summary>
    /// Rate limit is per day
    /// </summary>
    Day = 4,

    /// <summary>
    /// No rate limiting - unlimited requests
    /// </summary>
    Unlimited = 5
}
