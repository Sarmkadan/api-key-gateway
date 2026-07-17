// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Globalization;
using System.Threading.Tasks;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Extension methods that add higher‑level operations for <see cref="UsageQuotaRepository"/>.
/// </summary>
public static class UsageQuotaRepositoryExtensions
{
    /// <summary>
    /// Retrieves the <see cref="UsageQuota"/> for the specified API key, creating a new one
    /// using the supplied <paramref name="factory"/> when it does not exist.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <param name="factory">A factory delegate that creates a <see cref="UsageQuota"/> for the given API key.</param>
    /// <returns>The existing or newly created <see cref="UsageQuota"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> or <paramref name="factory"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    /// <exception cref="DataAccessException">Propagated from the underlying repository when a data operation fails.</exception>
    public static async Task<UsageQuota> GetOrCreateAsync(
        this UsageQuotaRepository repository,
        string apiKeyId,
        Func<string, UsageQuota> factory)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);
        ArgumentNullException.ThrowIfNull(factory);

        var existing = await repository.GetByApiKeyIdAsync(apiKeyId).ConfigureAwait(false);
        return existing ?? await CreateAndReturnAsync(repository, apiKeyId, factory);
    }

    /// <summary>
    /// Increments the <see cref="UsageQuota.CurrentUsage"/> of the quota associated with the specified API key
    /// and persists the change.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <param name="increment">The amount to add to the current usage. Must be greater than zero.</param>
    /// <returns>The updated <see cref="UsageQuota"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="increment"/> is less than or equal to zero.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no quota exists for the supplied API key.</exception>
    /// <exception cref="DataAccessException">Propagated from the underlying repository when a data operation fails.</exception>
    public static async Task<UsageQuota> IncrementUsageAsync(
        this UsageQuotaRepository repository,
        string apiKeyId,
        long increment = 1L)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(increment, 0L);

        var quota = await repository.GetByApiKeyIdAsync(apiKeyId).ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture,
                    "Usage quota not found for API key '{0}'.",
                    apiKeyId));

        quota.CurrentUsage += increment;
        await repository.UpdateAsync(quota).ConfigureAwait(false);
        return quota;
    }

    /// <summary>
    /// Deletes the quota for the given API key when it is disabled (<see cref="UsageQuota.IsEnabled"/> is false).
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <returns>True if a disabled quota was found and deleted; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    /// <exception cref="DataAccessException">Propagated from the underlying repository when a data operation fails.</exception>
    public static async Task<bool> DeleteIfDisabledAsync(
        this UsageQuotaRepository repository,
        string apiKeyId)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);

        var quota = await repository.GetByApiKeyIdAsync(apiKeyId).ConfigureAwait(false);
        return quota is { IsEnabled: false }
            && await DeleteQuotaAsync(repository, quota).ConfigureAwait(false);
    }

    private static async Task<UsageQuota> CreateAndReturnAsync(
        UsageQuotaRepository repository,
        string apiKeyId,
        Func<string, UsageQuota> factory)
    {
        var quota = factory(apiKeyId);
        await repository.CreateAsync(quota).ConfigureAwait(false);
        return quota;
    }

    private static async Task<bool> DeleteQuotaAsync(
        UsageQuotaRepository repository,
        UsageQuota quota)
    {
        await repository.DeleteAsync(quota.Id).ConfigureAwait(false);
        return true;
    }
}