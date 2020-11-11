// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Manages audit logging for compliance and security monitoring
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Services;

/// <summary>
/// Manages audit logging for compliance and security monitoring
/// </summary>
public interface IAuditLogService
{
    Task LogAsync(AuditLog log);
    Task<List<AuditLog>> GetLogsAsync(string resourceId, int limit = 100);
    Task<List<AuditLog>> GetLogsForPeriodAsync(DateTime startDate, DateTime endDate);
    Task CleanupOldLogsAsync(int retentionDays);
}

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(IAuditLogRepository repository, ILogger<AuditLogService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs an audit event
    /// </summary>
    public async Task LogAsync(AuditLog log)
    {
        if (log == null)
            throw new ArgumentNullException(nameof(log));

        try
        {
            await _repository.CreateAsync(log);
            _logger.LogInformation(
                "Audit log: {Action} on {ResourceType} {ResourceId} by {PerformedBy}",
                log.GetActionDescription(),
                log.ResourceType,
                log.ResourceId,
                log.PerformedBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log");
            throw new DataAccessException("Failed to write audit log", nameof(LogAsync), nameof(AuditLog), ex);
        }
    }

    /// <summary>
    /// Retrieves audit logs for a specific resource
    /// </summary>
    public async Task<List<AuditLog>> GetLogsAsync(string resourceId, int limit = 100)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
            throw new ArgumentException("Resource ID cannot be empty", nameof(resourceId));

        if (limit <= 0)
            throw new ArgumentException("Limit must be positive", nameof(limit));

        return await _repository.GetByResourceIdAsync(resourceId, limit);
    }

    /// <summary>
    /// Retrieves audit logs for a time period
    /// </summary>
    public async Task<List<AuditLog>> GetLogsForPeriodAsync(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must be after start date");

        return await _repository.GetByDateRangeAsync(startDate, endDate);
    }

    /// <summary>
    /// Removes old audit logs based on retention policy
    /// </summary>
    public async Task CleanupOldLogsAsync(int retentionDays)
    {
        if (retentionDays <= 0)
            throw new ArgumentException("Retention days must be positive", nameof(retentionDays));

        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var deletedCount = await _repository.DeleteOlderThanAsync(cutoffDate);
            _logger.LogInformation("Cleaned up {Count} audit logs older than {Date}", deletedCount, cutoffDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during audit log cleanup");
            throw new DataAccessException("Failed to cleanup audit logs", nameof(CleanupOldLogsAsync), nameof(AuditLog), ex);
        }
    }
}

/// <summary>
/// Repository interface for audit log data access
/// </summary>
public interface IAuditLogRepository
{
    Task CreateAsync(AuditLog log);
    Task<List<AuditLog>> GetByResourceIdAsync(string resourceId, int limit = 100);
    Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> DeleteOlderThanAsync(DateTime cutoffDate);
}
