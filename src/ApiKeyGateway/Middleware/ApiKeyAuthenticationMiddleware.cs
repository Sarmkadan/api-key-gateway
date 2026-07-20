// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Configuration;
using ApiKeyGateway.Domain.Exceptions;
using UnauthorizedAccessException = ApiKeyGateway.Domain.Exceptions.UnauthorizedAccessException;
using ApiKeyGateway.Services;

namespace ApiKeyGateway.Middleware;

/// <summary>
/// Middleware for API key authentication and request validation
/// </summary>
public class ApiKeyAuthenticationMiddleware
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private const string ApiKeyQueryName = "api_key";
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
    private readonly bool _failOpenOnKeyStoreUnavailable;

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyAuthenticationMiddleware> logger,
        Configuration.GatewayConfiguration? gatewayConfig = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _failOpenOnKeyStoreUnavailable = gatewayConfig?.FailOpenOnKeyStoreUnavailable ?? false;
    }

    /// <summary>
    /// Invokes the middleware to process the request. The gateway services are
    /// method-injected rather than constructor-injected because conventional
    /// middleware is constructed once from the root provider, while these
    /// services are scoped; resolving them per request keeps their lifetimes
    /// correct (one repository/connection scope per request).
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="authenticationService">Scoped authentication service for the current request.</param>
    /// <param name="usageTrackingService">Scoped usage tracking service for the current request.</param>
    /// <param name="rateLimitingService">Scoped rate limiting service for the current request.</param>
    /// <param name="usageQuotaService">Scoped usage quota service for the current request.</param>
    public async Task InvokeAsync(
        HttpContext context,
        IAuthenticationService authenticationService,
        IUsageTrackingService usageTrackingService,
        IRateLimitingService rateLimitingService,
        IUsageQuotaService usageQuotaService)
    {
        var startTime = DateTime.UtcNow;
        var apiKey = ExtractApiKey(context.Request);

        try
        {
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                var clientIp = ExtractClientIp(context);
                var authenticatedKey = await authenticationService.AuthenticateAsync(apiKey, clientIp);

                if (authenticatedKey != null)
                {
                    await rateLimitingService.CheckLimitAsync(authenticatedKey.Id);

				// Get rate limit configuration for headers
				var rateLimit = await rateLimitingService.GetLimitAsync(authenticatedKey.Id);
				if (rateLimit != null && rateLimit.IsEnabled && rateLimit.Unit != Domain.Enums.RateLimitUnit.Unlimited)
				{
					context.Response.Headers.Append("X-RateLimit-Limit", rateLimit.RequestsPerUnit.ToString());
					context.Response.Headers.Append("X-RateLimit-Remaining", rateLimit.RemainingRequests.ToString());
					context.Response.Headers.Append("X-RateLimit-Reset",
						new DateTimeOffset(rateLimit.LastResetAt ?? rateLimit.CreatedAt).AddSeconds(rateLimit.GetWindowInSeconds()).ToUnixTimeSeconds().ToString());
				}

                    // Enforce route-scope restrictions
                    var requestPath = context.Request.Path.ToString();
                    if (!authenticatedKey.IsScopeAllowed(requestPath))
                    {
                        _logger.LogWarning(
                            "API key {ApiKeyId} does not have scope permission for path {Path}",
                            authenticatedKey.Id, requestPath);
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "The API key does not have permission to access this route"
                        });
                        return;
                    }

                    // Check per-key usage quota (daily/monthly hard cap)
                    var quotaResult = await usageQuotaService.CheckAndRecordAsync(authenticatedKey.Id);
                    if (quotaResult.Limit != long.MaxValue)
                    {
                        context.Response.Headers.Append("X-RateLimit-Quota-Limit", quotaResult.Limit.ToString());
                        context.Response.Headers.Append("X-RateLimit-Quota-Remaining", quotaResult.Remaining.ToString());
                        context.Response.Headers.Append("X-RateLimit-Quota-Reset",
                            new DateTimeOffset(quotaResult.PeriodEnd).ToUnixTimeSeconds().ToString());
                    }

                    if (quotaResult.IsExceeded)
                    {
                        _logger.LogWarning("Usage quota exceeded for API key {ApiKeyId}", authenticatedKey.Id);
                        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "Usage quota exceeded for this period",
                            quotaResetAt = quotaResult.PeriodEnd
                        });
                        return;
                    }

                    context.Items["ApiKey"] = authenticatedKey;
                    context.Items["ConsumerId"] = authenticatedKey.ConsumerId;
                }
            }

            await _next(context);

            var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            await RecordUsageAsync(context, usageTrackingService, rateLimitingService, duration);
        }
        catch (RateLimitExceededException ex)
        {
            _logger.LogWarning("Rate limit exceeded for key {ApiKeyId}", ex.ApiKeyId);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.Append("Retry-After", ((int)ex.RetryAfter!.Value.Subtract(DateTime.UtcNow).TotalSeconds).ToString());
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (InvalidApiKeyException ex)
        {
            _logger.LogWarning("Invalid API key attempt: {Reason}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            if (ex.IsExpired)
                await context.Response.WriteAsJsonAsync(new { error = "api_key_expired" });
            else
                await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access attempt from {SourceIp}: {Reason}", ex.SourceIp, ex.Reason);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
        }
        catch (KeyStoreUnavailableException ex)
        {
            _logger.LogError(ex, "Key store unavailable; fail-open={FailOpen}", _failOpenOnKeyStoreUnavailable);

            if (_failOpenOnKeyStoreUnavailable)
            {
                // Fail-open: allow the request through without authentication.
                _logger.LogWarning("Allowing request through unauthenticated due to fail-open policy");
                await _next(context);
            }
            else
            {
                // Fail-closed (default): return 503 so clients know to retry.
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.Headers.Append("Retry-After", "30");
                await context.Response.WriteAsJsonAsync(new { error = "Authentication service temporarily unavailable. Please retry." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in authentication middleware");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Extracts the API key from the request
    /// </summary>
    private static string? ExtractApiKey(HttpRequest request)
    {
        if (request.Headers.TryGetValue(ApiKeyHeaderName, out var headerValue))
            return headerValue.ToString();

        if (request.Query.TryGetValue(ApiKeyQueryName, out var queryValue))
            return queryValue.ToString();

        return null;
    }

    /// <summary>
    /// Extracts the client IP address from the request
    /// </summary>
    private static string ExtractClientIp(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            return forwardedFor.ToString().Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Records usage metrics for analytics
    /// </summary>
    private async Task RecordUsageAsync(
        HttpContext context,
        IUsageTrackingService usageTrackingService,
        IRateLimitingService rateLimitingService,
        int durationMs)
    {
        try
        {
            if (context.Items.TryGetValue("ApiKey", out var keyObj) && keyObj is Domain.Models.ApiKey key &&
                context.Items.TryGetValue("ConsumerId", out var consumerObj) && consumerObj is string consumerId)
            {
                var record = new Domain.Models.UsageRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    ApiKeyId = key.Id,
                    ConsumerId = consumerId,
                    RecordedAt = DateTime.UtcNow,
                    Endpoint = context.Request.Path.ToString(),
                    Method = context.Request.Method,
                    ResponseStatusCode = context.Response.StatusCode,
                    RequestBytes = context.Request.ContentLength ?? 0,
                    ResponseBytes = context.Response.ContentLength ?? 0,
                    ResponseTimeMs = durationMs,
                    SourceIp = ExtractClientIp(context)
                };

                await usageTrackingService.RecordUsageAsync(record);
                await rateLimitingService.RecordRequestAsync(key.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record usage");
        }
    }
}

/// <summary>
/// Extension methods for middleware registration
/// </summary>
public static class ApiKeyAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
    }
}
