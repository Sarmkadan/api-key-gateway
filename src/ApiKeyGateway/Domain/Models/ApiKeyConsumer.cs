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
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Organization { get; init; } = string.Empty;
    public string Tier { get; init; } = "free";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? InactiveSince { get; set; }
    public string? ContactPerson { get; set; }
    public string? Notes { get; set; }
    public int TotalApiKeys { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public string? WebhookUrl { get; set; }
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
}
