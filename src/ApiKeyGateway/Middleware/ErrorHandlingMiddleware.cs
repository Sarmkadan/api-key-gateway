// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using ApiKeyGateway.Domain.Exceptions;
using UnauthorizedAccessException = ApiKeyGateway.Domain.Exceptions.UnauthorizedAccessException;

namespace ApiKeyGateway.Middleware;

/// <summary>
/// Centralized error handling middleware that catches all exceptions and returns
/// standardized error responses. This prevents sensitive information from leaking
/// to clients and ensures consistent error formatting across the API.
/// </summary>
public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the full exception internally for diagnostics
            _logger.LogError(ex, "Unhandled exception occurred during request processing");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            InvalidApiKeyException => new
            {
                statusCode = 401,
                message = exception.Message,
                error = "INVALID_API_KEY"
            },
            RateLimitExceededException => new
            {
                statusCode = 429,
                message = exception.Message,
                error = "RATE_LIMIT_EXCEEDED"
            },
            UnauthorizedAccessException => new
            {
                statusCode = 403,
                message = exception.Message,
                error = "UNAUTHORIZED_ACCESS"
            },
            DataAccessException => new
            {
                statusCode = 500,
                message = "Database operation failed",
                error = "DATABASE_ERROR"
            },
            _ => new
            {
                statusCode = 500,
                message = "An unexpected error occurred",
                error = "INTERNAL_SERVER_ERROR"
            }
        };

        // Only set status code if not already set
        if (context.Response.StatusCode == 200)
        {
            // All switch arms above produce the same anonymous type, so the
            // property is statically available; reflection via typeof(object)
            // returned null and threw NullReferenceException here.
            context.Response.StatusCode = response.statusCode;
        }

        var jsonResponse = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(jsonResponse);
    }
}
