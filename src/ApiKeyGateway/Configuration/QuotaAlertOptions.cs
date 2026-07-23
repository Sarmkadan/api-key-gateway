// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Options controlling proactive quota threshold alerting.
/// Bound from the "QuotaAlerts" configuration section.
/// </summary>
public class QuotaAlertOptions
{
    /// <summary>Configuration section name the options are bound from.</summary>
    public const string SectionName = "QuotaAlerts";

    /// <summary>
    /// Whether quota threshold alerting is enabled. Defaults to <see langword="true"/>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Percentage thresholds (0-100] of the quota limit at which a
    /// <see cref="Events.QuotaThresholdReachedEvent"/> is published.
    /// Defaults to 80% and 100%.
    /// </summary>
    public List<int> ThresholdPercentages { get; set; } = [80, 100];

    /// <summary>
    /// Returns the configured thresholds sanitized for evaluation:
    /// only values in (0, 100], de-duplicated and sorted ascending.
    /// </summary>
    /// <returns>Sorted list of valid threshold percentages.</returns>
    public IReadOnlyList<int> GetEffectiveThresholds() =>
        (ThresholdPercentages ?? [])
            .Where(t => t is > 0 and <= 100)
            .Distinct()
            .Order()
            .ToList();
}
