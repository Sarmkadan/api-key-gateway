# RotationResult

The `RotationResult` type serves as the primary data contract and service entry point for API key rotation operations within the `api-key-gateway` project. It encapsulates the outcome of a key rotation event, providing identifiers for both the legacy and newly generated keys, the associated consumer context, and the operational status of the request. Beyond acting as a result container, this type also exposes asynchronous service methods to execute immediate key rotations or batch-process keys approaching their expiration thresholds, ensuring seamless credential lifecycle management without service interruption.

## API

### `OldKeyId`
*   **Type**: `public string`
*   **Description**: Retrieves the unique identifier of the API key that was active prior to the rotation attempt. This value is populated regardless of whether the rotation succeeded or failed, allowing for audit tracing of the specific key targeted for replacement.

### `NewKeyId`
*   **Type**: `public string`
*   **Description**: Retrieves the unique identifier of the newly generated API key. If the `Success` property is `false`, this field may contain an empty string or a partial ID depending on the failure stage, but it is guaranteed to be non-null.

### `ConsumerId`
*   **Type**: `public string`
*   **Description**: Identifies the specific consumer or tenant account associated with the rotated key. This ensures that rotation events are correctly scoped to the owning entity within the gateway's multi-tenant architecture.

### `Success`
*   **Type**: `public bool`
*   **Description**: Indicates the final status of the rotation operation. A value of `true` confirms that the old key has been revoked (or marked for revocation) and the new key is active and ready for use. A value of `false` indicates the operation failed and the original key remains active.

### `FailureReason`
*   **Type**: `public string?`
*   **Description**: Provides a human-readable explanation if `Success` is `false`. Common reasons include database connectivity issues, concurrency conflicts during the swap, or policy violations preventing the generation of a new key. This property is `null` when `Success` is `true`.

### `NewKeyExpiresAt`
*   **Type**: `public DateTime?`
*   **Description**: Specifies the absolute expiration timestamp for the `NewKeyId`. This value is populated only upon successful rotation. It is `null` if the rotation failed or if the configured policy dictates non-expiring keys (though rare in rotation contexts).

### `ApiKeyRotationService`
*   **Type**: `public ApiKeyRotationService`
*   **Description**: Provides access to the underlying service instance responsible for executing rotation logic. This property allows consumers to access lower-level configuration or state held by the service instance associated with this result context.

### `RotateKeyAsync`
*   **Signature**: `public async Task<RotationResult> RotateKeyAsync`
*   **Description**: Initiates an immediate rotation of the API key identified in the current context.
*   **Parameters**: None (operates on the context defined by the instance).
*   **Return Value**: Returns a `Task<RotationResult>` representing the asynchronous operation. The result contains the details of the specific rotation attempt triggered by this call.
*   **Exceptions**: Throws `InvalidOperationException` if the instance is not initialized with a valid target key ID. Throws `ApiKeyGatewayException` for underlying infrastructure failures.

### `RotateExpiringSoonAsync`
*   **Signature**: `public async Task<List<RotationResult>> RotateExpiringSoonAsync`
*   **Description**: Scans for all API keys associated with the current `ConsumerId` that are within the configured pre-expiration window and initiates rotation for each.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task<List<RotationResult>>` containing a collection of results, one for each key attempted. The list includes both successful rotations and failures to allow for granular error handling.
*   **Exceptions**: Throws `ApiKeyGatewayException` if the scan operation fails due to data store unavailability. Individual failures within the batch are captured in the `FailureReason` of the respective list items rather than throwing.

## Usage

### Example 1: Immediate Single Key Rotation
This example demonstrates how to trigger an immediate rotation for a specific key and handle the outcome based on the `Success` flag.

```csharp
// Assume 'rotationContext' is an existing RotationResult instance initialized with a target KeyId
try 
{
    var result = await rotationContext.RotateKeyAsync();

    if (result.Success)
    {
        Console.WriteLine($"Key rotated successfully. New ID: {result.NewKeyId}");
        Console.WriteLine($"New key expires at: {result.NewKeyExpiresAt:yyyy-MM-dd HH:mm:ss}");
        
        // Distribute new key to consumer securely
        await NotifyConsumerAsync(result.ConsumerId, result.NewKeyId);
    }
    else
    {
        Console.Error.WriteLine($"Rotation failed for {result.OldKeyId}: {result.FailureReason}");
        // Alert monitoring system; old key is still valid
    }
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Invalid operation: {ex.Message}");
}
```

### Example 2: Batch Rotation of Expiring Keys
This example utilizes the batch method to proactively rotate all keys for a consumer that are nearing expiration, processing the mixed results of the bulk operation.

```csharp
// Assume 'rotationContext' is initialized with a specific ConsumerId
var batchResults = await rotationContext.RotateExpiringSoonAsync();

int successCount = 0;
var failures = new List<string>();

foreach (var result in batchResults)
{
    if (result.Success)
    {
        successCount++;
        // Log successful rotation
        AuditLog.LogRotation(result.ConsumerId, result.OldKeyId, result.NewKeyId);
    }
    else
    {
        failures.Add($"Key {result.OldKeyId}: {result.FailureReason}");
    }
}

Console.WriteLine($"Batch complete: {successCount} rotated, {failures.Count} failed.");
if (failures.Any())
{
    // Handle partial failure scenario
    foreach (var error in failures)
    {
        Console.Error.WriteLine(error);
    }
}
```

## Notes

*   **Immutability of Result Data**: Once a `RotationResult` instance is returned from an asynchronous method, its properties (`OldKeyId`, `Success`, etc.) should be treated as immutable snapshots of that specific operation. Do not modify these properties directly as they reflect the state of the system at the moment of completion.
*   **Thread Safety**: The `RotateKeyAsync` and `RotateExpiringSoonAsync` methods are not guaranteed to be thread-safe when invoked concurrently on the *same* instance of `RotationResult` if that instance holds mutable internal state within `ApiKeyRotationService`. It is recommended to instantiate a new context or ensure external synchronization if triggering multiple rotations simultaneously from a single object reference. However, distinct instances targeting different keys or consumers are safe to run in parallel.
*   **Partial Batch Failures**: When using `RotateExpiringSoonAsync`, the operation follows a "best effort" pattern. The method will not throw an exception if individual key rotations within the batch fail; instead, failures are isolated to the specific `RotationResult` objects within the returned list. Callers must iterate the list to identify and handle specific failures.
*   **Time Sensitivity**: The `NewKeyExpiresAt` value is calculated based on the server time at the moment of successful rotation. Clients should account for potential clock skew when validating this timestamp locally, though the gateway enforces the authoritative expiration.
