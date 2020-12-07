# ApiKeyConsumer

Represents a consumer of API keys within the `api-key-gateway` system, encapsulating metadata, configuration, and lifecycle management for API key usage.

## API

### Properties

- **`Id`** (string)
  Unique identifier for the consumer. Used as a primary reference across the system. Read-only.

- **`Name`** (string)
  Human-readable name of the consumer. Required for display and identification purposes.

- **`Email`** (string)
  Contact email address for the consumer. Used for notifications and administrative communication.

- **`Organization`** (string)
  Name of the organization associated with the consumer. May be used for grouping and access control.

- **`Tier`** (string)
  Subscription tier or plan level assigned to the consumer. Affects rate limits, quotas, and feature access.

- **`IsActive`** (bool)
  Indicates whether the consumer is currently active and able to use API keys. Updated via `Activate` and `Deactivate`.

- **`CreatedAt`** (DateTime)
  Timestamp when the consumer record was created. Immutable after creation.

- **`InactiveSince`** (DateTime?)
  Timestamp when the consumer was deactivated, if applicable. `null` indicates the consumer is active.

- **`ContactPerson`** (string?)
  Name of a specific contact person within the consumer's organization. Optional field for escalation.

- **`Notes`** (string?)
  Free-form text field for internal remarks or operational context. Not exposed externally.

- **`TotalApiKeys`** (int)
  Current count of active API keys associated with this consumer. Updated automatically as keys are created or revoked.

- **`LastActivityAt`** (DateTime?)
  Timestamp of the most recent API key usage by this consumer. Updated via `UpdateLastActivity`. `null` indicates no activity.

- **`WebhookUrl`** (string?)
  Optional URL to which webhook notifications should be sent (e.g., for quota alerts or key expiration). Must be a valid HTTPS endpoint if provided.

- **`CustomProperties`** (Dictionary<string, string>)
  Collection of key-value pairs for arbitrary metadata. Used to extend consumer attributes without schema changes. Keys are case-sensitive.

### Methods

- **`Deactivate()`**
  Marks the consumer as inactive and sets `InactiveSince` to the current UTC timestamp. Throws if the consumer is already inactive. Does not affect existing API keys directly (they remain valid until revoked).

- **`Activate()`**
  Marks the consumer as active and clears `InactiveSince`. Throws if the consumer is already active.

- **`UpdateLastActivity()`**
  Updates `LastActivityAt` to the current UTC timestamp. Used to track recent usage without requiring a full key operation.

- **`IsValid()`** → bool
  Returns `true` if the consumer is active (`IsActive == true`) and not expired (no automatic expiration logic is enforced by this type). Returns `false` otherwise.

## Usage

```csharp
// Example 1: Creating and activating a new consumer
var consumer = new ApiKeyConsumer
{
    Id = "consumer-123",
    Name = "Acme Corp API",
    Email = "api@acme.example",
    Organization = "Acme Corp",
    Tier = "enterprise",
    ContactPerson = "Jane Doe",
    WebhookUrl = "https://acme.example/webhook/api-events"
};

consumer.Activate();
consumer.UpdateLastActivity();

// Store or transmit the consumer record
```

```csharp
// Example 2: Checking consumer validity and deactivating
if (!consumer.IsValid())
{
    Console.WriteLine("Consumer is not valid.");
    return;
}

if (consumer.TotalApiKeys > 100)
{
    Console.WriteLine("High key count detected. Proceeding with caution.");
}

consumer.Deactivate();
```

## Notes

- **Thread Safety**: This type is not thread-safe. External synchronization is required when modifying shared instances from multiple threads, especially for `CustomProperties` and methods like `UpdateLastActivity`.

- **Immutability**: `Id`, `CreatedAt`, and `TotalApiKeys` should be treated as immutable after construction. Changes to these fields may lead to inconsistent state.

- **Validation**: The system may enforce additional constraints (e.g., valid email format, webhook URL validation) at persistence or usage time, though this type does not perform such checks internally.

- **WebhookUrl**: If set, the URL must be HTTPS and reachable at the time of use. Misconfigured URLs may result in failed notifications.

- **Deactivation**: Deactivated consumers remain in the system for auditability. Reactivation restores access without recreating keys.
