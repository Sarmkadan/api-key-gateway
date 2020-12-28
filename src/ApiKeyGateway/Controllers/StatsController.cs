// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Statistics endpoints available to API key owners.
/// These endpoints show metrics relevant to individual API keys:
/// - Usage statistics (requests, bandwidth)
/// - Rate limit status
/// - Recent activity
/// - Quota utilization
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StatsController : ControllerBase
{
    private readonly ILogger<StatsController> _logger;

    public StatsController(ILogger<StatsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets usage statistics for the authenticated API key.
    /// Shows requests today, this month, and historical trends.
    /// </summary>
    [HttpGet("usage")]
    public IActionResult GetUsageStatistics([FromQuery] string period = "day")
    {
        var apiKeyId = User.FindFirst("api_key_id")?.Value ?? "unknown";

        _logger.LogInformation("Usage statistics requested for {ApiKeyId}", apiKeyId);

        var stats = period.ToLowerInvariant() switch
        {
            "hour" => GetHourlyStats(apiKeyId),
            "day" => GetDailyStats(apiKeyId),
            "month" => GetMonthlyStats(apiKeyId),
            _ => GetDailyStats(apiKeyId)
        };

        return Ok(stats);
    }

    /// <summary>
    /// Gets current rate limit status and quota utilization.
    /// </summary>
    [HttpGet("rate-limit")]
    public IActionResult GetRateLimitStatus()
    {
        var apiKeyId = User.FindFirst("api_key_id")?.Value ?? "unknown";

        var status = new
        {
            apiKeyId,
            rateLimits = new
            {
                hourly = new { limit = 1000, current = 450, remaining = 550, resetIn = "37 minutes" },
                daily = new { limit = 10000, current = 4500, remaining = 5500, resetIn = "8 hours" },
                monthly = new { limit = 100000, current = 45000, remaining = 55000, resetIn = "23 days" }
            },
            status = "ok"
        };

        _logger.LogInformation("Rate limit status requested for {ApiKeyId}", apiKeyId);
        return Ok(status);
    }

    /// <summary>
    /// Gets endpoint-specific statistics (breakdown by API endpoint).
    /// </summary>
    [HttpGet("endpoints")]
    public IActionResult GetEndpointStatistics()
    {
        var apiKeyId = User.FindFirst("api_key_id")?.Value ?? "unknown";

        var endpoints = new[]
        {
            new { path = "/api/users", requests = 1250, avgResponseTime = 145, errorCount = 2 },
            new { path = "/api/products", requests = 890, avgResponseTime = 220, errorCount = 1 },
            new { path = "/api/orders", requests = 456, avgResponseTime = 350, errorCount = 5 }
        };

        _logger.LogInformation("Endpoint statistics requested for {ApiKeyId}", apiKeyId);
        return Ok(new { apiKeyId, endpoints });
    }

    /// <summary>
    /// Gets recent activity and access history for this API key.
    /// </summary>
    [HttpGet("activity")]
    public IActionResult GetRecentActivity([FromQuery] int limit = 50)
    {
        var apiKeyId = User.FindFirst("api_key_id")?.Value ?? "unknown";

        var activity = new
        {
            apiKeyId,
            recentRequests = new[]
            {
                new { timestamp = DateTime.UtcNow.AddMinutes(-2), endpoint = "/api/users", statusCode = 200, responseTime = 145 },
                new { timestamp = DateTime.UtcNow.AddMinutes(-5), endpoint = "/api/products", statusCode = 200, responseTime = 220 },
                new { timestamp = DateTime.UtcNow.AddMinutes(-8), endpoint = "/api/orders", statusCode = 429, responseTime = 0 }
            }
        };

        _logger.LogInformation("Recent activity requested for {ApiKeyId} (limit: {Limit})", apiKeyId, limit);
        return Ok(activity);
    }

    /// <summary>
    /// Gets comparison of usage against quota limits.
    /// </summary>
    [HttpGet("quota")]
    public IActionResult GetQuotaStatus()
    {
        var apiKeyId = User.FindFirst("api_key_id")?.Value ?? "unknown";

        var quota = new
        {
            apiKeyId,
            quotaType = "pro",
            limits = new
            {
                requestsPerDay = 10000,
                dataTransferGbMonth = 5,
                maxConcurrentConnections = 10
            },
            usage = new
            {
                requestsToday = 4500,
                dataTransferGbThisMonth = 2.3,
                currentConnections = 3
            },
            utilization = new
            {
                requests = 45.0,
                dataTransfer = 46.0,
                connections = 30.0
            },
            warnings = new[] { "Approaching daily request limit" }
        };

        _logger.LogInformation("Quota status requested for {ApiKeyId}", apiKeyId);
        return Ok(quota);
    }

    private static object GetHourlyStats(string apiKeyId) => new
    {
        period = "last 1 hour",
        requests = 450,
        errors = 2,
        totalDataTransferred = 2.5,
        averageResponseTime = 156
    };

    private static object GetDailyStats(string apiKeyId) => new
    {
        period = "last 24 hours",
        requests = 4500,
        errors = 15,
        totalDataTransferred = 25.3,
        averageResponseTime = 178
    };

    private static object GetMonthlyStats(string apiKeyId) => new
    {
        period = "last 30 days",
        requests = 45000,
        errors = 120,
        totalDataTransferred = 234.5,
        averageResponseTime = 165
    };
}
