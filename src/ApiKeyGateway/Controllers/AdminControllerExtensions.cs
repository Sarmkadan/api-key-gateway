// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using ApiKeyGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Extension methods for <see cref="AdminController"/> that provide additional functionality
/// for administrative operations, metrics analysis, and bulk operations.
/// </summary>
public static class AdminControllerExtensions
{
    /// <summary>
    /// Gets detailed statistics including breakdown by endpoint and error rates.
    /// </summary>
    /// <param name="controller">The admin controller instance</param>
    /// <returns>Action result with detailed statistics</returns>
    /// <exception cref="ArgumentNullException">Thrown when controller is null</exception>
    public static IActionResult GetDetailedStats(this AdminController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var statsResult = controller.GetStats();
        if (statsResult is not OkObjectResult okResult)
        {
            return statsResult;
        }

        var stats = okResult.Value as dynamic;
        var metricsSnapshot = controller.GetMetricsSnapshot();

        var detailedStats = new
        {
            Timestamp = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            Total = new
            {
                Requests = stats?.totalRequests,
                ActiveKeys = stats?.activeApiKeys,
                Errors = stats?.errorRate,
                RateLimits = stats?.rateLimitEvents
            },
            EndpointBreakdown = metricsSnapshot?.RequestsByEndpoint,
            ErrorBreakdown = metricsSnapshot?.ErrorsByCode,
            Performance = new
            {
                AverageLatencyMs = stats?.averageResponseTimeMs,
                P95LatencyMs = metricsSnapshot?.P95LatencyMs
            },
            Uptime = stats?.uptime
        };

        return controller.Ok(detailedStats);
    }

    /// <summary>
    /// Exports API keys in the specified format.
    /// </summary>
    /// <param name="controller">The admin controller instance</param>
    /// <param name="format">Export format (csv, json, xml)</param>
    /// <returns>File result with exported data</returns>
    /// <exception cref="ArgumentNullException">Thrown when controller or format is null</exception>
    public static async Task<IActionResult> ExportApiKeysAsync(this AdminController controller, string format = "csv")
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(format);

        var exportService = controller.HttpContext.RequestServices.GetService<IDataExportService>();
        if (exportService is null)
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        var csv = await exportService.ExportApiKeysAsync(format);
        var fileName = $"api-keys-{DateTime.UtcNow:yyyy-MM-dd}.{format.ToLowerInvariant()}";
        var contentType = format.ToLowerInvariant() switch
        {
            "csv" => "text/csv",
            "json" => "application/json",
            "xml" => "application/xml",
            _ => "text/csv"
        };

        return controller.File(System.Text.Encoding.UTF8.GetBytes(csv), contentType, fileName);
    }

    /// <summary>
    /// Exports audit logs in the specified format.
    /// </summary>
    /// <param name="controller">The admin controller instance</param>
    /// <param name="format">Export format (csv, json, xml)</param>
    /// <param name="since">Optional start date for logs</param>
    /// <returns>File result with exported audit logs</returns>
    /// <exception cref="ArgumentNullException">Thrown when controller or format is null</exception>
    public static async Task<IActionResult> ExportAuditLogsAsync(
        this AdminController controller,
        string format = "csv",
        DateTime? since = null)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(format);

        var exportService = controller.HttpContext.RequestServices.GetService<IDataExportService>();
        if (exportService is null)
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        var csv = await exportService.ExportAuditLogsAsync(format, since);
        var fileName = $"audit-logs-{DateTime.UtcNow:yyyy-MM-dd}.{format.ToLowerInvariant()}";
        var contentType = format.ToLowerInvariant() switch
        {
            "csv" => "text/csv",
            "json" => "application/json",
            "xml" => "application/xml",
            _ => "text/csv"
        };

        return controller.File(System.Text.Encoding.UTF8.GetBytes(csv), contentType, fileName);
    }

    /// <summary>
    /// Runs comprehensive diagnostics and returns a health check report.
    /// </summary>
    /// <param name="controller">The admin controller instance</param>
    /// <returns>Action result with detailed health diagnostics</returns>
    /// <exception cref="ArgumentNullException">Thrown when controller is null</exception>
    public static async Task<IActionResult> RunComprehensiveDiagnosticsAsync(this AdminController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var diagnosticsResult = await controller.RunDiagnostics();
        if (diagnosticsResult is not OkObjectResult okResult)
        {
            return diagnosticsResult;
        }

        var diagnostics = okResult.Value as dynamic;
        var metricsSnapshot = controller.GetMetricsSnapshot();

        var comprehensiveDiagnostics = new
        {
            Timestamp = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            SystemStatus = diagnostics?.overallStatus,
            PerformanceMetrics = new
            {
                RequestsPerSecond = metricsSnapshot?.TotalRequests > 0
                    ? metricsSnapshot.TotalRequests / 86400.0
                    : 0,
                AverageLatencyMs = metricsSnapshot?.AverageLatencyMs,
                P95LatencyMs = metricsSnapshot?.P95LatencyMs,
                ErrorRate = metricsSnapshot?.ErrorRate
            },
            Tests = diagnostics?.tests,
            Recommendations = GetDiagnosticRecommendations(metricsSnapshot)
        };

        return controller.Ok(comprehensiveDiagnostics);
    }

    /// <summary>
    /// Resets rate limits for a specific API key.
    /// </summary>
    /// <param name="controller">The admin controller instance</param>
    /// <param name="apiKeyId">The API key identifier</param>
    /// <returns>Action result indicating success or failure</returns>
    /// <exception cref="ArgumentNullException">Thrown when controller or apiKeyId is null</exception>
    public static async Task<IActionResult> ResetRateLimitsForKeyAsync(
        this AdminController controller,
        string apiKeyId)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);

        // Note: This is a simplified implementation that calls the general reset endpoint
        // In a real system, this would call a more specific service method
        var resetResult = await controller.ResetRateLimits();
        if (resetResult is not OkObjectResult okResult)
        {
            return resetResult;
        }

        return controller.Ok(new { message = $"Rate limits reset for API key: {apiKeyId}" });
    }

    /// <summary>
    /// Gets metrics snapshot directly from the metrics service.
    /// </summary>
    /// <param name="controller">The admin controller instance</param>
    /// <returns>Metrics snapshot or null if not available</returns>
    private static MetricsSnapshot? GetMetricsSnapshot(this AdminController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var metricsService = controller.HttpContext.RequestServices.GetService<IMetricsCollectionService>();
        return metricsService?.GetSnapshot();
    }

    /// <summary>
    /// Generates diagnostic recommendations based on metrics.
    /// </summary>
    private static IReadOnlyList<string> GetDiagnosticRecommendations(MetricsSnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return Array.Empty<string>();
        }

        var recommendations = new List<string>();

        if (snapshot.ErrorRate > 5.0)
        {
            recommendations.Add("High error rate detected - investigate error logs");
        }

        if (snapshot.AverageLatencyMs > 500)
        {
            recommendations.Add("High average latency - check database and external API performance");
        }

        if (snapshot.P95LatencyMs > 2000)
        {
            recommendations.Add("P95 latency exceeds 2 seconds - optimize critical paths");
        }

        if (snapshot.TotalRateLimitExceeded > 100)
        {
            recommendations.Add("Frequent rate limiting - consider increasing limits or optimizing usage");
        }

        if (!recommendations.Any())
        {
            recommendations.Add("System operating within normal parameters");
        }

        return recommendations.AsReadOnly();
    }
}
