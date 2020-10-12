// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Repositories;

namespace ApiKeyGateway.Events;

/// <summary>
/// Event handlers that react to domain events.
/// These handlers implement cross-cutting concerns like audit logging,
/// metrics collection, and webhook publishing triggered by key events.
/// Decouples event producers from consumers for better maintainability.
/// </summary>
public sealed class AuditLogEventHandler
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditLogEventHandler> _logger;

    public AuditLogEventHandler(IAuditLogRepository auditLogRepository, ILogger<AuditLogEventHandler> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    /// <summary>
    /// Logs API key creation events to audit trail.
    /// </summary>
    public async Task HandleApiKeyCreatedAsync(ApiKeyCreatedEvent @event)
    {
        _logger.LogInformation(
            "Recording audit: API key created {KeyId} by {CreatedBy}",
            @event.ApiKeyId,
            @event.CreatedBy);

        // In production, persist to audit log table
        await Task.CompletedTask;
    }

    /// <summary>
    /// Logs API key rotation events for compliance.
    /// </summary>
    public async Task HandleApiKeyRotatedAsync(ApiKeyRotatedEvent @event)
    {
        _logger.LogInformation(
            "Recording audit: API key rotated {KeyId} by {RotatedBy}",
            @event.ApiKeyId,
            @event.RotatedBy);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Logs API key disable events for security tracking.
    /// </summary>
    public async Task HandleApiKeyDisabledAsync(ApiKeyDisabledEvent @event)
    {
        _logger.LogWarning(
            "Recording audit: API key disabled {KeyId} by {DisabledBy} - Reason: {Reason}",
            @event.ApiKeyId,
            @event.DisabledBy,
            @event.Reason);

        await Task.CompletedTask;
    }
}

/// <summary>
/// Event handler for usage tracking events.
/// Collects metrics for billing, quotas, and analytics.
/// </summary>
public sealed class UsageTrackingEventHandler
{
    private readonly ILogger<UsageTrackingEventHandler> _logger;

    public UsageTrackingEventHandler(ILogger<UsageTrackingEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Records successful API usage for billing and analytics.
    /// </summary>
    public async Task HandleApiKeyUsedAsync(ApiKeyUsedEvent @event)
    {
        _logger.LogDebug(
            "Usage recorded: {ApiKeyId} {Endpoint} {StatusCode} {TimeMs}ms {SizeBytes}B",
            @event.ApiKeyId,
            @event.Endpoint,
            @event.HttpStatusCode,
            @event.ResponseTimeMs,
            @event.ResponseSizeBytes);

        // In production, update usage counters for quota tracking
        await Task.CompletedTask;
    }

    /// <summary>
    /// Alerts on quota exhaustion events.
    /// </summary>
    public async Task HandleQuotaExhaustedAsync(QuotaExhaustedEvent @event)
    {
        _logger.LogError(
            "CRITICAL: Quota exhausted for {ApiKeyId} - {Limit} requests, reset at {ResetTime}",
            @event.ApiKeyId,
            @event.Limit,
            @event.WindowResetTime);

        // In production:
        // - Send alert email to key owner
        // - Create incident in monitoring system
        // - Notify admins if key is critical
        await Task.CompletedTask;
    }

    /// <summary>
    /// Warns when approaching quota limits.
    /// </summary>
    public async Task HandleUsageWarningAsync(UsageWarningEvent @event)
    {
        _logger.LogWarning(
            "Usage warning: {ApiKeyId} at {Percentage}% of quota ({Current}/{Limit})",
            @event.ApiKeyId,
            @event.PercentageUsed,
            @event.CurrentUsage,
            @event.Limit);

        // In production:
        // - Send notification to API key owner
        // - Update dashboard
        await Task.CompletedTask;
    }
}

/// <summary>
/// Event handler for rate limiting events.
/// Tracks and alerts on rate limit violations.
/// </summary>
public sealed class RateLimitEventHandler
{
    private readonly ILogger<RateLimitEventHandler> _logger;

    public RateLimitEventHandler(ILogger<RateLimitEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles rate limit exceeded events.
    /// </summary>
    public async Task HandleRateLimitExceededAsync(RateLimitExceededEvent @event)
    {
        _logger.LogWarning(
            "Rate limit exceeded: {ApiKeyId} {Endpoint} - {Current}/{Limit}, resets in {SecondsUntilReset}s",
            @event.ApiKeyId,
            @event.Endpoint,
            @event.CurrentUsage,
            @event.Limit,
            @event.SecondsUntilReset);

        // In production:
        // - Increment rate limit violation counter for metrics
        // - Check if key should be temporarily disabled
        // - Alert if patterns indicate abuse
        await Task.CompletedTask;
    }
}
