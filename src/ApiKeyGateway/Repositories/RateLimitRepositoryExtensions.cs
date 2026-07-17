// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Globalization;
using System.Threading.Tasks;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Extension methods that add higher-level operations for <see cref="RateLimitRepository"/>.
/// </summary>
public static class RateLimitRepositoryExtensions
{
    /// <summary>
    /// Retrieves the <see cref="RateLimit"/> for the specified API key, creating a new one
    /// with the supplied <paramref name="factory"/> when none exists.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <param name="factory">
    /// A delegate that creates a <see cref="RateLimit"/> when one does not already exist.
    /// The delegate receives the <paramref name="apiKeyId"/> as its argument.
    /// </param>
    /// <returns>The existing or newly created <see cref="RateLimit"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="apiKeyId"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <see langword="null"/>.</exception>
    /// <exception cref="DataAccessException">Propagated from the underlying repository when creation fails.</exception>
    public static async Task<RateLimit> GetOrCreateAsync(
        this RateLimitRepository repository,
        string apiKeyId,
        Func<string, RateLimit> factory)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);
        ArgumentNullException.ThrowIfNull(factory);

        var existing = await repository.GetByApiKeyIdAsync(apiKeyId).ConfigureAwait(false);
        if (existing is not null)
            return existing;

        var newRateLimit = factory(apiKeyId);
        await repository.CreateAsync(newRateLimit).ConfigureAwait(false);
        return newRateLimit;
    }

    /// <summary>
    /// Increments the <see cref="RateLimit.CurrentRequestCount"/> for the given API key
    /// and persists the change.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <returns>A task that completes when the increment has been persisted.</returns>
    /// <exception cref="ArgumentException"><paramref name="apiKeyId"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no <see cref="RateLimit"/> exists for the supplied <paramref name="apiKeyId"/>.
    /// </exception>
    /// <exception cref="DataAccessException">Propagated from the underlying repository when the update fails.</exception>
    public static async Task IncrementRequestCountAsync(this RateLimitRepository repository, string apiKeyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);

        var rateLimit = await repository.GetByApiKeyIdAsync(apiKeyId).ConfigureAwait(false)
        ?? throw new InvalidOperationException(
            $"Rate limit not found for API key '{apiKeyId}'.");

        rateLimit.CurrentRequestCount++;
        await repository.UpdateAsync(rateLimit).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether the supplied API key has exceeded its configured request quota.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <returns>
    /// <c>true</c> if the current request count is greater than or equal to the allowed
    /// <see cref="RateLimit.RequestsPerUnit"/> and the limit is enabled; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="apiKeyId"/> is <see langword="null"/> or whitespace.</exception>
    public static async Task<bool> IsRateLimitedAsync(this RateLimitRepository repository, string apiKeyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);

        var rateLimit = await repository.GetByApiKeyIdAsync(apiKeyId).ConfigureAwait(false);
        if (rateLimit is null || !rateLimit.IsEnabled)
            return false;

        return rateLimit.CurrentRequestCount >= rateLimit.RequestsPerUnit;
    }

    /// <summary>
    /// Resets the request counter if the time window defined by <see cref="RateLimit.Unit"/>
    /// has elapsed since <see cref="RateLimit.LastResetAt"/>.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="apiKeyId">The API key identifier.</param>
    /// <returns>A task that completes when the reset (if any) has been persisted.</returns>
    /// <exception cref="ArgumentException"><paramref name="apiKeyId"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="DataAccessException">Propagated from the underlying repository when the update fails.</exception>
    public static async Task ResetIfWindowExpiredAsync(this RateLimitRepository repository, string apiKeyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);

        var rateLimit = await repository.GetByApiKeyIdAsync(apiKeyId).ConfigureAwait(false);
        if (rateLimit is null)
            return;

        var now = DateTime.UtcNow;
        var window = rateLimit.Unit switch
        {
            RateLimitUnit.Second => TimeSpan.FromSeconds(1),
            RateLimitUnit.Minute => TimeSpan.FromMinutes(1),
            RateLimitUnit.Hour => TimeSpan.FromHours(1),
            RateLimitUnit.Day => TimeSpan.FromDays(1),
            _ => TimeSpan.FromHours(1)
        };

        var lastReset = rateLimit.LastResetAt ?? now;
        if (now - lastReset >= window)
        {
            rateLimit.CurrentRequestCount = 0;
            rateLimit.LastResetAt = now;
            await repository.UpdateAsync(rateLimit).ConfigureAwait(false);
        }
    }
}