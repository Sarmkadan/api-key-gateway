# ApiKeyRotationService

`ApiKeyRotationService` implements automated and manual API key rotation. Rotating a key creates a replacement key that inherits the original's metadata, IP whitelist, and scope restrictions, then revokes the original so callers must switch to the new value. The service is defined in `src/ApiKeyGateway/Services/ApiKeyRotationService.cs` together with its contracts:

- `IApiKeyRotationService`: the public contract. Depend on this interface in consumers such as `ApiKeysController` and `KeyRotationScheduler`.
- `ApiKeyRotationService`: `public partial class ApiKeyRotationService : IApiKeyRotationService`. It is registered with `AddScoped` in `Configuration/ServiceCollectionExtensions.cs`.
- `RotationResult`: the outcome of one rotation. See [RotationResult](RotationResult.md).

## Constructor

```csharp
public ApiKeyRotationService(
    IApiKeyService apiKeyService,
    IApiKeyRepository repository,
    ILogger<ApiKeyRotationService> logger)
```

| Parameter | Type | Required | Description |
|---|---|---|---|
| `apiKeyService` | `IApiKeyService` | Yes | Creates the replacement key, updates its metadata, and revokes the original. |
| `repository` | `IApiKeyRepository` | Yes | Loads the original key, persists IP whitelist and scope changes, and finds keys nearing expiry. |
| `logger` | `ILogger<ApiKeyRotationService>` | Yes | Receives rotation diagnostics. |

Throws `ArgumentNullException` if any argument is `null`.

## Types

### `RotationResult`

A plain result object with `init`-only properties. It is returned by every rotation call, including failed ones.

| Property | Type | Description |
|---|---|---|
| `OldKeyId` | `string` | ID of the key that was targeted. Always set. |
| `NewKeyId` | `string` | ID of the replacement key. Set only when the rotation succeeds. Empty otherwise. |
| `ConsumerId` | `string` | Owner of the key. Set when the original key was loaded. Empty if the key was not found. |
| `Success` | `bool` | `true` if the replacement was created and the original was revoked. |
| `FailureReason` | `string?` | Explanation when `Success` is `false`; `null` on success. |
| `NewKeyExpiresAt` | `DateTime?` | Expiry of the replacement key, or `null` if it does not expire. |

## Methods

### `IApiKeyRotationService.RotateKeyAsync`

```csharp
Task<RotationResult> RotateKeyAsync(string keyId, int? newExpirationDays = null);
```

Rotates one key by ID. The `ApiKeyRotationService` implementation does the following:

1. Throws `ValidationException` if `keyId` is empty or whitespace, or if `newExpirationDays` is set and not positive.
2. Acquires a per-key lock, so concurrent rotations of the same ID in this process run one at a time.
3. Loads the original key. If it does not exist, returns a failed result with `FailureReason = "Key not found"`.
4. If the original is not active (revoked, disabled, suspended, or expired), returns a failed result with `FailureReason = "Key is not active (status: {Status})"`.
5. Chooses the replacement's TTL:
   - If `newExpirationDays` is set, it is used.
   - Otherwise, if the original has an `ExpiresAt`, the original's full lifetime (`ExpiresAt - CreatedAt`, rounded up to whole days) is used.
   - Otherwise the replacement does not expire.
6. Creates the replacement through `IApiKeyService.CreateKeyAsync`. Its name is `"{original name} (rotated yyyy-MM-dd)"`.
7. Copies `IpWhitelist` and `AllowedScopes` to the replacement. If the original has metadata, it is copied with `IApiKeyService.UpdateKeyMetadataAsync`. The replacement is then saved with `IApiKeyRepository.UpdateAsync`.
8. Revokes the original with `IApiKeyService.RevokeKeyAsync`.

