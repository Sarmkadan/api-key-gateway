# BatchOperationHandler

`BatchOperationHandler` processes an administrative operation against a list of API key IDs and reports the outcome for each key. It is defined in `src/ApiKeyGateway/Integration/BatchOperationHandler.cs` together with its contract and models:

- `IBatchOperationHandler`: the public contract with a single method, `ExecuteAsync`. Depend on this interface in consumers.
- `BatchOperationHandler`: `public sealed class BatchOperationHandler : IBatchOperationHandler`. It is registered with `AddScoped` in `Configuration/ServiceRegistrationExtensions.cs`.
- `BatchOperation`: the request record (operation type, key IDs, optional parameters).
- `BatchOperationResult`: the aggregated response with counters and per-key items.
- `BatchOperationItemResult`: the outcome for one key.

> **Scope note.** The class summary describes a "single atomic operation" with "transaction support". The current implementation does not provide either. It has no transaction, no rollback, and no persistence. Each key is processed independently, and the built-in operation types only compute a result string; they do not change stored key state. See [Current behavior](#current-behavior).

## Usage

```csharp
public sealed class KeyAdminService
{
    private readonly IBatchOperationHandler _batch;

    public KeyAdminService(IBatchOperationHandler batch)
    {
        _batch = batch;
    }

    public async Task<BatchOperationResult> SetQuotaAsync(IEnumerable<string> keyIds, int limit)
    {
        var operation = new BatchOperation
        {
            OperationType = "set-quota",
            ApiKeyIds = keyIds.ToList(),
            Parameters = new Dictionary<string, object> { ["quotaLimit"] = limit }
        };

        var result = await _batch.ExecuteAsync(operation);

        foreach (var item in result.Items.Where(i => !i.Success))
        {
            Console.WriteLine($"{item.ApiKeyId}: {item.ErrorMessage}");
        }

        return result;
    }
}
```

Check `FailureCount` or `Items` after the call. A batch can return normally even when some keys failed.

## Types

### `BatchOperation`

```csharp
public record BatchOperation
{
    public Guid Id { get; }                              // generated on construction
    public required string OperationType { get; set; }
    public required List<string> ApiKeyIds { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public DateTime CreatedAt { get; set; }              // DateTime.UtcNow by default
}
```

| Member | Required | Description |
|---|---|---|
| `Id` | Set automatically | Unique identifier. It is copied to `BatchOperationResult.OperationId` and written to every log entry for the run. It cannot be assigned through an initializer. |
| `OperationType` | Yes | Operation name. Matched case-insensitively. See [Operation types](#operation-types). |
| `ApiKeyIds` | Yes | Key IDs to process. The list is not copied, and it is not de-duplicated. |
| `Parameters` | No | Operation-specific values. Only `set-quota` reads them. |
| `CreatedAt` | No | Creation timestamp in UTC. The handler does not read it. |

### `BatchOperationResult`

```csharp
public record BatchOperationResult
{
    public Guid OperationId { get; set; }
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BatchOperationItemResult> Items { get; set; } = new();
    public DateTime CompletedAt { get; set; }
}
```

| Member | Description |
|---|---|
| `OperationId` | The `Id` of the `BatchOperation` that produced this result. |
| `TotalCount` | Number of entries in `ApiKeyIds`, duplicates included. |
| `SuccessCount` | Number of items with `Success == true`. |
| `FailureCount` | Number of items with `Success == false`. |
| `Items` | One entry per input key, in the same order as `ApiKeyIds`. |
| `CompletedAt` | UTC timestamp. See [Current behavior](#current-behavior). |

`SuccessCount + FailureCount` always equals `TotalCount` for a result returned by `ExecuteAsync`.

### `BatchOperationItemResult`

```csharp
public record BatchOperationItemResult
{
    public string ApiKeyId { get; set; } = null!;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public object? Result { get; set; }
}
```

| Member | Description |
|---|---|
| `ApiKeyId` | The key ID this item refers to. |
| `Success` | `true` if the operation succeeded for this key. |
| `ErrorMessage` | Set when `Success` is `false`. It is either a validation message or the exception message. |
| `Result` | Set on success. Its type is `object?`, but the built-in operations store a `string`. |

## Methods

### `IBatchOperationHandler.ExecuteAsync` / `BatchOperationHandler.ExecuteAsync`

```csharp
Task<BatchOperationResult> ExecuteAsync(BatchOperation operation);
```

Runs `operation` against each key in `operation.ApiKeyIds` in order, awaiting each key before starting the next, and returns the aggregated result.

Argument checks run before any key is processed:

| Condition | Exception |
|---|---|
| `operation` is `null` | `ArgumentNullException` (parameter `operation`) |
| `ApiKeyIds` is `null` or empty | `ValidationException` with message `At least one API key ID must be specified` |

Because the method is `async`, both exceptions are delivered through the returned task and surface when it is awaited.

Errors for individual keys do not throw. The handler catches any exception thrown while processing a key, records it as a failed item with `ErrorMessage = ex.Message`, and continues with the next key. An unknown `OperationType` is also reported as a failed item, not as an exception.

Logging:

- Information at start: operation ID, type, and key count.
- Error per failed key, with the exception.
- Information at completion: operation ID, success count, and failure count.

## Operation types

`OperationType` is compared with `ToLowerInvariant()`, so `"Disable"` and `"DISABLE"` are equivalent.

| Type | Per-key outcome | `Result` on success | Required parameters |
|---|---|---|---|
| `disable` | Success | `"Key disabled"` | None |
| `enable` | Success | `"Key enabled"` | None |
| `rotate` | Success | `"Key rotated"` | None |
| `set-quota` | Success if `quotaLimit` is valid | `"Quota set to {limit}"` | `quotaLimit` in `Parameters` |
| Anything else | Failure | None | `ErrorMessage` is `Unknown operation type: {OperationType}` |

For `set-quota`, `quotaLimit` is read with `int.TryParse(value.ToString())`. Integers, numeric strings, and values such as `JsonElement` that render as an integer are accepted. Negative and zero values are also accepted. If the key is missing, the value is `null`, or the text is not an integer, the item fails with `Missing or invalid quotaLimit parameter`.

## Current behavior

These points follow from the implementation. Consider them before building on the handler.

- **No persistence.** `disable`, `enable`, `rotate`, and `set-quota` do not call a repository or service. A successful item means only that validation passed. To apply the change, a caller must do it separately, or the handler must be extended.
- **No atomicity.** Keys are processed one at a time. If the batch fails partway through, earlier keys keep their outcome. The handler does not roll back.
- **Partial success is normal.** `ExecuteAsync` returns a result even when every key fails. Check `FailureCount` rather than relying on an exception.
- **Duplicates are processed twice.** A key ID that appears twice produces two items, and both count toward `TotalCount`.
- **`CompletedAt` is set when the result object is created**, which happens before processing starts. It does not record the time the batch finished.
- **Cancellation is not supported.** `ExecuteAsync` has no `CancellationToken` parameter.
- **Large batches are not chunked.** All keys are processed in one call. Callers that need limits must split the list themselves.

## Extending

To add an operation, add a case to the switch in `ProcessSingleKeyAsync` (private). Each case must return a `BatchOperationItemResult` and must not throw for expected conditions. An exception is caught and recorded as a failure, so it does not stop the batch, but the message becomes the caller-visible `ErrorMessage`.

## Related

- [ApiKeyService](ApiKeyService.md): the usual place for key state changes that a batch operation would need to call.
- [AuditLogService](AuditLogService.md): records administrative changes. Batch callers should log their own audit entries, because the handler does not write any.
