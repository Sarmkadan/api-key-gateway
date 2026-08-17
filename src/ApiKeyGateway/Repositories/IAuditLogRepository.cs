using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Repository interface for audit log data access
/// </summary>
public interface IAuditLogRepository
{
    Task CreateAsync(AuditLog log);
    Task<List<AuditLog>> GetByResourceIdAsync(string resourceId, int limit = 100);
    Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> DeleteOlderThanAsync(DateTime cutoffDate);
    Task<List<AuditLog>> SearchAsync(AuditAction action, DateTime fromUtc, DateTime toUtc, int limit = 100);
    Task<string> ExportByResourceIdToXmlAsync(string resourceId, int limit = 100);
    Task<string> ExportByDateRangeToXmlAsync(DateTime startDate, DateTime endDate, int limit = 100);
}
