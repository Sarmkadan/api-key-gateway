// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Manages audit logging for compliance and security monitoring
// =============================================================================

using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Utilities;
using System.Collections.Concurrent;

namespace ApiKeyGateway.Services;

/// <summary>
/// Manages audit logging for compliance and security monitoring
/// </summary>
public interface IAuditLogService : IDisposable
{
    /// <summary>
    /// Logs an audit event
    /// </summary>
    /// <param name="log">The audit log entry to record.</param>
    Task LogAsync(AuditLog log);

    /// <summary>
    /// Retrieves audit logs for a specific resource
    /// </summary>
    /// <param name="resourceId">ID of the resource to get logs for.</param>
    /// <param name="limit">Maximum number of logs to return.</param>
    /// <returns>List of audit logs for the resource.</returns>
    Task<List<AuditLog>> GetLogsAsync(string resourceId, int limit = 100);

    /// <summary>
    /// Retrieves audit logs for a time period
    /// </summary>
    /// <param name="startDate">Start of the time period.</param>
    /// <param name="endDate">End of the time period.</param>
    /// <returns>List of audit logs within the time period.</returns>
    Task<List<AuditLog>> GetLogsForPeriodAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Retrieves audit logs for a specific resource and exports them as XML
    /// </summary>
    /// <param name="resourceId">ID of the resource to get logs for.</param>
    /// <param name="limit">Maximum number of logs to return.</param>
    /// <returns>XML representation of audit logs for the resource.</returns>
    Task<string> ExportLogsToXmlAsync(string resourceId, int limit = 100);

    /// <summary>
    /// Retrieves audit logs for a time period and exports them as XML
    /// </summary>
    /// <param name="startDate">Start of the time period.</param>
    /// <param name="endDate">End of the time period.</param>
    /// <returns>XML representation of audit logs within the time period.</returns>
    Task<string> ExportLogsForPeriodToXmlAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Removes old audit logs based on retention policy
    /// </summary>
    /// <param name="retentionDays">Number of days to retain logs.</param>
    Task CleanupOldLogsAsync(int retentionDays);
}

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<AuditLogService> _logger;
    private readonly ConcurrentQueue<AuditLog> _logQueue = new ConcurrentQueue<AuditLog>();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly Task _flushTask;
    private readonly TimeSpan _flushInterval = TimeSpan.FromSeconds(5);
    private readonly int _batchSize = 50;
    private bool _disposed;

    public AuditLogService(IAuditLogRepository repository, ILogger<AuditLogService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Start background flusher
        _flushTask = Task.Run(FlushLogsAsync);
    }

    /// <summary>
    /// Logs an audit event. Persistence failures are logged and swallowed so a
    /// broken audit store never takes down the request flow being audited.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is null.</exception>
    public async Task LogAsync(AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        // Add to queue instead of writing immediately
        _logQueue.Enqueue(log);
    }

    /// <summary>
    /// Background task that periodically flushes logs from the queue to persistent storage
    /// </summary>
    private async Task FlushLogsAsync()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_flushInterval, _cancellationTokenSource.Token);
                await FlushBatchAsync();
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during audit log batch flush");
                await Task.Delay(TimeSpan.FromSeconds(1), _cancellationTokenSource.Token); // Backoff on error
            }
        }
    }

    /// <summary>
    /// Flushes a batch of logs from the queue to persistent storage
    /// </summary>
    private async Task FlushBatchAsync()
    {
        var batch = new List<AuditLog>(_batchSize);

        // Dequeue up to batch size
        while (batch.Count < _batchSize && _logQueue.TryDequeue(out var log))
        {
            batch.Add(log);
        }

        if (batch.Count == 0)
        {
            return; // No logs to flush
        }

        try
        {
            // Write batch to repository
            foreach (var log in batch)
            {
                await _repository.CreateAsync(log);
            }

            _logger.LogDebug("Flushed {Count} audit logs to persistent storage", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flush {Count} audit logs to persistent storage", batch.Count);

            // If batch write fails, requeue the logs for retry
            foreach (var log in batch)
            {
                _logQueue.Enqueue(log);
            }
        }
    }

    /// <summary>
    /// Forces an immediate flush of all queued logs
    /// </summary>
    public async Task FlushAsync()
    {
        await FlushBatchAsync();
    }

    /// <summary>
    /// Retrieves audit logs for a specific resource. An empty resource ID cannot
    /// match any log, so it returns an empty list rather than throwing.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="limit"/> is not positive.</exception>
    public async Task<List<AuditLog>> GetLogsAsync(string resourceId, int limit = 100)
    {
        if (limit <= 0)
            throw new ArgumentException("Limit must be positive", nameof(limit));

        if (string.IsNullOrWhiteSpace(resourceId))
            return [];

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
    /// Retrieves audit logs for a specific resource and exports them as XML
    /// </summary>
    /// <param name="resourceId">ID of the resource to get logs for.</param>
    /// <param name="limit">Maximum number of logs to return.</param>
    /// <returns>XML representation of audit logs for the resource.</returns>
    public async Task<string> ExportLogsToXmlAsync(string resourceId, int limit = 100)
    {
        return await _repository.ExportByResourceIdToXmlAsync(resourceId, limit);
    }

    /// <summary>
    /// Retrieves audit logs for a time period and exports them as XML
    /// </summary>
    /// <param name="startDate">Start of the time period.</param>
    /// <param name="endDate">End of the time period.</param>
    /// <returns>XML representation of audit logs within the time period.</returns>
    public async Task<string> ExportLogsForPeriodToXmlAsync(DateTime startDate, DateTime endDate)
    {
        return await _repository.ExportByDateRangeToXmlAsync(startDate, endDate);
    }

    /// <summary>
    /// Removes old audit logs based on retention policy. Cleanup is best-effort:
    /// repository failures are logged and swallowed so the retention worker keeps
    /// running and retries on its next cycle.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="retentionDays"/> is not positive.</exception>
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
        }
    }

    /// <summary>
    /// Disposes the service, flushing any remaining logs and stopping the background flusher
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Signal cancellation and wait for flusher to complete
            _cancellationTokenSource.Cancel();

            try
            {
                // Wait for current flush to complete
                _flushTask?.Wait(TimeSpan.FromSeconds(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while waiting for audit log flusher to complete");
            }

            // Flush any remaining logs
            _flushTask?.Dispose();
            _cancellationTokenSource.Dispose();

            // Force final flush of any remaining logs
            try
            {
                FlushAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during final audit log flush on disposal");
            }
        }

        _disposed = true;
    }
}
