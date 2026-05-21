// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Repository implementation for rate limit data persistence
/// </summary>
public class RateLimitRepository : IRateLimitRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<RateLimitRepository> _logger;

    public RateLimitRepository(IDbConnection connection, ILogger<RateLimitRepository> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a rate limit by API key ID
    /// </summary>
    public async Task<RateLimit?> GetByApiKeyIdAsync(string apiKeyId)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            return null;

        try
        {
            const string query = "SELECT * FROM RateLimits WHERE ApiKeyId = @ApiKeyId";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@ApiKeyId", apiKeyId));

            await _connection.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            if (await reader.ReadAsync())
            {
                var rateLimit = MapFromReader(reader);
                await _connection.CloseAsync().ConfigureAwait(false);
                return rateLimit;
            }

            await _connection.CloseAsync().ConfigureAwait(false);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve rate limit for API key {ApiKeyId}", apiKeyId);
            return null;
        }
    }

    /// <summary>
    /// Creates a new rate limit configuration
    /// </summary>
    public async Task<RateLimit> CreateAsync(RateLimit rateLimit)
    {
        if (rateLimit == null)
            throw new ArgumentNullException(nameof(rateLimit));

        try
        {
            const string query = @"
                INSERT INTO RateLimits
                (Id, ApiKeyId, RequestsPerUnit, Unit, IsEnabled, CreatedAt, LastResetAt, CurrentRequestCount)
                VALUES (@Id, @ApiKeyId, @RequestsPerUnit, @Unit, @IsEnabled, @CreatedAt, @LastResetAt, @CurrentRequestCount)";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            AddParameters(cmd, rateLimit);

            await _connection.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await _connection.CloseAsync().ConfigureAwait(false);

            _logger.LogDebug("Rate limit created for API key {ApiKeyId}", rateLimit.ApiKeyId);
            return rateLimit;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create rate limit");
            throw new DataAccessException("Failed to create rate limit", "CREATE", "RateLimit");
        }
    }

    /// <summary>
    /// Updates an existing rate limit
    /// </summary>
    public async Task UpdateAsync(RateLimit rateLimit)
    {
        if (rateLimit == null)
            throw new ArgumentNullException(nameof(rateLimit));

        try
        {
            const string query = @"
                UPDATE RateLimits
                SET RequestsPerUnit = @RequestsPerUnit, Unit = @Unit, IsEnabled = @IsEnabled,
                    LastResetAt = @LastResetAt, CurrentRequestCount = @CurrentRequestCount
                WHERE Id = @Id";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@RequestsPerUnit", rateLimit.RequestsPerUnit));
            cmd.Parameters.Add(CreateParameter("@Unit", (int)rateLimit.Unit));
            cmd.Parameters.Add(CreateParameter("@IsEnabled", rateLimit.IsEnabled));
            cmd.Parameters.Add(CreateParameter("@LastResetAt", rateLimit.LastResetAt));
            cmd.Parameters.Add(CreateParameter("@CurrentRequestCount", rateLimit.CurrentRequestCount));
            cmd.Parameters.Add(CreateParameter("@Id", rateLimit.Id));

            await _connection.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await _connection.CloseAsync().ConfigureAwait(false);

            _logger.LogDebug("Rate limit updated for API key {ApiKeyId}", rateLimit.ApiKeyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update rate limit {Id}", rateLimit.Id);
            throw new DataAccessException("Failed to update rate limit", "UPDATE", "RateLimit");
        }
    }

    /// <summary>
    /// Deletes a rate limit configuration
    /// </summary>
    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        try
        {
            const string query = "DELETE FROM RateLimits WHERE Id = @Id";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@Id", id));

            await _connection.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await _connection.CloseAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete rate limit {Id}", id);
            throw new DataAccessException("Failed to delete rate limit", "DELETE", "RateLimit");
        }
    }

    private RateLimit MapFromReader(DbDataReader reader) => new RateLimit
    {
        Id = reader["Id"].ToString() ?? string.Empty,
        ApiKeyId = reader["ApiKeyId"].ToString() ?? string.Empty,
        RequestsPerUnit = (int?)reader["RequestsPerUnit"] ?? 1000,
        Unit = (Domain.Enums.RateLimitUnit)(reader["Unit"] as int? ?? 3),
        IsEnabled = (reader["IsEnabled"] as bool?) ?? true,
        CreatedAt = reader["CreatedAt"] as DateTime? ?? DateTime.UtcNow,
        LastResetAt = reader["LastResetAt"] as DateTime?,
        CurrentRequestCount = (int?)reader["CurrentRequestCount"] ?? 0
    };

    private void AddParameters(DbCommand cmd, RateLimit rateLimit)
    {
        cmd.Parameters.Add(CreateParameter("@Id", rateLimit.Id));
        cmd.Parameters.Add(CreateParameter("@ApiKeyId", rateLimit.ApiKeyId));
        cmd.Parameters.Add(CreateParameter("@RequestsPerUnit", rateLimit.RequestsPerUnit));
        cmd.Parameters.Add(CreateParameter("@Unit", (int)rateLimit.Unit));
        cmd.Parameters.Add(CreateParameter("@IsEnabled", rateLimit.IsEnabled));
        cmd.Parameters.Add(CreateParameter("@CreatedAt", rateLimit.CreatedAt));
        cmd.Parameters.Add(CreateParameter("@LastResetAt", rateLimit.LastResetAt));
        cmd.Parameters.Add(CreateParameter("@CurrentRequestCount", rateLimit.CurrentRequestCount));
    }

    private DbParameter CreateParameter(string name, object? value)
    {
        var param = _connection.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        return param;
    }
}
