// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Extension methods for <see cref="AuditLog"/> to enhance auditing capabilities.
/// </summary>
public static class AuditLogExtensions
{
    /// <summary>
    /// Generates a human-readable summary of the audit log entry.
    /// </summary>
    /// <param name="log">The audit log entry.</param>
    /// <returns>A formatted summary string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is null.</exception>
    public static string ToSummaryString(this AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        var actionDesc = log.GetActionDescription();
        var httpStatusText = log.HttpStatusCode.HasValue
            ? $"(HTTP {log.HttpStatusCode.Value})"
            : string.Empty;

        var outcome = log.IsSuccess
            ? "Success"
            : log.ErrorMessage is not null
                ? $"Failed with error: {log.ErrorMessage}"
                : "Failed";

        return $"{log.PerformedAt:yyyy-MM-dd HH:mm:ss}: {actionDesc} on {log.ResourceType} {log.ResourceId} by {log.PerformedBy} - {outcome} {httpStatusText}";
    }

    /// <summary>
    /// Determines if the audit log entry represents a security-sensitive action.
    /// </summary>
    /// <param name="log">The audit log entry.</param>
    /// <returns>True if the action is security-related; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is null.</exception>
    public static bool IsSecurityRelevant(this AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        return log.Action switch
        {
            Enums.AuditAction.UnauthorizedAttempt => true,
            Enums.AuditAction.KeyRevoked => true,
            Enums.AuditAction.KeyDisabled => true,
            Enums.AuditAction.RateLimitExceeded => true,
            _ => false
        };
    }

    /// <summary>
    /// Retrieves the most recently recorded change from the audit log.
    /// </summary>
    /// <param name="log">The audit log entry.</param>
    /// <returns>An immutable dictionary of the most recent change, or empty if none.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is null.</exception>
    public static IReadOnlyDictionary<string, object> GetMostRecentChange(this AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        if (log.Changes.Count == 0)
        {
            return new Dictionary<string, object>();
        }

        var changeEntries = log.Changes
            .Select(kvp => new
            {
                Key = kvp.Key,
                Value = kvp.Value as dynamic
            })
            .Where(x => x.Value?.ChangedAt != null)
            .OrderByDescending(x => x.Value.ChangedAt)
            .FirstOrDefault();

        return changeEntries is null
            ? new Dictionary<string, object>()
            : new Dictionary<string, object>
            {
                [changeEntries.Key] = new
                {
                    Old = changeEntries.Value.Old,
                    New = changeEntries.Value.New,
                    ChangedAt = changeEntries.Value.ChangedAt
                }
            };
    }
}
