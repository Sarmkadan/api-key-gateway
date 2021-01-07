// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Extension methods for <see cref="ApiKeyRepository"/> to add common query patterns
/// and domain-specific operations without modifying the core repository implementation.
/// </summary>
public static class ApiKeyRepositoryExtensions
{
    /// <summary>
    /// Checks if an API key with the specified ID exists in the repository.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="id">The ID of the API key to check.</param>
    /// <returns>True if the API key exists, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null or whitespace.</exception>
    public static async Task<bool> ExistsByIdAsync(this ApiKeyRepository repository, string id)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(id);
        return (await repository.GetByIdAsync(id)) is not null;
    }

    /// <summary>
    /// Retrieves all active API keys for a given consumer.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="consumerId">The consumer ID to filter by.</param>
    /// <returns>An IReadOnlyList of active <see cref="ApiKey"/> instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="consumerId"/> is null or whitespace.</exception>
    public static async Task<IReadOnlyList<ApiKey>> GetActiveKeysByConsumerAsync(this ApiKeyRepository repository, string consumerId)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(consumerId);
        var allKeys = await repository.GetByConsumerIdAsync(consumerId);
        return allKeys
            .Where(k => k.Status == ApiKeyStatus.Active)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Counts the number of active API keys for a given consumer.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="consumerId">The consumer ID to count for.</param>
    /// <returns>The number of active API keys.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="consumerId"/> is null or whitespace.</exception>
    public static async Task<int> GetActiveKeyCountByConsumerAsync(this ApiKeyRepository repository, string consumerId)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(consumerId);
        return (await repository.GetByConsumerIdAsync(consumerId))
            .Count(k => k.Status == ApiKeyStatus.Active);
    }
}
