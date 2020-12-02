# AuditLogEventHandler

The `AuditLogEventHandler` class serves as a centralized orchestrator for processing and persisting audit trails related to API key lifecycle events, usage metrics, and security constraints within the `api-key-gateway` project. It aggregates specialized handlers for distinct operational domains—specifically API key management, usage tracking, and rate limiting—to ensure consistent logging behavior across asynchronous event streams. By delegating specific event types to dedicated sub-handlers, this class maintains a clear separation of concerns while providing a unified interface for the gateway's event subscription model.

## API

### Constructors

#### `public AuditLogEventHandler()`
Initializes a new instance of the `AuditLogEventHandler` class. This constructor typically instantiates and wires up the internal dependencies required for `UsageTrackingEventHandler` and `RateLimitEventHandler`, preparing the instance to process incoming events immediately.

### Methods

#### `public async Task HandleApiKeyCreatedAsync`
Processes the audit log entry when a new API key is successfully generated.
*   **Purpose**: Records the creation event, including metadata such as the key identifier, owner, and initial permissions.
*   **Parameters**: Accepts event data context implied by the gateway's event bus (specific signature details depend on the injected event model).
*   **Return Value**: Returns a `Task` that completes when the audit record has been successfully persisted.
*   **Throws**: Throws an exception if the underlying storage mechanism fails or if the event payload is malformed.

#### `public async Task HandleApiKeyRotatedAsync`
Processes the audit log entry when an existing API key is rotated.
*   **Purpose**: Logs the rotation event, capturing the transition from the old key version to the new one to maintain a secure history.
*   **Parameters**: Accepts event data context containing the key identifier and rotation timestamp.
*   **Return Value**: Returns a `Task` that completes upon successful logging.
*   **Throws**: Throws an exception if the audit write operation fails.

#### `public async Task HandleApiKeyDisabledAsync`
Processes the audit log entry when an API key is disabled or revoked.
*   **Purpose**: Records the disablement event, noting the reason for revocation and the effective time of the status change.
*   **Parameters**: Accepts event data context containing the key identifier and disablement reason.
*   **Return Value**: Returns a `Task` that completes upon successful logging.
*   **Throws**: Throws an exception if the audit write operation fails.

#### `public async Task HandleApiKeyUsedAsync`
Delegates the processing of API key usage events to the `UsageTrackingEventHandler`.
*   **Purpose**: Logs individual request attempts associated with a specific API key for traffic analysis and billing.
*   **Parameters**: Accepts event data context containing usage metrics (e.g., timestamp, endpoint, latency).
*   **Return Value**: Returns a `Task` that completes when the usage record is processed.
*   **Throws**: Propagates exceptions thrown by the internal `UsageTrackingEventHandler`.

#### `public async Task HandleQuotaExhaustedAsync`
Delegates the processing of quota exhaustion events to the `UsageTrackingEventHandler`.
*   **Purpose**: Records when an API key exceeds its allocated usage quota, triggering potential alerts or hard blocks.
*   **Parameters**: Accepts event data context containing the key identifier and quota limit details.
*   **Return Value**: Returns a `Task` that completes when the event is logged.
*   **Throws**: Propagates exceptions thrown by the internal `UsageTrackingEventHandler`.

#### `public async Task HandleUsageWarningAsync`
Delegates the processing of usage threshold warning events to the `UsageTrackingEventHandler`.
*   **Purpose**: Logs warnings when an API key approaches its usage limit, allowing for proactive notification.
*   **Parameters**: Accepts event data context containing current usage percentage and threshold details.
*   **Return Value**: Returns a `Task` that completes when the event is logged.
*   **Throws**: Propagates exceptions thrown by the internal `UsageTrackingEventHandler`.

#### `public async Task HandleRateLimitExceededAsync`
Delegates the processing of rate limit violation events to the `RateLimitEventHandler`.
*   **Purpose**: Records instances where an API key exceeds the allowed request frequency, essential for security monitoring.
*   **Parameters**: Accepts event data context containing the key identifier, limit type, and excess count.
*   **Return Value**: Returns a `Task` that completes when the event is logged.
*   **Throws**: Propagates exceptions thrown by the internal `RateLimitEventHandler`.

