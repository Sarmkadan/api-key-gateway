// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

// ...

## IWebhookManager

The `IWebhookManager` interface is responsible for managing webhook subscriptions and delivery coordination. It provides methods for registering, unregistering, and retrieving webhook subscriptions, as well as tracking delivery counts.

### Example Usage

```csharp
using ApiKeyGateway.Integration;

var webhookManager = new WebhookManager();

// Register a new webhook subscription
var webhookId = await webhookManager.RegisterWebhookAsync(
    "key_123",
    "https://example.com/webhook",
    new[] { "ApiKeyUsedEvent", "RateLimitExceededEvent" },
    "my_secret");

// Get all webhook subscriptions for a given API key
var webhooks = await webhookManager.GetWebhooksForKeyAsync("key_123");

// Unregister a webhook subscription
await webhookManager.UnregisterWebhookAsync(webhookId);

// Get the delivery count for a webhook subscription
var deliveryCount = await webhookManager.GetDeliveryCountAsync(webhookId);
```

// ...
