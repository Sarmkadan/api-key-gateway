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
    public string Id { get; init; } = string.Empty;
    public string ApiKeyId { get; init; } = string.Empty;
    public int RequestsPerUnit { get; init; }
    public Enums.RateLimitUnit Unit { get; init; } = Enums.RateLimitUnit.Hour;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? LastResetAt { get; set; }
    public int CurrentRequestCount { get; set; }
    public bool IsViolated => CurrentRequestCount >= RequestsPerUnit && IsEnabled;
    public int RemainingRequests => Math.Max(0, RequestsPerUnit - CurrentRequestCount);

    /// <summary>
    /// Gets the window duration in seconds based on the unit
    /// </summary>
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
