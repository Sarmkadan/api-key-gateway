// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Middleware;
using System.Text.Json;
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
        context.Response.ContentType = "application/problem+json";

        var problemDetails = exception switch
        {
            InvalidApiKeyException => GatewayProblemDetailsFactory.CreateInvalidKeyProblem(context, exception.Message),
            RateLimitExceededException ex => GatewayProblemDetailsFactory.CreateRateLimitExceededProblem(
                context,
                ex.RetryAfter ?? DateTime.UtcNow.AddSeconds(ex.WindowInSeconds),
                ex.Limit,
                ex.WindowInSeconds),
            UnauthorizedAccessException => GatewayProblemDetailsFactory.CreateUnauthorizedAccessProblem(context),
            DataAccessException => GatewayProblemDetailsFactory.CreateInternalServerErrorProblem(context),
            _ => GatewayProblemDetailsFactory.CreateInternalServerErrorProblem(context)
        };

        // Set status code from ProblemDetails
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}