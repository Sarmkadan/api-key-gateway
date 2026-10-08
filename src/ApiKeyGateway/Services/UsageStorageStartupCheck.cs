// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Configuration;
using ApiKeyGateway.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace ApiKeyGateway.Services;

/// <summary>
/// Pings the usage store once when the host starts. If the store is unreachable the host
/// fails to start with a <see cref="StorageUnavailableException"/> naming the provider,
/// instead of the first request failing later. Disabled by
/// <see cref="UsageTrackingOptions.SkipStartupCheck"/>.
/// </summary>
public sealed class UsageStorageStartupCheck : IHostedService
{
    private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(10);

    private readonly IUsageStorageProbe _probe;
    private readonly IOptions<UsageTrackingOptions> _options;
    private readonly ILogger<UsageStorageStartupCheck> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="UsageStorageStartupCheck"/>.
    /// </summary>
    /// <param name="probe">The probe used to ping the usage store.</param>
    /// <param name="options">The usage tracking options, including <see cref="UsageTrackingOptions.SkipStartupCheck"/>.</param>
    /// <param name="logger">Logger instance.</param>
    public UsageStorageStartupCheck(
        IUsageStorageProbe probe,
        IOptions<UsageTrackingOptions> options,
        ILogger<UsageStorageStartupCheck> logger)
    {
        _probe = probe ?? throw new ArgumentNullException(nameof(probe));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var provider = _probe.ProviderName;

        if (_options.Value.SkipStartupCheck)
        {
            _logger.LogWarning("Usage storage startup check is disabled; provider {Provider} will not be pinged", provider);
            return;
        }

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(PingTimeout);

        try
        {
            await _probe.PingAsync(timeoutSource.Token);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Usage storage provider {Provider} is unreachable", provider);
            throw new StorageUnavailableException(
                $"Usage storage provider '{provider}' is unavailable: {ex.Message}",
                provider,
                ex);
        }

        _logger.LogInformation("Usage storage provider {Provider} is reachable", provider);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
