# IDeadLetterQueue

`IDeadLetterQueue` defines a store for events that could not be published after all retry attempts were exhausted. Each failed event is captured as a `DeadLetterEntry` that records the event type, an optional JSON payload, the failure message, the stack trace, and the number of retries made. Keeping these entries lets operators inspect and diagnose failures instead of silently losing events.

The interface is declared in `src/ApiKeyGateway/Events/IDeadLetterQueue.cs`. Its default implementation is `InMemoryDeadLetterQueue`, which is registered as a singleton in `Configuration/EventConfiguration.cs`. Other implementations (file-based, database-backed) can be substituted through the same interface.

## API

### int Count

Gets the number of entries currently held in the queue.

- **Returns**: A non-negative `int`. `0` when the queue is empty.
- **Used by**: `RetryingEventPublisher` reads this value when it logs the queue state after moving an event to the queue.

### bool IsEmpty

Gets a value indicating whether the queue holds no entries. Equivalent to `Count == 0`.

- **Returns**: `true` when the queue is empty; otherwise `false`.

### void Add

Adds a failed event to the back of the queue.

- **Parameters**:
  - `DeadLetterEntry entry`: The entry describing the failed event. Typically created with `DeadLetterEntry.Create<T>(...)` or `DeadLetterEntry.CreateMinimal<T>(...)`.
- **Returns**: `void`. The entry is stored before the method returns.
- **Behavior**: Entries are kept in FIFO order. A bounded implementation may evict the oldest entry to make room for the new one. Callers should not rely on `Add` failing when the queue is full.
- **Throws**: `ArgumentNullException` when `entry` is `null` (in `InMemoryDeadLetterQueue`).

### IReadOnlyList\<DeadLetterEntry\> GetAll

Returns a snapshot of every entry in the queue, oldest first. Entries are not removed.

- **Parameters**: None.
- **Returns**: A read-only list. The list is a copy, so later changes to the queue do not affect it. Returns an empty list when the queue is empty.

### void Clear

Removes all entries from the queue.

- **Parameters**: None.
- **Returns**: `void`. After the call, `Count` is `0` and `IsEmpty` is `true`.

### DeadLetterEntry? Take

Removes the oldest entry from the queue and returns it. Use this to process entries exactly once, for example when replaying them.

- **Parameters**: None.
- **Returns**: The oldest `DeadLetterEntry`, or `null` when the queue is empty.

### DeadLetterEntry? Peek

Returns the oldest entry without removing it.

- **Parameters**: None.
- **Returns**: The oldest `DeadLetterEntry`, or `null` when the queue is empty.

## Contract

- **Ordering**: Entries are returned by `GetAll`, `Take`, and `Peek` in the order they were added (FIFO).
- **Empty queue**: `Take` and `Peek` return `null` rather than throwing. `GetAll` returns an empty list. `Clear` on an empty queue is a no-op.
- **Entry identity**: Each `DeadLetterEntry` carries its own `Id` (a GUID string) and `FailedAt` timestamp (`DateTimeOffset`, UTC), both assigned at construction.
- **Persistence**: The interface does not define durability. The in-memory implementation loses all entries on process restart.
- **Thread safety**: The interface does not state a thread-safety requirement. `InMemoryDeadLetterQueue` is safe for concurrent use. Implementations should document their own guarantees.

## Implementation: InMemoryDeadLetterQueue

`InMemoryDeadLetterQueue` is a `sealed` class that stores entries in a `ConcurrentQueue<DeadLetterEntry>`. Mutating operations (`Add`, `Clear`, `Take`, `Peek`, `GetAll`) are serialized with a private lock so that the bounded-size check and the enqueue happen atomically.

### Constructor

```csharp
public InMemoryDeadLetterQueue(int maxSize = 1000)
```

- **`maxSize`**: The maximum number of entries to retain. The default is `1000`. A value of `0` means unbounded. Negative values are treated as `0`.

In the running gateway, the size comes from `EventPublisherOptions.MaxDeadLetterQueueSize`, which is validated to the range `0`–`10000`.

### Bounded behavior

When `maxSize` is greater than `0` and the queue is full, `Add` removes the oldest entry before enqueuing the new one. The queue therefore never exceeds `maxSize` entries, and the most recent failures are always kept. Evicted entries are discarded without being logged or returned, so the bound should be sized to the volume of expected failures.

### Behavior notes

- `Count` and `IsEmpty` delegate to the underlying `ConcurrentQueue`, so they reflect the queue state at the moment of the call.
- `GetAll` copies the queue into a new list and wraps it with `AsReadOnly()`, so callers can iterate it while other threads keep adding entries.
- `Clear` drains the queue one entry at a time under the lock.

## Usage

### Example 1: Checking the queue and draining it for replay

```csharp
public sealed class DeadLetterReplayer
{
    private readonly IDeadLetterQueue _deadLetterQueue;

    public DeadLetterReplayer(IDeadLetterQueue deadLetterQueue)
    {
        _deadLetterQueue = deadLetterQueue;
    }

    public void ReplayAll()
    {
        while (!_deadLetterQueue.IsEmpty)
        {
            var entry = _deadLetterQueue.Take();
            if (entry is null)
            {
                break;
            }

            // Re-publish entry.EventPayload or log entry.FailureReason here.
        }
    }
}
```

### Example 2: Inspecting entries without consuming them

```csharp
var pending = deadLetterQueue.GetAll();

foreach (var entry in pending)
{
    Console.WriteLine($"{entry.FailedAt:O} {entry.EventType} retries={entry.RetryAttempts}: {entry.FailureReason}");
}
```

### Example 3: Registering a custom implementation

```csharp
services.AddSingleton<IDeadLetterQueue, MyPersistentDeadLetterQueue>();
```

Replace the default registration in `EventConfiguration.cs` so that the rest of the event pipeline, including `RetryingEventPublisher`, receives the custom queue. The new implementation must honor the FIFO ordering and `null` return rules described in the Contract section.

## Related types

- `DeadLetterEntry` (`src/ApiKeyGateway/Events/DeadLetterEntry.cs`): The item stored in the queue.
- `RetryingEventPublisher`: Adds an entry after retries are exhausted.
- `EventPublisherOptions.MaxDeadLetterQueueSize`: Sets the bound used by the default registration.
