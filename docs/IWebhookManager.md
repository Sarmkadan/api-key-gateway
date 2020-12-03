# IWebhookManager

Centralizes management of webhook subscriptions for API key events, enabling external systems to receive real-time notifications when specific events occur. It tracks subscription metadata, delivery status, and failure counts to support observability and retry mechanisms.

## API

### Properties

#### `Id`
Unique identifier for the webhook subscription. Read-only; assigned at registration.

#### `ApiKeyId`
Identifier of the API key associated with this webhook. Read-only; set at creation.

#### `TargetUrl`
Destination URL where webhook payloads are delivered. Must be a valid HTTPS endpoint unless overridden by environment configuration.

#### `EventTypes`
Array of event names this webhook subscribes to (e.g., `"created"`, `"revoked"`). Empty array indicates subscription to all events.

#### `Secret`
Optional symmetric key used to sign payloads. If `null`, payloads are delivered unsigned.

#### `IsActive`
Indicates whether the webhook is enabled for delivery. Inactive webhooks are ignored during event processing.

#### `RegisteredAt`
Timestamp when the webhook was registered. Read-only; immutable after creation.

#### `LastDeliveryAt`
Timestamp of the most recent successful delivery attempt. `null` if never delivered.

#### `TotalDeliveries`
Cumulative count of delivery attempts, including failures.

#### `FailedDeliveries`
Cumulative count of failed delivery attempts.

### Methods

#### `RegisterWebhookAsync`
Registers a new webhook subscription.