### Properties

#### `public UsageTrackingEventHandler UsageTrackingEventHandler`
Gets the internal handler responsible for processing usage-related events (`HandleApiKeyUsedAsync`, `HandleQuotaExhaustedAsync`, `HandleUsageWarningAsync`). This property exposes the underlying component for inspection or advanced configuration but should generally remain encapsulated.

#### `public RateLimitEventHandler RateLimitEventHandler`
Gets the internal handler responsible for processing rate-limiting events (`HandleRateLimitExceededAsync`). This property exposes the underlying component for inspection or advanced configuration.

## Usage

### Example 1: Direct Event Handling
This example demonstrates instantiating the handler and directly invoking methods in response to specific domain events within a service layer.

```csharp
using ApiKeyGateway.Handlers;
using ApiKeyGateway.Events;

public class KeyManagementService
{
    private readonly AuditLogEventHandler _auditHandler;

    public KeyManagementService(AuditLogEventHandler auditHandler)
    {
        _auditHandler = auditHandler;
    }

    public async Task RotateKeyAsync(string keyId)
    {
        // Perform rotation logic...
        
        // Create and dispatch the audit event
        var rotationEvent = new ApiKeyRotatedEvent(keyId, DateTime.UtcNow);
        await _auditHandler.HandleApiKeyRotatedAsync(rotationEvent);
    }

    public async Task LogUsageAsync(string keyId, long latencyMs)
    {
        var usageEvent = new ApiKeyUsedEvent(keyId, latencyMs, DateTime.UtcNow);
        await _auditHandler.HandleApiKeyUsedAsync(usageEvent);
    }
}
```

### Example 2: Event Bus Subscription
This example illustrates how the handler might be wired into a generic event bus to automatically route events based on type.

```csharp
using ApiKeyGateway.Handlers;
using ApiKeyGateway.Events;
using System.Threading.Tasks;

public class EventBusConfiguration
{
    public static void Configure(AuditLogEventHandler handler, IEventBus bus)
    {
        // Subscribe lifecycle events
        bus.Subscribe<ApiKeyCreatedEvent>(async e => await handler.HandleApiKeyCreatedAsync(e));
        bus.Subscribe<ApiKeyDisabledEvent>(async e => await handler.HandleApiKeyDisabledAsync(e));
        
        // Subscribe usage events
        bus.Subscribe<ApiKeyUsedEvent>(async e => await handler.HandleApiKeyUsedAsync(e));
        bus.Subscribe<QuotaExhaustedEvent>(async e => await handler.HandleQuotaExhaustedAsync(e));
        
        // Subscribe security events
        bus.Subscribe<RateLimitExceededEvent>(async e => await handler.HandleRateLimitExceededAsync(e));
    }
}
```

## Notes

*   **Thread Safety**: The `AuditLogEventHandler` and its delegated sub-handlers (`UsageTrackingEventHandler`, `RateLimitEventHandler`) are designed to be thread-safe for concurrent invocations. Multiple asynchronous calls to methods like `HandleApiKeyUsedAsync` and `HandleRateLimitExceededAsync` can occur simultaneously without data corruption, assuming the underlying storage implementations support concurrent writes.
*   **Exception Propagation**: All `Handle...Async` methods are `async Task` returning void-equivalent tasks. If the underlying persistence layer fails (e.g., database connection loss), the exception is not swallowed; it propagates to the caller. Callers should implement appropriate retry policies or circuit breakers when invoking these methods to prevent audit log loss from destabilizing the main request pipeline.
*   **Event Ordering**: While individual handler executions are thread-safe, strict global ordering of events (e.g., ensuring `HandleApiKeyCreatedAsync` strictly completes before `HandleApiKeyUsedAsync` for the same key) is not guaranteed by this class alone if events arrive concurrently from different threads. Ordering guarantees must be enforced by the upstream event producer or the storage layer.
*   **Dependency Lifecycle**: The exposed properties `UsageTrackingEventHandler` and `RateLimitEventHandler` return references to internal components. Modifying the state of these returned instances externally may lead to unpredictable behavior in the `AuditLogEventHandler` logic and should be avoided unless explicitly supported by the component's public API.
