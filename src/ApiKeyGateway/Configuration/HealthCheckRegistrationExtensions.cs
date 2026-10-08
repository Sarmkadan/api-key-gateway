// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Extension methods that register the gateway health check.
/// </summary>
public static class HealthCheckRegistrationExtensions
{
    /// <summary>Tag applied to the gateway health check; use it to filter readiness endpoints.</summary>
    public const string ReadyTag = "ready";

    /// <summary>Name the gateway health check is registered under.</summary>
    public const string HealthCheckName = "api-key-gateway";

    /// <summary>
    /// Adds the gateway health check (store reachability, usage flush queue, rate limiter backend),
    /// tagged <see cref="ReadyTag"/>.
    /// </summary>
    /// <param name="builder">The health checks builder, e.g. <c>services.AddHealthChecks()</c>.</param>
    /// <param name="configure">Optional override of <see cref="ApiKeyGatewayHealthCheckOptions"/>.</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static IHealthChecksBuilder AddApiKeyGatewayHealthCheck(
        this IHealthChecksBuilder builder,
        Action<ApiKeyGatewayHealthCheckOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.Configure<ApiKeyGatewayHealthCheckOptions>(options => configure?.Invoke(options));

        return builder.AddCheck<ApiKeyGatewayHealthCheck>(
            HealthCheckName,
            failureStatus: null,
            tags: [ReadyTag]);
    }
}
