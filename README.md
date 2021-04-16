// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

// ...

## UsageEvent

The `UsageEvent` is an abstract base class for tracking API key usage events (successful requests, rate limits, quotas, etc.). It captures metadata like event ID, timestamp, API key ID, endpoint, HTTP status, and usage metrics, enabling consumption analysis and policy enforcement. Concrete events like `ApiKeyUsedEvent` and `RateLimitExceededEvent` extend this base class.

### Example Usage

```csharp
using ApiKeyGateway.Events;

var usageEvent = new ApiKeyUsedEvent
{
    ApiKeyId = "key_123",
    Endpoint = "/api/v1/data",
    HttpStatusCode = 200,
    ResponseTimeMs = 150,
    ResponseSizeBytes = 2048
};

Console.WriteLine($"Event ID: {usageEvent.EventId}");
Console.WriteLine($"Occurred at: {usageEvent.Timestamp}");
Console.WriteLine($"API Key: {usageEvent.ApiKeyId}");
Console.WriteLine($"Endpoint: {usageEvent.Endpoint}");
Console.WriteLine($"Status Code: {usageEvent.HttpStatusCode}");
```

## ApiKeyEvent

The `ApiKeyEvent` is an abstract base class for all API key lifecycle events (creation, rotation, disablement, etc.). It provides metadata like event ID, timestamp, and API key ID, enabling system-wide reactions to key changes without tight coupling. Concrete events extend this base class to add operation-specific details.

### Example Usage

```csharp
using ApiKeyGateway.Events;

var createdEvent = new ApiKeyCreatedEvent
{
    ApiKeyId = "key_123",
    Name = "ProductionKey",
    CreatedBy = "admin_user"
};

Console.WriteLine($"Event ID: {createdEvent.EventId}");
Console.WriteLine($"Occurred at: {createdEvent.Timestamp}");
Console.WriteLine($"API Key: {createdEvent.ApiKeyId}");
Console.WriteLine($"Created by: {createdEvent.CreatedBy}");
```

## AuditLogEventHandler

The `AuditLogEventHandler` class is responsible for logging key events to the audit trail. It handles three types of events: API key creation, rotation, and disablement. This class is designed to be used in conjunction with the `IServiceScopeFactory` to create a scope for logging and persisting audit logs.

### Example Usage

```csharp
using ApiKeyGateway.Events;
using ApiKeyGateway.Services;

public class Program
{
    public static async Task Main(string[] args)
    {
        var scopeFactory = new ServiceScopeFactory();
        var logger = new Logger<AuditLogEventHandler>();
        var auditLogEventHandler = new AuditLogEventHandler(scopeFactory, logger);

        // Log API key creation event
        var apiKeyCreatedEvent = new ApiKeyCreatedEvent("consumer_001", "DevKey");
        await auditLogEventHandler.HandleApiKeyCreatedAsync(apiKeyCreatedEvent);

        // Log API key rotation event
        var apiKeyRotatedEvent = new ApiKeyRotatedEvent("consumer_001", "RotatedKey");
        await auditLogEventHandler.HandleApiKeyRotatedAsync(apiKeyRotatedEvent);

        // Log API key disablement event
        var apiKeyDisabledEvent = new ApiKeyDisabledEvent("consumer_001", "DisabledKey");
        await auditLogEventHandler.HandleApiKeyDisabledAsync(apiKeyDisabledEvent);
    }
}
```

// ...
