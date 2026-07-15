// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Defines rate limiting configuration for an API key
/// </summary>
public class RateLimit
{
    /// <summary>Unique identifier for the rate limit configuration</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>ID of the API key this rate limit applies to</summary>
    public string ApiKeyId { get; init; } = string.Empty;

    /// <summary>Maximum number of requests allowed per time unit</summary>
    public int RequestsPerUnit { get; init; }

    /// <summary>Time window unit for rate limiting</summary>
    public Enums.RateLimitUnit Unit { get; init; } = Enums.RateLimitUnit.Hour;

    /// <summary>Whether rate limiting is enabled for this key</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>When the rate limit was created</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Last time the rate limit counter was reset</summary>
    public DateTime? LastResetAt { get; set; }

    /// <summary>Current count of requests in the current time window</summary>
    public int CurrentRequestCount { get; set; }

    /// <summary>Indicates if the rate limit has been violated</summary>
    public bool IsViolated => CurrentRequestCount >= RequestsPerUnit && IsEnabled;

    /// <summary>Number of remaining requests before rate limit is hit</summary>
    public int RemainingRequests => Math.Max(0, RequestsPerUnit - CurrentRequestCount);

    /// <summary>
    /// Gets the window duration in seconds based on the unit
    /// </summary>
    /// <returns>Duration in seconds for the rate limit window</returns>
    public int GetWindowInSeconds() => Unit switch
    {
        Enums.RateLimitUnit.Second => 1,
        Enums.RateLimitUnit.Minute => 60,
        Enums.RateLimitUnit.Hour => 3600,
        Enums.RateLimitUnit.Day => 86400,
        Enums.RateLimitUnit.Unlimited => int.MaxValue,
        _ => 3600
    };

    /// <summary>
    /// Checks if the current request should be allowed based on rate limits
    /// </summary>
    /// <returns>True if the request can be processed; otherwise, false</returns>
    public bool CanProcessRequest()
    {
        if (!IsEnabled || Unit == Enums.RateLimitUnit.Unlimited)
            return true;

        return CurrentRequestCount < RequestsPerUnit;
    }

    /// <summary>
    /// Records a request against the rate limit
    /// </summary>
    public void RecordRequest()
    {
        if (IsEnabled && Unit != Enums.RateLimitUnit.Unlimited)
        {
            CurrentRequestCount++;
        }
    }

    /// <summary>
    /// Resets the request counter for a new time window
    /// </summary>
    public void ResetWindow()
    {
        CurrentRequestCount = 0;
        LastResetAt = DateTime.UtcNow;
    }
}
