// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using ApiKeyGateway.BackgroundWorkers;
using ApiKeyGateway.Caching;
using ApiKeyGateway.Events;
using ApiKeyGateway.Integration;
using ApiKeyGateway.Services;
using Microsoft.Extensions.DependencyInjection;

using System;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Consolidated service registration extension.
/// Simplifies Program.cs by grouping all service registrations.
/// Makes it easy to understand what services are available at startup.
/// </summary>
public static class ServiceRegistrationExtensions
{
    /// <summary>
    /// Registers all gateway services in the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing gateway settings.</param>
    /// <returns>The <see cref="IServiceCollection"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddGatewayServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Core services
        services.AddGatewayCoreServices(configuration);

        // Caching
        services.AddGatewayCaching(configuration);

        // Event publishing
        services.AddEventPublishing(configuration);

        // Integration
        services.AddScoped<IWebhookHandler, WebhookHandler>();
        services.AddScoped<IWebhookManager, WebhookManager>();
        services.AddScoped<IBatchOperationHandler, BatchOperationHandler>();

        // Export
        services.AddScoped<IDataExportService, DataExportService>();

        // Metrics
        services.AddSingleton<IMetricsCollectionService, MetricsCollectionService>();

        // HTTP clients
        services.AddHttpClient();
        services.AddScoped<IExternalApiClient>(sp =>
            new HttpExternalApiClient(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
                sp.GetRequiredService<ICacheProvider>(),
                sp.GetRequiredService<ILogger<HttpExternalApiClient>>(),
                "ExternalApi"));

        // Background services
        services.AddHostedService<UsageAggregationWorker>();
        services.AddHostedService<RateLimitResetScheduler>();
        services.AddHostedService<AuditLogCleanupWorker>();
        services.AddHostedService<KeyRotationScheduler>();

        // Middleware
        services.AddGatewayMiddleware(configuration);

        // Request transformation pipeline (v2)
        services.AddRequestTransformationPipeline(configuration);

        return services;
    }

    /// <summary>
    /// Validates all critical services during startup.
    /// Fails fast if dependencies are misconfigured.
    /// </summary>
    /// <param name="serviceProvider">The service provider to validate.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous validation operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    public static async Task ValidateServicesAsync(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(ServiceRegistrationExtensions).FullName!);

        try
        {
            // Validate cache configuration
            await CachingConfiguration.ValidateCacheConfiguration(serviceProvider);
            logger.LogInformation("Cache validation completed successfully");

            // Validate database connectivity
            var apiKeyRepo = serviceProvider.GetRequiredService<IApiKeyRepository>();
            await apiKeyRepo.GetAllAsync();
            logger.LogInformation("Database connectivity validated");

            logger.LogInformation("All service validations passed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Service validation failed");
            throw;
        }
    }
}
