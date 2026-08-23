using ApiKeyGateway.Utilities;
using Microsoft.Extensions.Logging;

namespace ApiKeyGateway.Middleware;

/// <summary>
/// Middleware that establishes correlation context for request tracing.
/// Generates or extracts correlation IDs from headers to track request flow
/// across multiple services. This is critical for debugging distributed systems.
/// </summary>
public sealed class CorrelationContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationContextMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationContextMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="next"/> or <paramref name="logger"/> is null.</exception>
    public CorrelationContextMiddleware(RequestDelegate next, ILogger<CorrelationContextMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    internal RequestDelegate GetNextDelegate() => _next;

    internal ILogger<CorrelationContextMiddleware> GetLogger() => _logger;

    /// <summary>
    /// Invokes the middleware to handle the request context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that represents the completion of request processing.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = RequestContextHelper.GetOrCreateCorrelationId(context.Request);
        var apiKey = RequestContextHelper.ExtractApiKey(context.Request);

        // Store correlation ID in context for use by handlers
        context.Items["CorrelationId"] = correlationId;
        context.Items["ApiKeyId"] = apiKey ?? "anonymous";
        context.Items["ClientIp"] = RequestContextHelper.GetClientIpAddress(context.Request);

        // Add correlation ID to response headers
        context.Response.Headers.Add(CorrelationIdHeader, correlationId);

        _logger.LogInformation("InvokeAsync called with {CorrelationId}", correlationId);
        _logger.LogInformation("Request initiated | Correlation: {CorrelationId} | API-Key: {ApiKey} | Client: {ClientIp}", correlationId, apiKey?.Substring(0, 4) + "..." ?? "NONE", context.Items["ClientIp"]);
        await _next(context);
        _logger.LogInformation("Request completed | Correlation: {CorrelationId} | Status: {StatusCode}", correlationId, context.Response.StatusCode);
    }
}

/// <summary>
/// Helper for accessing correlation context in handlers.
/// </summary>
public static class CorrelationContextExtensions
{
    /// <summary>
    /// Gets correlation ID from the current HttpContext.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The correlation ID if found; otherwise, "unknown".</returns>
    public static string GetCorrelationId(this HttpContext context) =>
        context.Items["CorrelationId"] as string ?? "unknown";

    /// <summary>
    /// Gets API key from the current HttpContext.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The API key ID if found; otherwise, "anonymous".</returns>
    public static string GetApiKeyId(this HttpContext context) =>
        context.Items["ApiKeyId"] as string ?? "anonymous";

    /// <summary>
    /// Gets client IP address from the current HttpContext.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The client IP address if found; otherwise, "unknown".</returns>
    public static string GetClientIp(this HttpContext context) =>
        context.Items["ClientIp"] as string ?? "unknown";
}
