# AuditLogEventHandlerExtensions

Static extension methods for `AuditLogEventHandler` (`ApiKeyGateway.Events`) that handle several events in one call and create a single untyped delegate for event pipelines. The class lives in `src/ApiKeyGateway/Events/AuditLogEventHandlerExtensions.cs`.

The extensions only call the handler's public `HandleApiKeyCreatedAsync`, `HandleApiKeyRotatedAsync` and `HandleApiKeyDisabledAsync` methods. They add no persistence or logging of their own, so each event is written to the audit trail exactly as if it were handled individually.

The extensions are optional. `Configuration/EventConfiguration.cs` subscribes the handler's single-event methods directly, so the bulk helpers are for callers that already hold a batch of events or want one delegate for a mixed stream.

## API

### `HandleApiKeyCreatedAsync(this AuditLogEventHandler handler, IEnumerable<ApiKeyCreatedEvent> events)` → `Task`

Passes each event to `handler.HandleApiKeyCreatedAsync` in enumeration order. Each event is awaited before the next one starts.

- Each event is handled with `ConfigureAwait(false)`.
- The first exception stops the loop. Events handled before it have already been persisted, and the remaining events are not handled. There is no rollback.
- **Throws** `ArgumentNullException` if `handler` or `events` is `null`. The method is `async`, so this exception is stored in the returned `Task` and is not thrown synchronously. Await the task or check its faulted state to observe it.
- An empty sequence returns a completed task and writes nothing.

```csharp
using ApiKeyGateway.Events;

var handler = serviceProvider.GetRequiredService<AuditLogEventHandler>();

var created = new List<ApiKeyCreatedEvent>
{
    new() { ApiKeyId = "key-1", Name = "Billing", CreatedBy = "admin@acme.example" },
    new() { ApiKeyId = "key-2", Name = "Reports", CreatedBy = "ops-bot" }
};

await handler.HandleApiKeyCreatedAsync(created);
```

### `HandleApiKeyRotatedAsync(this AuditLogEventHandler handler, IEnumerable<ApiKeyRotatedEvent> events)` → `Task`

Passes each event to `handler.HandleApiKeyRotatedAsync` in enumeration order, with the same sequencing, `ConfigureAwait(false)` and failure behavior as `HandleApiKeyCreatedAsync`.

- **Throws** `ArgumentNullException` (inside the returned `Task`) if `handler` or `events` is `null`.
- Each rotation event is recorded with the rotation reason fixed by the handler.

```csharp
var rotated = new[]
{
    new ApiKeyRotatedEvent { ApiKeyId = "key-1", RotatedBy = "admin@acme.example" }
};

await handler.HandleApiKeyRotatedAsync(rotated);
```

### `HandleApiKeyDisabledAsync(this AuditLogEventHandler handler, IEnumerable<ApiKeyDisabledEvent> events)` → `Task`

Passes each event to `handler.HandleApiKeyDisabledAsync` in enumeration order, with the same sequencing, `ConfigureAwait(false)` and failure behavior as `HandleApiKeyCreatedAsync`. Each event's `Reason` is copied into the audit record.

- **Throws** `ArgumentNullException` (inside the returned `Task`) if `handler` or `events` is `null`.

```csharp
var disabled = new[]
{
    new ApiKeyDisabledEvent
    {
        ApiKeyId = "key-2",
        DisabledBy = "security-bot",
        Reason = "Suspected credential leak"
    }
};

await handler.HandleApiKeyDisabledAsync(disabled);
```

### `CreateEventDelegate(this AuditLogEventHandler handler)` → `Func<object, Task>`

Returns a delegate that accepts any object and routes it to the matching handler method by runtime type:

| Event type | Handler method called |
|------------|------------------------|
| `ApiKeyCreatedEvent` | `HandleApiKeyCreatedAsync` |
| `ApiKeyRotatedEvent` | `HandleApiKeyRotatedAsync` |
| `ApiKeyDisabledEvent` | `HandleApiKeyDisabledAsync` |
| anything else, including `null` | ignored; the returned task completes without writing anything |

The lookup uses type patterns, so subclasses of the three event records are routed the same way. `ApiKeyUpdatedEvent` and other event types are ignored.

- The returned delegate captures `handler` and holds no other state, so it can be reused for any number of events.
- **Throws** `ArgumentNullException` synchronously if `handler` is `null`. The method is not `async`, so the check runs when the method is called.
- Exceptions from the handler methods propagate through the returned task.

`Func<object, Task>` does not match `IEventPublisher.Subscribe<T>(Func<T, Task>)` for a specific `T`. Use the delegate where an untyped pipeline accepts `object`, such as a custom dispatcher, or wrap it in a lambda that casts to the type the caller expects.

```csharp
Func<object, Task> dispatch = handler.CreateEventDelegate();

await dispatch(new ApiKeyCreatedEvent { ApiKeyId = "key-3", Name = "Export", CreatedBy = "ops-bot" });
await dispatch(new ApiKeyUpdatedEvent { ApiKeyId = "key-3" }); // ignored
await dispatch(null!);                                          // ignored
```

## Related

- [AuditLogEventHandler](AuditLogEventHandler.md) - the handler these extensions wrap.
- [ApiKeyEvent](ApiKeyEvent.md) - the event records consumed here.
