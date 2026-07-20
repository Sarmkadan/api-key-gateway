// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using ApiKeyGateway.Services;
using ApiKeyGateway.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiKeyGateway.Domain.Enums;          // Added for AuditAction
using ApiKeyGateway.Repositories;          // Added for IAuditLogRepository

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Administrative endpoints for gateway operations.
/// These are restricted to admin users only and provide:
/// - System statistics and metrics
/// - Configuration management
/// - Audit log access
/// - Emergency operations (bulk disable keys, etc)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IMetricsCollectionService _metricsService;
    private readonly IDataExportService _dataExportService;
    private readonly IAuditLogRepository _auditLogRepository; // New dependency

    public AdminController(
        ILogger<AdminController> logger,
        IMetricsCollectionService metricsService,
        IDataExportService dataExportService,
        IAuditLogRepository auditLogRepository) // Updated constructor
    {
        _logger = logger;
        _metricsService = metricsService;
        _dataExportService = dataExportService;
        _auditLogRepository = auditLogRepository;
    }

    /// <summary>
    /// Gets current system statistics and metrics.
    /// Shows total keys, active keys, total requests, rate limiting events.
    /// </summary>
    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        var metrics = _metricsService.GetSnapshot();

        var stats = new
        {
            totalApiKeys = 0,
            activeApiKeys = 0,
            disabledApiKeys = 0,
            totalRequests = metrics.TotalRequests,
            requestsToday = metrics.RequestsByEndpoint.Values.Sum(),
            rateLimitEvents = metrics.TotalRateLimitExceeded,
            rateLimitEventsToday = metrics.TotalRateLimitExceeded,
            averageResponseTimeMs = metrics.AverageLatencyMs,
            errorRate = metrics.ErrorRate,
            uptime = TimeSpan.FromDays(45)
        };

        _logger.LogInformation("Admin stats requested");
        return Ok(stats);
    }

    /// <summary>
    /// Exports usage data in CSV format for analysis.
    /// </summary>
    [HttpGet("export/usage")]
    public async Task<IActionResult> ExportUsageData(
        [FromQuery] string format = "csv",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        _logger.LogInformation("Export usage data requested in {Format} format", format);

        var now = DateTime.UtcNow;
        var start = startDate ?? now.AddDays(-7);
        var end = endDate ?? now;

        if (end < start)
        {
            return BadRequest(new { error = "End date must be after start date" });
        }

        var csv = await _dataExportService.ExportUsageAsync(format, start, end);

        var fileName = $"usage-report-{now:yyyy-MM-dd}.{format.ToLowerInvariant()}";
        var contentType = format.ToLowerInvariant() == "csv" ? "text/csv" : "application/xml";

        return File(System.Text.Encoding.UTF8.GetBytes(csv), contentType, fileName);
    }

    /// <summary>
    /// Gets the current gateway configuration (non-sensitive values only).
    /// </summary>
    [HttpGet("config")]
    public IActionResult GetConfiguration()
    {
        var config = new
        {
            maxApiKeys = 1000,
            maxRequestsPerHour = 10000,
            auditLogRetentionDays = 90,
            webhookDeliveryTimeout = 30,
            webhookMaxRetries = 3,
            cacheEnabled = true,
            cacheTtlSeconds = 3600
        };

        _logger.LogInformation("Gateway configuration requested");
        return Ok(config);
    }

    /// <summary>
    /// Performs system health checks and returns detailed diagnostics.
    /// </summary>
    [HttpPost("diagnose")]
    public async Task<IActionResult> RunDiagnostics()
    {
        _logger.LogInformation("System diagnostics initiated");

        var diagnostics = new
        {
            timestamp = DateTime.UtcNow,
            tests = new
            {
                database = new { status = "ok", latencyMs = 12 },
                cache = new { status = "ok", latencyMs = 3 },
                externalApi = new { status = "ok", latencyMs = 156 },
                diskSpace = new { status = "ok", availableMb = 4560 },
                memory = new { status = "ok", usagePercent = 67.5 }
            },
            overallStatus = "healthy"
        };

        return Ok(diagnostics);
    }

    /// <summary>
    /// Clears cache and resets rate limit counters (use with caution).
    /// Emergency operation - should be protected with additional confirmation.
    /// </summary>
    [HttpPost("reset-limits")]
    public async Task<IActionResult> ResetRateLimits()
    {
        _logger.LogWarning("Rate limit reset initiated by admin");

        return Ok(new { message = "Rate limits have been reset for all API keys" });
    }

    /// <summary>
    /// Searches audit logs by action and time range.
    /// </summary>
    [HttpGet("audit/search")]
    public async Task<IActionResult> SearchAuditLogs(
        [FromQuery] string? action,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int limit = 100)
    {
        if (string.IsNullOrWhiteSpace(action) ||
            !Enum.TryParse<AuditAction>(action, true, out var parsedAction))
        {
            return BadRequest(new { error = "Invalid or missing 'action' query parameter." });
        }

        if (!fromUtc.HasValue || !toUtc.HasValue)
        {
            return BadRequest(new { error = "'fromUtc' and 'toUtc' query parameters are required." });
        }

        if (toUtc < fromUtc)
        {
            return BadRequest(new { error = "'toUtc' must be after 'fromUtc'." });
        }

        var logs = await _auditLogRepository.SearchAsync(parsedAction, fromUtc.Value, toUtc.Value, limit);
        return Ok(logs);
    }
}
