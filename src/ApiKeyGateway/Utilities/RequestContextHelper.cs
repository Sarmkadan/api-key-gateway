// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Helper for extracting and validating information from HTTP requests.
/// Centralizes request parsing logic to ensure consistency across the application.
/// Handles header validation, parameter extraction, and correlation tracking.
/// </summary>
public static class RequestContextHelper
{
    /// <summary>
    /// Extracts API key from request headers.
    /// </summary>
    public static string? ExtractApiKey(HttpRequest request)
    {
        // Try standard header first
        if (request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            return apiKey.ToString();
        }

        // Fallback to Authorization header (Bearer token pattern)
        if (request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var headerValue = authHeader.ToString();
            if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return headerValue["Bearer ".Length..];
            }
        }

        return null;
    }

    /// <summary>
    /// Gets or creates a correlation ID for request tracing.
    /// Allows tracking related requests across systems.
    /// </summary>
    public static string GetOrCreateCorrelationId(HttpRequest request)
    {
        const string correlationIdHeader = "X-Correlation-ID";

        if (request.Headers.TryGetValue(correlationIdHeader, out var value))
        {
            return value.ToString();
        }

        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Extracts pagination parameters from query string.
    /// </summary>
    public static (int pageNumber, int pageSize) ExtractPaginationParams(HttpRequest request)
    {
        var pageNumber = 1;
        var pageSize = 50;

        if (int.TryParse(request.Query["pageNumber"], out var page))
        {
            pageNumber = Math.Max(1, page);
        }

        if (int.TryParse(request.Query["pageSize"], out var size))
        {
            // Cap page size to prevent resource exhaustion
            pageSize = Math.Clamp(size, 1, 1000);
        }

        return (pageNumber, pageSize);
    }

    /// <summary>
    /// Gets the client's IP address from request context.
    /// Handles proxy headers (X-Forwarded-For) for behind-NAT scenarios.
    /// </summary>
    public static string GetClientIpAddress(HttpRequest request)
    {
        // Check for X-Forwarded-For header (behind proxy/load balancer)
        if (request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.ToString().Split(',').First().Trim();
            if (!string.IsNullOrEmpty(ip))
                return ip;
        }

        // Check X-Real-IP header
        if (request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            if (!string.IsNullOrEmpty(realIp.ToString()))
                return realIp.ToString();
        }

        // Fall back to connection remote address
        return request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Extracts request scope (which API key/user made the request).
    /// </summary>
    public static string GetRequestScope(HttpRequest request)
    {
        var apiKey = ExtractApiKey(request);
        return apiKey ?? "anonymous";
    }

    /// <summary>
    /// Validates request accepts JSON (check Accept header).
    /// </summary>
    public static bool AcceptsJson(HttpRequest request)
    {
        return request.Headers.Accept.Any(a =>
            a.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
            a.Contains("*/*", StringComparison.OrdinalIgnoreCase));
    }
}
