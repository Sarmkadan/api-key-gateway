// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using ApiKeyGateway.Configuration;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Events;

namespace ApiKeyGateway.Services;

/// <summary>
/// Manages rate limiting enforcement and tracking for API keys
/// </summary>
public interface IRateLimitingService
{
    /// <summary>
    /// Checks if a request is allowed under the rate limit
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>True if the request is allowed; otherwise, false.</returns>
    Task<bool> CheckLimitAsync(string apiKeyId);

    /// <summary>
    /// Records a request for rate limit tracking
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    Task RecordRequestAsync(string apiKeyId);

    /// <summary>
    /// Retrieves the rate limit configuration for an API key
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>The rate limit configuration if found; otherwise, null.</returns>
    Task<RateLimit?> GetLimitAsync(string apiKeyId);

    /// <summary>
    /// Updates the rate limit configuration
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <param name="requestsPerUnit">Maximum requests per time unit.</param>
    /// <param name="unit">Time unit for rate limiting.</param>
    /// <returns>True if the limit was updated; otherwise, false.</returns>
    Task<bool> UpdateLimitAsync(string apiKeyId, int requestsPerUnit, Domain.Enums.RateLimitUnit unit);

    /// <summary>
    /// Manually resets the rate limit window
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
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
    private readonly RateLimitingOptions _options;
    private readonly IEventPublisher? _eventPublisher;

    // Circuit breaker state for limit-store availability, guarded by _breakerLock.
    private readonly object _breakerLock = new();
    private int _consecutiveStoreFailures;
    private DateTime _firstFailureAtUtc;
    private DateTime _circuitOpenUntilUtc;

    public RateLimitingService(IRateLimitRepository repository, ILogger<RateLimitingService> logger)
        : this(repository, logger, DefaultClockSkewToleranceSeconds) { }

    public RateLimitingService(IRateLimitRepository repository, ILogger<RateLimitingService> logger, double clockSkewToleranceSeconds)
        : this(repository, logger, clockSkewToleranceSeconds, new RateLimitingOptions(), null) { }

    /// <summary>
    /// Initializes a new instance of <see cref="RateLimitingService"/> with an explicit
    /// store-failure policy and optional event publisher for degradation warnings.
    /// </summary>
    /// <param name="repository">The rate limit store.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="clockSkewToleranceSeconds">Clock skew tolerance for window resets; negative values fall back to the default.</param>
    /// <param name="options">Failure policy and circuit breaker settings.</param>
    /// <param name="eventPublisher">Optional publisher for <see cref="RateLimitStoreFailureEvent"/> notifications.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/>, <paramref name="logger"/>, or <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="options"/> contains invalid circuit breaker values.</exception>
    public RateLimitingService(
        IRateLimitRepository repository,
        ILogger<RateLimitingService> logger,
        double clockSkewToleranceSeconds,
        RateLimitingOptions options,
        IEventPublisher? eventPublisher)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        _repository = repository;
        _logger = logger;
        _clockSkewToleranceSeconds = clockSkewToleranceSeconds >= 0 ? clockSkewToleranceSeconds : DefaultClockSkewToleranceSeconds;
        _options = options;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Checks if a request is allowed under the rate limit
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>True if the request is allowed; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or whitespace.</exception>
    /// <exception cref="RateLimitExceededException">Thrown when the key has exhausted its rate limit window.</exception>
    /// <exception cref="KeyStoreUnavailableException">Thrown when the limit store is unavailable and the failure policy is <see cref="RateLimitFailurePolicy.FailClosed"/>.</exception>
    public async Task<bool> CheckLimitAsync(string apiKeyId)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            throw new ArgumentException("API Key ID cannot be empty", nameof(apiKeyId));

        if (IsCircuitOpen())
            return await ApplyFailurePolicyAsync(apiKeyId, nameof(CheckLimitAsync), circuitOpen: true, error: null);

        RateLimit? rateLimit;
        try
        {
            rateLimit = await _repository.GetByApiKeyIdAsync(apiKeyId);
        }
        catch (DataAccessException ex)
        {
            RecordStoreFailure();
            return await ApplyFailurePolicyAsync(apiKeyId, nameof(CheckLimitAsync), circuitOpen: false, error: ex);
        }

