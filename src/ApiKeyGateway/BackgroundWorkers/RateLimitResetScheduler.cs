// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Caching;

namespace ApiKeyGateway.BackgroundWorkers;

/// <summary>
/// Background worker that manages rate limit window resets.
/// Clears expired rate limit counters from cache at appropriate times.
/// This ensures that users get fresh quotas when their time windows reset.
/// Runs periodically to check and reset limits that have expired.
/// </summary>
public sealed class RateLimitResetScheduler : BackgroundServiceBase<RateLimitResetScheduler>
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public RateLimitResetScheduler(IServiceProvider serviceProvider, ILogger<RateLimitResetScheduler> logger)
        : base(serviceProvider, logger)
    {
    }

    /// <inheritdoc/>
    protected override async Task ExecuteCycleAsync(CancellationToken stoppingToken)
    {
        using var scope = CreateScope();
        var cacheProvider = scope.ServiceProvider.GetRequiredService<ICacheProvider>();

        // Clear any expired rate limit entries
        // This is proactive cleanup - in production, rely on cache TTL primarily
        var clearedCount = await cacheProvider.RemoveByPatternAsync(
            CacheKeyGenerator.GetRateLimitInvalidationPattern());

        if (clearedCount > 0)
        {
            _logger.LogInformation(
                "Rate limit reset scheduler cleared {Count} expired entries",
                clearedCount);
        }

        // Wait for next check cycle
        await Task.Delay(_checkInterval, stoppingToken);
    }

    protected override TimeSpan GetErrorDelay(CancellationToken stoppingToken)
    {
        return TimeSpan.FromSeconds(30);
    }
}
