// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Events;

/// <summary>
/// Base class for usage tracking events.
/// Fired whenever an API key is used to make a request,
/// allowing the system to track consumption metrics and enforce quotas.
/// </summary>
public abstract record UsageEvent
{
    /// <summary>Unique identifier of the event.</summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>UTC time when the event was created.</summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>Identifier of the API key that was used.</summary>
    public string ApiKeyId { get; set; } = null!;

    /// <summary>Endpoint that was requested.</summary>
    public string Endpoint { get; set; } = null!;

    /// <summary>HTTP status code returned for the request.</summary>
    public int HttpStatusCode { get; set; }
}

/// <summary>
/// Fired when an API key successfully makes a request.
/// </summary>
public record ApiKeyUsedEvent : UsageEvent
{
    /// <summary>Time taken to process the request, in milliseconds.</summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>Size of the response body, in bytes.</summary>
    public long ResponseSizeBytes { get; set; }
}

/// <summary>
/// Fired when a request fails due to rate limiting.
/// </summary>
public record RateLimitExceededEvent : UsageEvent
{
    /// <summary>Usage count in the current window at the time of the event.</summary>
    public int CurrentUsage { get; set; }

    /// <summary>Maximum number of requests allowed in the window.</summary>
    public int Limit { get; set; }

    /// <summary>Seconds until the rate limit window resets.</summary>
    public int SecondsUntilReset { get; set; }
}

/// <summary>
/// Fired when usage exceeds a warning threshold (usually 80% of quota).
/// </summary>
public record UsageWarningEvent : UsageEvent
{
    /// <summary>Current usage count.</summary>
    public int CurrentUsage { get; set; }

    /// <summary>Total quota limit.</summary>
    public int Limit { get; set; }

    /// <summary>Percentage of the quota that has been used.</summary>
    public int PercentageUsed { get; set; }
}

/// <summary>
/// Fired when quota is completely exhausted.
/// </summary>
public record QuotaExhaustedEvent : UsageEvent
{
    /// <summary>Total quota limit.</summary>
    public int Limit { get; set; }

    /// <summary>UTC time when the quota window resets.</summary>
    public DateTime WindowResetTime { get; set; }
}
