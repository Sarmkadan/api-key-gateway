# ApiKeysController

Manages the lifecycle and configuration of API keys within the API key gateway. This controller provides endpoints for creating, retrieving, rotating, disabling, enabling, revoking, and deleting API keys, as well as managing per-key IP whitelists. It operates within an authenticated context, typically scoped to a specific consumer identified during request processing.

## API

### `public ApiKeysController`
Constructor. Initializes a new instance of the controller with required dependencies such as key storage, validation services, and logging infrastructure. Not called directly by consumers; instantiated by the framework dependency injection container per request.

### `public async Task<ActionResult<CreateKeyResponse>> CreateKey`
Creates a new API key for the current consumer context.

**Parameters:**
- `ConsumerId` (string?, optional): Override for the consumer identifier. If null, the consumer is derived from the authenticated context.
- `Name` (string?, optional): A human-readable label for the key.
- `ExpirationDays` (int?, optional): Number of days until the key expires. If null, a default or no expiration may apply depending on server configuration.

**Returns:** `CreateKeyResponse` containing the newly generated key material, its unique identifier, and metadata such as creation timestamp and expiration.

**Throws:** `InvalidOperationException` if the consumer cannot be resolved. `ValidationException` if the provided parameters fail validation (e.g., negative expiration days).

### `public async Task<ActionResult<GetKeyResponse>> GetKeyById`
Retrieves a specific API key by its unique identifier.

**Parameters:**
- `KeyId` (string): The unique identifier of the key to retrieve.

**Returns:** `GetKeyResponse` with full key metadata, including status, creation date, expiration, and associated IP whitelist summary. The raw key material is never returned.

**Throws:** `KeyNotFoundException` if no key with the given ID exists or it does not belong to the current consumer.

### `public async Task<ActionResult<List<GetKeyResponse>>> GetConsumerKeys`
Lists all API keys belonging to the current consumer.

**Parameters:**
- `ConsumerId` (string): The consumer whose keys should be listed. Must match the authenticated context.

**Returns:** A list of `GetKeyResponse` objects, each representing a key's metadata. Returns an empty list if the consumer has no keys.

**Throws:** `UnauthorizedAccessException` if the requested consumer does not match the authenticated identity.

### `public async Task<ActionResult<object>> DisableKey`
Temporarily disables an API key, preventing its use for authentication without permanently removing it.

**Parameters:**
- `KeyId` (string): The unique identifier of the key to disable.

**Returns:** An object containing a confirmation message and the updated key status.

**Throws:** `KeyNotFoundException` if the key does not exist or is not owned by the current consumer. `InvalidOperationException` if the key is already disabled or in a terminal state (revoked, deleted).

### `public async Task<ActionResult<object>> EnableKey`
Re-enables a previously disabled API key, restoring its ability to authenticate requests.

**Parameters:**
- `KeyId` (string): The unique identifier of the key to enable.

**Returns:** An object containing a confirmation message and the updated key status.

**Throws:** `KeyNotFoundException` if the key does not exist or is not owned by the current consumer. `InvalidOperationException` if the key is not currently disabled (e.g., already active, revoked, or deleted).

### `public async Task<ActionResult<object>> RevokeKey`
Permanently revokes an API key. A revoked key cannot be re-enabled and is effectively dead, though its metadata may remain for audit purposes.

**Parameters:**
- `KeyId` (string): The unique identifier of the key to revoke.

**Returns:** An object containing a confirmation message and the updated key status.

**Throws:** `KeyNotFoundException` if the key does not exist or is not owned by the current consumer. `InvalidOperationException` if the key is already revoked or deleted.

### `public async Task<ActionResult<IpWhitelistResponse>> GetIpWhitelist`
Retrieves the current IP whitelist configuration for a specific key.

**Parameters:**
- `KeyId` (string): The unique identifier of the key.

**Returns:** `IpWhitelistResponse` containing the list of allowed IP addresses or CIDR ranges, and whether the whitelist is enforced.

**Throws:** `KeyNotFoundException` if the key does not exist or is not owned by the current consumer.

### `public async Task<ActionResult<IpWhitelistResponse>> SetIpWhitelist`
Replaces the entire IP whitelist for a key with a new set of addresses or ranges.

**Parameters:**
- `KeyId` (string): The unique identifier of the key.
- Request body containing the complete list of IP addresses/CIDR ranges to set.

**Returns:** `IpWhitelistResponse` reflecting the newly applied whitelist.

**Throws:** `KeyNotFoundException` if the key does not exist or is not owned by the current consumer. `ValidationException` if any provided IP address or CIDR range is malformed.

### `public async Task<ActionResult<IpWhitelistResponse>> AddIpToWhitelist`
Adds one or more IP addresses or CIDR ranges to an existing whitelist without removing current entries.

**Parameters:**
- `KeyId` (string): The unique identifier of the key.
- Request body containing the IP addresses/CIDR ranges to add.

