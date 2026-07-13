// =============================================================================
// Author: 
// =============================================================================

using ApiKeyGateway.Domain.Models;
using System.Globalization;

namespace ApiKeyGateway.Services;

/// <summary>
/// Provides extension methods for <see cref="AnalyticsSummary"/>.
/// </summary>
public static class AnalyticsSummaryExtensions
{
    /// <summary>
    /// Gets a human-readable string representation of rate-limited endpoint call attempts.
    /// </summary>
    /// <param name="summary">The analytics summary.</param>
    /// <returns>An object representation rate-limited calls.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="summary"/> is null.</exception>
    public static string GetRateLimitedSummary(this AnalyticsSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return $"Total: {summary.TotalRequests}, Success: {summary.SuccessfulRequests}, Failure: {summary.FailedRequests}";
    }

    /// <summary>
    /// Formats <see cref="AnalyticsSummary.AverageResponseTimeMs"/> to a string representation for logging.
    /// </summary>
    /// <param name="summary">The analytics summary.</param>
    /// <param name="format">Optional; format string.</param>
    /// <returns>A formatted string; null if <paramref name="summary"/> is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="summary"/> is null.</exception>
    public static string FormatAverageResponseTime(this AnalyticsSummary summary, string? format = null)
    {
        if (summary is null) return string.Empty;
        
        return summary.AverageResponseTimeMs.ToString(format ?? "F2", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats <see cref="AnalyticsSummary.SuccessRatePercent"/> to a string representation for logging.
    /// </summary>
    /// <param name="summary">The analytics summary.</param>
    /// <param name="format">Optional; format string.</param>
    /// <returns>A formatted string; null if <paramref name="summary"/> is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="summary"/> is null.</exception>
    public static string FormatSuccessRatePercent(this AnalyticsSummary summary, string? format = null)
    {
        if (summary is null) return string.Empty;
        
        return summary.SuccessRatePercent.ToString(format ?? "F2", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats <see cref="AnalyticsSummary.ErrorRatePercent"/> to a string representation for logging.
    /// </summary>
    /// <param name="summary">The analytics summary.</param>
    /// <param name="format">Optional; format string.</param>
    /// <returns>A formatted string; null if <paramref name="summary"/> is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="summary"/> is null.</exception>
    public static string FormatErrorRatePercent(this AnalyticsSummary summary, string? format = null)
    {
        if (summary is null) return string.Empty;
        
        return summary.ErrorRatePercent.ToString(format ?? "F2", CultureInfo.InvariantCulture);
    }
}
