// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Events;

/// <summary>
/// Base class for all API key related events in the system.
/// Events are fired when important state changes occur (key created, rotated, disabled).
/// This allows different parts of the application to react to these changes
/// without tight coupling - useful for audit logging, webhooks, metrics, etc.
/// </summary>
public abstract record ApiKeyEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string ApiKeyId { get; set; } = null!;
}

/// <summary>
/// Fired when a new API key is created.
/// </summary>
public record ApiKeyCreatedEvent : ApiKeyEvent
{
    public string Name { get; set; } = null!;
    public string CreatedBy { get; set; } = null!;
}

/// <summary>
/// Fired when an API key is rotated (new key generated, old one invalidated).
/// </summary>
public record ApiKeyRotatedEvent : ApiKeyEvent
{
    public string RotatedBy { get; set; } = null!;
}

/// <summary>
/// Fired when an API key is disabled or revoked.
/// </summary>
public record ApiKeyDisabledEvent : ApiKeyEvent
{
    public string DisabledBy { get; set; } = null!;
    public string Reason { get; set; } = null!;
}

/// <summary>
/// Fired when an API key's metadata (name, description) is updated.
/// </summary>
public record ApiKeyUpdatedEvent : ApiKeyEvent
{
    public Dictionary<string, object> ChangedFields { get; set; } = null!;
    public string UpdatedBy { get; set; } = null!;
}

/// <summary>
/// Fired when API key quota limits are modified.
/// </summary>
public record ApiKeyQuotaChangedEvent : ApiKeyEvent
{
    public int OldLimit { get; set; }
    public int NewLimit { get; set; }
    public string ChangedBy { get; set; } = null!;
}
