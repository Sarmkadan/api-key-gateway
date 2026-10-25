# RequestCoalescingExtensions

The `RequestCoalescingExtensions` class is a static class in the `ApiKeyGateway.Configuration` namespace. It provides extension methods for `IServiceCollection` that register the request coalescing service with the dependency injection container. Request coalescing lets concurrent callers that ask for the same expensive result share a single in-flight operation instead of each running it separately. See [IRequestCoalescingService](IRequestCoalescingService.md) for the service contract.

## API

### Extension Methods

#### `AddRequestCoalescing`
```csharp
public static IServiceCollection AddRequestCoalescing(this IServiceCollection services)
```
Registers `IRequestCoalescingService` with the `RequestCoalescingService` implementation as a **singleton**.

The service is registered as a singleton because it holds a shared dictionary of in-flight requests that must span all HTTP scopes. If it were scoped or transient, concurrent requests would not see each other's in-flight tasks, and coalescing would have no effect.

This method is already called by `AddGatewayCoreServices` in `ServiceCollectionExtensions`, so the gateway's own services get coalescing without a separate call. Call it directly only when you build a custom service collection that does not go through that path.

- **Parameters**
  - `services`: The `IServiceCollection` to add the service to.
- **Returns**
  The same `IServiceCollection`, so calls can be chained.
- **Throws**
  - `ArgumentNullException` if `services` is `null`.

**Example**

```csharp
builder.Services.AddRequestCoalescing();
```

After registration, inject `IRequestCoalescingService` into any component that runs expensive operations, such as database lookups or external API calls, that should be deduplicated under concurrent load.

## Related

- [IRequestCoalescingService](IRequestCoalescingService.md)
- [ServiceCollectionExtensions](ServiceCollectionExtensions.md)
