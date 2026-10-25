# CachingConfiguration

`src/ApiKeyGateway/Configuration/CachingConfiguration.cs` is a static class that holds the DI registration and startup validation for the gateway's cache. It is the single place where the cache backend is wired up. Consumers of the cache depend on `ICacheProvider` and never reference `IMemoryCache` directly; see [CacheProvider](CacheProvider.md) for the abstraction itself.

```csharp
namespace ApiKeyGateway.Configuration;

public static class CachingConfiguration
{
    public static IServiceCollection AddGatewayCaching(this IServiceCollection services, IConfiguration configuration);
    public static Task ValidateCacheConfiguration(IServiceProvider serviceProvider);
}
```

## Properties

`CachingConfiguration` has no properties and no instance state. It is a static class with two methods. The cache settings are not read from `appsettings.json`:

| Setting | Where it is set | Value |
|---------|-----------------|-------|
| Memory cache size limit | `AddGatewayCaching`, `services.AddMemoryCache(options => options.SizeLimit)` | `100 * 1024 * 1024` (100 MB), hard-coded |
| Cache backend | `AddGatewayCaching`, `services.AddSingleton<ICacheProvider, InMemoryCacheProvider>()` | Always in-memory |

Changing either value means editing the source. The `configuration` parameter is validated for `null` but is not read: the Redis branch that would use `Cache:Type` and `ConnectionStrings:Redis` exists only as a commented-out block.

## Methods

### AddGatewayCaching

```csharp
public static IServiceCollection AddGatewayCaching(
    this IServiceCollection services,
    IConfiguration configuration)
```

Registers the cache infrastructure and returns `services` so calls can be chained.

What it registers:

1. `IMemoryCache` through `AddMemoryCache`, with `SizeLimit` set to 100 MB.
2. `ICacheProvider` as a singleton backed by `InMemoryCacheProvider`. One instance serves the whole process.

Both arguments must be non-null. Otherwise it throws `ArgumentNullException`.

It is called from `AddGatewayServices` in `Configuration/ServiceRegistrationExtensions.cs`, so applications that use the gateway's standard registration get the cache without calling it themselves.

### ValidateCacheConfiguration

```csharp
public static async Task ValidateCacheConfiguration(IServiceProvider serviceProvider)
```

A startup health check. It resolves `ICacheProvider` and `ILoggerFactory` from `serviceProvider`, then:

1. Writes the key `health_check_test` with the value `test_value` and a 10-second expiration.
2. Reads the key back as a `string`.
3. Logs `Cache configuration validated successfully` and removes the key if the value matches.

If the value does not match, it logs an error and throws `InvalidOperationException("Cache health check failed")`. Any exception from the provider is logged and rethrown unchanged.

It is called from `ServiceRegistrationExtensions.ValidateServicesAsync`, before the database connectivity check. `src/ApiKeyGateway/Program.cs` does not call `ValidateServicesAsync`, so this check runs only if the host calls it explicitly. When it runs, a failure stops startup.

`serviceProvider` must be non-null. The provider and logger factory must be registered, which `AddGatewayCaching` takes care of.

## Typical usage

Standard setup goes through the gateway's service registration:

```csharp
// Program.cs
builder.Services.AddGatewayServices(builder.Configuration); // calls AddGatewayCaching internally

var app = builder.Build();
// Optional: fail fast if the cache is unusable. Program.cs does not do this today.
await ServiceRegistrationExtensions.ValidateServicesAsync(app.Services); // calls ValidateCacheConfiguration
```

Calling the two methods directly is useful in a custom host or in tests that need only the cache:

```csharp
var services = new ServiceCollection();
services.AddLogging();
services.AddGatewayCaching(configuration);

await using var provider = services.BuildServiceProvider();
await CachingConfiguration.ValidateCacheConfiguration(provider);
```

To check the cache in a test, resolve `ICacheProvider` from the same provider instead of constructing `InMemoryCacheProvider` yourself. This keeps the test on the same registration the application uses.

## Notes

- **Size limit is not enforced yet.** `SizeLimit` is set, but `InMemoryCacheProvider.SetAsync` creates `MemoryCacheEntryOptions` without a `Size`. With a size limit configured, `MemoryCache` expects each entry to declare its size. Check this against the runtime behavior before relying on the 100 MB cap. The fix would be to set `Size` on each entry in `CacheProvider.cs`, or to remove the limit.
- **Redis is not wired.** The commented-out block in `AddGatewayCaching` describes the intended second backend. Adding it means registering `AddStackExchangeRedisCache` and an `ICacheProvider` implementation, and reading `Cache:Type` and the `Redis` connection string. Consumers do not need to change, because they depend only on `ICacheProvider`.
- **Single-instance only.** The in-memory cache is per process. With several gateway instances, a key revoked on one instance stays valid on the others until its entry expires. Use a distributed backend for multi-instance deployments.
