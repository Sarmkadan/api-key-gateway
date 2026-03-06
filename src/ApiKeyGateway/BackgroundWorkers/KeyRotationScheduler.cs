// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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

    public KeyRotationScheduler(
        IServiceProvider serviceProvider,
        ILogger<KeyRotationScheduler> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var section = configuration.GetSection("KeyRotation");
        _checkInterval = TimeSpan.FromHours(section.GetValue("CheckIntervalHours", 24));
        _warningDays = section.GetValue("WarningDays", 7);

        var newTtl = section.GetValue<int?>("NewExpirationDays", null);
        _newExpirationDays = newTtl > 0 ? newTtl : null;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Key rotation scheduler started (interval: {Interval}, warning window: {Days} days)",
            _checkInterval, _warningDays);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunRotationCycleAsync(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
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
            succeeded, failed);

        foreach (var failure in results.Where(r => !r.Success))
        {
            _logger.LogWarning(
                "Failed to rotate key {OldKeyId} for consumer {ConsumerId}: {Reason}",
                failure.OldKeyId, failure.ConsumerId, failure.FailureReason);
        }
    }
}
