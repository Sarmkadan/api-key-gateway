// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Caching;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Extension method for configuring caching infrastructure.
/// Centralizes all cache-related DI registration in one place.
/// Makes it easy to swap between in-memory and distributed caching.
/// </summary>
public static class CachingConfiguration
{
    /// <summary>
    /// Adds caching services to dependency injection container.
    /// Configures memory cache with reasonable defaults.
    /// </summary>
    public static IServiceCollection AddGatewayCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add memory cache
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 100 * 1024 * 1024; // 100 MB max
        });

        // Register cache provider
        services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();

        // In production, you might conditionally register distributed cache:
        // var cacheType = configuration["Cache:Type"] ?? "memory";
        // if (cacheType == "redis")
        // {
        //     services.AddStackExchangeRedisCache(options =>
        //     {
        //         options.Configuration = configuration.GetConnectionString("Redis");
        //     });
        //     services.AddSingleton<ICacheProvider, RedisCacheProvider>();
        // }

        return services;
    }

    /// <summary>
    /// Validates cache configuration at startup.
    /// Ensures required cache backends are accessible.
    /// </summary>
    public static async Task ValidateCacheConfiguration(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<CachingConfiguration>>();
        var cacheProvider = serviceProvider.GetRequiredService<ICacheProvider>();

        try
        {
            // Test cache by writing and reading a value
            var testKey = "health_check_test";
            var testValue = "test_value";

            await cacheProvider.SetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
            var retrieved = await cacheProvider.GetAsync<string>(testKey);

            if (retrieved == testValue)
            {
                logger.LogInformation("Cache configuration validated successfully");
            }
            else
            {
                logger.LogError("Cache health check failed: value mismatch");
                throw new InvalidOperationException("Cache health check failed");
            }

            await cacheProvider.RemoveAsync(testKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cache validation failed");
            throw;
        }
    }
}
