# InMemoryDeadLetterQueue
The `InMemoryDeadLetterQueue` type is a thread-safe, in-memory, bounded dead-letter queue for failed events in the `api-key-gateway` project. It implements the [`IDeadLetterQueue`](IDeadLetterQueue.md) interface with FIFO (first-in, first-out) semantics. When the queue reaches its configured maximum size, the oldest entry is removed to make room for a new one. Entries exist only in process memory and are lost when the application stops.

## API
The `InMemoryDeadLetterQueue` type is `sealed` and exposes the following public members:
* `InMemoryDeadLetterQueue(int maxSize = 1000)`: Constructor. Parameters: `maxSize` is the maximum number of entries to keep; `0` means unbounded, and negative values are treated as `0`. Return value: a new instance. Throws: none.
* `Count`: Gets the number of entries currently in the queue. Type: `int`. Read without taking the internal lock, so the value is a point-in-time snapshot that may be stale by the time it is used.
* `IsEmpty`: Gets a value indicating whether the queue is empty. Type: `bool`. Read without taking the internal lock, with the same snapshot caveat as `Count`.
* `Add(DeadLetterEntry entry)`: Appends an entry to the end of the queue. If the queue is bounded and full, the oldest entry is removed first, silently and without logging or notification. Parameters: `entry` is the entry to add. Return value: none. Throws: `ArgumentNullException` if `entry` is `null`.
* `GetAll()`: Returns a snapshot of all entries in FIFO order (oldest first) without removing them. Parameters: none. Return value: an `IReadOnlyList<DeadLetterEntry>` that is a copy and is not affected by later changes to the queue. Throws: none.
* `Take()`: Removes and returns the oldest entry. Parameters: none. Return value: the oldest `DeadLetterEntry`, or `null` if the queue is empty. Throws: none.
* `Peek()`: Returns the oldest entry without removing it. Parameters: none. Return value: the oldest `DeadLetterEntry`, or `null` if the queue is empty. Throws: none.
* `Clear()`: Removes all entries from the queue. Parameters: none. Return value: none. Throws: none.

## Usage
The following examples demonstrate how to use the `InMemoryDeadLetterQueue` type:
```csharp
// Example 1: Create a bounded queue that keeps the 500 most recent failures
var deadLetters = new InMemoryDeadLetterQueue(maxSize: 500);

// Example 2: Record a failed event after retries are exhausted
try
{
    await publisher.PublishAsync(evt);
}
catch (Exception ex)
{
    deadLetters.Add(DeadLetterEntry.Create(evt, ex, retryAttempts: 3));
}

// Example 3: Inspect the queue without consuming it
foreach (DeadLetterEntry entry in deadLetters.GetAll())
{
    Console.WriteLine($"{entry.FailedAt:O} {entry.EventType}: {entry.FailureReason}");
}

// Example 4: Consume entries in FIFO order
while (deadLetters.Take() is { } oldest)
{
    // Reprocess or log the entry
}

// Example 5: Remove everything
deadLetters.Clear();
```

## Thread safety
`InMemoryDeadLetterQueue` is safe to use from multiple threads concurrently. The characteristics are as follows:
* `Add`, `GetAll`, `Take`, `Peek`, and `Clear` each run under a single private lock (`_syncLock`). Each call is atomic with respect to the others. In particular, the capacity check and the eviction in `Add` happen together under the lock, so concurrent writers cannot push the queue past `maxSize`.
* `Count` and `IsEmpty` do not take the lock. They read the underlying `ConcurrentQueue<DeadLetterEntry>` directly, so they are safe to call from any thread but may return a value that changes immediately afterward.
* `GetAll` returns a copy taken under the lock. The caller can enumerate the result without holding any lock and without coordinating with writers.
* Check-then-act sequences are not atomic across calls. For example, `Peek` followed by `Take` can remove a different entry than the one peeked if another thread consumes first. To claim the oldest entry safely, call `Take` alone and handle `null`. Likewise, do not use `Count` or `IsEmpty` as a guard before `Add`, `Take`, or `Clear`.
* Entries are not copied or locked individually. `DeadLetterEntry` is effectively immutable after creation (its properties use `init` or are read-only), so sharing a retrieved entry between threads is safe.

## Notes
Eviction is silent. When a bounded queue is full, the oldest entry is discarded without being logged or passed to any handler, so failures can be lost if the queue is smaller than the failure rate requires. Choose `maxSize` with that in mind, or use `0` for unbounded growth, which removes the cap but can consume unlimited memory during a sustained outage.

The queue does not persist data. Entries are held only in memory and are lost on process restart. Use a different [`IDeadLetterQueue`](IDeadLetterQueue.md) implementation if failed events must survive restarts.

`Clear` and `GetAll` are O(n) in the number of entries because they copy or drain the whole queue while holding the lock. `Add`, `Take`, and `Peek` take the lock but do constant work. For high-volume workloads, keep the queue bounded so that these operations stay small.

The default `maxSize` of `1000` is used when the constructor is called without arguments. In the application, the size is taken from the event configuration (`EventConfiguration`), which passes its `MaxDeadLetterQueueSize` option to the constructor.
