// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Data.Common;
using System.Text.Json;
using ApiKeyGateway.Data;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Transformation;
using Microsoft.Extensions.Logging;

namespace ApiKeyGateway.Repositories;

public sealed class DatabaseTransformationRuleRepository : ITransformationRuleRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<DatabaseTransformationRuleRepository> _logger;

    public DatabaseTransformationRuleRepository(
        IDbConnection dbConnection,
        ILogger<DatabaseTransformationRuleRepository> logger)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<TransformationRule>> GetByApiKeyAsync(string apiKeyId, CancellationToken cancellationToken = default)
    {
        var rules = new List<TransformationRule>();
        await _dbConnection.OpenAsync();
        try
        {
            using var command = _dbConnection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Description, Scope, ApiKeyId, ConsumerId, Type, Action, LuaScript, Parameters, Priority, IsEnabled, CreatedAt, UpdatedAt, CreatedBy
                FROM TransformationRules
                WHERE ApiKeyId = @ApiKeyId AND IsEnabled = 1
                ORDER BY Priority ASC";

            var param = command.CreateParameter();
            param.ParameterName = "@ApiKeyId";
            param.Value = apiKeyId;
            command.Parameters.Add(param);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rules.Add(MapToTransformationRule(reader));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve transformation rules by API key {ApiKeyId}", apiKeyId);
            throw new DataAccessException($"Failed to retrieve transformation rules by API key {apiKeyId}", ex);
        }
        finally
        {
            await _dbConnection.CloseAsync();
        }
        return rules;
    }

    public async Task<IReadOnlyList<TransformationRule>> GetByConsumerAsync(string consumerId, CancellationToken cancellationToken = default)
    {
        var rules = new List<TransformationRule>();
        await _dbConnection.OpenAsync();
        try
        {
            using var command = _dbConnection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Description, Scope, ApiKeyId, ConsumerId, Type, Action, LuaScript, Parameters, Priority, IsEnabled, CreatedAt, UpdatedAt, CreatedBy
                FROM TransformationRules
                WHERE ConsumerId = @ConsumerId AND IsEnabled = 1
                ORDER BY Priority ASC";

            var param = command.CreateParameter();
            param.ParameterName = "@ConsumerId";
            param.Value = consumerId;
            command.Parameters.Add(param);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rules.Add(MapToTransformationRule(reader));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve transformation rules by consumer {ConsumerId}", consumerId);
            throw new DataAccessException($"Failed to retrieve transformation rules by consumer {consumerId}", ex);
        }
        finally
        {
            await _dbConnection.CloseAsync();
        }
        return rules;
    }

    public async Task<IReadOnlyList<TransformationRule>> GetGlobalRulesAsync(CancellationToken cancellationToken = default)
    {
        var rules = new List<TransformationRule>();
        await _dbConnection.OpenAsync();
        try
        {
            using var command = _dbConnection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Description, Scope, ApiKeyId, ConsumerId, Type, Action, LuaScript, Parameters, Priority, IsEnabled, CreatedAt, UpdatedAt, CreatedBy
                FROM TransformationRules
                WHERE Scope = @GlobalScope AND IsEnabled = 1
                ORDER BY Priority ASC";

            var param = command.CreateParameter();
            param.ParameterName = "@GlobalScope";
            param.Value = (int)TransformationScope.Global;
            command.Parameters.Add(param);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rules.Add(MapToTransformationRule(reader));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve global transformation rules");
            throw new DataAccessException("Failed to retrieve global transformation rules", ex);
        }
        finally
        {
            await _dbConnection.CloseAsync();
        }
        return rules;
    }

    public async Task<string> CreateAsync(TransformationRule rule, CancellationToken cancellationToken = default)
    {
        rule.Id = rule.Id ?? Guid.NewGuid().ToString();
        rule.CreatedAt = DateTime.UtcNow;
        rule.IsEnabled = true; // Rules are enabled by default on creation

        await _dbConnection.OpenAsync();
        try
        {
            using var command = _dbConnection.CreateCommand();
            command.CommandText = @"
                INSERT INTO TransformationRules
                (Id, Name, Description, Scope, ApiKeyId, ConsumerId, Type, Action, LuaScript, Parameters, Priority, IsEnabled, CreatedAt, CreatedBy)
                VALUES
                (@Id, @Name, @Description, @Scope, @ApiKeyId, @ConsumerId, @Type, @Action, @LuaScript, @Parameters, @Priority, @IsEnabled, @CreatedAt, @CreatedBy)";

            AddRuleParameters(command, rule);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create transformation rule {RuleId}", rule.Id);
            throw new DataAccessException($"Failed to create transformation rule {rule.Id}", ex);
        }
        finally
        {
            await _dbConnection.CloseAsync();
        }
        return rule.Id;
    }

    public async Task<bool> UpdateAsync(TransformationRule rule, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rule.Id))
        {
            throw new ArgumentException("Rule ID cannot be empty for update operation.", nameof(rule.Id));
        }

        rule.UpdatedAt = DateTime.UtcNow;

        await _dbConnection.OpenAsync();
        try
        {
            using var command = _dbConnection.CreateCommand();
            command.CommandText = @"
                UPDATE TransformationRules
                SET Name = @Name, Description = @Description, Scope = @Scope, ApiKeyId = @ApiKeyId, ConsumerId = @ConsumerId,
                    Type = @Type, Action = @Action, LuaScript = @LuaScript, Parameters = @Parameters, Priority = @Priority,
                    IsEnabled = @IsEnabled, UpdatedAt = @UpdatedAt, CreatedBy = @CreatedBy
                WHERE Id = @Id";

            AddRuleParameters(command, rule);

            var paramId = command.CreateParameter();
            paramId.ParameterName = "@Id";
            paramId.Value = rule.Id;
            command.Parameters.Add(paramId);

            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update transformation rule {RuleId}", rule.Id);
            throw new DataAccessException($"Failed to update transformation rule {rule.Id}", ex);
        }
        finally
        {
            await _dbConnection.CloseAsync();
        }
    }

    public async Task<bool> DeleteAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        await _dbConnection.OpenAsync();
        try
        {
            using var command = _dbConnection.CreateCommand();
            command.CommandText = @"
                UPDATE TransformationRules
                SET IsEnabled = 0, UpdatedAt = @UpdatedAt
                WHERE Id = @RuleId";

            var paramId = command.CreateParameter();
            paramId.ParameterName = "@RuleId";
            paramId.Value = ruleId;
            command.Parameters.Add(paramId);

            var paramUpdatedAt = command.CreateParameter();
            paramUpdatedAt.ParameterName = "@UpdatedAt";
            paramUpdatedAt.Value = DateTime.UtcNow;
            command.Parameters.Add(paramUpdatedAt);

            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete transformation rule {RuleId}", ruleId);
            throw new DataAccessException($"Failed to delete transformation rule {ruleId}", ex);
        }
        finally
        {
            await _dbConnection.CloseAsync();
        }
    }

    private TransformationRule MapToTransformationRule(DbDataReader reader)
    {
        var parametersJson = reader["Parameters"] as string;
        var parameters = parametersJson != null
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(parametersJson)
            : new Dictionary<string, string>();

        return new TransformationRule
        {
            Id = reader["Id"] as string,
            Name = reader["Name"] as string,
            Description = reader["Description"] as string,
            Scope = (TransformationScope)(int)reader["Scope"],
            ApiKeyId = reader["ApiKeyId"] as string,
            ConsumerId = reader["ConsumerId"] as string,
            Type = (TransformationRuleType)(int)reader["Type"],
            Action = (BuiltInAction)(int)reader["Action"],
            LuaScript = reader["LuaScript"] as string,
            Parameters = parameters,
            Priority = (int)reader["Priority"],
            IsEnabled = (bool)reader["IsEnabled"],
            CreatedAt = (DateTime)reader["CreatedAt"],
            UpdatedAt = (DateTime)reader["UpdatedAt"],
            CreatedBy = reader["CreatedBy"] as string
        };
    }

    private void AddRuleParameters(DbCommand command, TransformationRule rule)
    {
        command.Parameters.Add(CreateParam(command, "@Id", rule.Id));
        command.Parameters.Add(CreateParam(command, "@Name", rule.Name));
        command.Parameters.Add(CreateParam(command, "@Description", rule.Description));
        command.Parameters.Add(CreateParam(command, "@Scope", (int)rule.Scope));
        command.Parameters.Add(CreateParam(command, "@ApiKeyId", rule.ApiKeyId));
        command.Parameters.Add(CreateParam(command, "@ConsumerId", rule.ConsumerId));
        command.Parameters.Add(CreateParam(command, "@Type", (int)rule.Type));
        command.Parameters.Add(CreateParam(command, "@Action", (int)rule.Action));
        command.Parameters.Add(CreateParam(command, "@LuaScript", rule.LuaScript));
        command.Parameters.Add(CreateParam(command, "@Parameters", JsonSerializer.Serialize(rule.Parameters)));
        command.Parameters.Add(CreateParam(command, "@Priority", rule.Priority));
        command.Parameters.Add(CreateParam(command, "@IsEnabled", rule.IsEnabled));
        command.Parameters.Add(CreateParam(command, "@CreatedAt", rule.CreatedAt));
        command.Parameters.Add(CreateParam(command, "@UpdatedAt", rule.UpdatedAt));
        command.Parameters.Add(CreateParam(command, "@CreatedBy", rule.CreatedBy));
    }

    private DbParameter CreateParam(DbCommand command, string name, object? value)
    {
        var param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        return param;
    }
}