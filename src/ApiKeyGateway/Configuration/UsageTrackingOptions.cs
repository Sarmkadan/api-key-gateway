// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Options controlling usage tracking storage: which provider persists usage records,
/// how often buffered records are flushed, how long they are retained, and whether the
/// store is pinged at startup.
/// </summary>
public class UsageTrackingOptions
{
    /// <summary>Configuration section name the options are bound from.</summary>
    public const string SectionName = "UsageTracking";

    /// <summary>Storage providers the gateway can persist usage records to.</summary>
    public static readonly IReadOnlyList<string> SupportedStoreProviders = ["SqlServer"];

    /// <summary>
    /// Storage provider that persists usage records. Must be one of
    /// <see cref="SupportedStoreProviders"/> (case-insensitive). Defaults to "SqlServer".
    /// </summary>
    public string StoreProvider { get; set; } = "SqlServer";

    /// <summary>
    /// Maximum time buffered usage records wait before being written to the store.
    /// Must be greater than zero.
    /// </summary>
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Number of days usage records are kept. Must be greater than zero and cover at least
    /// one <see cref="FlushInterval"/>.
    /// </summary>
    public int RetentionDays { get; set; } = 90;

    /// <summary>
    /// When true, the gateway does not ping the usage store at startup. The first request
    /// that touches the store then surfaces any connectivity problem instead.
    /// </summary>
    public bool SkipStartupCheck { get; set; } = false;
}
