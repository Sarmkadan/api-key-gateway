// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Data;
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Extension methods for dependency injection configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all gateway services and repositories
    /// </summary>
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

        services.AddScoped<IDbConnection>(sp =>
            new SqlServerConnection(connectionString, sp.GetRequiredService<ILogger<SqlServerConnection>>()));

        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IApiKeyService, ApiKeyService>();

        services.AddScoped<IRateLimitRepository, RateLimitRepository>();
        services.AddScoped<IRateLimitingService, RateLimitingService>();

        services.AddScoped<IUsageRepository, UsageRepository>();
        services.AddScoped<IUsageTrackingService, UsageTrackingService>();

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();

        services.AddSingleton<GatewayConfiguration>(sp =>
            sp.GetRequiredService<IConfiguration>().GetSection("Gateway")
                .Get<GatewayConfiguration>() ?? new GatewayConfiguration());

        return services;
    }

    /// <summary>
    /// Registers API documentation with Swagger/OpenAPI
    /// </summary>
    public static IServiceCollection AddGatewayDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "API Key Gateway",
                Version = "v1",
                Description = "Lightweight API key authentication gateway for self-hosted services",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Vladyslav Zaiets",
                    Url = new Uri("https://sarmkadan.com")
                }
            });

            options.AddSecurityDefinition("ApiKeyAuth", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Name = "X-API-Key",
                Description = "API Key for gateway authentication"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "ApiKeyAuth"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}

/// <summary>
/// Configuration model bound from appsettings.json
/// </summary>
public class GatewayConfiguration
{
    public bool RequireSsl { get; set; } = true;
    public bool LogAllRequests { get; set; } = true;
    public int MaxKeyLength { get; set; } = 256;
    public int MinKeyLength { get; set; } = 16;
    public int DefaultKeyExpirationDays { get; set; } = 365;
    public int AuditLogRetentionDays { get; set; } = 90;
    public bool EnableRateLimiting { get; set; } = true;
    public int DefaultRateLimitPerHour { get; set; } = 10000;
    public int MaxConcurrentRequests { get; set; } = 1000;
}
