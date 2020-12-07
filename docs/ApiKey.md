# ApiKey

Represents a single API key managed by the gateway, containing its identifying data, usage statistics, and state information. The type is intended to be persisted in a store and manipulated through service‑layer operations that enforce business rules such as expiration, disabling, and quota enforcement.

## API

### Id  
**Type:** `string`  
**Purpose:** Unique identifier for the API key within the system. Assigned when the key is created and never changes.  
**Remarks:** Should be treated as immutable; modifying it after persistence may break look‑ups.

### ConsumerId  
**Type:** `string`  
**Purpose:** Identifier of the consumer (application or user) that owns this key. Links the key to a consumer record for ownership and billing purposes.  
**Remarks:** Must not be null or empty when the key is persisted.

### Name  
**Type:** `string`  
**Purpose:** Human‑readable label for the key, useful for UI display and administrative tasks.  
**Remarks:** Can be changed at any time; does not affect key functionality.

### KeyHash  
**Type:** `string`  
**Purpose:** Cryptographic hash of the actual secret key value. The plaintext secret is never stored; only this hash is kept for verification.  
**Remarks:** Set once upon key creation and should never be altered.

### Prefix  
**Type:** `string`  
**Purpose:** First few characters of the raw key (e.g., `ak_`) that can be safely exposed in logs or UI without revealing the secret.  
**Remarks:** Useful for debugging while maintaining security.

### Status  
**Type:** `Enums.ApiKeyStatus`  
**Purpose:** Current lifecycle state of the key (e.g., `Active`, `Disabled`, `Expired`).  
**Remarks:** Transitions are performed via the `Disable` method or expiration logic; setting this field directly is discouraged.

### CreatedAt  
**Type:** `DateTime`  
**Purpose:** Timestamp indicating when the key was first created.  
**Remarks:** Set automatically on creation; never changes.

### ExpiresAt  
**Type:** `DateTime?`  
**Purpose:** Optional expiration date and time after which the key is considered invalid.  
**Remarks:** If `null`, the key does not expire based on time.

### LastUsedAt  
**Type:** `DateTime?`  
**Purpose:** Timestamp of the most recent successful request that used this key.  
**Remarks:** Updated by `RecordUsage`; remains `null` until the key has been used.

### DisabledAt  
**Type:** `DateTime?`  
**Purpose:** Timestamp indicating when the key was disabled via the `Disable` method.  
**Remarks:** `null` when the key is active; otherwise set to the disabling time.

### Description  
**Type:** `string?`  
**Purpose:** Free‑form text describing the key’s purpose, restrictions, or any administrative notes.  
**Remarks:** May be `null` or empty.

### Metadata  
**Type:** `Dictionary<string, string>`  
**Purpose:** Arbitrary key‑value pairs for extending the key with custom attributes (e.g., tags, feature flags).  
**Remarks:** The dictionary is never `null`; callers should initialize it if needed. Modifications are reflected directly in the instance.

### RequestCount  
**Type:** `int`  
**Purpose:** Total number of successful requests that have been recorded for this key.  
**Remarks:** Incremented by `RecordUsage`.

### BytesTransferred  
**Type:** `long`  
**Purpose:** Cumulative number of bytes transmitted in requests and responses associated with this key.  
**Remarks:** Updated by `RecordUsage`.

### IpWhitelist  
**Type:** `string?`  
**Purpose:** Comma‑separated list of IP addresses or CIDR ranges that are allowed to use the key.  
**Remarks:** `null` or empty means no IP restriction is applied.

### RateLimitId  
**Type:** `string?`  
**Purpose:** Identifier of a rate‑limit policy bound to this key.  
**Remarks:** `null` indicates the key uses the default rate limit or none.

### AllowedScopes  
**Type:** `string?`  
**Purpose:** Space‑ or comma‑separated list of scopes that the key is authorized to access.  
**Remarks:** `null` or empty implies no scope restrictions.

### CanBeUsed  
**Type:** `bool`  
**Purpose:** Indicates whether the key is currently usable based on its `Status`, expiration, and disable timestamps.  
**Remarks:** This field is updated automatically by the gateway when `Status`, `ExpiresAt`, or `DisabledAt` change; it should not be set directly.

