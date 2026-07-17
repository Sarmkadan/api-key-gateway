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
    /// Seals the class to prevent inheritance where not needed.
    /// </summary>
    static AdminControllerExtensions() { }

    /// <summary>
    /// Gets detailed statistics including breakdown by endpoint and error rates.
    /// </summary>
    /// <param name="controller">The admin controller instance</param>
    /// <returns>Action result with detailed statistics</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null</exception>
    public static IActionResult GetDetailedStats(this AdminController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var statsResult = controller.GetStats();
        return statsResult switch
        {
            OkObjectResult okResult => ProcessStatsResult(okResult, controller),
            _ => statsResult
        };
    }

    /// <summary>
    /// Processes the statistics result and returns detailed statistics.
    /// </summary>
    /// <param name="okResult">The successful result containing stats</param>
    /// <param name="controller">The admin controller instance</param>
    /// <returns>Action result with detailed statistics</returns>
    private static IActionResult ProcessStatsResult(OkObjectResult okResult, AdminController controller)
    {
        var stats = okResult.Value as dynamic;
        var metricsSnapshot = controller.GetMetricsSnapshot();

        var detailedStats = new DetailedStatsDto
        {
            Timestamp = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            Total = new StatsSummary
            {
                Requests = stats?.totalRequests,
                ActiveKeys = stats?.activeApiKeys,
                Errors = stats?.errorRate,
                RateLimits = stats?.rateLimitEvents
            },
            EndpointBreakdown = metricsSnapshot?.RequestsByEndpoint as IReadOnlyDictionary<string, long>,
            ErrorBreakdown = metricsSnapshot?.ErrorsByCode as IReadOnlyDictionary<int, long>,
            Performance = new PerformanceMetrics
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> or <paramref name="format"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="format"/> is null or empty</exception>
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> or <paramref name="format"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="format"/> is null or empty</exception>
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null</exception>
    public static async Task<IActionResult> RunComprehensiveDiagnosticsAsync(this AdminController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var diagnosticsResult = await controller.RunDiagnostics();
        return diagnosticsResult switch
        {
            OkObjectResult okResult => ProcessDiagnosticsResult(okResult, controller),
            _ => diagnosticsResult
        };
    }

    /// <summary>
    /// Processes the diagnostics result and returns comprehensive diagnostics.
    /// </summary>
    /// <param name="okResult">The successful result containing diagnostics</param>
    /// <param name="controller">The admin controller instance</param>
    /// <returns>Action result with detailed health diagnostics</returns>
    private static IActionResult ProcessDiagnosticsResult(OkObjectResult okResult, AdminController controller)
    {
        var diagnostics = okResult.Value as dynamic;
        var metricsSnapshot = controller.GetMetricsSnapshot();

        var comprehensiveDiagnostics = new ComprehensiveDiagnosticsDto
        {
            Timestamp = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            SystemStatus = diagnostics?.overallStatus,
            PerformanceMetrics = new PerformanceMetrics
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty</exception>
    public static async Task<IActionResult> ResetRateLimitsForKeyAsync(
        this AdminController controller,
        string apiKeyId)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);

        var resetResult = await controller.ResetRateLimits();
        return resetResult switch
        {
            OkObjectResult okResult => ProcessResetResult(okResult, apiKeyId),
            _ => resetResult
        };
    }

    /// <summary>
    /// Processes the reset result and returns success response.
    /// </summary>
    /// <param name="okResult">The successful reset result</param>
    /// <param name="apiKeyId">The API key identifier</param>
    /// <returns>Action result indicating success</returns>
    private static IActionResult ProcessResetResult(OkObjectResult okResult, string apiKeyId)
    {
        return okResult.Value switch
        {
            true => new OkObjectResult(new { message = $"Rate limits reset for API key: {apiKeyId}" }),
            _ => new OkObjectResult(new { message = $"Rate limits reset successfully" })
        };
    }

    /// <summary>
    /// Gets metrics snapshot directly from the metrics service.
    /// </summary>
    /// <param name="controller">The admin controller instance</param>
    /// <returns>Metrics snapshot or null if not available</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null</exception>
    private static MetricsSnapshot? GetMetricsSnapshot(this AdminController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var metricsService = controller.HttpContext.RequestServices.GetService<IMetricsCollectionService>();
        return metricsService?.GetSnapshot();
    }

    /// <summary>
    /// Generates diagnostic recommendations based on metrics.
    /// </summary>
    /// <param name="snapshot">The metrics snapshot to analyze</param>
    /// <returns>List of diagnostic recommendations</returns>
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

    /// <summary>
    /// Data transfer object for detailed statistics.
    /// </summary>
    private sealed class DetailedStatsDto
    {
        public string Timestamp { get; init; } = string.Empty;
        public StatsSummary Total { get; init; } = new();
        public IReadOnlyDictionary<string, long>? EndpointBreakdown { get; init; }
        public IReadOnlyDictionary<int, long>? ErrorBreakdown { get; init; }
        public PerformanceMetrics Performance { get; init; } = new();
        public TimeSpan? Uptime { get; init; }
    }

    /// <summary>
    /// Data transfer object for statistics summary.
    /// </summary>
    private sealed class StatsSummary
    {
        public long? Requests { get; init; }
        public int? ActiveKeys { get; init; }
        public double? Errors { get; init; }
        public int? RateLimits { get; init; }
    }

    /// <summary>
    /// Data transfer object for performance metrics.
    /// </summary>
    private sealed class PerformanceMetrics
    {
        public double? AverageLatencyMs { get; init; }
        public double? P95LatencyMs { get; init; }
        public double? ErrorRate { get; init; }
        public double RequestsPerSecond { get; init; }
    }

    /// <summary>
    /// Data transfer object for comprehensive diagnostics.
    /// </summary>
    private sealed class ComprehensiveDiagnosticsDto
    {
        public string Timestamp { get; init; } = string.Empty;
        public string? SystemStatus { get; init; }
        public PerformanceMetrics PerformanceMetrics { get; init; } = new();
        public object? Tests { get; init; }
        public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
    }
}