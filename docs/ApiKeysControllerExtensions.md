# ApiKeysControllerExtensions

Static extension methods for `ApiKeysController` (`ApiKeyGateway.Controllers`) that wrap the controller's single-key actions into helpers for toggling key status and for revoking several keys at once. The class lives in `src/ApiKeyGateway/Controllers/ApiKeysControllerExtensions.cs`.

Both methods are plain C# extension methods. They do not add routes and do not change `ApiKeysController`. They call the controller's own `EnableKey`, `DisableKey` and `RevokeKey` actions, so each result is the same `ActionResult<object>` that the action returns (`200 OK` with a message, or `404 Not Found` with an error object).

## API

### `ToggleKeyStatusAsync(this ApiKeysController controller, string keyId, bool enable)` → `Task<ActionResult<object>>`

Enables or disables the API key with the given ID. When `enable` is `true` it calls `EnableKey(keyId)`; when `false` it calls `DisableKey(keyId)`. The result of the chosen action is returned unchanged.

- **Throws** `ArgumentNullException` if `controller` is `null`.
- **Throws** `ArgumentNullException` if `keyId` is `null`.
- **Throws** `ArgumentException` if `keyId` is an empty string. Whitespace-only values are not rejected by this method; they are passed to the controller as-is.
- An unknown key ID does not throw. The controller returns `404 Not Found` (`{ error = "API key not found" }`).

```csharp
using ApiKeyGateway.Controllers;

// Inside a controller action or a service that holds an ApiKeysController instance
ActionResult<object> disabled = await apiKeysController.ToggleKeyStatusAsync("key-123", enable: false);

// Re-enable the same key later
ActionResult<object> enabled = await apiKeysController.ToggleKeyStatusAsync("key-123", enable: true);
```

### `RevokeMultipleKeysAsync(this ApiKeysController controller, IEnumerable<string> keyIds)` → `Task<IReadOnlyList<ActionResult<object>>>`

Revokes each key in `keyIds` by calling `RevokeKey` for it, one after another in the order of enumeration. It returns a read-only list with one `ActionResult<object>` per processed key, in the same order as the input.

Behavior to be aware of:

- Keys are processed sequentially, not in parallel.
- An empty sequence returns an empty read-only list and makes no calls.
- A key that does not exist does not stop the batch. Its entry in the list is the controller's `404 Not Found` result, and the loop continues.
- If `RevokeKey` throws for one key (for example, a failure in the underlying service), the exception propagates out of this method and the remaining keys are not processed. Results already collected are discarded.
- Duplicate IDs are not removed; each occurrence is revoked (the second attempt typically reports `404 Not Found`).
- Each element is passed to the controller without validation, so a `null` or empty element is forwarded to `RevokeKey` as-is.
- The returned list is a read-only wrapper over a new list. The method does not modify `keyIds`.

- **Throws** `ArgumentNullException` if `controller` is `null`.
- **Throws** `ArgumentNullException` if `keyIds` is `null`.

```csharp
using ApiKeyGateway.Controllers;

var keyIds = new[] { "key-1", "key-2", "missing-key" };

IReadOnlyList<ActionResult<object>> results = await apiKeysController.RevokeMultipleKeysAsync(keyIds);

// results[0] and results[1]: 200 OK ("API key revoked successfully")
// results[2]: 404 Not Found ("API key not found"); the batch still completed
```

## Notes

- The `<summary>` on the class describes these methods as "bulk operations". Only `RevokeMultipleKeysAsync` is a bulk operation; `ToggleKeyStatusAsync` acts on a single key.
- The XML comments in the source describe whitespace-only input as rejected by `ToggleKeyStatusAsync`, and describe a `null` `keyId` as an `ArgumentException`. The code does not do this: it calls `ArgumentException.ThrowIfNullOrEmpty`, which throws `ArgumentNullException` for `null` and `ArgumentException` only for an empty string. This page documents the code's behavior.
