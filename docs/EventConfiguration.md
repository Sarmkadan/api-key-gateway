# EventConfiguration

`EventConfiguration` is a static class of extension methods in `ApiKeyGateway.Configuration` (`src/ApiKeyGateway/Configuration/EventConfiguration.cs`). It wires up event publishing for the gateway in two steps:

1. `AddEventPublishing(IServiceCollection, IConfiguration)` registers the publisher, the dead-letter queue (DLQ) and the event handlers in the DI container. It is called from `ServiceRegistrationExtensions.AddGatewayServices`.
2. `SubscribeEventHandlers(IApplicationBuilder)` subscribes the handlers to their event types. It must run at startup, after the container is built.

The class has no settings of its own. Its only configuration input is the `EventPublishing` section, which binds to [`EventPublisherOptions`](EventPublisherOptions.md). That page documents each property, its default and its allowed range.

## Configuration section

`AddEventPublishing` binds the `EventPublishing` section to `EventPublisherOptions`:

```csharp
services.Configure<EventPublisherOptions>(configuration.GetSection("EventPublishing"));
```

```json
{
  "EventPublishing": {
    "MaxRetryAttempts": 3,
    "InitialRetryDelayMs": 100,
    "MaxRetryDelayMs": 5000,
    "MaxDeadLetterQueueSize": 1000,
    "IncludeEventDetailsInDeadLetter": true
  }
}
```

| Key | Type | Default | Allowed range | Used by |
|-----|------|---------|---------------|---------|
| `MaxRetryAttempts` | `int` | `3` | `0`–`10` | `RetryingEventPublisher` |
| `InitialRetryDelayMs` | `int` | `100` | `1`–`10000` | `RetryingEventPublisher` |
| `MaxRetryDelayMs` | `int` | `5000` | `1`–`30000` | `RetryingEventPublisher` |
| `MaxDeadLetterQueueSize` | `int` | `1000` | `0`–`10000` | `InMemoryDeadLetterQueue` |
| `IncludeEventDetailsInDeadLetter` | `bool` | `true` | — | `RetryingEventPublisher` |

The section is absent from the default `appsettings.json`, so the defaults apply unless it is added. Property semantics are in [EventPublisherOptions](EventPublisherOptions.md).

## Registrations

`AddEventPublishing` registers the following services. All of them are singletons.

| Service | Implementation | Purpose |
|---------|----------------|---------|
| `IEventPublisher` | `InMemoryEventPublisher` | Base publisher that keeps the subscriber list. |
| `RetryingEventPublisher` | itself | Retry and DLQ wrapper, built by a factory. |
| `IEventPublisher` | factory returning `RetryingEventPublisher` | Default publisher. It wraps the base publisher. |
| `IDeadLetterQueue` | `InMemoryDeadLetterQueue` | Holds events that failed after all retries. Sized by `MaxDeadLetterQueueSize`. |
| `AuditLogEventHandler` | itself | Handles API key lifecycle events. |
| `UsageTrackingEventHandler` | itself | Handles usage and quota events. |
| `RateLimitEventHandler` | itself | Handles rate limit events. |

The factory for `IEventPublisher` validates the options with `EventPublisherOptions.Validate()` before it builds the `RetryingEventPublisher`. An invalid configuration therefore fails when `IEventPublisher` is first resolved.

Because the last `IEventPublisher` registration is the factory, consumers of `IEventPublisher` receive the `RetryingEventPublisher`. Publishing through it runs the retry logic and then forwards the call to the inner `InMemoryEventPublisher`.

## Event subscriptions

`SubscribeEventHandlers` subscribes each handler method to its event type on the inner `InMemoryEventPublisher`:

| Event | Handler method | Handler class |
|-------|----------------|---------------|
| `ApiKeyCreatedEvent` | `HandleApiKeyCreatedAsync` | `AuditLogEventHandler` |
| `ApiKeyRotatedEvent` | `HandleApiKeyRotatedAsync` | `AuditLogEventHandler` |
| `ApiKeyDisabledEvent` | `HandleApiKeyDisabledAsync` | `AuditLogEventHandler` |
| `ApiKeyUsedEvent` | `HandleApiKeyUsedAsync` | `UsageTrackingEventHandler` |
| `QuotaExhaustedEvent` | `HandleQuotaExhaustedAsync` | `UsageTrackingEventHandler` |
| `UsageWarningEvent` | `HandleUsageWarningAsync` | `UsageTrackingEventHandler` |
| `RateLimitExceededEvent` | `HandleRateLimitExceededAsync` | `RateLimitEventHandler` |

The method logs `Event subscriptions configured` at information level when it finishes.

## Usage

```csharp
// Program.cs
builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();
app.SubscribeEventHandlers();
```

Subscribe before the app starts handling requests. Events published before subscription have no handlers to run.

## Known issues

These were found by reading the code. They have not been confirmed by running the app.

- **Self-referencing `IEventPublisher` factory.** The factory for `IEventPublisher` calls `sp.GetRequiredService<IEventPublisher>()` to get the inner publisher. That resolves the same factory again, so the call recurses. The inner publisher needs to be resolved as `InMemoryEventPublisher` or registered under its concrete type.
- **`EventPublisherOptions` is not registered directly.** The factories call `GetRequiredService<EventPublisherOptions>()`. `Configure<T>` registers only `IOptions<T>` / `IOptionsSnapshot<T>` / `IOptionsMonitor<T>`, so the concrete type is not in the container and the call throws. Resolving `IOptions<EventPublisherOptions>` and reading `.Value` avoids this.
- **`InMemoryEventPublisher` is not registered under its concrete type.** `SubscribeEventHandlers` calls `GetRequiredService<InMemoryEventPublisher>()`, but only the `IEventPublisher` interface maps to that class. The lookup throws at startup.

Until these are fixed, `SubscribeEventHandlers` and the default `IEventPublisher` resolution are expected to fail at runtime.
