# WebhookHandler

`WebhookHandler` delivers domain events to HTTP endpoints. A caller registers a URL with a list of event type names, and then passes events to `DeliverWebhookAsync`. Each matching subscription receives a `POST` with the event serialized as JSON. Deliveries are retried on transient exceptions with exponential backoff, and payloads can be signed with HMAC-SHA256.

The class is defined in `src/ApiKeyGateway/Integration/WebhookHandler.cs` and implements `IWebhookHandler` (`src/ApiKeyGateway/Integration/IWebhookHandler.cs`). It is registered in `src/ApiKeyGateway/Configuration/ServiceRegistrationExtensions.cs` as a scoped service:

```csharp
services.AddScoped<IWebhookHandler, WebhookHandler>();
```

> **Important:** subscriptions are kept in a `List` inside the instance. Because the service is scoped, each DI scope (for example, each HTTP request) starts with no subscriptions. Nothing is persisted, and state is not shared between processes. See [Limitations](#limitations).

## Types

### IWebhookHandler

The contract implemented by `WebhookHandler`. Inject this interface rather than the concrete class.

| Member | Returns | Description |
|---|---|---|
| `RegisterWebhookAsync(string url, string[] eventTypes, string? secret = null)` | `Task<string>` | Stores a new subscription and returns its ID. |
| `DeliverWebhookAsync<T>(T @event)` where `T : ApiKeyEvent` | `Task` | Sends the event to every active subscription whose event types include `typeof(T).Name`. |

### WebhookHandler

`public sealed class WebhookHandler : IWebhookHandler`

**Constructor:** `WebhookHandler(ILogger<WebhookHandler> logger, GatewayConfiguration gatewayConfig)`. Throws `ArgumentNullException` if either argument is `null`. The retry settings are read from `gatewayConfig` at delivery time.

### WebhookSubscription (private)

The subscription record is a private nested `record` and cannot be used from outside the class. Its fields are listed here because they describe the state the handler maintains:

| Property | Type | Description |
|---|---|---|
| `Id` | `string` | `Guid` string generated at registration. |
| `Url` | `string` | Target URL for `POST` requests. |
| `EventTypes` | `string[]` | Event type names the subscription receives. |
| `Secret` | `string?` | Signing secret. When set, payloads are signed. |
| `RegisteredAt` | `DateTime` | UTC time of registration. |
| `IsActive` | `bool` | `true` at registration. Nothing in the class sets it to `false`. |
| `LastDeliveryAt` | `DateTime?` | UTC time of the last successful delivery. |
| `TotalDeliveries` | `int` | Incremented once for each delivery whose final response is a success status. |
| `FailedDeliveries` | `int` | Incremented once for each delivery whose final response is not a success status. |

## Configuration

The retry policy is taken from `GatewayConfiguration` (`src/ApiKeyGateway/Domain/Models/GatewayConfiguration.cs`):

| Property | Default | Used as |
|---|---|---|
| `WebhookMaxRetries` | `3` | Maximum number of retries after the first attempt. |
| `WebhookBaseDelayMs` | `1000` | Delay before the first retry, in milliseconds. |
| `WebhookBackoffMultiplier` | `2.0` | Factor applied to the delay after each retry. |
| `WebhookMaxDelayMs` | `30000` | Upper bound for the delay between retries. |

The HTTP client comes from `HttpClientFactory.CreateWebhookClient()` (`src/ApiKeyGateway/Utilities/HttpClientFactory.cs`) with a 60-second timeout. A new client is created for each endpoint delivery and disposed afterwards.

## Methods

### RegisterWebhookAsync

```csharp
Task<string> RegisterWebhookAsync(string url, string[] eventTypes, string? secret = null)
```

Creates a subscription with a new `Guid` ID, stores it in memory, and returns the ID. The checks run in this order, and each one throws synchronously:

1. `url` is empty or whitespace: `ArgumentException` (parameter `url`).
2. `eventTypes` is `null` or empty: `ArgumentException` (parameter `eventTypes`).

Other checks are not made. The URL is not parsed or required to use HTTPS, and entries in `eventTypes` are not validated. `eventTypes` is stored by reference, so later changes to the array affect the subscription.

Event types are matched against `typeof(T).Name` at delivery time. Use the class name, for example `"ApiKeyCreatedEvent"`, not a dotted name such as `"apikey.created"`. A subscription with a misspelled name never receives anything, and no error is raised.

### DeliverWebhookAsync

```csharp
Task DeliverWebhookAsync<T>(T @event) where T : ApiKeyEvent
```

Throws `ArgumentNullException` if `@event` is `null`. Otherwise:

1. Selects the subscriptions where `IsActive` is `true` and `EventTypes` contains `typeof(T).Name`.
2. If none match, logs at debug level and returns.
3. Serializes the event once with `JsonSerializationHelper.SerializeCompact`.
4. Sends the payload to all matching subscriptions in parallel and waits for all of them with `Task.WhenAll`.

Each delivery is a `POST` with `Content-Type: application/json` and these headers:

| Header | Value |
|---|---|
| `X-Event-Id` | The event's `EventId`. |
| `X-Delivery-Attempt` | Always `1` (see [Limitations](#limitations)). |
| `X-Webhook-Signature` | `sha256=` followed by the lowercase hex HMAC-SHA256 of the payload, keyed with the subscription's `Secret`. Sent only when `Secret` is set. |

The signature is computed over the exact UTF-8 bytes of the JSON body, so a receiver should verify it against the raw request body before parsing.

Retries and outcomes:

- A response with a success status code counts as a success.
- A response with any other status code is **not retried**. It is counted as a failure, and a warning and an error are logged.
- An `HttpRequestException` or `TaskCanceledException` (including timeouts) is retried, up to `WebhookMaxRetries` times, with the configured backoff. If the last attempt still throws, the exception propagates out of `DeliverWebhookAsync`. The failure is not counted in `FailedDeliveries`.

Because `Task.WhenAll` is used, if one endpoint throws after its retries, `DeliverWebhookAsync` throws. Other endpoints still run to completion, but the caller only sees the first exception.

## Usage

The example below registers an endpoint for API key creation events and delivers one event. It uses the scoped `IWebhookHandler` from DI, so the registration and the delivery must happen in the same scope.

```csharp
using ApiKeyGateway.Events;
using ApiKeyGateway.Integration;

public sealed class ApiKeyNotifier
{
    private readonly IWebhookHandler _webhooks;

    public ApiKeyNotifier(IWebhookHandler webhooks)
    {
        _webhooks = webhooks;
    }

    public Task SubscribeAsync(string endpointUrl, string signingSecret)
    {
        return _webhooks.RegisterWebhookAsync(
            endpointUrl,
            new[] { nameof(ApiKeyCreatedEvent) },
            signingSecret);
    }

    public Task NotifyCreatedAsync(string apiKeyId, string keyName, string createdBy)
    {
        var @event = new ApiKeyCreatedEvent
        {
            ApiKeyId = apiKeyId,
            Name = keyName,
            CreatedBy = createdBy
        };

        return _webhooks.DeliverWebhookAsync(@event);
    }
}
```

The receiving endpoint can check the signature like this:

```csharp
using System.Security.Cryptography;
using System.Text;

static bool IsValidSignature(string rawBody, string signatureHeader, string secret)
{
    var key = Encoding.UTF8.GetBytes(secret);
    var hash = HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(rawBody));
    var expected = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();

    return CryptographicOperations.FixedTimeEquals(
        Encoding.UTF8.GetBytes(expected),
        Encoding.UTF8.GetBytes(signatureHeader));
}
```

For a unit test, the class can be built directly. It needs a logger and a `GatewayConfiguration`. Lower the retry settings so failing tests finish quickly:

```csharp
var config = new GatewayConfiguration
{
    WebhookMaxRetries = 0
};

var handler = new WebhookHandler(NullLogger<WebhookHandler>.Instance, config);
var id = await handler.RegisterWebhookAsync(
    "https://hooks.example.com/gateway",
    new[] { nameof(ApiKeyCreatedEvent) });
```

## Limitations

* **Scoped state.** Subscriptions are lost when the DI scope ends or the process restarts. A subscription registered during one request is not visible to a delivery in another request. Store subscriptions elsewhere if they must outlive a scope.
* **No unregister or deactivation.** The interface has no method to remove a subscription. `IsActive` is always `true`.
* **Separate from `WebhookManager`.** `WebhookManager` (`src/ApiKeyGateway/Integration/WebhookManager.cs`) keeps its own subscriptions and does not send anything. Subscriptions created through one are not seen by the other.
* **`X-Delivery-Attempt` is always `1`.** The header is set once when the request is built, and it is not updated on retries.
* **Request reuse on retry.** The same `HttpRequestMessage` instance is passed to `SendAsync` on every attempt. .NET's `HttpClient` does not allow a request message to be sent twice, so a retry after an `HttpRequestException` or `TaskCanceledException` is expected to fail with `InvalidOperationException`, which is not retried. This was found by reading the code and has not been confirmed by a test run. Non-success status codes are not affected because they are not retried.
* **Counters are per subscription.** `TotalDeliveries` and `FailedDeliveries` count one outcome per endpoint per call. They do not count individual retry attempts.
* **No URL or secret validation.** Any non-empty string is accepted as a URL, including `http://` URLs. Callers should validate the URL before registering it. `WebhookManager` enforces HTTPS, but `WebhookHandler` does not.
* **Not thread-safe.** The subscription list is a plain `List<T>`. Concurrent registration and delivery on the same instance are not safe.
* **Plain-text secrets.** `Secret` is stored as given, in memory.

## Related

* `src/ApiKeyGateway/Integration/WebhookHandler.cs`: the implementation.
* `src/ApiKeyGateway/Integration/IWebhookHandler.cs`: the interface.
* `src/ApiKeyGateway/Integration/WebhookManager.cs`: subscription bookkeeping, see [WebhookManager](WebhookManager.md).
* `src/ApiKeyGateway/Events/ApiKeyEvent.cs`: base type for events that can be delivered.
* `src/ApiKeyGateway/Utilities/RetryPolicyBuilder.cs`: the retry helper used for delivery.
* `src/ApiKeyGateway/Utilities/HttpClientFactory.cs`: creates the webhook `HttpClient`.
