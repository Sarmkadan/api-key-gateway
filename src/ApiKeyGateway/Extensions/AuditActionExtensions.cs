// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Enums;

namespace ApiKeyGateway.Extensions;

/// <summary>
/// Extension methods for <see cref="AuditAction"/>.
/// Provides display formatting and classification helpers for audit actions.
/// </summary>
public static class AuditActionExtensions
{
    /// <summary>
    /// Returns a human-readable description of the audit action, suitable for display in dashboards and logs.
    /// </summary>
    /// <param name="action">The audit action to format.</param>
    /// <returns>The display string for the audit action.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="action"/> is not a defined value.</exception>
    public static string ToDisplayString(this AuditAction action) => action switch
    {
        AuditAction.KeyCreated => "API key created",
        AuditAction.KeyUsed => "API key used",
        AuditAction.KeyDisabled => "API key disabled",
        AuditAction.KeyEnabled => "API key enabled",
        AuditAction.KeyRevoked => "API key revoked",
        AuditAction.RateLimitExceeded => "Rate limit exceeded",
        AuditAction.ConfigurationUpdated => "Configuration updated",
        AuditAction.UnauthorizedAttempt => "Unauthorized access attempt",
        AuditAction.StatsViewed => "Statistics viewed",
        AuditAction.UsageDataExported => "Usage data exported",
        AuditAction.ConfigurationViewed => "Configuration viewed",
        AuditAction.DiagnosticsRun => "Diagnostics run",
        AuditAction.RateLimitsReset => "Rate limits reset",
        AuditAction.AuditLogsSearched => "Audit logs searched",
        AuditAction.AuditLogsExportedByResource => "Audit logs exported by resource",
        AuditAction.AuditLogsExportedByPeriod => "Audit logs exported by period",
        AuditAction.RateLimitOverrideUpdated => "Rate limit override updated",
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Undefined AuditAction value.")
    };

    /// <summary>
    /// Determines whether the audit action removes or disables access, or discards accumulated state.
    /// <see cref="AuditAction.KeyDisabled"/>, <see cref="AuditAction.KeyRevoked"/> and
    /// <see cref="AuditAction.RateLimitsReset"/> are destructive. Read-only and creation actions are not.
    /// </summary>
    /// <param name="action">The audit action to check.</param>
    /// <returns><see langword="true"/> if the action is destructive; otherwise, <see langword="false"/>.</returns>
    public static bool IsDestructive(this AuditAction action) =>
        action is AuditAction.KeyDisabled or AuditAction.KeyRevoked or AuditAction.RateLimitsReset;
}
