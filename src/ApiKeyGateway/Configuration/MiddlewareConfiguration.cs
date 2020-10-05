// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Middleware;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Extension methods for configuring the request pipeline.
/// Centralizes middleware ordering and configuration.
/// Order matters: authentication must come before authorization,
/// error handling should be early, performance monitoring should be late.
/// </summary>
public static class MiddlewareConfiguration
{
    /// <summary>
    /// Configures the complete middleware pipeline for the gateway.
    /// </summary>
    public static void UseGatewayMiddleware(this WebApplication app)
    {
        // 1. Error handling should be first to catch all exceptions
        app.UseMiddleware<ErrorHandlingMiddleware>();

        // 2. Request logging happens early to capture all requests
        app.UseMiddleware<RequestLoggingMiddleware>();

        // 3. Request validation prevents malformed requests from reaching handlers
        app.UseMiddleware<RequestValidationMiddleware>();

        // 4. Performance monitoring wraps execution to measure response times
        app.UseMiddleware<PerformanceMonitoringMiddleware>();

        // 5. Standard ASP.NET Core middleware
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        // 6. Map endpoints
        app.MapControllers();
    }

    /// <summary>
    /// Registers all middleware dependencies in the DI container.
    /// </summary>
    public static IServiceCollection AddGatewayMiddleware(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure logging
        services.AddLogging(configure =>
        {
            configure.ClearProviders();
            configure.AddConsole();
            configure.AddDebug();

            // Set minimum log level from configuration
            var logLevel = configuration["Logging:LogLevel:Default"] ?? "Information";
            if (Enum.TryParse<LogLevel>(logLevel, out var level))
            {
                configure.SetMinimumLevel(level);
            }
        });

        return services;
    }
}
