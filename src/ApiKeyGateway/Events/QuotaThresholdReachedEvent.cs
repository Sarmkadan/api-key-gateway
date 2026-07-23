// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Events;

/// <summary>
/// Published when a consumer's usage in the current quota period crosses a
/// configured alert threshold (for example 80% or 100% of the quota limit).
/// Fired at most once per consumer, per threshold, per quota period.
/// </summary>
public record QuotaThresholdReachedEvent
{
    /// <summary>Unique identifier of this event instance.</summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>UTC time the event was created.</summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>Identifier of the consumer whose usage crossed the threshold.</summary>
    public string ConsumerId { get; init; } = string.Empty;

    /// <summary>Identifier of the API key whose quota was evaluated.</summary>
    public string ApiKeyId { get; init; } = string.Empty;

    /// <summary>The threshold percentage that was reached (e.g. 80 or 100).</summary>
    public int ThresholdPercentage { get; init; }

    /// <summary>Number of requests counted in the current quota period.</summary>
    public long CurrentUsage { get; init; }

    /// <summary>Total quota limit for the period.</summary>
    public long QuotaLimit { get; init; }

    /// <summary>Actual usage as a percentage of the quota limit.</summary>
    public double PercentageUsed { get; init; }

    /// <summary>UTC start of the quota period the usage was measured in.</summary>
    public DateTime PeriodStart { get; init; }

    /// <summary>UTC end of the quota period (when the counter resets).</summary>
    public DateTime PeriodEnd { get; init; }
}
