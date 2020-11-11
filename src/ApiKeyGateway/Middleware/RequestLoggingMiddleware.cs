// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace ApiKeyGateway.Middleware;

/// <summary>
/// Middleware that logs incoming HTTP requests and outgoing responses.
/// This is critical for debugging, monitoring, and audit trail purposes.
/// We measure response time to identify performance bottlenecks early.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            using (var memoryStream = new MemoryStream())
            {
                context.Response.Body = memoryStream;

                // Extract API key from header for audit purposes (masked for security)
                var apiKeyHeader = context.Request.Headers
                    .FirstOrDefault(h => h.Key.Equals("X-API-Key", StringComparison.OrdinalIgnoreCase)).Value;

                var maskedApiKey = string.IsNullOrEmpty(apiKeyHeader)
                    ? "NONE"
                    : $"{apiKeyHeader.ToString()[..4]}...";

                _logger.LogInformation(
                    "HTTP {Method} {Path} started | API-Key: {ApiKey}",
                    context.Request.Method,
                    context.Request.Path,
                    maskedApiKey);

                await _next(context);

                stopwatch.Stop();

                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);

                // Log response with timing information for performance monitoring
                _logger.LogInformation(
                    "HTTP {Method} {Path} completed | Status: {StatusCode} | Duration: {DurationMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}
