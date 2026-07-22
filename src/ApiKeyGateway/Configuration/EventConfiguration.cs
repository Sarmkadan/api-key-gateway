// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Extension methods for configuring event publishing and handling.
/// Registers event publishers and subscribes handlers to events.
/// Centralizes event orchestration in one place for easier testing and modification.
/// </summary>
public static class EventConfiguration
{
    /// <summary>
    /// Adds event publishing infrastructure to the container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddEventPublishing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure event publisher options from configuration
        services.Configure<EventPublisherOptions>(configuration.GetSection("EventPublishing"));

        // Register the base event publisher
        services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();

        // Register the resilient retrying publisher as the default implementation
        // This wraps the inner publisher with retry and dead-letter handling
        services.AddSingleton<RetryingEventPublisher>();
        services.AddSingleton<IEventPublisher>(sp =>
        {
            var options = sp.GetRequiredService<EventPublisherOptions>();
            var deadLetterQueue = sp.GetRequiredService<IDeadLetterQueue>();
            var logger = sp.GetRequiredService<ILogger<RetryingEventPublisher>>();
            var innerPublisher = sp.GetRequiredService<IEventPublisher>();

            // Validate options before creating publisher
            options.Validate();

            return new RetryingEventPublisher(
                innerPublisher,
                deadLetterQueue,
                options,
                logger);
        });

        // Register the dead-letter queue
        services.AddSingleton<IDeadLetterQueue>(sp =>
        {
            var options = sp.GetRequiredService<EventPublisherOptions>();
            return new InMemoryDeadLetterQueue(options.MaxDeadLetterQueueSize);
        });

        // Register event handlers
        services.AddSingleton<AuditLogEventHandler>();
        services.AddSingleton<UsageTrackingEventHandler>();
        services.AddSingleton<RateLimitEventHandler>();

        return services;
    }

    /// <summary>
    /// Subscribes event handlers to their respective events.
    /// Must be called during app startup after DI is configured.
    /// </summary>
    /// <remarks>
    /// This method casts the <see cref="IEventPublisher"/> to <see cref="InMemoryEventPublisher"/>
    /// to register subscribers. The <see cref="RetryingEventPublisher"/> decorator will
    /// forward all publish calls to the inner <see cref="InMemoryEventPublisher"/>.
    /// </remarks>
    public static void SubscribeEventHandlers(this IApplicationBuilder app)
    {
        // Get the inner InMemoryEventPublisher that's wrapped by RetryingEventPublisher
        var innerPublisher = app.ApplicationServices.GetRequiredService<InMemoryEventPublisher>();

        var auditHandler = app.ApplicationServices.GetRequiredService<AuditLogEventHandler>();
        var usageHandler = app.ApplicationServices.GetRequiredService<UsageTrackingEventHandler>();
        var rateLimitHandler = app.ApplicationServices.GetRequiredService<RateLimitEventHandler>();

        // Subscribe audit log handlers
        innerPublisher.Subscribe<ApiKeyCreatedEvent>(auditHandler.HandleApiKeyCreatedAsync);
        innerPublisher.Subscribe<ApiKeyRotatedEvent>(auditHandler.HandleApiKeyRotatedAsync);
        innerPublisher.Subscribe<ApiKeyDisabledEvent>(auditHandler.HandleApiKeyDisabledAsync);

        // Subscribe usage tracking handlers
        innerPublisher.Subscribe<ApiKeyUsedEvent>(usageHandler.HandleApiKeyUsedAsync);
        innerPublisher.Subscribe<QuotaExhaustedEvent>(usageHandler.HandleQuotaExhaustedAsync);
        innerPublisher.Subscribe<UsageWarningEvent>(usageHandler.HandleUsageWarningAsync);

        // Subscribe rate limit handlers
        innerPublisher.Subscribe<RateLimitExceededEvent>(rateLimitHandler.HandleRateLimitExceededAsync);

        var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(EventConfiguration).FullName!);
        logger.LogInformation("Event subscriptions configured");
    }
}
