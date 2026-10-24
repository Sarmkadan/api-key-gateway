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
public record ApiKeyEvent
{
    /// <summary>
    /// Gets the unique identifier of this event
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the UTC time the event was raised
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the identifier of the API key the event relates to
    /// </summary>
    public string ApiKeyId { get; set; } = null!;
}

/// <summary>
/// Fired when a new API key is created.
/// </summary>
public record ApiKeyCreatedEvent : ApiKeyEvent
{
    /// <summary>
    /// Gets or sets the name of the created API key
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the identity of the user or service that created the key
    /// </summary>
    public string CreatedBy { get; set; } = null!;
}

/// <summary>
/// Fired when an API key is rotated (new key generated, old one invalidated).
/// </summary>
public record ApiKeyRotatedEvent : ApiKeyEvent
{
    /// <summary>
    /// Gets or sets the identity of the user or service that rotated the key
    /// </summary>
    public string RotatedBy { get; set; } = null!;
}

/// <summary>
/// Fired when an API key is disabled or revoked.
/// </summary>
public record ApiKeyDisabledEvent : ApiKeyEvent
{
    /// <summary>
    /// Gets or sets the identity of the user or service that disabled the key
    /// </summary>
    public string DisabledBy { get; set; } = null!;

    /// <summary>
    /// Gets or sets the reason the key was disabled
    /// </summary>
    public string Reason { get; set; } = null!;
}

/// <summary>
/// Fired when an API key's metadata (name, description) is updated.
/// </summary>
public record ApiKeyUpdatedEvent : ApiKeyEvent
{
    /// <summary>
    /// Gets or sets the fields that changed, keyed by field name
    /// </summary>
    public Dictionary<string, object> ChangedFields { get; set; } = null!;

    /// <summary>
    /// Gets or sets the identity of the user or service that updated the key
    /// </summary>
    public string UpdatedBy { get; set; } = null!;
}

/// <summary>
/// Fired when API key quota limits are modified.
/// </summary>
public record ApiKeyQuotaChangedEvent : ApiKeyEvent
{
    /// <summary>
    /// Gets or sets the quota limit before the change
    /// </summary>
    public int OldLimit { get; set; }

    /// <summary>
    /// Gets or sets the quota limit after the change
    /// </summary>
    public int NewLimit { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or service that changed the quota
    /// </summary>
    public string ChangedBy { get; set; } = null!;
}