Any exception from steps 3 to 8 is caught and logged with event ID `4101`. It is not rethrown. The method returns a failed result whose `FailureReason` is the exception message. A failure after step 6 can leave a replacement key behind; see [Notes](#notes).

Returns a `Task<RotationResult>`. Only the argument validation in step 1 throws.

### `IApiKeyRotationService.RotateExpiringSoonAsync`

```csharp
Task<List<RotationResult>> RotateExpiringSoonAsync(int warningDays = 7, int? newExpirationDays = null);
```

Rotates every active key whose `ExpiresAt` is on or before `DateTime.UtcNow.AddDays(warningDays)`.

- Throws `ValidationException` if `warningDays` or `newExpirationDays` (when set) is not positive.
- Queries `IApiKeyRepository.GetKeysExpiringBeforeAsync(threshold)`. Repository exceptions propagate.
- Calls `RotateKeyAsync` for each key in turn, passing `newExpirationDays`. Keys are processed sequentially.
- Logs the number of keys found (event `4102`) and the number rotated successfully (event `4103`).
- Returns one `RotationResult` per key processed, in the order the repository returned them. The list is empty when nothing is due.

Per-key failures do not stop the batch. They appear as failed entries in the returned list.

## Usage

### Example 1: Rotate one key on demand

This follows the pattern used by `ApiKeysController`. The service is resolved from DI, so the caller does not construct it.

```csharp
public sealed class KeyMaintenanceJob
{
    private readonly IApiKeyRotationService _rotationService;
    private readonly ILogger<KeyMaintenanceJob> _logger;

    public KeyMaintenanceJob(IApiKeyRotationService rotationService, ILogger<KeyMaintenanceJob> logger)
    {
        _rotationService = rotationService;
        _logger = logger;
    }

    public async Task RotateAsync(string keyId)
    {
        // Keep the original TTL: newExpirationDays is omitted
        var result = await _rotationService.RotateKeyAsync(keyId);

        if (result.Success)
        {
            _logger.LogInformation(
                "Rotated {OldKeyId} to {NewKeyId}, expires {ExpiresAt}",
                result.OldKeyId,
                result.NewKeyId,
                result.NewKeyExpiresAt?.ToString("yyyy-MM-dd") ?? "never");

            // Deliver the new key to the consumer through a secure channel
            // The raw key value is not part of RotationResult
        }
        else
        {
            _logger.LogWarning("Rotation of {KeyId} failed: {Reason}", result.OldKeyId, result.FailureReason);
        }
    }
}
```

### Example 2: Batch rotation of keys nearing expiry

This resolves the service the same way `KeyRotationScheduler` does, inside a DI scope. It rotates keys that expire within 14 days and gives each replacement a 90-day TTL.

```csharp
using var scope = serviceProvider.CreateScope();
var rotationService = scope.ServiceProvider.GetRequiredService<IApiKeyRotationService>();

var results = await rotationService.RotateExpiringSoonAsync(warningDays: 14, newExpirationDays: 90);

var succeeded = results.Where(r => r.Success).ToList();
var failed = results.Where(r => !r.Success).ToList();

Console.WriteLine($"Rotated {succeeded.Count} of {results.Count} keys.");

foreach (var failure in failed)
{
    Console.Error.WriteLine($"{failure.OldKeyId}: {failure.FailureReason}");
}
```

## Notes

- **Replacement key values.** The raw value of a new key is not returned in `RotationResult`. Deliver it to the consumer through whatever process your deployment uses for new credentials.
- **Partial failure.** Rotation is not a single transaction. The replacement is created before the original is revoked, so a failure after creation can leave both keys active. In that case the result has `Success = false` and an empty `NewKeyId`, even though a replacement exists in the database. The replacement is not removed automatically. Check for keys named `(rotated yyyy-MM-dd)` that belong to the consumer, and revoke any you do not need.
- **Expired keys in batches.** `GetKeysExpiringBeforeAsync` only filters on `Status = Active`, so it can return keys whose `ExpiresAt` has already passed. Their `RotateKeyAsync` call fails with `Key is not active (status: Active)`. This is expected. Such keys are not rotated.
- **Locking scope.** The per-key lock is an in-memory `SemaphoreSlim` in a `ConcurrentDictionary`. It serializes rotations of the same key within one process only. Running several gateway instances against the same database does not prevent concurrent rotations of one key across instances.
- **Expiry timing.** The lookahead window is measured from the server's `DateTime.UtcNow`. Keys that expire just after the window are picked up by a later run.
- **Logging.** Events use IDs `4100` to `4103`: rotation succeeded, rotation failed (logged at error level with the exception), expiring keys found, and batch completed.
