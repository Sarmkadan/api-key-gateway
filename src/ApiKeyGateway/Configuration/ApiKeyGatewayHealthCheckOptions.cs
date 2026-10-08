// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Thresholds for the gateway health check registered by
/// <c>AddApiKeyGatewayHealthCheck</c>.
/// </summary>
public class ApiKeyGatewayHealthCheckOptions
{
    /// <summary>
    /// Number of tracked requests waiting to be flushed to the usage store above which the
    /// health check reports Degraded. Defaults to 10,000, the buffered tracker's channel capacity.
    /// </summary>
    public long UsageFlushQueueDegradedThreshold { get; set; } = 10_000;
}
