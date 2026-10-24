// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Represents an API consumer - a user or service using the gateway
/// </summary>
public class ApiKeyConsumer
{
    /// <summary>
    /// Gets or sets the unique consumer identifier
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the consumer name
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the consumer email address
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the consumer's organization
    /// </summary>
    public string Organization { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the consumer tier (defaults to "free")
    /// </summary>
    public string Tier { get; init; } = "free";

    /// <summary>
    /// Gets or sets a value indicating whether the consumer is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the UTC time the consumer was created
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC time since which the consumer has been inactive
    /// </summary>
    public DateTime? InactiveSince { get; set; }

    /// <summary>
    /// Gets or sets the contact person for the consumer
    /// </summary>
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Gets or sets free-form notes about the consumer
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the number of API keys associated with the consumer
    /// </summary>
    public int TotalApiKeys { get; set; }

    /// <summary>
    /// Gets or sets the UTC time of the consumer's last activity
    /// </summary>
    public DateTime? LastActivityAt { get; set; }

    /// <summary>
    /// Gets or sets the webhook URL used for notifications
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Gets or sets arbitrary custom key/value properties
    /// </summary>
    public Dictionary<string, string> CustomProperties { get; set; } = [];

    /// <summary>
    /// Deactivates the consumer and all associated API keys
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        InactiveSince = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates a previously deactivated consumer
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        InactiveSince = null;
    }

    /// <summary>
    /// Updates the last activity timestamp
    /// </summary>
    public void UpdateLastActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates consumer information
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(Organization) &&
               Email.Contains('@');
    }

    /// <summary>
    /// Returns a string representation of the consumer's status fields
    /// </summary>
    /// <returns>A string describing the consumer's key properties</returns>
    public override string ToString() => $"ApiKeyConsumer {{ IsActive = {IsActive}, InactiveSince = {InactiveSince}, ContactPerson = {ContactPerson}, Notes = {Notes}, TotalApiKeys = {TotalApiKeys}, LastActivityAt = {LastActivityAt} }}";
}