### RecordUsage  
**Signature:** `public void RecordUsage(int requestCount = 1, long bytes = 0)`  
**Purpose:** Logs that the key was used for a request, updating usage statistics and the last‑used timestamp.  
**Parameters:**  
- `requestCount` – Number of requests to add to `RequestCount` (default 1).  
- `bytes` – Number of bytes transferred to add to `BytesTransferred` (default 0).  
**Exceptions:**  
- `ObjectDisposedException` – If the key instance has been logically disposed (not applicable in current implementation but reserved).  
- `InvalidOperationException` – If the key is disabled (`Status == Disabled`) or expired (`ExpiresAt` in the past) and the gateway enforces usage blocking.  
**Remarks:** Also sets `LastUsedAt` to `DateTime.UtcNow`. Does not modify `CanBeUsed` directly; that property is recomputed elsewhere.

### Disable  
**Signature:** `public void Disable()`  
**Purpose:** Marks the key as disabled, preventing further use.  
**Exceptions:**  
- `InvalidOperationException` – If the key is already disabled.  
**Remarks:** Sets `Status` to `ApiKeyStatus.Disabled`, records `DisabledAt` as `DateTime.UtcNow`, and ensures `CanBeUsed` evaluates to `false`. Does not affect existing usage statistics.

## Usage

```csharp
// Example: Creating and using an API key
using ApiKeyGateway.Models;

// Assume a service provides a new key
var newKey = new ApiKey
{
    Id = Guid.NewGuid().ToString(),
    ConsumerId = "consumer-123",
    Name = "Development key",
    KeyHash = ComputeHash("sk_live_abcdef123456"),
    Prefix = "sk_live",
    Status = Enums.ApiKeyStatus.Active,
    CreatedAt = DateTime.UtcNow,
    ExpiresAt = DateTime.UtcNow.AddYears(1),
    Metadata = new Dictionary<string, string> { { "environment", "dev" } }
};

// Record a request
newKey.RecordUsage(requestCount: 1, bytes: 1024);

// Check if the key can still be used
if (newKey.CanBeUsed)
{
    // Proceed with request handling
}
else
{
    // Respond with 401 or 403
}
```

```c
// Example: Disabling a key and verifying its state
var key = repository.GetApiKey("existing-key-id");

// Disable the key (idempotent)
key.Disable();

// After disabling, usage should be rejected
try
{
    key.RecordUsage(); // May throw InvalidOperationException if usage is blocked
}
catch (InvalidOperationException ex)
{
    // Log or handle as appropriate
    logger.Warning(ex, "Attempted to use disabled key {KeyId}", key.Id);
}

// Verify fields
Console.WriteLine($"Status: {key.Status}");          // Disabled
Console.WriteLine($"DisabledAt: {key.DisabledAt}"); // Timestamp of disable
Console.WriteLine($"CanBeUsed: {key.CanBeUsed}");   // False
```

## Notes

- **Immutability of identifiers:** `Id`, `ConsumerId`, `KeyHash`, and `Prefix` are intended to be set once and never changed. Altering them after persistence can break lookup and validation logic.
- **Nullability:** Reference‑type members that are declared nullable (`string?`, `DateTime?`) may legitimately be `null`. Code consuming an `ApiKey` instance should check for null before using these values (e.g., when parsing `IpWhitelist` or evaluating `ExpiresAt`).
- **Metadata safety:** The `Metadata` dictionary is never `null`, but callers must guard against concurrent modifications if the instance is accessed from multiple threads without external synchronization.
- **Thread safety:** The type does **not** provide internal locking. Concurrent calls to `RecordUsage` or `Disable` from multiple threads can lead to race conditions (e.g., lost increments or inconsistent state). Consumers should synchronize access (e.g., using `lock` or concurrent collections) when sharing an `ApiKey` instance across threads.
- **CanBeUsed semantics:** The flag reflects the logical state derived from `Status`, `ExpiresAt`, and `DisabledAt`. It is not automatically updated by the property setters; the gateway service recomputes it after any change that could affect usability.
- **Usage recording:** `RecordUsage` does not enforce rate limits or quotas; those checks are performed elsewhere in the pipeline. The method merely updates counters and timestamps.
- **Extension points:** Future versions may add additional fields (e.g., refresh token references) without breaking existing code, as long as they are added as new members with appropriate defaults.
