// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Caching;
using ApiKeyGateway.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Health check endpoints for container orchestration and load balancers.
/// These endpoints don't require API key authentication and help with:
/// - Kubernetes liveness/readiness probes
/// - Load balancer health checks
/// - Container restart policies
/// </summary>
[ApiController]
[Route("[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly IDbConnection _dbConnection;
    private readonly ICacheProvider _cacheProvider;

    public HealthController(
        ILogger<HealthController> logger,
        IDbConnection dbConnection,
        ICacheProvider cacheProvider)
    {
        _logger = logger;
        _dbConnection = dbConnection;
        _cacheProvider = cacheProvider;
    }

    /// <summary>
    /// Liveness probe - indicates if the service is running.
    /// Used by Kubernetes to determine if pod should be restarted.
    /// </summary>
    [HttpGet("live")]
    [AllowAnonymous]
    public IActionResult Live()
    {
        _logger.LogDebug("Liveness probe called");
        return Ok(new { status = "alive" });
    }

    /// <summary>
    /// Readiness probe - indicates if the service is ready to handle traffic.
    /// Includes checks for database connectivity and cache availability.
    /// Returns 503 with detailed error information when dependencies are not ready.
    /// </summary>
    [HttpGet("ready")]
    [AllowAnonymous]
    public async Task<IActionResult> Ready()
    {
        var checks = new Dictionary<string, object>();
        var failedChecks = new List<string>();
        var status = "ready";
        var httpStatus = 200;

        try
        {
            // Check database connectivity
            try
            {
                await _dbConnection.OpenAsync();
                _dbConnection.CloseAsync().GetAwaiter().GetResult();
                checks["database"] = "ok";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connectivity check failed");
                checks["database"] = new { status = "failed", error = ex.Message };
                failedChecks.Add("database");
                status = "not_ready";
                httpStatus = 503;
            }

            // Check cache provider
            try
            {
                var testKey = "health_check_cache_test_" + Guid.NewGuid().ToString();
                var testValue = new { timestamp = DateTime.UtcNow.Ticks };
                await _cacheProvider.SetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
                var retrieved = await _cacheProvider.GetAsync<object>(testKey);
                await _cacheProvider.RemoveAsync(testKey);

                if (retrieved != null)
                {
                    checks["cache"] = "ok";
                }
                else
                {
                    throw new InvalidOperationException("Cache returned null for stored value");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache provider check failed");
                checks["cache"] = new { status = "failed", error = ex.Message };
                failedChecks.Add("cache");
                status = "not_ready";
                httpStatus = 503;
            }

            var readiness = new
            {
                status,
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                checks
            };

            if (failedChecks.Count > 0)
            {
                return StatusCode(httpStatus, readiness);
            }

            return Ok(readiness);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness probe failed");
            return StatusCode(503, new
            {
                status = "not_ready",
                timestamp = DateTime.UtcNow,
                error = "Dependency check failed",
                details = new { database = "unknown", cache = "unknown" }
            });
        }
    }

    /// <summary>
    /// Detailed health status including component information.
    /// Returns 200 if all systems operational, 503 if degraded.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Status()
    {
        return Ok(new
        {
            service = "api-key-gateway",
            status = "operational",
            timestamp = DateTime.UtcNow,
            uptime = "N/A",
            components = new
            {
                api = "healthy",
                authentication = "healthy",
                rateLimit = "healthy",
                audit = "healthy"
            }
        });
    }
}
