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
/// Repository implementation for rate limit data persistence.
/// </summary>
public class RateLimitRepository : IRateLimitRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<RateLimitRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitRepository"/> class.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when connection or logger is null.</exception>
    public RateLimitRepository(IDbConnection connection, ILogger<RateLimitRepository> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a rate limit by API key ID asynchronously.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>The <see cref="RateLimit"/> found, or null if not found or an error occurs.</returns>
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

            await _connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var rateLimit = MapFromReader(reader);
                await _connection.CloseAsync();
                return rateLimit;
            }

            await _connection.CloseAsync();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve rate limit for API key {ApiKeyId}", apiKeyId);
            return null;
        }
    }

    /// <summary>
    /// Creates a new rate limit configuration asynchronously.
    /// </summary>
    /// <param name="rateLimit">The rate limit configuration to create.</param>
    /// <returns>The created <see cref="RateLimit"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when rateLimit is null.</exception>
    /// <exception cref="DataAccessException">Thrown when creation fails.</exception>
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

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();

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
    /// Updates an existing rate limit asynchronously.
    /// </summary>
    /// <param name="rateLimit">The rate limit configuration to update.</param>
    /// <exception cref="ArgumentNullException">Thrown when rateLimit is null.</exception>
    /// <exception cref="DataAccessException">Thrown when update fails.</exception>
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

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();

            _logger.LogDebug("Rate limit updated for API key {ApiKeyId}", rateLimit.ApiKeyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update rate limit {Id}", rateLimit.Id);
            throw new DataAccessException("Failed to update rate limit", "UPDATE", "RateLimit");
        }
    }

    /// <summary>
    /// Deletes a rate limit configuration asynchronously.
    /// </summary>
    /// <param name="id">The ID of the rate limit to delete.</param>
    /// <exception cref="DataAccessException">Thrown when deletion fails.</exception>
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

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
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
