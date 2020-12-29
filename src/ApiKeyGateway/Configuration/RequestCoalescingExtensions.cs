// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Extension methods for registering request coalescing services into the dependency injection
/// container.
/// </summary>
public static class RequestCoalescingExtensions
{
    /// <summary>
    /// Adds <see cref="IRequestCoalescingService"/> to the service collection as a singleton.
    /// </summary>
    /// <remarks>
    /// The service must be registered as a singleton because it maintains a shared dictionary of in-flight
    /// requests that spans all HTTP scopes. Registering it as scoped or transient would prevent
    /// concurrent requests from finding each other's in-flight tasks and defeat the purpose of
    /// coalescing.
    ///
    /// Call this method inside your service registration setup, for example:
    /// <code>
    /// builder.Services.AddRequestCoalescing();
    /// </code>
    /// Then inject <see cref="IRequestCoalescingService"/> wherever expensive operations such as
    /// database lookups or external API calls should be deduplicated under concurrent load.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> to allow method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddRequestCoalescing(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IRequestCoalescingService, RequestCoalescingService>();

        return services;
    }

}