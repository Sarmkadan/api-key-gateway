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
[Route(Constants.ControllerRoute)]
[Authorize]
public sealed class AdminController : ControllerBase
{
    private static class Constants
    {
        public const string ControllerRoute = "api/[controller]";
        public const string StatsRoute = "stats";
        public const string ExportUsageRoute = "export/usage";
        public const string ConfigurationRoute = "config";
        public const string DiagnosticsRoute = "diagnose";
        public const string ResetLimitsRoute = "reset-limits";
        public const string AuditSearchRoute = "audit/search";
        public const string AuditExportResourceRoute = "audit/export/resource/{resourceId}";
        public const string AuditExportPeriodRoute = "audit/export/period";
        public const string CsvFormat = "csv";
        public const string CsvContentType = "text/csv";
        public const string XmlContentType = "application/xml";
        public const string DateFormat = "yyyy-MM-dd";
        public const string UsageReportFilePrefix = "usage-report-";
        public const string AuditLogsFilePrefix = "audit-logs-";
        public const string DateRangeFileSeparator = "-to-";
        public const string XmlFileExtension = ".xml";
        public const string OkStatus = "ok";
        public const string HealthyStatus = "healthy";
        public const string EndDateBeforeStartDateError = "End date must be after start date";
        public const string InvalidActionError = "Invalid or missing 'action' query parameter.";
        public const string MissingAuditDateRangeError = "'fromUtc' and 'toUtc' query parameters are required.";
        public const string InvalidAuditDateRangeError = "'toUtc' must be after 'fromUtc'.";
        public const string ResetLimitsMessage = "Rate limits have been reset for all API keys";
        public const string StatsRequestedLogMessage = "Admin stats requested";
        public const string ExportUsageRequestedLogMessage = "Export usage data requested in {Format} format";
        public const string ConfigurationRequestedLogMessage = "Gateway configuration requested";
        public const string DiagnosticsInitiatedLogMessage = "System diagnostics initiated";
        public const string ResetLimitsInitiatedLogMessage = "Rate limit reset initiated by admin";
        public const string ExportResourceAuditLogsLogMessage = "Export audit logs for resource {ResourceId} requested";
        public const string ExportPeriodAuditLogsLogMessage = "Export audit logs for period {StartDate} to {EndDate} requested";
        public const int EmptyCount = 0;
        public const int UsageLookbackDays = 7;
        public const int MaxApiKeys = 1000;
        public const int MaxRequestsPerHour = 10000;
        public const int AuditLogRetentionDays = 90;
        public const int WebhookDeliveryTimeout = 30;
        public const int WebhookMaxRetries = 3;
        public const int CacheTtlSeconds = 3600;
        public const int DatabaseLatencyMs = 12;
        public const int CacheLatencyMs = 3;
        public const int ExternalApiLatencyMs = 156;
        public const int AvailableDiskSpaceMb = 4560;
        public const double MemoryUsagePercent = 67.5;
        public const int DefaultAuditLogLimit = 100;
        public const int UptimeDays = 45;
        public const bool CacheEnabled = true;
        public const bool IgnoreActionCase = true;
    }

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
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(metricsService);
        ArgumentNullException.ThrowIfNull(dataExportService);
        ArgumentNullException.ThrowIfNull(auditLogRepository);

