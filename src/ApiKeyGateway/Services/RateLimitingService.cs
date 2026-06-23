// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Services;

/// <summary>
/// Manages rate limiting enforcement and tracking for API keys
/// </summary>
public interface IRateLimitingService
{
    Task<bool> CheckLimitAsync(string apiKeyId);
    Task RecordRequestAsync(string apiKeyId);
    Task<RateLimit?> GetLimitAsync(string apiKeyId);
    Task<bool> UpdateLimitAsync(string apiKeyId, int requestsPerUnit, Domain.Enums.RateLimitUnit unit);
    Task ResetWindowAsync(string apiKeyId);
}

public class RateLimitingService : IRateLimitingService
{
    /// <summary>
    /// Extra seconds added to the window expiry check to absorb clock skew between
    /// the gateway host and the backing store (e.g. a remote Redis/SQL instance).
    /// Prevents premature counter resets when clocks drift by up to this value.
    /// Configurable via Gateway:ClockSkewToleranceSeconds (default 1 second).
    /// </summary>
    public const double DefaultClockSkewToleranceSeconds = 1.0;

    private readonly IRateLimitRepository _repository;
    private readonly ILogger<RateLimitingService> _logger;
    private readonly ConcurrentDictionary<string, DateTime> _windowResetCache = new();
    private readonly double _clockSkewToleranceSeconds;

    public RateLimitingService(IRateLimitRepository repository, ILogger<RateLimitingService> logger)
        : this(repository, logger, DefaultClockSkewToleranceSeconds) { }

    public RateLimitingService(IRateLimitRepository repository, ILogger<RateLimitingService> logger, double clockSkewToleranceSeconds)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _clockSkewToleranceSeconds = clockSkewToleranceSeconds >= 0 ? clockSkewToleranceSeconds : DefaultClockSkewToleranceSeconds;
    }

    /// <summary>
    /// Checks if a request is allowed under the rate limit
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>True if the request is allowed; otherwise, false.</returns>
    public async Task<bool> CheckLimitAsync(string apiKeyId)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));

        var rateLimit = await _repository.GetByApiKeyIdAsync(apiKeyId);
        if (rateLimit == null)
            return true;

        await CheckAndResetWindowAsync(rateLimit);

        if (!rateLimit.CanProcessRequest())
        {
            _logger.LogWarning("Rate limit exceeded for API key {ApiKeyId}", apiKeyId);
            throw new RateLimitExceededException(apiKeyId, rateLimit.RequestsPerUnit, rateLimit.GetWindowInSeconds());
        }

        return true;
    }

    /// <summary>
    /// Records a request for rate limit tracking
    /// </summary>
    public async Task RecordRequestAsync(string apiKeyId)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            return;

        var rateLimit = await _repository.GetByApiKeyIdAsync(apiKeyId);
        if (rateLimit == null)
            return;

        await CheckAndResetWindowAsync(rateLimit);
        rateLimit.RecordRequest();
        await _repository.UpdateAsync(rateLimit);

        _logger.LogDebug("Recorded request for API key {ApiKeyId}. Remaining: {Remaining}/{Total}",
            apiKeyId, rateLimit.RemainingRequests, rateLimit.RequestsPerUnit);
    }

    /// <summary>
    /// Retrieves the rate limit configuration for an API key
    /// </summary>
    public async Task<RateLimit?> GetLimitAsync(string apiKeyId)
    {
        return await _repository.GetByApiKeyIdAsync(apiKeyId);
    }

    /// <summary>
    /// Updates the rate limit configuration
    /// </summary>
    public async Task<bool> UpdateLimitAsync(string apiKeyId, int requestsPerUnit, Domain.Enums.RateLimitUnit unit)
    {
        var rateLimit = await _repository.GetByApiKeyIdAsync(apiKeyId);
        if (rateLimit == null)
            return false;

        rateLimit = new RateLimit
        {
            Id = rateLimit.Id,
            ApiKeyId = apiKeyId,
            RequestsPerUnit = requestsPerUnit,
            Unit = unit,
            CreatedAt = rateLimit.CreatedAt,
            LastResetAt = DateTime.UtcNow
        };

        await _repository.UpdateAsync(rateLimit);
        _logger.LogInformation("Rate limit updated for API key {ApiKeyId}: {Requests} per {Unit}",
            apiKeyId, requestsPerUnit, unit);

        return true;
    }

    /// <summary>
    /// Manually resets the rate limit window
    /// </summary>
    public async Task ResetWindowAsync(string apiKeyId)
    {
        var rateLimit = await _repository.GetByApiKeyIdAsync(apiKeyId);
        if (rateLimit == null)
            return;

        rateLimit.ResetWindow();
        await _repository.UpdateAsync(rateLimit);
        _windowResetCache.AddOrUpdate(apiKeyId, DateTime.UtcNow, (_, _) => DateTime.UtcNow);

        _logger.LogInformation("Rate limit window reset for API key {ApiKeyId}", apiKeyId);
    }

    /// <summary>
    /// Checks if window needs reset and resets if necessary.
    /// A tolerance buffer is added to the elapsed-time comparison so that
    /// clock skew between the gateway host and the backing store cannot cause
    /// the window to be reset prematurely, which would allow burst traffic
    /// beyond the configured limit.
    /// </summary>
    private async Task CheckAndResetWindowAsync(RateLimit rateLimit)
    {
        if (rateLimit.Unit == Domain.Enums.RateLimitUnit.Unlimited)
            return;

        var windowSeconds = rateLimit.GetWindowInSeconds();
        var lastReset = rateLimit.LastResetAt ?? rateLimit.CreatedAt;
        var now = DateTime.UtcNow;

        var elapsed = (now - lastReset).TotalSeconds;

        // If elapsed is negative the backing store has a clock ahead of ours;
        // do not reset in that case.  Require the full window PLUS a skew
        // tolerance to elapse before resetting the counter so that small clock
        // differences between the gateway and a remote Redis/SQL host cannot
        // trigger an early reset.
        if (elapsed >= 0 && elapsed >= windowSeconds + _clockSkewToleranceSeconds)
        {
            rateLimit.ResetWindow();
            await _repository.UpdateAsync(rateLimit);
        }
    }
}

/// <summary>
/// Repository interface for rate limit data access
/// </summary>
public interface IRateLimitRepository
{
    Task<RateLimit?> GetByApiKeyIdAsync(string apiKeyId);
    Task<RateLimit> CreateAsync(RateLimit rateLimit);
    Task UpdateAsync(RateLimit rateLimit);
    Task DeleteAsync(string id);
}
