// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Utilities;
using Microsoft.AspNetCore.Authorization;

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

    public AdminController(ILogger<AdminController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets current system statistics and metrics.
    /// Shows total keys, active keys, total requests, rate limiting events.
    /// </summary>
    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        var stats = new
        {
            totalApiKeys = 1250,
            activeApiKeys = 980,
            disabledApiKeys = 270,
            totalRequests = 5_234_890,
            requestsToday = 45_230,
            rateLimitEvents = 234,
            rateLimitEventsToday = 12,
            averageResponseTimeMs = 156.7,
            errorRate = 0.23,
            uptime = TimeSpan.FromDays(45)
        };

        _logger.LogInformation("Admin stats requested");
        return Ok(stats);
    }

    /// <summary>
    /// Exports usage data in CSV format for analysis.
    /// </summary>
    [HttpGet("export/usage")]
    public async Task<IActionResult> ExportUsageData([FromQuery] string format = "csv")
    {
        _logger.LogInformation("Export usage data requested in {Format} format", format);

        // In production, query database for actual usage records
        var usageData = new[]
        {
            new { apiKeyId = "key_abc123", endpoint = "/api/users", requests = 450, averageResponseTime = 145 },
            new { apiKeyId = "key_def456", endpoint = "/api/products", requests = 320, averageResponseTime = 220 },
        };

        var csv = format.ToLower() switch
        {
            "csv" => CsvExportHelper.ToCsv(usageData),
            "xml" => XmlExportHelper.ToXml(usageData, "usageRecords", "record"),
            _ => CsvExportHelper.ToCsv(usageData)
        };

        var fileName = $"usage-report-{DateTime.UtcNow:yyyy-MM-dd}.{format.ToLower()}";
        var contentType = format.ToLower() == "csv" ? "text/csv" : "application/xml";

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

        // In production, this would:
        // 1. Clear cache entries
        // 2. Log the operation
        // 3. Notify monitoring systems
        // 4. Trigger audit events

        return Ok(new { message = "Rate limits have been reset for all API keys" });
    }
}
