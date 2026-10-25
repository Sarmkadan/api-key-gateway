// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using ApiKeyGateway.Middleware;

namespace ApiKeyGateway.Middleware;

/// <summary>
/// Middleware that validates incoming requests before they reach handlers.
/// Checks for required headers, valid content types, and request size limits.
/// Early validation prevents wasted processing on malformed requests.
/// </summary>
public sealed class RequestValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestValidationMiddleware> _logger;
    private const long MaxBodySizeBytes = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestValidationMiddleware"/> class
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline</param>
    /// <param name="logger">Logger for validation diagnostics</param>
    public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);

        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Validates the request headers and body size, then passes control to the next middleware
    /// </summary>
    /// <param name="context">The HTTP context of the current request</param>
    /// <returns>A task that completes when the request has been processed</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip validation for GET/HEAD requests that typically have no body
        if (context.Request.Method is "GET" or "HEAD" or "OPTIONS")
        {
            await _next(context);
            return;
        }

        // Validate Content-Type header for requests with bodies
        if (context.Request.ContentLength > 0)
        {
            if (!IsValidContentType(context.Request.ContentType))
            {
                _logger.LogWarning(
                    "Invalid Content-Type: {ContentType} for {Method} {Path}",
                    context.Request.ContentType,
                    context.Request.Method,
                    context.Request.Path);

                context.Response.StatusCode = 415;
                var problemDetails = GatewayProblemDetailsFactory.CreateUnsupportedMediaTypeProblem(context);
                await context.WriteProblemAsync(problemDetails);
                return;
            }

            // Check request body size
            if (context.Request.ContentLength > MaxBodySizeBytes)
            {
                _logger.LogWarning(
                    "Request body exceeds size limit: {Size} bytes > {MaxSize} bytes",
                    context.Request.ContentLength,
                    MaxBodySizeBytes);

                context.Response.StatusCode = 413;
                var problemDetails = GatewayProblemDetailsFactory.CreatePayloadTooLargeProblem(context, MaxBodySizeBytes);
                await context.WriteProblemAsync(problemDetails);
                return;
            }
        }

        // Validate required headers for API endpoints
        if (!context.Request.Path.StartsWithSegments("/health") &&
            !context.Request.Path.StartsWithSegments("/admin/login"))
        {
            if (!context.Request.Headers.ContainsKey("X-API-Key"))
            {
                _logger.LogWarning(
                    "Missing API key header for {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                context.Response.StatusCode = 400;
                var problemDetails = GatewayProblemDetailsFactory.CreateMissingKeyHeaderProblem(context);
                await context.WriteProblemAsync(problemDetails);
                return;
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Validates that Content-Type is supported by the API.
    /// </summary>
    private static bool IsValidContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            return false;
        }

        // Accept application/json and multipart/form-data
        return contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ||
               contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase);
    }
}
