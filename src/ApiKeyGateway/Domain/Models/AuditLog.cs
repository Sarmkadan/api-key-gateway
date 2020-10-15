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
    public string Id { get; init; } = string.Empty;
    public string ResourceId { get; init; } = string.Empty;
    public string ResourceType { get; init; } = string.Empty;
    public Enums.AuditAction Action { get; init; }
    public string PerformedBy { get; init; } = "system";
    public DateTime PerformedAt { get; init; } = DateTime.UtcNow;
    public int? HttpStatusCode { get; set; }
    public string? SourceIp { get; set; }
    public string? Reason { get; set; }
    public Dictionary<string, object> Changes { get; set; } = [];
    public string? ErrorMessage { get; set; }
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Gets a human-readable description of the action
    /// </summary>
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
