// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Services;

namespace ApiKeyGateway.Middleware;

/// <summary>
/// Composite middleware that chains API key authentication, rate limiting,
/// and usage tracking into a single pipeline step. Delegates the actual
/// authentication to <see cref="ApiKeyAuthenticationMiddleware"/> and adds
/// rate-limit enforcement and real-time usage tracking around it.
/// </summary>
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private readonly IRateLimiter? _rateLimiter;
    private readonly IUsageTracker? _usageTracker;

    private const string ApiKeyHeaderName = "X-API-Key";
    private const string ApiKeyQueryName = "api_key";
    private const string RateLimitRemainingHeader = "X-RateLimit-Remaining";
    private const string RateLimitResetHeader = "X-RateLimit-Reset";

    public ApiKeyMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyMiddleware> logger,
        IRateLimiter? rateLimiter = null,
        IUsageTracker? usageTracker = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rateLimiter = rateLimiter;
        _usageTracker = usageTracker;
    }

    /// <summary>
    /// Processes the HTTP request through authentication, rate limiting, and usage tracking.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var apiKey = ExtractApiKey(context);

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Request missing API key from {RemoteIp}",
                context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "API key is required" });
            return;
        }

        // Rate limit check
        if (_rateLimiter != null)
        {
            if (!_rateLimiter.TryAcquire(apiKey))
            {
                var resetTime = _rateLimiter.GetWindowResetTime(apiKey);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers[RateLimitResetHeader] =
                    new DateTimeOffset(resetTime).ToUnixTimeSeconds().ToString();
                await context.Response.WriteAsJsonAsync(new { error = "Rate limit exceeded" });
                return;
            }

            // Add rate limit headers
            var remaining = _rateLimiter.GetRemainingPermits(apiKey);
            context.Response.Headers[RateLimitRemainingHeader] = remaining.ToString();
        }

        // Track usage
        _usageTracker?.TrackRequest(apiKey);

        var startTimestamp = TimeProvider.System.GetTimestamp();

        await _next(context);

        var elapsed = TimeProvider.System.GetElapsedTime(startTimestamp);
        _logger.LogDebug("Request for key {ApiKeyPrefix}*** completed in {ElapsedMs}ms with status {StatusCode}",
            apiKey.Length > 8 ? apiKey[..8] : apiKey, elapsed.TotalMilliseconds, context.Response.StatusCode);
    }

    private static string? ExtractApiKey(HttpContext context)
    {
        // Header first
        if (context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var headerValue)
            && !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue.ToString();
        }

        // Query string fallback
        if (context.Request.Query.TryGetValue(ApiKeyQueryName, out var queryValue)
            && !string.IsNullOrWhiteSpace(queryValue))
        {
            return queryValue.ToString();
        }

        return null;
    }
}

/// <summary>
/// Extension methods for registering <see cref="ApiKeyMiddleware"/> in the pipeline.
/// </summary>
public static class ApiKeyMiddlewareExtensions
{
    /// <summary>
    /// Adds the composite API key middleware to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyMiddleware>();
    }
}
