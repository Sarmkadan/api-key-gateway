// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Background worker that automatically rotates API keys nearing expiration.
// Runs on a daily cadence and delegates rotation logic to
// <see cref="IApiKeyRotationService"/>. The look-ahead window and replacement
// key TTL are configurable via appsettings under the <c>KeyRotation</c> section.
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Services;

namespace ApiKeyGateway.BackgroundWorkers;

/// <summary>
/// Background worker that automatically rotates API keys nearing expiration.
/// Runs on a daily cadence and delegates rotation logic to
/// <see cref="IApiKeyRotationService"/>. The look-ahead window and replacement
/// key TTL are configurable via appsettings under the <c>KeyRotation</c> section.
/// </summary>
public sealed class KeyRotationScheduler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KeyRotationScheduler> _logger;
    private readonly TimeSpan _checkInterval;
    private readonly int _warningDays;
    private readonly int? _newExpirationDays;
    private readonly double _jitterPercentage; // 0.0 = no jitter, 0.1 = 10% default
    private static readonly Random _random = new();

    public KeyRotationScheduler(
        IServiceProvider serviceProvider,
        ILogger<KeyRotationScheduler> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        var section = configuration.GetSection("KeyRotation");
        _checkInterval = TimeSpan.FromHours(section.GetValue("CheckIntervalHours", 24));
        _warningDays = section.GetValue("WarningDays", 7);

        var newTtl = section.GetValue<int?>("NewExpirationDays", null);
        _newExpirationDays = newTtl > 0 ? newTtl : null;

        // Jitter percentage is optional; default to 10% (0.1)
        _jitterPercentage = section.GetValue<double?>("JitterPercentage", 0.1) ?? 0.1;
        if (_jitterPercentage < 0 || _jitterPercentage > 1)
            throw new ConfigurationException("Jitter percentage must be between 0 and 1", "KeyRotation:JitterPercentage");

        if (_checkInterval <= TimeSpan.Zero)
            throw new ConfigurationException("Check interval must be positive", "KeyRotation:CheckIntervalHours");

        if (_warningDays <= 0)
            throw new ConfigurationException("Warning days must be positive", "KeyRotation:WarningDays");
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Key rotation scheduler started (interval: {Interval}, warning window: {Days} days, jitter: {Jitter:P})",
            _checkInterval,
            _warningDays,
            _jitterPercentage);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunRotationCycleAsync(stoppingToken);
                var delay = ApplyJitter(_checkInterval);
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Key rotation scheduler shutting down");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in key rotation scheduler");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task RunRotationCycleAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var rotationService = scope.ServiceProvider.GetRequiredService<IApiKeyRotationService>();

        _logger.LogDebug("Starting scheduled key rotation cycle");

        var results = await rotationService.RotateExpiringSoonAsync(_warningDays, _newExpirationDays);

        if (results.Count == 0)
        {
            _logger.LogDebug("No keys require rotation at this time");
            return;
        }

        var succeeded = results.Count(r => r.Success);
        var failed = results.Count(r => !r.Success);

        _logger.LogInformation(
            "Rotation cycle complete: {Succeeded} rotated, {Failed} failed",
            succeeded,
            failed);

        foreach (var failure in results.Where(r => !r.Success))
        {
            _logger.LogWarning(
                "Failed to rotate key {OldKeyId} for consumer {ConsumerId}: {Reason}",
                failure.OldKeyId,
                failure.ConsumerId,
                failure.FailureReason);
        }
    }

    /// <summary>
    /// Applies a random jitter to the provided interval based on the configured jitter percentage.
    /// When the jitter percentage is 0, the original interval is returned unchanged.
    /// </summary>
    /// <param name="interval">Base interval.</param>
    /// <returns>Interval with jitter applied.</returns>
    private TimeSpan ApplyJitter(TimeSpan interval)
    {
        if (_jitterPercentage <= 0)
            return interval;

        // Compute a factor between (1 - jitter) and (1 + jitter)
        double factor;
        lock (_random)
        {
            factor = 1.0 + (_random.NextDouble() * 2 - 1) * _jitterPercentage;
        }

        var jitteredTicks = (long)(interval.Ticks * factor);
        // Ensure we never produce a non‑positive delay
        if (jitteredTicks < 1)
            jitteredTicks = 1;

        return TimeSpan.FromTicks(jitteredTicks);
    }
}
