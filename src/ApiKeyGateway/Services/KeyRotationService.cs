// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Events;

namespace ApiKeyGateway.Services;

/// <summary>
/// High-level key rotation orchestrator that coordinates rotation policies,
/// notifications, and grace-period management on top of <see cref="IApiKeyRotationService"/>.
/// </summary>
public interface IKeyRotationService
{
    /// <summary>
    /// Evaluates all active keys and rotates those whose age exceeds the configured policy.
    /// </summary>
    Task<KeyRotationReport> RotateExpiredKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules a future rotation for a specific key.
    /// </summary>
    Task ScheduleRotationAsync(string apiKeyId, DateTime rotateAt);

    /// <summary>
    /// Returns the rotation policy for a given consumer.
    /// </summary>
    KeyRotationPolicy GetPolicy(string consumerId);

    /// <summary>
    /// Updates the rotation policy for a consumer.
    /// </summary>
    void SetPolicy(string consumerId, KeyRotationPolicy policy);
}

/// <summary>
/// Policy that controls automatic key rotation behavior.
/// </summary>
public class KeyRotationPolicy
{
    /// <summary>Maximum age of a key before automatic rotation</summary>
    public TimeSpan MaxKeyAge { get; set; } = TimeSpan.FromDays(90);

    /// <summary>Grace period during which the old key remains valid after rotation</summary>
    public TimeSpan GracePeriod { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Whether to send notifications before rotation</summary>
    public bool NotifyBeforeRotation { get; set; } = true;

    /// <summary>How far in advance to send the rotation warning</summary>
    public TimeSpan NotificationLeadTime { get; set; } = TimeSpan.FromDays(7);
}

/// <summary>
/// Summary of a bulk rotation run.
/// </summary>
public class KeyRotationReport
{
    /// <summary>Number of keys evaluated</summary>
    public int KeysEvaluated { get; init; }

    /// <summary>Number of keys successfully rotated</summary>
    public int KeysRotated { get; init; }

    /// <summary>Number of keys that failed rotation</summary>
    public int KeysFailed { get; init; }

    /// <summary>Individual rotation results</summary>
    public List<RotationResult> Results { get; init; } = new();

    /// <summary>When the rotation run started</summary>
    public DateTime StartedAt { get; init; }

    /// <summary>When the rotation run completed</summary>
    public DateTime CompletedAt { get; init; }
}

/// <inheritdoc cref="IKeyRotationService"/>
public class KeyRotationService : IKeyRotationService
{
    private readonly IApiKeyRotationService _rotationService;
    private readonly IApiKeyService _apiKeyService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<KeyRotationService> _logger;
    private readonly ConcurrentDictionary<string, KeyRotationPolicy> _policies = new();
    private readonly ConcurrentDictionary<string, DateTime> _scheduledRotations = new();

    public KeyRotationService(
        IApiKeyRotationService rotationService,
        IApiKeyService apiKeyService,
        IEventPublisher eventPublisher,
        ILogger<KeyRotationService> logger)
    {
        _rotationService = rotationService ?? throw new ArgumentNullException(nameof(rotationService));
        _apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<KeyRotationReport> RotateExpiredKeysAsync(CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;
        var results = new List<RotationResult>();
        // Check keys expiring within the max policy window (default 90 days lookahead)
        var expiringKeys = await _apiKeyService.GetExpiringKeysAsync(TimeSpan.FromDays(365));
        var keysEvaluated = 0;

        foreach (var key in expiringKeys)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            keysEvaluated++;
            var policy = GetPolicy(key.ConsumerId);

            var isScheduled = _scheduledRotations.TryGetValue(key.Id, out var scheduledAt)
                              && scheduledAt <= DateTime.UtcNow;

            var isExpiredByPolicy = key.CreatedAt.Add(policy.MaxKeyAge) <= DateTime.UtcNow;

            if (!isScheduled && !isExpiredByPolicy)
                continue;

            try
            {
                var result = await _rotationService.RotateKeyAsync(
                    key.Id,
                    (int?)policy.GracePeriod.TotalDays);

                results.Add(result);
                _scheduledRotations.TryRemove(key.Id, out _);

                if (result.Success)
                {
                    _logger.LogInformation("Rotated key {OldKeyId} -> {NewKeyId} for consumer {ConsumerId}",
                        result.OldKeyId, result.NewKeyId, result.ConsumerId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rotate key {KeyId}", key.Id);
                results.Add(new RotationResult
                {
                    OldKeyId = key.Id,
                    ConsumerId = key.ConsumerId,
                    Success = false,
                    FailureReason = ex.Message
                });
            }
        }

        var report = new KeyRotationReport
        {
            KeysEvaluated = keysEvaluated,
            KeysRotated = results.Count(r => r.Success),
            KeysFailed = results.Count(r => !r.Success),
            Results = results,
            StartedAt = startedAt,
            CompletedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Rotation run complete: {Evaluated} evaluated, {Rotated} rotated, {Failed} failed",
            report.KeysEvaluated, report.KeysRotated, report.KeysFailed);

        return report;
    }

    /// <inheritdoc/>
    public Task ScheduleRotationAsync(string apiKeyId, DateTime rotateAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKeyId);

        _scheduledRotations[apiKeyId] = rotateAt;
        _logger.LogInformation("Scheduled rotation for key {ApiKeyId} at {RotateAt}", apiKeyId, rotateAt);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public KeyRotationPolicy GetPolicy(string consumerId)
    {
        return _policies.GetOrAdd(consumerId, _ => new KeyRotationPolicy());
    }

    /// <inheritdoc/>
    public void SetPolicy(string consumerId, KeyRotationPolicy policy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerId);
        ArgumentNullException.ThrowIfNull(policy);

        _policies[consumerId] = policy;
        _logger.LogInformation("Updated rotation policy for consumer {ConsumerId}: MaxAge={MaxAge}, Grace={Grace}",
            consumerId, policy.MaxKeyAge, policy.GracePeriod);
    }
}
