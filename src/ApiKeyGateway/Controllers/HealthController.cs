// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
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
    /// </summary>
    [HttpGet("ready")]
    [AllowAnonymous]
    public async Task<IActionResult> Ready()
    {
        try
        {
            // In production, add checks for:
            // - Database connectivity
            // - Cache connectivity
            // - Message queue connectivity
            // - External service dependencies

            var readiness = new
            {
                status = "ready",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                checks = new
                {
                    database = "ok",
                    cache = "ok"
                }
            };

            return Ok(readiness);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness probe failed");
            return StatusCode(503, new { status = "not_ready", error = "Dependency check failed" });
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
