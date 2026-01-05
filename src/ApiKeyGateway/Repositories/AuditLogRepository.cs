// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Repository implementation for audit log data persistence
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<AuditLogRepository> _logger;

    public AuditLogRepository(IDbConnection connection, ILogger<AuditLogRepository> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new audit log entry
    /// </summary>
    public async Task CreateAsync(AuditLog log)
    {
        if (log == null)
            throw new ArgumentNullException(nameof(log));

        try
        {
            const string query = @"
                INSERT INTO AuditLogs
                (Id, ResourceId, ResourceType, Action, PerformedBy, PerformedAt, HttpStatusCode, SourceIp, Reason, IsSuccess)
                VALUES (@Id, @ResourceId, @ResourceType, @Action, @PerformedBy, @PerformedAt, @HttpStatusCode, @SourceIp, @Reason, @IsSuccess)";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            AddParameters(cmd, log);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();

            _logger.LogDebug("Audit log created: {ResourceType} {ResourceId} - {Action}",
                log.ResourceType, log.ResourceId, log.GetActionDescription());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log");
            throw new DataAccessException("Failed to create audit log", "CREATE", "AuditLog");
        }
    }

    /// <summary>
    /// Retrieves audit logs for a specific resource
    /// </summary>
    public async Task<List<AuditLog>> GetByResourceIdAsync(string resourceId, int limit = 100)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
            return [];

        try
        {
            const string query = @"
                SELECT TOP (@Limit) * FROM AuditLogs
                WHERE ResourceId = @ResourceId
                ORDER BY PerformedAt DESC";

            var logs = new List<AuditLog>();

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@ResourceId", resourceId));
            cmd.Parameters.Add(CreateParameter("@Limit", limit));

            await _connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                logs.Add(MapFromReader(reader));
            }

            await _connection.CloseAsync();
            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs for resource {ResourceId}", resourceId);
            return [];
        }
    }

    /// <summary>
    /// Retrieves audit logs for a time period
    /// </summary>
    public async Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            const string query = @"
                SELECT * FROM AuditLogs
                WHERE PerformedAt >= @StartDate AND PerformedAt <= @EndDate
                ORDER BY PerformedAt DESC";

            var logs = new List<AuditLog>();

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@StartDate", startDate));
            cmd.Parameters.Add(CreateParameter("@EndDate", endDate));

            await _connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                logs.Add(MapFromReader(reader));
            }

            await _connection.CloseAsync();
            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs for date range");
            return [];
        }
    }

    /// <summary>
    /// Deletes audit logs older than the cutoff date
    /// </summary>
    public async Task<int> DeleteOlderThanAsync(DateTime cutoffDate)
    {
        try
        {
            const string query = "DELETE FROM AuditLogs WHERE PerformedAt < @CutoffDate";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@CutoffDate", cutoffDate));

            await _connection.OpenAsync();
            var deletedCount = await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();

            _logger.LogInformation("Deleted {Count} audit logs older than {CutoffDate}", deletedCount, cutoffDate);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete old audit logs");
            return 0;
        }
    }

    private AuditLog MapFromReader(DbDataReader reader) => new AuditLog
    {
        Id = reader["Id"].ToString() ?? string.Empty,
        ResourceId = reader["ResourceId"].ToString() ?? string.Empty,
        ResourceType = reader["ResourceType"].ToString() ?? string.Empty,
        Action = (Domain.Enums.AuditAction)(reader["Action"] as int? ?? 1),
        PerformedBy = reader["PerformedBy"].ToString() ?? "system",
        PerformedAt = reader["PerformedAt"] as DateTime? ?? DateTime.UtcNow,
        HttpStatusCode = reader["HttpStatusCode"] as int?,
        SourceIp = reader["SourceIp"]?.ToString(),
        Reason = reader["Reason"]?.ToString(),
        IsSuccess = (reader["IsSuccess"] as bool?) ?? true
    };

    private void AddParameters(DbCommand cmd, AuditLog log)
    {
        cmd.Parameters.Add(CreateParameter("@Id", log.Id));
        cmd.Parameters.Add(CreateParameter("@ResourceId", log.ResourceId));
        cmd.Parameters.Add(CreateParameter("@ResourceType", log.ResourceType));
        cmd.Parameters.Add(CreateParameter("@Action", (int)log.Action));
        cmd.Parameters.Add(CreateParameter("@PerformedBy", log.PerformedBy));
        cmd.Parameters.Add(CreateParameter("@PerformedAt", log.PerformedAt));
        cmd.Parameters.Add(CreateParameter("@HttpStatusCode", (object?)log.HttpStatusCode ?? DBNull.Value));
        cmd.Parameters.Add(CreateParameter("@SourceIp", (object?)log.SourceIp ?? DBNull.Value));
        cmd.Parameters.Add(CreateParameter("@Reason", (object?)log.Reason ?? DBNull.Value));
        cmd.Parameters.Add(CreateParameter("@IsSuccess", log.IsSuccess));
    }

    private DbParameter CreateParameter(string name, object? value)
    {
        var param = _connection.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        return param;
    }
}
