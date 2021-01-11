using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Extension methods for <see cref="DatabaseTransformationRuleRepository"/> 
/// to add common query patterns and domain-specific operations 
/// without modifying the core repository implementation.
/// </summary>
public static class DatabaseTransformationRuleRepositoryExtensions
{
    /// <summary>
    /// Retrieves all enabled transformation rules 
    /// for the specified API key and consumer.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="apiKeyId">The API key ID to filter by.</param>
    /// <param name="consumerId">The consumer ID to filter by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An IReadOnlyList of <see cref="TransformationRule"/> instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="apiKeyId"/> or <paramref name="consumerId"/> is null or whitespace.</exception>
    public static async Task<IReadOnlyList<TransformationRule>> GetByApiKeyAndConsumerAsync(
        this DatabaseTransformationRuleRepository repository,
        string apiKeyId,
        string consumerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(apiKeyId);
        ArgumentNullException.ThrowIfNullOrEmpty(consumerId);

        var apiKeyRules = await repository.GetByApiKeyAsync(apiKeyId, cancellationToken);
        var consumerRules = await repository.GetByConsumerAsync(consumerId, cancellationToken);
        var globalRules = await repository.GetGlobalRulesAsync(cancellationToken);

        return apiKeyRules
            .Concat(consumerRules)
            .Concat(globalRules)
            .Where(r => r.IsEnabled)
            .OrderBy(r => r.Priority)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Checks if a transformation rule with the specified ID exists 
    /// in the repository and is enabled.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="ruleId">The ID of the transformation rule to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the transformation rule exists and is enabled, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ruleId"/> is null or whitespace.</exception>
    public static async Task<bool> ExistsAndIsEnabledAsync(
        this DatabaseTransformationRuleRepository repository,
        string ruleId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(ruleId);

        var rule = await repository.GetByIdAsync(ruleId, cancellationToken);
        return rule is not null && rule.IsEnabled;
    }

    private static async Task<TransformationRule?> GetByIdAsync(
        this DatabaseTransformationRuleRepository repository,
        string ruleId,
        CancellationToken cancellationToken)
    {
        // This assumes there's no direct GetById method; 
        // adjusts query to use ID instead of existing methods
        var rules = await repository.GetGlobalRulesAsync(cancellationToken);
        var rule = rules
            .Concat(await repository.GetByApiKeyAsync(Guid.NewGuid().ToString(), cancellationToken))
            .Concat(await repository.GetByConsumerAsync(Guid.NewGuid().ToString(), cancellationToken))
            .FirstOrDefault(r => r.Id == ruleId);

        return rule;
    }
}
