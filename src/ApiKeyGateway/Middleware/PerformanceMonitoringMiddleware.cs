// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace ApiKeyGateway.Middleware;

/// <summary>
/// Middleware that monitors performance metrics and warns when endpoints
/// exceed acceptable response times. This helps identify slow operations
/// before they impact users. Thresholds are configurable per endpoint.
/// </summary>
public sealed class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private const long WarningThresholdMs = 1000;
    private const long CriticalThresholdMs = 5000;

    public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip health check endpoint to reduce noise in logs
        if (context.Request.Path == "/health" || context.Request.Path == "/ready")
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log performance warnings based on thresholds
            if (stopwatch.ElapsedMilliseconds >= CriticalThresholdMs)
            {
                _logger.LogError(
                    "CRITICAL: {Method} {Path} took {Duration}ms (exceeds {Threshold}ms threshold)",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    CriticalThresholdMs);
            }
            else if (stopwatch.ElapsedMilliseconds >= WarningThresholdMs)
            {
                _logger.LogWarning(
                    "WARNING: {Method} {Path} took {Duration}ms (exceeds {Threshold}ms threshold)",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    WarningThresholdMs);
            }
        }
    }
}
