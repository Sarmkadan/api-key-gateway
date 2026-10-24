using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Repository interface for audit log data access
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Creates a new audit log entry
    /// </summary>
    /// <param name="log">The audit log entry to persist</param>
    Task CreateAsync(AuditLog log);

    /// <summary>
    /// Retrieves audit logs for a specific resource
    /// </summary>
    /// <param name="resourceId">The resource identifier to filter by</param>
    /// <param name="limit">The maximum number of entries to return</param>
    /// <returns>The matching audit log entries</returns>
    Task<List<AuditLog>> GetByResourceIdAsync(string resourceId, int limit = 100);

    /// <summary>
    /// Retrieves audit logs for a time period
    /// </summary>
    /// <param name="startDate">The start of the time period</param>
    /// <param name="endDate">The end of the time period</param>
    /// <returns>The audit log entries within the period</returns>
    Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Deletes audit logs older than the cutoff date
    /// </summary>
    /// <param name="cutoffDate">Entries performed before this date are deleted</param>
    /// <returns>The number of deleted entries</returns>
    Task<int> DeleteOlderThanAsync(DateTime cutoffDate);

    /// <summary>
    /// Searches audit logs by action and time range
    /// </summary>
    /// <param name="action">The audit action to filter by</param>
    /// <param name="fromUtc">The start of the UTC time range</param>
    /// <param name="toUtc">The end of the UTC time range</param>
    /// <param name="limit">The maximum number of entries to return</param>
    /// <returns>The matching audit log entries</returns>
    Task<List<AuditLog>> SearchAsync(AuditAction action, DateTime fromUtc, DateTime toUtc, int limit = 100);

    /// <summary>
    /// Exports audit logs for a specific resource as XML
    /// </summary>
    /// <param name="resourceId">The resource identifier to filter by</param>
    /// <param name="limit">The maximum number of entries to export</param>
    /// <returns>The audit logs serialized as XML</returns>
    Task<string> ExportByResourceIdToXmlAsync(string resourceId, int limit = 100);

    /// <summary>
    /// Exports audit logs for a time period as XML
    /// </summary>
    /// <param name="startDate">The start of the time period</param>
    /// <param name="endDate">The end of the time period</param>
    /// <param name="limit">The maximum number of entries to export</param>
    /// <returns>The audit logs serialized as XML</returns>
    Task<string> ExportByDateRangeToXmlAsync(DateTime startDate, DateTime endDate, int limit = 100);
}
