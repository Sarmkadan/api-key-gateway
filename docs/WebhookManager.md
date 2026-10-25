# WebhookManager

`WebhookManager` manages webhook subscriptions for API keys: it registers a subscription with a target HTTPS URL and a list of event types, lists the active subscriptions for a key, removes subscriptions, and reports a per-subscription delivery count.

The type and its interface `IWebhookManager` are defined in `src/ApiKeyGateway/Integration/WebhookManager.cs`, together with the `WebhookSubscription` record. It is registered in `src/ApiKeyGateway/Configuration/ServiceRegistrationExtensions.cs` as a scoped service:

```csharp
services.AddScoped<IWebhookManager, WebhookManager>();
```

> **Important:** subscriptions are kept in an in-memory dictionary inside the instance. Because the service is scoped, each DI scope (for example, each HTTP request) gets its own empty store, so a subscription registered in one request is not visible in another. Nothing is persisted to SQL Server or any other store, and state is not shared between processes. See [Limitations](#limitations).

## Types

### IWebhookManager

The contract implemented by `WebhookManager`. Inject this interface rather than the concrete class.

| Member | Returns | Description |
|---|---|---|
| `RegisterWebhookAsync(string apiKeyId, string targetUrl, string[] eventTypes, string? secret = null)` | `Task<string>` | Validates the input, stores a new active subscription, and returns its ID. |
| `UnregisterWebhookAsync(string webhookId)` | `Task` | Removes a subscription. Does nothing if the ID is unknown. |
| `GetWebhooksForKeyAsync(string apiKeyId)` | `Task<IEnumerable<WebhookSubscription>>` | Returns the active subscriptions registered for the key. |
| `GetDeliveryCountAsync(string webhookId)` | `Task<int>` | Returns the stored `TotalDeliveries` value, or `0` if the subscription is unknown. |

### WebhookSubscription

A `record` describing one subscription. All properties have public setters.

| Property | Type | Description |
|---|---|---|
| `Id` | `string` | Unique identifier, a `Guid` string generated at registration. |
| `ApiKeyId` | `string` | The API key that owns the subscription. |
| `TargetUrl` | `string` | HTTPS URL that receives deliveries. |
| `EventTypes` | `string[]` | Event types the subscription receives. |
| `Secret` | `string?` | Optional shared secret, stored as given. |
| `IsActive` | `bool` | `true` when the subscription is created. |
| `RegisteredAt` | `DateTime` | UTC time of registration. |
| `LastDeliveryAt` | `DateTime?` | UTC time of the most recent delivery attempt, or `null`. |
| `TotalDeliveries` | `int` | Total number of delivery attempts. |
| `FailedDeliveries` | `int` | Number of failed delivery attempts. |

### WebhookManager

`public sealed class WebhookManager : IWebhookManager`

**Constructor:** `WebhookManager(ILogger<WebhookManager> logger)`. Throws `ArgumentNullException` if `logger` is `null`.

## Methods

### RegisterWebhookAsync

```csharp
Task<string> RegisterWebhookAsync(string apiKeyId, string targetUrl, string[] eventTypes, string? secret = null)
```

Creates a subscription with `IsActive = true` and `RegisteredAt = DateTime.UtcNow`, stores it, and returns its `Id`. The checks run in this order:

1. `apiKeyId` is empty or whitespace: `ArgumentException` (parameter `apiKeyId`).
2. `targetUrl` is empty or whitespace: `ArgumentException` (parameter `targetUrl`).
3. `eventTypes` is `null` or empty: `ArgumentException` (parameter `eventTypes`).
4. `targetUrl` does not start with `https://` (case-insensitive): `ValidationException` with message `Webhook URLs must use HTTPS`.
5. `targetUrl` is not an absolute URI: `ValidationException` with message `Invalid webhook URL format`.

The array is only checked for being non-empty. Individual entries are not validated, and the names are not checked against a known list of event types. `eventTypes` is stored by reference, not copied.

### UnregisterWebhookAsync

```csharp
Task UnregisterWebhookAsync(string webhookId)
```

Removes the subscription with the given ID and logs the removal. An unknown ID is not an error. Throws `ArgumentException` if `webhookId` is empty or whitespace.

### GetWebhooksForKeyAsync

```csharp
Task<IEnumerable<WebhookSubscription>> GetWebhooksForKeyAsync(string apiKeyId)
```

Returns a new list with the subscriptions where `ApiKeyId` matches and `IsActive` is `true`. The list contains the same record instances stored in the manager, so changes made to a returned record are visible to later calls. Throws `ArgumentException` if `apiKeyId` is empty or whitespace.

### GetDeliveryCountAsync

```csharp
Task<int> GetDeliveryCountAsync(string webhookId)
```

Returns `TotalDeliveries` for the subscription, or `0` if no subscription has that ID. Throws `ArgumentException` if `webhookId` is empty or whitespace.

## Error handling

Argument problems throw `ArgumentException` or `ValidationException` (`ApiKeyGateway.Domain.Exceptions`). `ValidationException` derives from `ApiKeyGatewayException`, so `ErrorHandlingMiddleware` maps it to a ProblemDetails response when it escapes a request.

Each method also wraps unexpected exceptions from the dictionary operations in `DataAccessException`, after logging them. In practice this path is not reached with the current in-memory store.

The methods are not declared `async`. They run synchronously and return completed tasks, so argument checks throw when the method is called rather than through the returned `Task`. Callers that use `await` see the same behavior either way.

## Usage

The example below assumes an endpoint that lets an API key owner manage webhooks. It uses the scoped `IWebhookManager` from DI:

```csharp
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Integration;

public sealed class WebhookSubscriptionService
{
    private readonly IWebhookManager _webhooks;

    public WebhookSubscriptionService(IWebhookManager webhooks)
    {
        _webhooks = webhooks;
    }

    public async Task<string> SubscribeAsync(string apiKeyId, string url, string signingSecret)
    {
        try
        {
            return await _webhooks.RegisterWebhookAsync(
                apiKeyId,
                url,                                   // must start with https://
                new[] { "quota.threshold.reached", "apikey.revoked" },
                signingSecret);
        }
        catch (ValidationException ex)
        {
            // Non-HTTPS or malformed URL
            throw new InvalidOperationException($"Webhook rejected: {ex.Message}", ex);
        }
    }

    public async Task ListAsync(string apiKeyId)
    {
        foreach (var subscription in await _webhooks.GetWebhooksForKeyAsync(apiKeyId))
        {
            Console.WriteLine($"{subscription.Id} -> {subscription.TargetUrl} ({string.Join(", ", subscription.EventTypes)})");
        }
    }

    public async Task<int> GetAttemptsAsync(string webhookId)
    {
        return await _webhooks.GetDeliveryCountAsync(webhookId);
    }

    public Task UnsubscribeAsync(string webhookId)
    {
        return _webhooks.UnregisterWebhookAsync(webhookId);
    }
}
```

Direct use, for example in a unit test, needs only a logger:

```csharp
var manager = new WebhookManager(NullLogger<WebhookManager>.Instance);

var id = await manager.RegisterWebhookAsync(
    "key-123",
    "https://hooks.example.com/gateway",
    new[] { "quota.threshold.reached" });

var active = await manager.GetWebhooksForKeyAsync("key-123");  // contains one subscription
await manager.UnregisterWebhookAsync(id);
```

## Limitations

The current implementation covers subscription bookkeeping only. Keep these points in mind before relying on it:

* **No persistence.** Subscriptions live in a `Dictionary<string, WebhookSubscription>` and are lost when the scope ends or the process restarts. The class summary says "with persistence", but no persistence exists in this file.
* **No delivery.** `WebhookManager` does not send HTTP requests and does not sign payloads with `Secret`. `LastDeliveryAt`, `TotalDeliveries`, and `FailedDeliveries` are only data fields; nothing in this class updates them. `GetDeliveryCountAsync` therefore returns `0` for every subscription unless some other code changes the record.
* **Separate from `WebhookHandler`.** Delivery is done by `WebhookHandler` (`src/ApiKeyGateway/Integration/WebhookHandler.cs`), which has its own `RegisterWebhookAsync` and its own subscription store. Subscriptions created here are not passed to it, and it does not read `WebhookManager`'s `Secret`. `WebhookHandler` signs payloads with HMAC-SHA256 in the `X-Webhook-Signature` header when its subscription has a secret.
* **Deactivation is not implemented.** `IsActive` is always `true`. The only way to stop a subscription is `UnregisterWebhookAsync`.
* **Secrets are stored in plain text** in memory, as provided by the caller.
* **Not thread-safe.** The dictionary is a plain `Dictionary`, so concurrent registration and removal are not safe.
* **Mutable records.** Returned `WebhookSubscription` instances are the stored objects, so callers can change them without going through the manager.
* **No event-type validation.** Any non-empty strings are accepted as event types.

## Related

* `src/ApiKeyGateway/Integration/WebhookManager.cs`: the implementation.
* `tests/api-key-gateway.Tests/WebhookManagerTests.cs`: unit tests for this class.
* `src/ApiKeyGateway/Domain/Exceptions/ValidationException.cs`, `DataAccessException.cs`: exceptions thrown by the methods.
