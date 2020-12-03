# IWebhookHandler
The `IWebhookHandler` interface represents a handler for webhooks, providing a standardized way to manage and interact with webhooks. It allows for registration, delivery, and tracking of webhooks, making it a crucial component in the `api-key-gateway` project.

## API
* `WebhookHandler`: The constructor for the `IWebhookHandler` implementation.
* `Task<string> RegisterWebhookAsync`: Registers a new webhook and returns a unique identifier for the registered webhook. This method is asynchronous and may throw exceptions if the registration process fails.
* `async Task DeliverWebhookAsync<T>`: Delivers a webhook payload of type `T` to the registered webhook. This method is asynchronous and may throw exceptions if the delivery process fails.
* `string Id`: Gets the unique identifier of the webhook.
* `string Url`: Gets the URL of the webhook.
* `string[] EventTypes`: Gets the event types that the webhook is subscribed to.
* `string? Secret`: Gets the secret key associated with the webhook, if any.
* `DateTime RegisteredAt`: Gets the date and time when the webhook was registered.
* `bool IsActive`: Gets a value indicating whether the webhook is active.
* `DateTime? LastDeliveryAt`: Gets the date and time of the last successful delivery, if any.
* `int TotalDeliveries`: Gets the total number of deliveries attempted for the webhook.
* `int FailedDeliveries`: Gets the total number of failed deliveries for the webhook.

## Usage
The following examples demonstrate how to use the `IWebhookHandler` interface:
```csharp
// Example 1: Registering a new webhook
var webhookHandler = new WebhookHandler();
var webhookId = await webhookHandler.RegisterWebhookAsync();
Console.WriteLine($"Webhook registered with ID: {webhookId}");

// Example 2: Delivering a webhook payload
var payload = new MyWebhookPayload { Message = "Hello, World!" };
await webhookHandler.DeliverWebhookAsync<MyWebhookPayload>(payload);
Console.WriteLine("Webhook payload delivered successfully");
```

## Notes
When using the `IWebhookHandler` interface, consider the following edge cases and thread-safety remarks:
* The `RegisterWebhookAsync` method may throw exceptions if the registration process fails, such as due to network errors or invalid input.
* The `DeliverWebhookAsync` method may throw exceptions if the delivery process fails, such as due to network errors or invalid payload.
* The `IWebhookHandler` interface is designed to be thread-safe, allowing multiple threads to access and manipulate the webhook handler concurrently.
* However, it is still important to ensure that the `IWebhookHandler` implementation is properly synchronized to prevent data corruption or other concurrency-related issues.
* When handling webhook payloads, it is essential to validate and sanitize the input data to prevent security vulnerabilities, such as injection attacks.
