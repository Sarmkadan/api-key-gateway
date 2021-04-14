// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

// ...

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
