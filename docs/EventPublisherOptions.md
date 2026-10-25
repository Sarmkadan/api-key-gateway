# EventPublisherOptions

`EventPublisherOptions` is the configuration class for event publishing in the `api-key-gateway` project. It controls how `RetryingEventPublisher` retries failed event deliveries, how the backoff between attempts grows, and how the dead-letter queue (DLQ) stores events that could not be delivered. The type lives in `ApiKeyGateway.Events` (`src/ApiKeyGateway/Events/EventPublisherOptions.cs`) and is a `sealed` class with public settable properties.

## Configuration binding

`EventConfiguration.AddEventPublishing` binds the options from the `EventPublishing` configuration section:

```csharp
services.Configure<EventPublisherOptions>(configuration.GetSection("EventPublishing"));
```

The same options instance is used to build the `RetryingEventPublisher` and the `InMemoryDeadLetterQueue`. `Validate()` runs when the publisher is created, so an invalid configuration fails at first resolution of `IEventPublisher`, not at startup.

```json
{
  "EventPublishing": {
    "MaxRetryAttempts": 3,
    "InitialRetryDelayMs": 100,
    "MaxRetryDelayMs": 5000,
    "MaxDeadLetterQueueSize": 1000,
    "IncludeEventDetailsInDeadLetter": true
  }
}
```

The section is not present in the default `appsettings.json`, so the defaults below apply unless you add it.

## Properties

### `MaxRetryAttempts`
Maximum number of retries after the initial publish attempt fails.
- **Type**: `int`
- **Default**: `3` (1 initial attempt + 3 retries = 4 attempts in total)
- **Allowed range**: `0`–`10` (`0` disables retries; a failed event goes straight to the DLQ)
- **Validation**: `Validate()` rejects negative values with `ValidationException`.

### `InitialRetryDelayMs`
Base delay, in milliseconds, before the first retry. Later retries use exponential backoff: the delay before retry *n* is `InitialRetryDelayMs * 2^(n-1)`, plus a small random jitter (up to a quarter of the exponential delay), and the result is capped by `MaxRetryDelayMs`.
- **Type**: `int`
- **Default**: `100`
- **Allowed range**: `1`–`10000`
- **Validation**: `Validate()` rejects values below `1`.

### `MaxRetryDelayMs`
Upper bound, in milliseconds, for any single delay between retries. It stops the exponential growth from producing unbounded waits.
- **Type**: `int`
- **Default**: `5000` (5 seconds)
- **Allowed range**: `1`–`30000`
- **Validation**: `Validate()` rejects values below `1`. The value is not compared against `InitialRetryDelayMs`, so an initial delay larger than the cap is clamped to the cap on the first retry.

### `MaxDeadLetterQueueSize`
Maximum number of failed events kept in the dead-letter queue. When the limit is reached, the oldest entry is discarded (FIFO).
- **Type**: `int`
- **Default**: `1000`
- **Allowed range**: `0`–`10000`
- **Validation**: `Validate()` rejects negative values. `0` keeps no entries.

### `IncludeEventDetailsInDeadLetter`
Controls how much information a DLQ entry stores.
- **Type**: `bool`
- **Default**: `true`
- **When `true`**: the entry includes the event payload, so the event can be inspected or replayed.
- **When `false`**: only metadata is stored (event type, timestamp, and failure reason). Use this when events may contain sensitive data that should not be kept in memory.

## Methods

### `Validate()`
Checks the lower bounds of `MaxRetryAttempts`, `InitialRetryDelayMs`, `MaxRetryDelayMs` and `MaxDeadLetterQueueSize`.
- **Throws**: `ValidationException` when a value is out of bounds.

`Validate()` does not check the upper bounds. Those are documented by the `[Range]` attributes on each property, which describe the intended limits.

## Notes

- **Retry scope**: A retry re-runs the entire inner `PublishAsync` call for the event. Any exception thrown out of it counts as a failed attempt.
- **DLQ lifetime**: The DLQ is in memory (`InMemoryDeadLetterQueue`). Entries are lost on restart and are not shared between instances.
- **Changing values**: The options are read when the publisher and DLQ are created (singletons), so changes made to the configuration section are not picked up until the application restarts.
- **Related**: See [IDeadLetterQueue](IDeadLetterQueue.md) for how DLQ entries are read and managed.
