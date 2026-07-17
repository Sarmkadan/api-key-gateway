// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace ApiKeyGateway.Services;

/// <summary>
/// Provides extension methods for formatting and summarizing <see cref="AnalyticsSummary"/> instances.
/// </summary>
public static class AnalyticsSummaryExtensions
{
    /// <summary>
    /// Formats the analytics summary as a human-readable string showing request statistics.
    /// </summary>
    /// <param name="summary">The analytics summary to format.</param>
    /// <returns>A formatted string representation of the analytics summary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="summary"/> is null.</exception>
    public static string ToSummaryString(this AnalyticsSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return $"API Key: {summary.ApiKeyId}, Period: {summary.From:yyyy-MM-dd} to {summary.To:yyyy-MM-dd}, " +
               $"Requests: {summary.TotalRequests} (Success: {summary.SuccessfulRequests}, Failed: {summary.FailedRequests}), " +
               $"Success Rate: {summary.SuccessRatePercent:F2}%, Error Rate: {summary.ErrorRatePercent:F2}%, " +
               $"Avg Response Time: {summary.AverageResponseTimeMs:F2}ms";
    }

    /// <summary>
    /// Formats <see cref="AnalyticsSummary.AverageResponseTimeMs"/> to a string representation for logging.
    /// </summary>
    /// <param name="summary">The analytics summary containing the response time to format.</param>
    /// <param name="format">Optional format string; defaults to "F2" for 2 decimal places.</param>
    /// <returns>A formatted string representation of the average response time; "0.00" if <paramref name="summary"/> is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="summary"/> is null.</exception>
    public static string FormatAverageResponseTime(this AnalyticsSummary summary, string? format = null)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return summary.AverageResponseTimeMs.ToString(format ?? "F2", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats <see cref="AnalyticsSummary.SuccessRatePercent"/> to a string representation for logging.
    /// </summary>
    /// <param name="summary">The analytics summary containing the success rate to format.</param>
    /// <param name="format">Optional format string; defaults to "F2" for 2 decimal places.</param>
    /// <returns>A formatted string representation of the success rate percentage; "0.00" if <paramref name="summary"/> is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="summary"/> is null.</exception>
    public static string FormatSuccessRatePercent(this AnalyticsSummary summary, string? format = null)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return summary.SuccessRatePercent.ToString(format ?? "F2", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats <see cref="AnalyticsSummary.ErrorRatePercent"/> to a string representation for logging.
    /// </summary>
    /// <param name="summary">The analytics summary containing the error rate to format.</param>
    /// <param name="format">Optional format string; defaults to "F2" for 2 decimal places.</param>
    /// <returns>A formatted string representation of the error rate percentage; "0.00" if <paramref name="summary"/> is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="summary"/> is null.</exception>
    public static string FormatErrorRatePercent(this AnalyticsSummary summary, string? format = null)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return summary.ErrorRatePercent.ToString(format ?? "F2", CultureInfo.InvariantCulture);
    }
}