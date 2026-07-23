// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using ApiKeyGateway.Configuration;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Middleware;
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
    /// <param name="quotaAlertEvaluator">Scoped quota alert evaluator for the current request.</param>
    public async Task InvokeAsync(
        HttpContext context,
        IAuthenticationService authenticationService,
        IUsageTrackingService usageTrackingService,
        IRateLimitingService rateLimitingService,
        IUsageQuotaService usageQuotaService,
        IQuotaAlertEvaluator quotaAlertEvaluator)
    {
        var startTime = DateTime.UtcNow;
        var apiKey = ExtractApiKey(context.Request);

        try
        {
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                var clientIp = ExtractClientIp(context);
                var authResult = await authenticationService.AuthenticateAsync(apiKey, clientIp);

                if (authResult.Success && authResult.ApiKey != null)
                {
                    await rateLimitingService.CheckLimitAsync(authResult.ApiKey.Id);

                    // Get rate limit configuration for headers
                    var rateLimit = await rateLimitingService.GetLimitAsync(authResult.ApiKey.Id);
                    if (rateLimit != null && rateLimit.IsEnabled && rateLimit.Unit != Domain.Enums.RateLimitUnit.Unlimited)
                    {
                        context.Response.Headers.Append("X-RateLimit-Limit", rateLimit.RequestsPerUnit.ToString());
                        context.Response.Headers.Append("X-RateLimit-Remaining", rateLimit.RemainingRequests.ToString());
                        context.Response.Headers.Append("X-RateLimit-Reset",
                            new DateTimeOffset(rateLimit.LastResetAt ?? rateLimit.CreatedAt).AddSeconds(rateLimit.GetWindowInSeconds()).ToUnixTimeSeconds().ToString());
                    }

                    // Enforce route-scope restrictions
                    var requestPath = context.Request.Path.ToString();
                    if (!authResult.ApiKey.IsScopeAllowed(requestPath))
                    {
                        _logger.LogWarning(
                            "API key {ApiKeyId} does not have scope permission for path {Path}",
                            authResult.ApiKey.Id,
                            requestPath);
                        var problemDetails = GatewayProblemDetailsFactory.CreateRouteScopeRestrictedProblem(context);
                        await context.WriteProblemAsync(problemDetails);
                        return;
                    }

                    // Check per-key usage quota (daily/monthly hard cap)
                    var quotaResult = await usageQuotaService.CheckAndRecordAsync(authResult.ApiKey.Id);
                    if (quotaResult.Limit != long.MaxValue)
                    {
                        context.Response.Headers.Append("X-RateLimit-Quota-Limit", quotaResult.Limit.ToString());
                        context.Response.Headers.Append("X-RateLimit-Quota-Remaining", quotaResult.Remaining.ToString());
                        context.Response.Headers.Append("X-RateLimit-Quota-Reset",
                            new DateTimeOffset(quotaResult.PeriodEnd).ToUnixTimeSeconds().ToString());
                    }

                    if (quotaResult.IsExceeded)
                    {
                        _logger.LogWarning("Usage quota exceeded for API key {ApiKeyId}", authResult.ApiKey.Id);
                        var problemDetails = GatewayProblemDetailsFactory.CreateQuotaExceededProblem(context, quotaResult.PeriodEnd);
                        await context.WriteProblemAsync(problemDetails);
                        return;
                    }

                    context.Items["ApiKey"] = authResult.ApiKey;
                    context.Items["ConsumerId"] = authResult.ApiKey.ConsumerId;
                }
                else
                {
                    // Handle authentication failure based on failure reason
                    switch (authResult.FailureReason)
                    {
                        case Domain.Models.AuthenticationFailureReason.MissingApiKey:
                        case Domain.Models.AuthenticationFailureReason.InvalidApiKeyFormat:
                        case Domain.Models.AuthenticationFailureReason.ApiKeyNotFound:
                            var invalidKeyProblem = GatewayProblemDetailsFactory.CreateInvalidKeyProblem(context, "Invalid API key");
                            await context.WriteProblemAsync(invalidKeyProblem);
                            return;
                        case Domain.Models.AuthenticationFailureReason.ApiKeyExpired:
                            var expiredProblem = GatewayProblemDetailsFactory.CreateKeyExpiredProblem(context);
                            await context.WriteProblemAsync(expiredProblem);
                            return;
                        case Domain.Models.AuthenticationFailureReason.ApiKeyDisabled:
                            var disabledProblem = GatewayProblemDetailsFactory.CreateKeyDisabledProblem(context);
                            await context.WriteProblemAsync(disabledProblem);
                            return;
                        case Domain.Models.AuthenticationFailureReason.IpNotWhitelisted:
                            var ipProblem = GatewayProblemDetailsFactory.CreateIpNotWhitelistedProblem(context);
                            await context.WriteProblemAsync(ipProblem);
                            return;
                        case Domain.Models.AuthenticationFailureReason.ServiceUnavailable:
                        default:
                            var serviceUnavailableProblem = GatewayProblemDetailsFactory.CreateServiceUnavailableProblem(context);
                            await context.WriteProblemAsync(serviceUnavailableProblem);
                            return;
                    }
                }
            }

            await _next(context);

            var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            await RecordUsageAsync(context, usageTrackingService, rateLimitingService, duration);
            await EvaluateQuotaAlertsAsync(context, quotaAlertEvaluator);
        }
        catch (RateLimitExceededException ex)
        {
            _logger.LogWarning("Rate limit exceeded for key {ApiKeyId}", ex.ApiKeyId);
            var problemDetails = GatewayProblemDetailsFactory.CreateRateLimitExceededProblem(
                context,
                ex.RetryAfter ?? DateTime.UtcNow.AddSeconds(ex.WindowInSeconds),
                ex.Limit,
                ex.WindowInSeconds);
            await context.WriteProblemAsync(problemDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in authentication middleware");
            var problemDetails = GatewayProblemDetailsFactory.CreateInternalServerErrorProblem(context);
            await context.WriteProblemAsync(problemDetails);
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

    /// <summary>
    /// Runs quota threshold alerting for the request's consumer.
    /// Best-effort: failures are logged and never affect the response.
    /// </summary>
    private async Task EvaluateQuotaAlertsAsync(HttpContext context, IQuotaAlertEvaluator quotaAlertEvaluator)
    {
        try
        {
            if (context.Items.TryGetValue("ApiKey", out var keyObj) && keyObj is Domain.Models.ApiKey key &&
                context.Items.TryGetValue("ConsumerId", out var consumerObj) && consumerObj is string consumerId &&
                !string.IsNullOrEmpty(consumerId))
            {
                await quotaAlertEvaluator.EvaluateAsync(consumerId, key.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quota alert evaluation failed");
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