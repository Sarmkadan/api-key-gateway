// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Data.Common;
using ApiKeyGateway.Data;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Persists usage quota state using ADO.NET with a simple in-process write-through cache.
/// </summary>
public class UsageQuotaRepository : IUsageQuotaRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<UsageQuotaRepository> _logger;
    private readonly Dictionary<string, UsageQuota> _cache = new();

    public UsageQuotaRepository(IDbConnection connection, ILogger<UsageQuotaRepository> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UsageQuota?> GetByApiKeyIdAsync(string apiKeyId)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            return null;

        if (_cache.TryGetValue(apiKeyId, out var cached))
            return cached;

        try
        {
            const string query = "SELECT * FROM UsageQuotas WHERE ApiKeyId = @ApiKeyId";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@ApiKeyId", apiKeyId));

            await _connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var quota = MapFromReader(reader);
                _cache[apiKeyId] = quota;
                return quota;
            }

            await _connection.CloseAsync();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve usage quota for API key {ApiKeyId}", apiKeyId);
            throw new DataAccessException("Failed to retrieve usage quota", "SELECT", "UsageQuota");
        }
    }

    public async Task<UsageQuota> CreateAsync(UsageQuota quota)
    {
        if (quota == null)
            throw new ArgumentNullException(nameof(quota));

        try
        {
            const string query = @"
                INSERT INTO UsageQuotas (Id, ApiKeyId, QuotaLimit, IsEnabled, Period, CreatedAt, PeriodStartAt, CurrentUsage)
                VALUES (@Id, @ApiKeyId, @QuotaLimit, @IsEnabled, @Period, @CreatedAt, @PeriodStartAt, @CurrentUsage)";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            AddParameters(cmd, quota);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();

            _cache[quota.ApiKeyId] = quota;
            return quota;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create usage quota for API key {ApiKeyId}", quota.ApiKeyId);
            throw new DataAccessException("Failed to create usage quota", "INSERT", "UsageQuota");
        }
    }

    public async Task UpdateAsync(UsageQuota quota)
    {
        if (quota == null)
            throw new ArgumentNullException(nameof(quota));

        try
        {
            const string query = @"
                UPDATE UsageQuotas
                SET CurrentUsage = @CurrentUsage, PeriodStartAt = @PeriodStartAt,
                    IsEnabled = @IsEnabled, QuotaLimit = @QuotaLimit, Period = @Period
                WHERE ApiKeyId = @ApiKeyId";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@CurrentUsage", quota.CurrentUsage));
            cmd.Parameters.Add(CreateParameter("@PeriodStartAt", quota.PeriodStartAt));
            cmd.Parameters.Add(CreateParameter("@IsEnabled", quota.IsEnabled));
            cmd.Parameters.Add(CreateParameter("@QuotaLimit", quota.QuotaLimit));
            cmd.Parameters.Add(CreateParameter("@Period", (int)quota.Period));
            cmd.Parameters.Add(CreateParameter("@ApiKeyId", quota.ApiKeyId));

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();

            _cache[quota.ApiKeyId] = quota;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update usage quota for API key {ApiKeyId}", quota.ApiKeyId);
            throw new DataAccessException("Failed to update usage quota", "UPDATE", "UsageQuota");
        }
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        try
        {
            const string query = "DELETE FROM UsageQuotas WHERE Id = @Id";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@Id", id));

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();

            var key = _cache.FirstOrDefault(kv => kv.Value.Id == id).Key;
            if (key != null)
                _cache.Remove(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete usage quota {Id}", id);
            throw new DataAccessException("Failed to delete usage quota", "DELETE", "UsageQuota");
        }
    }

    private static UsageQuota MapFromReader(DbDataReader reader) => new UsageQuota
    {
        Id = reader["Id"].ToString() ?? string.Empty,
        ApiKeyId = reader["ApiKeyId"].ToString() ?? string.Empty,
        QuotaLimit = reader["QuotaLimit"] as long? ?? Convert.ToInt64(reader["QuotaLimit"]),
        IsEnabled = reader["IsEnabled"] as bool? ?? true,
        Period = (Domain.Enums.QuotaPeriod)(reader["Period"] as int? ?? 1),
        CreatedAt = reader["CreatedAt"] as DateTime? ?? DateTime.UtcNow,
        PeriodStartAt = reader["PeriodStartAt"] as DateTime? ?? DateTime.UtcNow,
        CurrentUsage = reader["CurrentUsage"] as long? ?? Convert.ToInt64(reader["CurrentUsage"])
    };

    private void AddParameters(DbCommand cmd, UsageQuota quota)
    {
        cmd.Parameters.Add(CreateParameter("@Id", quota.Id));
        cmd.Parameters.Add(CreateParameter("@ApiKeyId", quota.ApiKeyId));
        cmd.Parameters.Add(CreateParameter("@QuotaLimit", quota.QuotaLimit));
        cmd.Parameters.Add(CreateParameter("@IsEnabled", quota.IsEnabled));
        cmd.Parameters.Add(CreateParameter("@Period", (int)quota.Period));
        cmd.Parameters.Add(CreateParameter("@CreatedAt", quota.CreatedAt));
        cmd.Parameters.Add(CreateParameter("@PeriodStartAt", quota.PeriodStartAt));
        cmd.Parameters.Add(CreateParameter("@CurrentUsage", quota.CurrentUsage));
    }

    private DbParameter CreateParameter(string name, object? value)
    {
        var param = _connection.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        return param;
    }
}
