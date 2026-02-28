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
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string ApiKeyId { get; set; } = null!;
    public string Endpoint { get; set; } = null!;
    public int HttpStatusCode { get; set; }
}

/// <summary>
/// Fired when an API key successfully makes a request.
/// </summary>
public record ApiKeyUsedEvent : UsageEvent
{
    public long ResponseTimeMs { get; set; }
    public long ResponseSizeBytes { get; set; }
}

/// <summary>
/// Fired when a request fails due to rate limiting.
/// </summary>
public record RateLimitExceededEvent : UsageEvent
{
    public int CurrentUsage { get; set; }
    public int Limit { get; set; }
    public int SecondsUntilReset { get; set; }
}

/// <summary>
/// Fired when usage exceeds a warning threshold (usually 80% of quota).
/// </summary>
public record UsageWarningEvent : UsageEvent
{
    public int CurrentUsage { get; set; }
    public int Limit { get; set; }
    public int PercentageUsed { get; set; }
}

/// <summary>
/// Fired when quota is completely exhausted.
/// </summary>
public record QuotaExhaustedEvent : UsageEvent
{
    public int Limit { get; set; }
    public DateTime WindowResetTime { get; set; }
}
