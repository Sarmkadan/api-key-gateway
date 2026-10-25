// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Options controlling the in-memory metrics collector: how many latency samples are kept
/// for average and p95 calculations before the oldest are discarded.
/// </summary>
public class MetricsOptions
{
    /// <summary>Configuration section name the options are bound from.</summary>
    public const string SectionName = "Metrics";

    /// <summary>
    /// Maximum number of latency samples retained in memory. When exceeded, the oldest
    /// samples are dropped so memory stays bounded. Must be greater than zero.
    /// Defaults to 100000.
    /// </summary>
    public int MaxLatencySamples { get; set; } = 100_000;
}
