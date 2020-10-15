// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Repository implementation for API key data persistence
/// </summary>
public class ApiKeyRepository : IApiKeyRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<ApiKeyRepository> _logger;
    private readonly Dictionary<string, ApiKey> _memoryStore;

    public ApiKeyRepository(IDbConnection connection, ILogger<ApiKeyRepository> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memoryStore = new Dictionary<string, ApiKey>();
    }

    /// <summary>
    /// Creates a new API key in the repository
    /// </summary>
    public async Task<ApiKey> CreateAsync(ApiKey apiKey)
    {
        if (apiKey == null)
            throw new ArgumentNullException(nameof(apiKey));

        try
        {
            const string query = @"
                INSERT INTO ApiKeys (Id, ConsumerId, Name, KeyHash, Prefix, Status, CreatedAt, ExpiresAt, Description)
                VALUES (@Id, @ConsumerId, @Name, @KeyHash, @Prefix, @Status, @CreatedAt, @ExpiresAt, @Description)";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            AddParameters(cmd, apiKey);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();

            _memoryStore[apiKey.Id] = apiKey;
            _logger.LogDebug("API key {Id} created successfully", apiKey.Id);

            return apiKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create API key");
            throw new DataAccessException("Failed to create API key", "CREATE", "ApiKey");
        }
    }

    /// <summary>
    /// Retrieves an API key by its ID
    /// </summary>
    public async Task<ApiKey?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        if (_memoryStore.TryGetValue(id, out var cached))
            return cached;

        try
        {
            const string query = "SELECT * FROM ApiKeys WHERE Id = @Id";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@Id", id));

            await _connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var apiKey = MapFromReader(reader);
                _memoryStore[id] = apiKey;
                return apiKey;
            }

            await _connection.CloseAsync();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve API key {Id}", id);
            throw new DataAccessException("Failed to retrieve API key", "SELECT", "ApiKey");
        }
    }

    /// <summary>
    /// Retrieves an API key by its hash
    /// </summary>
    public async Task<ApiKey?> GetByHashAsync(string keyHash)
    {
        if (string.IsNullOrWhiteSpace(keyHash))
            return null;

        try
        {
            const string query = "SELECT * FROM ApiKeys WHERE KeyHash = @KeyHash";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@KeyHash", keyHash));

            await _connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }

            await _connection.CloseAsync();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve API key by hash");
            throw new DataAccessException("Failed to retrieve API key by hash", "SELECT", "ApiKey");
        }
    }

    /// <summary>
    /// Retrieves all API keys for a consumer
    /// </summary>
    public async Task<List<ApiKey>> GetByConsumerIdAsync(string consumerId)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
            return [];

        try
        {
            const string query = "SELECT * FROM ApiKeys WHERE ConsumerId = @ConsumerId ORDER BY CreatedAt DESC";
            var keys = new List<ApiKey>();

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@ConsumerId", consumerId));

            await _connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                keys.Add(MapFromReader(reader));
            }

            await _connection.CloseAsync();
            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve keys for consumer {ConsumerId}", consumerId);
            return [];
        }
    }

    /// <summary>
    /// Updates an existing API key
    /// </summary>
    public async Task UpdateAsync(ApiKey apiKey)
    {
        if (apiKey == null)
            throw new ArgumentNullException(nameof(apiKey));

        try
        {
            const string query = @"
                UPDATE ApiKeys
                SET Status = @Status, ExpiresAt = @ExpiresAt, LastUsedAt = @LastUsedAt,
                    DisabledAt = @DisabledAt, RequestCount = @RequestCount
                WHERE Id = @Id";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@Status", (int)apiKey.Status));
            cmd.Parameters.Add(CreateParameter("@ExpiresAt", apiKey.ExpiresAt));
            cmd.Parameters.Add(CreateParameter("@LastUsedAt", apiKey.LastUsedAt));
            cmd.Parameters.Add(CreateParameter("@DisabledAt", apiKey.DisabledAt));
            cmd.Parameters.Add(CreateParameter("@RequestCount", apiKey.RequestCount));
            cmd.Parameters.Add(CreateParameter("@Id", apiKey.Id));

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();

            _memoryStore[apiKey.Id] = apiKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update API key {Id}", apiKey.Id);
            throw new DataAccessException("Failed to update API key", "UPDATE", "ApiKey");
        }
    }

    /// <summary>
    /// Deletes an API key
    /// </summary>
    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        try
        {
            const string query = "DELETE FROM ApiKeys WHERE Id = @Id";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@Id", id));

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();

            _memoryStore.Remove(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete API key {Id}", id);
            throw new DataAccessException("Failed to delete API key", "DELETE", "ApiKey");
        }
    }

    /// <summary>
    /// Retrieves expired API keys
    /// </summary>
    public async Task<List<ApiKey>> GetExpiredKeysAsync()
    {
        try
        {
            const string query = "SELECT * FROM ApiKeys WHERE ExpiresAt < @Now AND Status != @RevokedStatus";
            var keys = new List<ApiKey>();

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(CreateParameter("@Now", DateTime.UtcNow));
            cmd.Parameters.Add(CreateParameter("@RevokedStatus", (int)Domain.Enums.ApiKeyStatus.Revoked));

            await _connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                keys.Add(MapFromReader(reader));
            }

            await _connection.CloseAsync();
            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve expired keys");
            return [];
        }
    }

    private ApiKey MapFromReader(DbDataReader reader) => new ApiKey
    {
        Id = reader["Id"].ToString() ?? string.Empty,
        ConsumerId = reader["ConsumerId"].ToString() ?? string.Empty,
        Name = reader["Name"].ToString() ?? string.Empty,
        KeyHash = reader["KeyHash"].ToString() ?? string.Empty,
        Prefix = reader["Prefix"].ToString() ?? string.Empty,
        Status = (Domain.Enums.ApiKeyStatus)(reader["Status"] as int? ?? 1),
        CreatedAt = reader["CreatedAt"] as DateTime? ?? DateTime.UtcNow,
        ExpiresAt = reader["ExpiresAt"] as DateTime?,
        LastUsedAt = reader["LastUsedAt"] as DateTime?
    };

    private void AddParameters(DbCommand cmd, ApiKey key)
    {
        cmd.Parameters.Add(CreateParameter("@Id", key.Id));
        cmd.Parameters.Add(CreateParameter("@ConsumerId", key.ConsumerId));
        cmd.Parameters.Add(CreateParameter("@Name", key.Name));
        cmd.Parameters.Add(CreateParameter("@KeyHash", key.KeyHash));
        cmd.Parameters.Add(CreateParameter("@Prefix", key.Prefix));
        cmd.Parameters.Add(CreateParameter("@Status", (int)key.Status));
        cmd.Parameters.Add(CreateParameter("@CreatedAt", key.CreatedAt));
        cmd.Parameters.Add(CreateParameter("@ExpiresAt", key.ExpiresAt));
        cmd.Parameters.Add(CreateParameter("@Description", (object?)key.Description ?? DBNull.Value));
    }

    private DbParameter CreateParameter(string name, object? value)
    {
        var param = _connection.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        return param;
    }
}
