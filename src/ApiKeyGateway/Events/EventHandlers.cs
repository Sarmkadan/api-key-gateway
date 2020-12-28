// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Services;

namespace ApiKeyGateway.Events;

/// <summary>
/// Event handlers that react to domain events.
/// These handlers implement cross-cutting concerns like audit logging,
/// metrics collection, and webhook publishing triggered by key events.
/// Decouples event producers from consumers for better maintainability.
/// </summary>
/// <remarks>
/// Handlers are registered as singletons and subscribed once at startup, so they
/// resolve scoped dependencies (repositories, tracking services) through
/// <see cref="IServiceScopeFactory"/> per event instead of capturing them at
/// construction time.
/// </remarks>
public sealed class AuditLogEventHandler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogEventHandler> _logger;

    public AuditLogEventHandler(IServiceScopeFactory scopeFactory, ILogger<AuditLogEventHandler> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs API key creation events to audit trail.
    /// </summary>
    public async Task HandleApiKeyCreatedAsync(ApiKeyCreatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogInformation(
            "Recording audit: API key created {KeyId} by {CreatedBy}",
            @event.ApiKeyId,
            @event.CreatedBy);

        await PersistAsync(new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = @event.ApiKeyId,
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            PerformedBy = @event.CreatedBy,
            Reason = $"API key '{@event.Name}' created"
        });
    }

    /// <summary>
    /// Logs API key rotation events for compliance.
    /// </summary>
    public async Task HandleApiKeyRotatedAsync(ApiKeyRotatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogInformation(
            "Recording audit: API key rotated {KeyId} by {RotatedBy}",
            @event.ApiKeyId,
            @event.RotatedBy);

        await PersistAsync(new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = @event.ApiKeyId,
            ResourceType = "ApiKey",
            Action = AuditAction.KeyRevoked,
            PerformedBy = @event.RotatedBy,
            Reason = "API key rotated: previous secret revoked and replaced"
        });
    }

    /// <summary>
    /// Logs API key disable events for security tracking.
    /// </summary>
    public async Task HandleApiKeyDisabledAsync(ApiKeyDisabledEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogWarning(
            "Recording audit: API key disabled {KeyId} by {DisabledBy} - Reason: {Reason}",
            @event.ApiKeyId,
            @event.DisabledBy,
            @event.Reason);

        await PersistAsync(new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = @event.ApiKeyId,
            ResourceType = "ApiKey",
            Action = AuditAction.KeyDisabled,
            PerformedBy = @event.DisabledBy,
            Reason = @event.Reason
        });
    }

    private async Task PersistAsync(AuditLog entry)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
        await repository.CreateAsync(entry);
    }
}

/// <summary>
/// Event handler for usage tracking events.
/// Collects metrics for billing, quotas, and analytics.
/// </summary>
public sealed class UsageTrackingEventHandler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMetricsCollectionService _metrics;
    private readonly ILogger<UsageTrackingEventHandler> _logger;

    public UsageTrackingEventHandler(
        IServiceScopeFactory scopeFactory,
        IMetricsCollectionService metrics,
        ILogger<UsageTrackingEventHandler> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Records successful API usage for billing and analytics.
    /// </summary>
    public async Task HandleApiKeyUsedAsync(ApiKeyUsedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogDebug(
            "Usage recorded: {ApiKeyId} {Endpoint} {StatusCode} {TimeMs}ms {SizeBytes}B",
            @event.ApiKeyId,
            @event.Endpoint,
            @event.HttpStatusCode,
            @event.ResponseTimeMs,
            @event.ResponseSizeBytes);

        _metrics.RecordRequest(
            @event.ApiKeyId,
            @event.Endpoint,
            @event.HttpStatusCode,
            @event.ResponseTimeMs);

        using var scope = _scopeFactory.CreateScope();
        var usageTracking = scope.ServiceProvider.GetRequiredService<IUsageTrackingService>();
        await usageTracking.RecordUsageAsync(new UsageRecord
        {
            Id = Guid.NewGuid().ToString(),
            ApiKeyId = @event.ApiKeyId,
            RecordedAt = @event.Timestamp,
            Endpoint = @event.Endpoint,
            ResponseStatusCode = @event.HttpStatusCode,
            ResponseBytes = @event.ResponseSizeBytes,
            ResponseTimeMs = (int)Math.Clamp(@event.ResponseTimeMs, 0, int.MaxValue)
        });
    }

    /// <summary>
    /// Alerts on quota exhaustion events.
    /// </summary>
    public async Task HandleQuotaExhaustedAsync(QuotaExhaustedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogError(
            "CRITICAL: Quota exhausted for {ApiKeyId} - {Limit} requests, reset at {ResetTime}",
            @event.ApiKeyId,
            @event.Limit,
            @event.WindowResetTime);

        _metrics.RecordError(@event.ApiKeyId, "QUOTA_EXHAUSTED");

        using var scope = _scopeFactory.CreateScope();
        var auditRepository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
        await auditRepository.CreateAsync(new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = @event.ApiKeyId,
            ResourceType = "ApiKey",
            Action = AuditAction.RateLimitExceeded,
            IsSuccess = false,
            Reason = $"Usage quota of {@event.Limit} requests exhausted; resets at {@event.WindowResetTime:O}"
        });
    }

    /// <summary>
    /// Warns when approaching quota limits.
    /// </summary>
    public Task HandleUsageWarningAsync(UsageWarningEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogWarning(
            "Usage warning: {ApiKeyId} at {Percentage}% of quota ({Current}/{Limit})",
            @event.ApiKeyId,
            @event.PercentageUsed,
            @event.CurrentUsage,
            @event.Limit);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Event handler for rate limiting events.
/// Tracks and alerts on rate limit violations.
/// </summary>
public sealed class RateLimitEventHandler
{
    private readonly IMetricsCollectionService _metrics;
    private readonly ILogger<RateLimitEventHandler> _logger;

    public RateLimitEventHandler(IMetricsCollectionService metrics, ILogger<RateLimitEventHandler> logger)
    {
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles rate limit exceeded events by recording the violation
    /// in the metrics counters used for monitoring and abuse detection.
    /// </summary>
    public Task HandleRateLimitExceededAsync(RateLimitExceededEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogWarning(
            "Rate limit exceeded: {ApiKeyId} {Endpoint} - {Current}/{Limit}, resets in {SecondsUntilReset}s",
            @event.ApiKeyId,
            @event.Endpoint,
            @event.CurrentUsage,
            @event.Limit,
            @event.SecondsUntilReset);

        _metrics.RecordRateLimitExceeded(@event.ApiKeyId);
        return Task.CompletedTask;
    }
}
