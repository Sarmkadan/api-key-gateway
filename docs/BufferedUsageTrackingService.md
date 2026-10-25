# BufferedUsageTrackingService

`BufferedUsageTrackingService` is a decorator over `IUsageTrackingService`. It buffers `RecordUsageAsync` calls in a bounded in-memory channel and writes them to `IUsageRepository` in batches, which cuts database round-trips under high request volume. All other read methods pass straight through to the wrapped service. The class is defined in `src/ApiKeyGateway/Services/BufferedUsageTrackingService.cs`:

- `BufferedUsageTrackingService`: `public sealed class BufferedUsageTrackingService : IUsageTrackingService, IAsyncDisposable`.
- `BufferedUsageTrackingService.BufferedUsageTrackingOptions`: the nested, public options class that tunes batching and channel behavior.
- `IUsageTrackingService`: the contract it implements. Defined in `Services/UsageTrackingService.cs`. See that file for the full read API.

The service is intended to be used as a decorator around the plain `UsageTrackingService`. Consumers depend on `IUsageTrackingService` and do not need to know that buffering is in place.

## Constructor

```csharp
public BufferedUsageTrackingService(
    IUsageTrackingService innerService,
    IUsageRepository repository,
    BufferedUsageTrackingOptions? options = null,
    ILogger<BufferedUsageTrackingService>? logger = null)
```

| Parameter | Type | Required | Description |
|---|---|---|---|
| `innerService` | `IUsageTrackingService` | Yes | The service that read calls are delegated to, and that receives the fallback writes made during shutdown or after a failed batch. |
| `repository` | `IUsageRepository` | Yes | Receives batched writes through `WriteBatchAsync`. |
| `options` | `BufferedUsageTrackingOptions?` | No | Batching and channel settings. When `null`, a new instance with the defaults below is used. |
| `logger` | `ILogger<BufferedUsageTrackingService>?` | Effectively yes | Although the parameter is optional, passing `null` throws `ArgumentNullException`. Always supply a logger. |

The constructor throws `ArgumentNullException` if `innerService`, `repository`, or `logger` is `null`.

The constructor creates the bounded channel and starts the background flusher immediately. Nothing else needs to be called before the first `RecordUsageAsync`.

## Types

### `BufferedUsageTrackingOptions`

A mutable class with public setters. Set the values before passing the instance to the constructor. Changing them later has no effect, because the channel and flusher read them only once.

