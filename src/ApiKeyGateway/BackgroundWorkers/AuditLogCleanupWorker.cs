using System.Diagnostics;
using ApiKeyGateway.Repositories;

namespace ApiKeyGateway.BackgroundWorkers;

/// <summary>
/// Background worker that periodically cleans up old audit logs.
/// Audit logs are important for compliance, but keeping them forever
/// wastes storage. This worker archives or deletes logs older than
/// a configured retention period (e.g., 90 days).
/// </summary>
public sealed class AuditLogCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditLogCleanupWorker> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromDays(1);
    private readonly int _retentionDays = 90;
    private int _totalDeleted = 0;

    public AuditLogCleanupWorker(IServiceProvider serviceProvider, ILogger<AuditLogCleanupWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit log cleanup worker started");

        // Don't start cleanup immediately - wait a bit after startup
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var auditRepository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();

                var cutoffDate = DateTime.UtcNow.AddDays(-_retentionDays);

                _logger.LogInformation(
                    "Starting audit log cleanup for logs before {CutoffDate}",
                    cutoffDate);

                var stopwatch = Stopwatch.StartNew();
                var deletedCount = await auditRepository.DeleteOlderThanAsync(cutoffDate);
                stopwatch.Stop();

                _totalDeleted += deletedCount;

                _logger.LogInformation(
                    "Cleaned up {Count} audit log entries older than {RetentionDays} days",
                    deletedCount,
                    _retentionDays);

                _logger.LogInformation(
                    "Cleanup pass summary: deleted {DeletedThisPass} records, total {TotalDeleted} records, duration {DurationMs} ms",
                    deletedCount,
                    _totalDeleted,
                    stopwatch.ElapsedMilliseconds);

                // Wait for next cleanup cycle
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Audit log cleanup worker is shutting down");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during audit log cleanup");
                // Wait before retry
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