**Returns:** `IpWhitelistResponse` reflecting the merged whitelist.

**Throws:** `KeyNotFoundException` if the key does not exist or is not owned by the current consumer. `ValidationException` if any provided entry is malformed. `InvalidOperationException` if the whitelist is not currently enforced.

### `public async Task<ActionResult<IpWhitelistResponse>> RemoveIpFromWhitelist`
Removes one or more IP addresses or CIDR ranges from an existing whitelist.

**Parameters:**
- `KeyId` (string): The unique identifier of the key.
- Request body containing the IP addresses/CIDR ranges to remove.

**Returns:** `IpWhitelistResponse` reflecting the updated whitelist. If a specified entry was not present, it is silently ignored.

**Throws:** `KeyNotFoundException` if the key does not exist or is not owned by the current consumer. `ValidationException` if any provided entry is malformed.

### `public async Task<ActionResult<RotateKeyResponse>> RotateKey`
Rotates an API key, generating new key material while invalidating the old one. The key's metadata, status, and whitelist are preserved.

**Parameters:**
- `KeyId` (string): The unique identifier of the key to rotate.
- `Name` (string, optional): A new name for the rotated key. If null, the existing name is retained.
- `ExpiresAt` (DateTime?, optional): A new expiration date. If null, the existing expiration is retained or recalculated.

**Returns:** `RotateKeyResponse` containing the new key material and updated metadata. The old key material is immediately invalidated.

**Throws:** `KeyNotFoundException` if the key does not exist or is not owned by the current consumer. `InvalidOperationException` if the key is in a non-rotatable state (disabled, revoked, deleted).

### `public async Task<IActionResult> DeleteKey`
Permanently deletes an API key and all associated metadata, including IP whitelist configuration. This action is irreversible.

**Parameters:**
- `KeyId` (string): The unique identifier of the key to delete.

**Returns:** `204 No Content` on successful deletion.

**Throws:** `KeyNotFoundException` if the key does not exist or is not owned by the current consumer.

## Usage

### Example 1: Creating a Key and Configuring Its IP Whitelist
```csharp
// Assume controller is injected and consumer context is established
var createResponse = await controller.CreateKey(new CreateKeyRequest
{
    Name = "Production Mobile App",
    ExpirationDays = 365
});

var createResult = createResponse.Result as OkObjectResult;
var keyData = createResult?.Value as CreateKeyResponse;

// Add IPs to the newly created key's whitelist
await controller.AddIpToWhitelist(new AddIpToWhitelistRequest
{
    KeyId = keyData.KeyId,
    Ips = new List<string> { "203.0.113.0/24", "198.51.100.42" }
});
```

### Example 2: Rotating a Key and Cleaning Up Old Ones
```csharp
// Retrieve all keys for the current consumer
var listResponse = await controller.GetConsumerKeys("consumer-123");
var listResult = listResponse.Result as OkObjectResult;
var keys = listResult?.Value as List<GetKeyResponse>;

// Rotate the first active key found
var activeKey = keys?.FirstOrDefault(k => k.Status == KeyStatus.Active);
if (activeKey != null)
{
    var rotateResponse = await controller.RotateKey(new RotateKeyRequest
    {
        KeyId = activeKey.KeyId,
        Name = "Rotated - " + activeKey.Name,
        ExpiresAt = DateTime.UtcNow.AddDays(90)
    });

    // Revoke the old key if rotation succeeds
    if (rotateResponse.Result is OkObjectResult)
    {
        await controller.RevokeKey(new RevokeKeyRequest { KeyId = activeKey.KeyId });
    }
}
```

## Notes

- **Consumer Scoping:** All operations are scoped to the consumer resolved from the authenticated request context. Attempting to access or modify keys belonging to another consumer will result in `KeyNotFoundException` rather than a distinct authorization error, preventing consumer enumeration.
- **State Transitions:** Keys follow a strict lifecycle: `Active` ↔ `Disabled` → `Revoked` → `Deleted`. Enabling is only valid from the `Disabled` state. Revoking is terminal except for deletion. Rotating is only permitted on `Active` keys.
- **IP Whitelist Validation:** CIDR notation and individual IP addresses are validated for syntactic correctness. Semantic checks (e.g., whether an address is routable) are not performed. Setting a whitelist replaces all entries; partial updates must use the dedicated Add/Remove endpoints.
- **Thread Safety:** This controller is not inherently thread-safe. Concurrent requests targeting the same key (e.g., simultaneous rotate and delete) may produce race conditions. Callers should implement client-side serialization or rely on the underlying data store's optimistic concurrency controls where available.
- **Immutability of `KeyId`:** Once created, a key's unique identifier cannot be changed. Rotation generates a new key with the same identifier but new secret material.
- **Expiration Handling:** Expiration is evaluated at authentication time. An expired key behaves as if disabled regardless of its stored status. Setting `ExpiresAt` to a past timestamp is permitted at creation or rotation but will render the key immediately unusable.