| Property | Type | Default | Description |
|---|---|---|---|
| `MaxBatchSize` | `int` | `100` | Maximum number of records passed to a single `WriteBatchAsync` call. |
| `MaxFlushInterval` | `TimeSpan` | `2` seconds | Length of each wait while the flusher waits for records. See [Flush behavior](#flush-behavior) for what this does and does not control. |
| `ChannelFullMode` | `BoundedChannelFullMode` | `Wait` | What `RecordUsageAsync` does when the channel is full. See [Overflow behavior](#overflow-behavior). |
| `ChannelCapacity` | `int` | `10000` | Maximum number of buffered records held in memory. |

## Methods

### `RecordUsageAsync`

```csharp
public Task RecordUsageAsync(UsageRecord record, CancellationToken cancellationToken = default)
```

Adds `record` to the channel and returns. It does not wait for the database write.

- Throws `ArgumentNullException` if `record` is `null`.
- The channel write is bounded by the service's internal shutdown token, not by `cancellationToken`. The caller's token is passed only to the shutdown fallback described below.
- If the channel is full and `ChannelFullMode` is `Wait`, the call waits until space is available.
- If the write is cancelled because `DisposeAsync` has started, the record is written immediately through the inner service instead.
- Any other exception from the channel write is logged as an error and rethrown.

### Read methods

These methods delegate to the inner service with the same arguments and return its result. They do not wait for buffered records to flush, so a record recorded just before a read may not appear in it yet.

| Method | Returns |
|---|---|
| `GetUsageStatisticsAsync(string apiKeyId, DateTime startDate, DateTime endDate, CancellationToken)` | `Task<UsageStatistics>` |
| `GetUsageRecordsAsync(string apiKeyId, DateTime startDate, DateTime endDate, CancellationToken)` | `Task<List<UsageRecord>>` |
| `GetTotalBytesUsedAsync(string consumerId, DateTime startDate, DateTime endDate, CancellationToken)` | `Task<long>` |
| `GetUsageAsync(DateTime startDate, DateTime endDate, CancellationToken)` | `Task<List<UsageRecord>>` |

Argument validation, if any, is done by the inner service.

### `DisposeAsync`

```csharp
public ValueTask DisposeAsync()
```

Cancels the flusher, waits for it to finish, and completes the channel. Call it once when the service is no longer needed. Use `await using` so it runs on scope exit. Calls to `RecordUsageAsync` after `DisposeAsync` are not supported. See [Known limitations](#known-limitations).

## Behavior

### Flush behavior

The flusher is a single background task. In a loop it:

1. Reads up to `MaxBatchSize` records from the channel.
2. If any were read, passes them to `IUsageRepository.WriteBatchAsync`.

If `WriteBatchAsync` throws, the service logs the error and writes each record of that batch through the inner service's `RecordUsageAsync`. A failure on an individual record is logged and that record is dropped.

`MaxFlushInterval` is the length of each wait for new records inside the read step. In the current implementation the read step does not return when that wait expires. It keeps reading until the batch is full or shutdown starts. As a result, a partial batch is normally written only when it reaches `MaxBatchSize` or during shutdown. See [Known limitations](#known-limitations).

### Overflow behavior

`ChannelFullMode` is passed directly to `Channel.CreateBounded`.

| Mode | Effect when `ChannelCapacity` is reached |
|---|---|
| `Wait` (default) | `RecordUsageAsync` waits until the flusher frees space. The caller is slowed down. |
| `DropNewest` | The incoming record is discarded. `RecordUsageAsync` still returns successfully. |
| `DropOldest` | The oldest buffered record is discarded to make room. `RecordUsageAsync` still returns successfully. |

The drop modes do not log or count discarded records.

### Shutdown behavior

`DisposeAsync`:

1. Cancels the internal shutdown token.
2. Waits for the flusher task to finish, if it has not already.
3. Disposes the shutdown token source and completes the channel.

Once the token is cancelled, `RecordUsageAsync` writes new records directly through the inner service. Records that are still in the channel when the flusher stops are not written. See [Known limitations](#known-limitations).

## Usage

### Manual construction

```csharp
var options = new BufferedUsageTrackingService.BufferedUsageTrackingOptions
{
    MaxBatchSize = 100,
    MaxFlushInterval = TimeSpan.FromSeconds(2),
    ChannelFullMode = BoundedChannelFullMode.Wait,
    ChannelCapacity = 10000
};

await using var tracker = new BufferedUsageTrackingService(
    innerService,
    usageRepository,
    options,
    logger);

await tracker.RecordUsageAsync(new UsageRecord
{
    ApiKeyId = apiKeyId,
    // other fields as required by UsageRecord
});
```

### Dependency injection

`Configuration/ServiceCollectionExtensions.cs` registers the decorator as the `IUsageTrackingService` used by the gateway. It takes `FlushInterval` from the `UsageTracking` configuration section (`UsageTrackingOptions.FlushInterval`, `"00:00:02"` in `appsettings.json`). `MaxBatchSize` (100) and `ChannelCapacity` (10000) are fixed in that code, and `ChannelFullMode` is `Wait`.

Registration is done with `AddScoped`. Each scope gets its own instance, and therefore its own channel and flusher. Because the flusher is a background task, the instance must be disposed when the scope ends. See [Known limitations](#known-limitations) for a registration issue to check before relying on this path.

## Known limitations

These behaviors were found by reading the code. They are not covered by tests in the repository. Check them before relying on this service for data that must not be lost.

- **Partial batches wait for a full batch.** `MaxFlushInterval` does not force a flush. With low traffic, records can stay in memory until 100 are buffered, or until shutdown. The timeout branch in `ReadBatchAsync` continues the loop instead of returning the batch. Each timeout also leaves an unobserved `WaitToReadAsync` task behind.
- **Buffered records can be lost on shutdown.** `DisposeAsync` cancels the flusher. The final read in the flusher uses the same cancelled token, so `ReadBatchAsync` returns an empty batch without reading anything. Records still in the channel at that point are discarded. Only a batch that was already read when cancellation started is written.
- **Calls after `DisposeAsync` are unsafe.** The channel is completed, and the shutdown token source is disposed. Later `RecordUsageAsync` calls are not expected to succeed or fall back cleanly.
- **Drop modes are silent.** `DropNewest` and `DropOldest` discard records without logging.
- **Registration may recurse.** In `ServiceCollectionExtensions.cs`, the factory for `IUsageTrackingService` resolves `IUsageTrackingService` as its inner service. The last registration wins in the container, so this resolves to the same factory. Confirm that the gateway resolves this service at runtime before depending on the DI path. Constructing the decorator directly, as shown above, avoids it.
- **Flusher faults are not restarted.** `FlushBatchAsync` catches repository errors, so the flusher only faults on unexpected exceptions. If it does, nothing drains the channel and writes eventually block in `Wait` mode.

## Related

- `UsageTrackingService` (`Services/UsageTrackingService.cs`): the inner implementation and read API.
- [UsageRecord](UsageRecord.md): the record type buffered by this service.
