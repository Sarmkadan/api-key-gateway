// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Data;
using System.Data.Common;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Repository implementation for usage tracking data persistence
/// </summary>
public class UsageRepository : IUsageRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<UsageRepository> _logger;

    public UsageRepository(IDbConnection connection, ILogger<UsageRepository> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Records a new usage entry
    /// </summary>
    public async Task CreateAsync(UsageRecord record)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        try
        {
            const string query = @"
                INSERT INTO UsageRecords
                (Id, ApiKeyId, ConsumerId, RecordedAt, Endpoint, Method, ResponseStatusCode,
                 RequestBytes, ResponseBytes, ResponseTimeMs, SourceIp)
                VALUES (@Id, @ApiKeyId, @ConsumerId, @RecordedAt, @Endpoint, @Method,
                        @ResponseStatusCode, @RequestBytes, @ResponseBytes, @ResponseTimeMs, @SourceIp)";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            AddParameters(cmd, record);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();

            _logger.LogDebug("Usage record created for API key {ApiKeyId}", record.ApiKeyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create usage record");
            throw new DataAccessException("Failed to create usage record", "CREATE", "UsageRecord");
        }
    }

    /// <summary>
    /// Retrieves usage records for an API key within a date range
    /// </summary>
    public async Task<List<UsageRecord>> GetByApiKeyAndDateRangeAsync(string apiKeyId, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            return [];

        try
        {
            const string query = @"
                SELECT * FROM UsageRecords
                WHERE ApiKeyId = @ApiKeyId AND RecordedAt >= @StartDate AND RecordedAt <= @EndDate
                ORDER BY RecordedAt DESC";

            var records = new List<UsageRecord>();

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@ApiKeyId", apiKeyId));
            cmd.Parameters.Add(CreateParameter("@StartDate", startDate));
            cmd.Parameters.Add(CreateParameter("@EndDate", endDate));

            await _connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                records.Add(MapFromReader(reader));
            }

            await _connection.CloseAsync();
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve usage records for API key {ApiKeyId}", apiKeyId);
            throw new DataAccessException("Failed to retrieve usage records", "SELECT", "UsageRecord", ex);
        }
    }

    /// <summary>
    /// Retrieves all usage records within a date range, regardless of API key or consumer
    /// </summary>
    public async Task<List<UsageRecord>> GetUsageAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            const string query = @"
                SELECT * FROM UsageRecords
                WHERE RecordedAt >= @StartDate AND RecordedAt <= @EndDate
                ORDER BY RecordedAt DESC";

            var records = new List<UsageRecord>();

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@StartDate", startDate));
            cmd.Parameters.Add(CreateParameter("@EndDate", endDate));

            await _connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                records.Add(MapFromReader(reader));
            }

            await _connection.CloseAsync();
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve usage records for range {StartDate}-{EndDate}", startDate, endDate);
            throw new DataAccessException("Failed to retrieve usage records", "SELECT", "UsageRecord", ex);
        }
    }

    /// <summary>
    /// Retrieves usage records for a consumer within a date range
    /// </summary>
    public async Task<List<UsageRecord>> GetByConsumerAndDateRangeAsync(string consumerId, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
            return [];

        try
        {
            const string query = @"
                SELECT * FROM UsageRecords
                WHERE ConsumerId = @ConsumerId AND RecordedAt >= @StartDate AND RecordedAt <= @EndDate
                ORDER BY RecordedAt DESC";

            var records = new List<UsageRecord>();

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@ConsumerId", consumerId));
            cmd.Parameters.Add(CreateParameter("@StartDate", startDate));
            cmd.Parameters.Add(CreateParameter("@EndDate", endDate));

            await _connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                records.Add(MapFromReader(reader));
            }

            await _connection.CloseAsync();
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve usage records for consumer {ConsumerId}", consumerId);
            throw new DataAccessException("Failed to retrieve usage records", "SELECT", "UsageRecord", ex);
        }
    }

    /// <summary>
    /// Deletes usage records older than the retention period
    /// </summary>
    public async Task DeleteOldRecordsAsync(int retentionDays)
    {
        if (retentionDays <= 0)
            throw new ArgumentException("Retention days must be positive", nameof(retentionDays));

        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            const string query = "DELETE FROM UsageRecords WHERE RecordedAt < @CutoffDate";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@CutoffDate", cutoffDate));

            await _connection.OpenAsync();
            var deletedCount = await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();

            _logger.LogInformation("Deleted {Count} old usage records", deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete old usage records");
            throw new DataAccessException("Failed to delete old usage records", "DELETE", "UsageRecord");
        }
    }

    private UsageRecord MapFromReader(DbDataReader reader) => new UsageRecord
    {
        Id = reader["Id"].ToString() ?? string.Empty,
        ApiKeyId = reader["ApiKeyId"].ToString() ?? string.Empty,
        ConsumerId = reader["ConsumerId"].ToString() ?? string.Empty,
        RecordedAt = reader["RecordedAt"] as DateTime? ?? DateTime.UtcNow,
        Endpoint = reader["Endpoint"].ToString() ?? string.Empty,
        Method = reader["Method"].ToString() ?? "GET",
        ResponseStatusCode = (int?)reader["ResponseStatusCode"] ?? 200,
        RequestBytes = (long?)reader["RequestBytes"] ?? 0,
        ResponseBytes = (long?)reader["ResponseBytes"] ?? 0,
        ResponseTimeMs = (int?)reader["ResponseTimeMs"] ?? 0,
        SourceIp = reader["SourceIp"]?.ToString()
    };

    private void AddParameters(DbCommand cmd, UsageRecord record)
    {
        cmd.Parameters.Add(CreateParameter("@Id", record.Id));
        cmd.Parameters.Add(CreateParameter("@ApiKeyId", record.ApiKeyId));
        cmd.Parameters.Add(CreateParameter("@ConsumerId", record.ConsumerId));
        cmd.Parameters.Add(CreateParameter("@RecordedAt", record.RecordedAt));
        cmd.Parameters.Add(CreateParameter("@Endpoint", record.Endpoint));
        cmd.Parameters.Add(CreateParameter("@Method", record.Method));
        cmd.Parameters.Add(CreateParameter("@ResponseStatusCode", record.ResponseStatusCode));
        cmd.Parameters.Add(CreateParameter("@RequestBytes", record.RequestBytes));
        cmd.Parameters.Add(CreateParameter("@ResponseBytes", record.ResponseBytes));
        cmd.Parameters.Add(CreateParameter("@ResponseTimeMs", record.ResponseTimeMs));
        cmd.Parameters.Add(CreateParameter("@SourceIp", (object?)record.SourceIp ?? DBNull.Value));
    }

    private DbParameter CreateParameter(string name, object? value)
    {
        var param = _connection.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        return param;
    }
}
