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
    /// <param name="log">The audit log entry to format.</param>
    /// <returns>A formatted summary string containing timestamp, action description, resource information, performer, outcome, and HTTP status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The format is: <c>yyyy-MM-dd HH:mm:ss: [action] on [ResourceType] [ResourceId] by [PerformedBy] - [outcome] ([HTTP status])</c>
    /// where outcome is either "Success" or "Failed: [error message]".
    /// </remarks>
    public static string ToSummaryString(this AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        var actionDesc = log.GetActionDescription();
        var httpStatusText = log.HttpStatusCode.HasValue
            ? $" (HTTP {log.HttpStatusCode.Value})"
            : string.Empty;

        var outcome = log.IsSuccess
            ? "Success"
            : log.ErrorMessage is not null
                ? $"Failed: {log.ErrorMessage}"
                : "Failed";

        return $"{log.PerformedAt:yyyy-MM-dd HH:mm:ss}: {actionDesc} on {log.ResourceType} {log.ResourceId} by {log.PerformedBy} - {outcome}{httpStatusText}";
    }

    /// <summary>
    /// Determines whether the audit log entry represents a security-sensitive action.
    /// Security-relevant actions include unauthorized access attempts, key revocations,
    /// key disables, and rate limit violations.
    /// </summary>
    /// <param name="log">The audit log entry to evaluate.</param>
    /// <returns><see langword="true"/> if the action is security-related; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is <see langword="null"/>.</exception>
    public static bool IsSecurityRelevant(this AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        return log.Action is Enums.AuditAction.UnauthorizedAttempt
            or Enums.AuditAction.KeyRevoked
            or Enums.AuditAction.KeyDisabled
            or Enums.AuditAction.RateLimitExceeded;
    }

    /// <summary>
    /// Retrieves the most recently recorded change from the audit log.
    /// The change is determined by the <c>ChangedAt</c> timestamp in the change value.
    /// </summary>
    /// <param name="log">The audit log entry to process.</param>
    /// <returns>
    /// An immutable dictionary containing the most recent change with keys <c>Old</c>, <c>New</c>,
    /// and <c>ChangedAt</c>, or an empty dictionary if no valid changes exist.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method assumes that change values implement a structure with <c>ChangedAt</c>, <c>Old</c>, and <c>New</c> properties.
    /// Changes are ordered by <c>ChangedAt</c> in descending order to get the most recent.
    /// </remarks>
    public static IReadOnlyDictionary<string, object> GetMostRecentChange(this AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        if (log.Changes.Count == 0)
        {
            return new Dictionary<string, object>();
        }

        var changeEntries = log.Changes
            .Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value))
            .Where(x => x.Value is not null)
            .Select(x => new
            {
                Key = x.Key,
                Value = x.Value,
                ChangedAt = ((dynamic)x.Value).ChangedAt as DateTimeOffset?
            })
            .Where(x => x.ChangedAt != null)
            .OrderByDescending(x => x.ChangedAt)
            .FirstOrDefault();

        return changeEntries is null
            ? new Dictionary<string, object>()
            : new Dictionary<string, object>
            {
                [changeEntries.Key] = new
                {
                    Old = ((dynamic)changeEntries.Value).Old,
                    New = ((dynamic)changeEntries.Value).New,
                    ChangedAt = changeEntries.ChangedAt
                }
            };
    }
}
