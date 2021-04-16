// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

// ...

## IWebhookHandler

The `IWebhookHandler` interface manages webhook subscriptions and event delivery with retry logic and HMAC signing. It supports registering webhooks for specific event types, tracking delivery statistics, and ensuring reliable delivery through exponential backoff.

### Example Usage

```csharp
using ApiKeyGateway.Integration;
using ApiKeyGateway.Events;

var webhookHandler = new WebhookHandler();

// Register a webhook subscription
var subscriptionId = await webhookHandler.RegisterWebhookAsync(
    url: "https://example.com/webhook-endpoint",
    eventTypes: new[] { "ApiKeyCreated", "QuotaExceeded" },
    secret: "my-webhook-secret");

// Create a sample event to deliver
var sampleEvent = new SampleEvent
{
    EventId = Guid.NewGuid(),
    Timestamp = DateTime.UtcNow,
    ApiKeyId = "key_123"
};

// Deliver the event to all matching subscriptions
await webhookHandler.DeliverWebhookAsync(sampleEvent);

// Access subscription metadata
var subscription = webhookHandler.GetSubscription(subscriptionId); // Hypothetical helper method
Console.WriteLine($"Webhook {subscription.Id} has {subscription.TotalDeliveries} successful deliveries");
```

Where `SampleEvent` is a custom event implementing `ApiKeyEvent`:
```csharp
public class SampleEvent : ApiKeyEvent
{
    public Guid EventId { get; set; }
    public DateTime Timestamp { get; set; }
    public string ApiKeyId { get; set; }
}
```

// ...
