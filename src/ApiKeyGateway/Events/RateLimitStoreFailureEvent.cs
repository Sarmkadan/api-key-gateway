// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Configuration;

namespace ApiKeyGateway.Events;

/// <summary>
/// Fired when the rate limit store is unreachable and the gateway applies its
/// configured failure policy instead of a real limit check. Subscribers can
/// alert on these to detect a degraded (unenforced or rejecting) rate limiter.
/// </summary>
public record RateLimitStoreFailureEvent
{
    /// <summary>Unique identifier of this event instance.</summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>When the failure was observed (UTC).</summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>The API key whose request hit the failure.</summary>
    public required string ApiKeyId { get; init; }

    /// <summary>The limiter operation that failed (e.g. CheckLimit, RecordRequest).</summary>
    public required string Operation { get; init; }

    /// <summary>The failure policy that was applied.</summary>
    public required RateLimitFailurePolicy PolicyApplied { get; init; }

    /// <summary>Whether the circuit breaker is currently open (store being skipped).</summary>
    public bool CircuitOpen { get; init; }

    /// <summary>Message of the underlying data-access error, if any.</summary>
    public string? ErrorMessage { get; init; }
}