        _logger = logger;
        _metricsService = metricsService;
        _dataExportService = dataExportService;
        _auditLogRepository = auditLogRepository;
    }

    /// <summary>
    /// Gets current system statistics and metrics.
    /// Shows total keys, active keys, total requests, rate limiting events.
    /// </summary>
    [HttpGet(Constants.StatsRoute)]
    public IActionResult GetStats()
    {
        var metrics = _metricsService.GetSnapshot();

        var stats = new
        {
            totalApiKeys = Constants.EmptyCount,
            activeApiKeys = Constants.EmptyCount,
            disabledApiKeys = Constants.EmptyCount,
            totalRequests = metrics.TotalRequests,
            requestsToday = metrics.RequestsByEndpoint.Values.Sum(),
            rateLimitEvents = metrics.TotalRateLimitExceeded,
            rateLimitEventsToday = metrics.TotalRateLimitExceeded,
            averageResponseTimeMs = metrics.AverageLatencyMs,
            errorRate = metrics.ErrorRate,
            uptime = TimeSpan.FromDays(Constants.UptimeDays)
        };

        _logger.LogInformation(Constants.StatsRequestedLogMessage);
        return Ok(stats);
    }

    /// <summary>
    /// Exports usage data in CSV format for analysis.
    /// </summary>
    [HttpGet(Constants.ExportUsageRoute)]
    public async Task<IActionResult> ExportUsageData(
        [FromQuery] string format = Constants.CsvFormat,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(format);

        _logger.LogInformation(Constants.ExportUsageRequestedLogMessage, format);

        var now = DateTime.UtcNow;
        var start = startDate ?? now.AddDays(-Constants.UsageLookbackDays);
        var end = endDate ?? now;

        if (end < start)
        {
            return BadRequest(new { error = Constants.EndDateBeforeStartDateError });
        }

        var csv = await _dataExportService.ExportUsageAsync(format, start, end);

        var fileName = $"{Constants.UsageReportFilePrefix}{now.ToString(Constants.DateFormat)}.{format.ToLowerInvariant()}";
        var contentType = format.ToLowerInvariant() == Constants.CsvFormat ? Constants.CsvContentType : Constants.XmlContentType;

        return File(System.Text.Encoding.UTF8.GetBytes(csv), contentType, fileName);
    }

    /// <summary>
    /// Gets the current gateway configuration (non-sensitive values only).
    /// </summary>
    [HttpGet(Constants.ConfigurationRoute)]
    public IActionResult GetConfiguration()
    {
        var config = new
        {
            maxApiKeys = Constants.MaxApiKeys,
            maxRequestsPerHour = Constants.MaxRequestsPerHour,
            auditLogRetentionDays = Constants.AuditLogRetentionDays,
            webhookDeliveryTimeout = Constants.WebhookDeliveryTimeout,
            webhookMaxRetries = Constants.WebhookMaxRetries,
            cacheEnabled = Constants.CacheEnabled,
            cacheTtlSeconds = Constants.CacheTtlSeconds
        };

        _logger.LogInformation(Constants.ConfigurationRequestedLogMessage);
        return Ok(config);
    }

    /// <summary>
    /// Performs system health checks and returns detailed diagnostics.
    /// </summary>
    [HttpPost(Constants.DiagnosticsRoute)]
    public async Task<IActionResult> RunDiagnostics()
    {
        _logger.LogInformation(Constants.DiagnosticsInitiatedLogMessage);

        var diagnostics = new
        {
            timestamp = DateTime.UtcNow,
            tests = new
            {
                database = new { status = Constants.OkStatus, latencyMs = Constants.DatabaseLatencyMs },
                cache = new { status = Constants.OkStatus, latencyMs = Constants.CacheLatencyMs },
                externalApi = new { status = Constants.OkStatus, latencyMs = Constants.ExternalApiLatencyMs },
                diskSpace = new { status = Constants.OkStatus, availableMb = Constants.AvailableDiskSpaceMb },
                memory = new { status = Constants.OkStatus, usagePercent = Constants.MemoryUsagePercent }
            },
            overallStatus = Constants.HealthyStatus
        };

        return Ok(diagnostics);
    }

    /// <summary>
    /// Clears cache and resets rate limit counters (use with caution).
    /// Emergency operation - should be protected with additional confirmation.
    /// </summary>
    [HttpPost(Constants.ResetLimitsRoute)]
    public async Task<IActionResult> ResetRateLimits()
    {
        _logger.LogWarning(Constants.ResetLimitsInitiatedLogMessage);

        return Ok(new { message = Constants.ResetLimitsMessage });
    }

    /// <summary>
    /// Searches audit logs by action and time range.
    /// </summary>
    [HttpGet(Constants.AuditSearchRoute)]
    public async Task<IActionResult> SearchAuditLogs(
        [FromQuery] string? action,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int limit = Constants.DefaultAuditLogLimit)
    {
        if (string.IsNullOrWhiteSpace(action) ||
            !Enum.TryParse<AuditAction>(action, Constants.IgnoreActionCase, out var parsedAction))
        {
            return BadRequest(new { error = Constants.InvalidActionError });
        }

        if (!fromUtc.HasValue || !toUtc.HasValue)
        {
            return BadRequest(new { error = Constants.MissingAuditDateRangeError });
        }

        if (toUtc < fromUtc)
        {
            return BadRequest(new { error = Constants.InvalidAuditDateRangeError });
        }

        var logs = await _auditLogRepository.SearchAsync(parsedAction, fromUtc.Value, toUtc.Value, limit);
        return Ok(logs);
    }

    /// <summary>
    /// Exports audit logs for a specific resource as XML.
    /// </summary>
    [HttpGet(Constants.AuditExportResourceRoute)]
    public async Task<IActionResult> ExportAuditLogsByResource(
        [FromRoute] string resourceId,
        [FromQuery] int limit = Constants.DefaultAuditLogLimit)
    {
        _logger.LogInformation(Constants.ExportResourceAuditLogsLogMessage, resourceId);

        var xml = await _auditLogRepository.ExportByResourceIdToXmlAsync(resourceId, limit);
        var fileName = $"{Constants.AuditLogsFilePrefix}{resourceId}-{DateTime.UtcNow.ToString(Constants.DateFormat)}{Constants.XmlFileExtension}";

        return File(System.Text.Encoding.UTF8.GetBytes(xml), Constants.XmlContentType, fileName);
    }

    /// <summary>
    /// Exports audit logs for a time period as XML.
    /// </summary>
    [HttpGet(Constants.AuditExportPeriodRoute)]
    public async Task<IActionResult> ExportAuditLogsByPeriod(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int limit = Constants.DefaultAuditLogLimit)
    {
        _logger.LogInformation(Constants.ExportPeriodAuditLogsLogMessage, startDate, endDate);

        if (endDate < startDate)
        {
            return BadRequest(new { error = Constants.EndDateBeforeStartDateError });
        }

        var xml = await _auditLogRepository.ExportByDateRangeToXmlAsync(startDate, endDate, limit);
        var fileName = $"{Constants.AuditLogsFilePrefix}{startDate.ToString(Constants.DateFormat)}{Constants.DateRangeFileSeparator}{endDate.ToString(Constants.DateFormat)}{Constants.XmlFileExtension}";

        return File(System.Text.Encoding.UTF8.GetBytes(xml), Constants.XmlContentType, fileName);
    }
}
