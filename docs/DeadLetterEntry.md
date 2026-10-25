# DeadLetterEntry

The `DeadLetterEntry` class records an event that could not be published and was moved to the dead-letter queue. It keeps the failure reason, the number of retries made, and optionally the serialized event, so the failure can be inspected or replayed later.

The type is defined in `src/ApiKeyGateway/Events/DeadLetterEntry.cs`. It is sealed. Entries are stored through `IDeadLetterQueue` (implemented by `InMemoryDeadLetterQueue`) and are created by `RetryingEventPublisher` when retries for an event are exhausted.

## API

`Id` and `FailedAt` are assigned automatically and cannot be set by callers. The other properties are set with an object initializer and are read-only after construction.

* `Id`: A unique identifier for this entry, generated with `Guid.NewGuid()` and returned as a string.
* `FailedAt`: The UTC time the entry was created, set to `DateTimeOffset.UtcNow`. This is when the event was moved to the dead-letter queue.
* `EventType`: The full name of the event's .NET type (`typeof(T).FullName`, falling back to `typeof(T).Name`). Defaults to `string.Empty`.
* `EventPayload`: The event serialized to JSON with `System.Text.Json`. It is `null` when the entry was created without event details. Null properties are omitted from the JSON.
* `FailureReason`: The `Message` of the exception that caused the failure. Defaults to `string.Empty`.
* `StackTrace`: The `StackTrace` of the exception, if the exception has one. Exceptions that were never thrown have no stack trace, so this can be `null`.
* `RetryAttempts`: The number of retry attempts made before the event was moved to the dead-letter queue, as an `int`.

## Factory methods

Use the static factory methods rather than the object initializer. They fill in `EventType`, `FailureReason`, and `StackTrace` from the arguments.

* `Create<T>(T @event, Exception failureReason, int retryAttempts, bool includeEventDetails = true)`: Creates an entry for the event. When `includeEventDetails` is `true`, the event is serialized into `EventPayload`. When it is `false`, `EventPayload` is `null`. Both `@event` and `failureReason` must be non-null, or `ArgumentNullException` is thrown.
* `CreateMinimal<T>(Exception failureReason, int retryAttempts)`: Creates an entry with metadata only. `EventPayload` is always `null`, and no event instance is needed, so this is useful when the event object is not available or should not be stored. `failureReason` must be non-null.

`RetryingEventPublisher` calls `Create` when `IncludeEventDetailsInDeadLetter` is enabled and `CreateMinimal<T>` otherwise.

## Usage

Read entries from the queue and inspect them. `IDeadLetterQueue` is registered as a singleton in `Configuration/EventConfiguration.cs`:

```csharp
IDeadLetterQueue queue = serviceProvider.GetRequiredService<IDeadLetterQueue>();

foreach (DeadLetterEntry entry in queue.GetAll())
{
    logger.LogWarning(
        "Dead-lettered {EventType} at {FailedAt} after {RetryAttempts} retries: {FailureReason}",
        entry.EventType,
        entry.FailedAt,
        entry.RetryAttempts,
        entry.FailureReason);
}
```

Create an entry directly, for example in tests:

```csharp
var entry = DeadLetterEntry.Create(
    new QuotaThresholdReachedEvent { ConsumerId = "consumer-1" },
    new InvalidOperationException("Subscriber failed"),
    retryAttempts: 3);
```

## Notes

* `EventPayload` is built with default `System.Text.Json` options and `WhenWritingNull`. It is a snapshot taken at creation time. Changes to the event object after the entry is created are not reflected.
* Storing the payload can expose data from the event. Use `includeEventDetails: false` or `CreateMinimal<T>` for events that contain sensitive values.
* `FailureReason` contains only the exception message, not the full exception chain. Inner exceptions are not captured.
* Because `Id` and `FailedAt` are generated in the constructor, two entries created from the same event are always distinct.
* `EventType` is the full type name, so renaming or moving the event type changes the value stored in existing entries.
