// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Configuration;
using ApiKeyGateway.Data;
using ApiKeyGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace ApiKeyGateway.Diagnostics;

/// <summary>
/// Reports whether the gateway can serve traffic: API key store reachability, the pending
/// usage flush queue in <see cref="IUsageTracker"/>, and the rate limiter backend.
/// Registered with <c>AddApiKeyGatewayHealthCheck</c>; the check is tagged <c>ready</c>.
/// </summary>
/// <remarks>
/// Components are resolved from a fresh scope on each run. Store and provider error
/// messages are logged, not returned, because health endpoints are anonymous.
/// </remarks>
public sealed class ApiKeyGatewayHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ApiKeyGatewayHealthCheckOptions _options;
    private readonly ILogger<ApiKeyGatewayHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ApiKeyGatewayHealthCheck"/>.
    /// </summary>
    /// <param name="scopeFactory">Creates the scope used to resolve per-request services.</param>
    /// <param name="options">Health check thresholds.</param>
    /// <param name="logger">Logger for probe failures.</param>
    public ApiKeyGatewayHealthCheck(
        IServiceScopeFactory scopeFactory,
        IOptions<ApiKeyGatewayHealthCheckOptions> options,
        ILogger<ApiKeyGatewayHealthCheck> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var data = new Dictionary<string, object>();

        var storeReachable = await CheckStoreAsync(services, data);
        var flushQueueBacklogged = CheckUsageFlushQueue(services, data);
        CheckRateLimiterBackend(services, data);

        if (!storeReachable)
        {
            return HealthCheckResult.Unhealthy("API key store is unreachable", data: data);
        }

        if (flushQueueBacklogged)
        {
            return HealthCheckResult.Degraded("Usage flush queue is above its threshold", data: data);
        }

        return HealthCheckResult.Healthy("Gateway is ready", data: data);
    }

    private async Task<bool> CheckStoreAsync(IServiceProvider services, Dictionary<string, object> data)
    {
        try
        {
            using var connection = services.GetRequiredService<IDbConnection>();
            await connection.OpenAsync();
            await connection.CloseAsync();

            data["store"] = "reachable";
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API key store health probe failed");
            data["store"] = "unreachable";
            return false;
        }
    }

    private bool CheckUsageFlushQueue(IServiceProvider services, Dictionary<string, object> data)
    {
        var tracker = services.GetService<IUsageTracker>();
        if (tracker is null)
        {
            data["usageTracker"] = "not_registered";
            return false;
        }

        var pending = tracker.GetPendingRequestCount();
        var backlogged = pending > _options.UsageFlushQueueDegradedThreshold;

        data["usageTracker"] = backlogged ? "backlogged" : "ok";
        data["usageFlushPendingRequests"] = pending;
        data["usageFlushQueueThreshold"] = _options.UsageFlushQueueDegradedThreshold;
        return backlogged;
    }

    private static void CheckRateLimiterBackend(IServiceProvider services, Dictionary<string, object> data)
    {
        // The in-memory limiter has no remote dependency, so its backend is reported rather than probed.
        data["rateLimiterBackend"] = services.GetService<IRateLimiter>() is null ? "not_registered" : "in_memory";
    }
}
