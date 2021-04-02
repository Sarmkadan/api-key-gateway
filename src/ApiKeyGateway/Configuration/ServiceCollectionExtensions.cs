// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using ApiKeyGateway.Data;
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Extension methods for dependency injection configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all gateway services and repositories
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing gateway settings.</param>
    /// <returns>The <see cref="IServiceCollection"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddGatewayCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");

        services.AddScoped<IDbConnection>(sp =>
            new SqlServerConnection(connectionString, sp.GetRequiredService<ILogger<SqlServerConnection>>()));

        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IApiKeyService, ApiKeyService>();

        services.AddScoped<IRateLimitRepository, RateLimitRepository>();
        services.AddScoped<IRateLimitingService>(sp =>
            new RateLimitingService(
                sp.GetRequiredService<IRateLimitRepository>(),
                sp.GetRequiredService<ILogger<RateLimitingService>>(),
                sp.GetRequiredService<GatewayConfiguration>().ClockSkewToleranceSeconds));

        services.AddScoped<IUsageRepository, UsageRepository>();
        services.AddScoped<IUsageTrackingService, UsageTrackingService>();
        services.AddScoped<IUsageAnalyticsService, UsageAnalyticsService>();

        services.AddScoped<IUsageQuotaRepository, UsageQuotaRepository>();
        services.AddScoped<IUsageQuotaService, UsageQuotaService>();

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IApiKeyRotationService, ApiKeyRotationService>();

        services.AddRequestCoalescing();

        services.AddSingleton<GatewayConfiguration>(sp =>
            sp.GetRequiredService<IConfiguration>().GetSection("Gateway")
                .Get<GatewayConfiguration>()
            ?? throw new InvalidOperationException("Gateway configuration section 'Gateway' is missing or invalid."));

        return services;
    }

    /// <summary>
    /// Registers API documentation with Swagger/OpenAPI
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add documentation services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddGatewayDocumentation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOpenApi();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
            {
                Title = "API Key Gateway",
                Version = "v1",
                Description = "Lightweight API key authentication gateway for self-hosted services",
                Contact = new Microsoft.OpenApi.OpenApiContact
                {
                    Name = "Vladyslav Zaiets",
                    Url = new Uri("https://sarmkadan.com")
                }
            });

            options.AddSecurityDefinition("ApiKeyAuth", new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.SecuritySchemeType.ApiKey,
                In = Microsoft.OpenApi.ParameterLocation.Header,
                Name = "X-API-Key",
                Description = "API Key for gateway authentication"
            });

            options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
            {
                { new Microsoft.OpenApi.OpenApiSecuritySchemeReference("ApiKeyAuth", document), Array.Empty<string>().ToList() }
            });
        });

        return services;
    }
}

/// <summary>
/// Configuration model bound from appsettings.json
/// </summary>
public sealed class GatewayConfiguration
{
    /// <summary>
    /// Whether SSL/TLS is required for all requests. When true, requests are redirected to HTTPS.
    /// </summary>
    public bool RequireSsl { get; set; } = true;

    /// <summary>
    /// Whether to log all incoming requests for debugging and analytics purposes.
    /// </summary>
    public bool LogAllRequests { get; set; } = true;

    /// <summary>
    /// Maximum allowed length for API keys in characters.
    /// </summary>
    public int MaxKeyLength { get; set; } = 256;

    /// <summary>
    /// Minimum allowed length for API keys in characters.
    /// </summary>
    public int MinKeyLength { get; set; } = 16;

    /// <summary>
    /// Default number of days before API keys expire if no expiration is specified.
    /// </summary>
    public int DefaultKeyExpirationDays { get; set; } = 365;

    /// <summary>
    /// Number of days to retain audit logs before automatic cleanup.
    /// </summary>
    public int AuditLogRetentionDays { get; set; } = 90;

    /// <summary>
    /// Whether rate limiting is enabled by default for all API keys.
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// Default rate limit per hour for API keys without explicit limits.
    /// </summary>
    public int DefaultRateLimitPerHour { get; set; } = 10000;

    /// <summary>
    /// Maximum number of concurrent requests the gateway will handle.
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 1000;

    /// <summary>
    /// Seconds added to the rate-limit window expiry threshold to guard against
    /// clock skew between the gateway host and the backing store. Prevents
    /// premature window resets when clocks differ by up to this value.
    /// </summary>
    public double ClockSkewToleranceSeconds { get; set; } = RateLimitingService.DefaultClockSkewToleranceSeconds;

    /// <summary>
    /// When true the gateway allows requests through unauthenticated if the key
    /// store is unreachable (fail-open). When false (default) a 503 is returned
    /// so clients know to retry once the store recovers (fail-closed).
    /// </summary>
    public bool FailOpenOnKeyStoreUnavailable { get; set; } = false;
}