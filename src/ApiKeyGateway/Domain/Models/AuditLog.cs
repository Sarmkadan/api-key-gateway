// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Records security and administrative actions for compliance and debugging
/// </summary>
public class AuditLog
{
    /// <summary>Unique identifier for the audit log entry</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>ID of the resource being acted upon</summary>
    public string ResourceId { get; init; } = string.Empty;

    /// <summary>Type of resource (e.g., "ApiKey", "Consumer", "Configuration")</summary>
    public string ResourceType { get; init; } = string.Empty;

    /// <summary>Action that was performed</summary>
    public Enums.AuditAction Action { get; init; }

    /// <summary>User or system that performed the action</summary>
    public string PerformedBy { get; init; } = "system";

    /// <summary>When the action was performed (UTC)</summary>
    public DateTime PerformedAt { get; init; } = DateTime.UtcNow;

    /// <summary>HTTP status code if applicable</summary>
    public int? HttpStatusCode { get; set; }

    /// <summary>Source IP address of the request</summary>
    public string? SourceIp { get; set; }

    /// <summary>Reason or description of the action</summary>
    public string? Reason { get; set; }

    /// <summary>Collection of changes made in this action</summary>
    public Dictionary<string, object> Changes { get; set; } = [];

    /// <summary>Error message if the action failed</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Indicates if the action was successful</summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Gets a human-readable description of the action
    /// </summary>
    /// <returns>Description string for the audit action</returns>
    public string GetActionDescription() => Action switch
    {
        Enums.AuditAction.KeyCreated => "API key created",
        Enums.AuditAction.KeyUsed => "API key used",
        Enums.AuditAction.KeyDisabled => "API key disabled",
        Enums.AuditAction.KeyEnabled => "API key enabled",
        Enums.AuditAction.KeyRevoked => "API key revoked",
        Enums.AuditAction.RateLimitExceeded => "Rate limit exceeded",
        Enums.AuditAction.ConfigurationUpdated => "Configuration updated",
        Enums.AuditAction.UnauthorizedAttempt => "Unauthorized access attempt",
        _ => "Unknown action"
    };

    /// <summary>
    /// Records a change between old and new values
    /// </summary>
    /// <param name="fieldName">Name of the field that changed</param>
    /// <param name="oldValue">Previous value</param>
    /// <param name="newValue">New value</param>
    public void RecordChange(string fieldName, object? oldValue, object? newValue)
    {
        Changes[fieldName] = new
        {
            Old = oldValue,
            New = newValue,
            ChangedAt = DateTime.UtcNow
        };
    }
}
