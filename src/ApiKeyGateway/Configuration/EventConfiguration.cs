// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Events;

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
    public static IServiceCollection AddEventPublishing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register the event publisher
        services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();

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
    public static void SubscribeEventHandlers(this IApplicationBuilder app)
    {
        var eventPublisher = (InMemoryEventPublisher)app.ApplicationServices
            .GetRequiredService<IEventPublisher>();

        var auditHandler = app.ApplicationServices.GetRequiredService<AuditLogEventHandler>();
        var usageHandler = app.ApplicationServices.GetRequiredService<UsageTrackingEventHandler>();
        var rateLimitHandler = app.ApplicationServices.GetRequiredService<RateLimitEventHandler>();

        // Subscribe audit log handlers
        eventPublisher.Subscribe<ApiKeyCreatedEvent>(auditHandler.HandleApiKeyCreatedAsync);
        eventPublisher.Subscribe<ApiKeyRotatedEvent>(auditHandler.HandleApiKeyRotatedAsync);
        eventPublisher.Subscribe<ApiKeyDisabledEvent>(auditHandler.HandleApiKeyDisabledAsync);

        // Subscribe usage tracking handlers
        eventPublisher.Subscribe<ApiKeyUsedEvent>(usageHandler.HandleApiKeyUsedAsync);
        eventPublisher.Subscribe<QuotaExhaustedEvent>(usageHandler.HandleQuotaExhaustedAsync);
        eventPublisher.Subscribe<UsageWarningEvent>(usageHandler.HandleUsageWarningAsync);

        // Subscribe rate limit handlers
        eventPublisher.Subscribe<RateLimitExceededEvent>(rateLimitHandler.HandleRateLimitExceededAsync);

        var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(EventConfiguration).FullName!);
        logger.LogInformation("Event subscriptions configured");
    }
}