        ResetStoreFailures();

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
    /// <exception cref="KeyStoreUnavailableException">Thrown when the limit store is unavailable and the failure policy is <see cref="RateLimitFailurePolicy.FailClosed"/>.</exception>
    public async Task RecordRequestAsync(string apiKeyId)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            return;

        if (IsCircuitOpen())
        {
            await ApplyFailurePolicyAsync(apiKeyId, nameof(RecordRequestAsync), circuitOpen: true, error: null);
            return;
        }

        RateLimit? rateLimit;
        try
        {
            rateLimit = await _repository.GetByApiKeyIdAsync(apiKeyId);
            if (rateLimit == null)
            {
                ResetStoreFailures();
                return;
            }

            await CheckAndResetWindowAsync(rateLimit);
            rateLimit.RecordRequest();
            await _repository.UpdateAsync(rateLimit);
        }
        catch (DataAccessException ex)
        {
            RecordStoreFailure();
            await ApplyFailurePolicyAsync(apiKeyId, nameof(RecordRequestAsync), circuitOpen: false, error: ex);
            return;
        }

        ResetStoreFailures();

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
    /// Applies the configured <see cref="RateLimitFailurePolicy"/> for a limit-store
    /// failure: fail-open allows the request and publishes a warning event;
    /// fail-closed rejects with <see cref="KeyStoreUnavailableException"/> (mapped to 503).
    /// </summary>
    private async Task<bool> ApplyFailurePolicyAsync(string apiKeyId, string operation, bool circuitOpen, DataAccessException? error)
    {
        if (circuitOpen)
            _logger.LogWarning("Rate limit store circuit open; skipping store for API key {ApiKeyId} ({Operation})", apiKeyId, operation);
        else
            _logger.LogError(error, "Rate limit store failure for API key {ApiKeyId} ({Operation})", apiKeyId, operation);

        if (_options.FailurePolicy == RateLimitFailurePolicy.FailClosed)
        {
            throw new KeyStoreUnavailableException(
                "Rate limit store is unavailable and the gateway is configured to fail closed.",
                operation,
                error ?? new DataAccessException("Rate limit store circuit breaker is open", operation, nameof(RateLimit)));
        }

        await PublishFailureEventAsync(apiKeyId, operation, circuitOpen, error);
        return true;
    }

    private async Task PublishFailureEventAsync(string apiKeyId, string operation, bool circuitOpen, DataAccessException? error)
    {
        if (_eventPublisher == null)
            return;

        try
        {
            await _eventPublisher.PublishAsync(new RateLimitStoreFailureEvent
            {
                ApiKeyId = apiKeyId,
                Operation = operation,
                PolicyApplied = _options.FailurePolicy,
                CircuitOpen = circuitOpen,
                ErrorMessage = error?.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish rate limit store failure event for API key {ApiKeyId}", apiKeyId);
        }
    }

    private bool IsCircuitOpen()
    {
        lock (_breakerLock)
        {
            return DateTime.UtcNow < _circuitOpenUntilUtc;
        }
    }

    private void RecordStoreFailure()
    {
        lock (_breakerLock)
        {
            var now = DateTime.UtcNow;

            // Failures spaced further apart than the window restart the count.
            if (_consecutiveStoreFailures == 0 || now - _firstFailureAtUtc > _options.CircuitBreakerFailureWindow)
            {
                _consecutiveStoreFailures = 0;
                _firstFailureAtUtc = now;
            }

            _consecutiveStoreFailures++;

            if (_consecutiveStoreFailures >= _options.CircuitBreakerFailureThreshold)
            {
                _circuitOpenUntilUtc = now + _options.CircuitBreakerCooldown;
                _consecutiveStoreFailures = 0;
                _logger.LogWarning(
                    "Rate limit store circuit breaker opened for {Cooldown} after {Threshold} consecutive failures",
                    _options.CircuitBreakerCooldown, _options.CircuitBreakerFailureThreshold);
            }
        }
    }

    private void ResetStoreFailures()
    {
        lock (_breakerLock)
        {
            _consecutiveStoreFailures = 0;
        }
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
