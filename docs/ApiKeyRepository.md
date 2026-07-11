# ApiKeyRepository

The `ApiKeyRepository` provides data‑access operations for managing API keys within the `api-key-gateway` system. It encapsulates all create, read, update, and delete interactions with the underlying storage mechanism, allowing callers to work with `ApiKey` entities without concerning themselves with persistence details.

## API

### CreateAsync
```csharp
public Task<ApiKey> CreateAsync(ApiKey apiKey);
```
**Purpose** – Inserts a new API key record.  
**Parameters** – `apiKey`: The key to be created; must contain a valid consumer identifier, a plain‑text key value (which will be hashed before storage), and optional metadata such as expiration date.  
**Return value** – A `Task` that completes with the persisted `ApiKey` instance, including any database‑generated identifiers or hashed key value.  
**Exceptions** – Throws `ArgumentNullException` if `apiKey` is null; may throw `DbUpdateException` or a domain‑specific validation exception if the key violates uniqueness constraints or required fields are missing.

### GetByIdAsync
```csharp
public Task<ApiKey?> GetByIdAsync(Guid id);
```
**Purpose** – Retrieves a single API key by its primary key.  
**Parameters** – `id`: The unique identifier of the key to fetch.  
**Return value** – A `Task` that completes with the matching `ApiKey` or `null` if no record exists.  
**Exceptions** – Throws `ArgumentException` if `id` is `Guid.Empty`; any underlying data‑access errors propagate as `DbException`.

### GetByHashAsync
```csharp
public Task<ApiKey?> GetByHashAsync(string keyHash);
```
**Purpose** – Looks up an API key by its stored hash value (used during authentication).  
**Parameters** – `keyHash`: The cryptographic hash of the plain‑text key presented by a client.  
**Return value** – A `Task` that completes with the corresponding `ApiKey` or `null` if the hash does not match any stored key.  
**Exceptions** – Throws `ArgumentNullException` if `keyHash` is null; throws `ArgumentException` if the string is empty or whitespace.

### GetByConsumerIdAsync
```csharp
public Task<List<ApiKey>> GetByConsumerIdAsync(Guid consumerId);
```
**Purpose** – Returns all API keys associated with a specific consumer.  
**Parameters** – `consumerId`: The identifier of the consumer whose keys are requested.  
**Return value** – A `Task` that completes with a list of `ApiKey` objects; the list may be empty if the consumer has no keys.  
**Exceptions** – Throws `ArgumentException` if `consumerId` is `Guid.Empty`; data‑access failures are wrapped in `DbException`.

### UpdateAsync
```csharp
public Task UpdateAsync(ApiKey apiKey);
```
**Purpose** – Persists changes to an existing API key record.  
**Parameters** – `apiKey`: The key instance containing updated values; must represent an existing record (its `Id` must be non‑empty).  
**Return value** – A `Task` that completes when the update operation finishes. No entity is returned.  
**Exceptions** – Throws `ArgumentNullException` if `apiKey` is null; throws `InvalidOperationException` if the key’s `Id` is empty; may throw `DbUpdateConcurrencyException` if the record has been modified concurrently.

### DeleteAsync
```csharp
public Task DeleteAsync(Guid id);
```
**Purpose** – Removes an API key from the store.  
**Parameters** – `id`: The identifier of the key to delete.  
**Return value** – A `Task` that completes when the deletion is finished.  
**Exceptions** – Throws `ArgumentException` if `id` is `Guid.Empty`; throws `DbUpdateException` if the key cannot be removed due to foreign‑key constraints or other store errors.

### GetAllAsync
```csharp
public Task<List<ApiKey>> GetAllAsync();
```
**Purpose** – Retrieves every API key stored in the system.  
**Parameters** – None.  
**Return value** – A `Task` that completes with a list containing all `ApiKey` entities; returns an empty list if no keys exist.  
**Exceptions** – Any data‑access error is propagated as `DbException`.

### GetKeysExpiringBeforeAsync
```csharp
public Task<List<ApiKey>> GetKeysExpiringBeforeAsync(DateTimeOffset expiryUtc);
```
**Purpose** – Finds keys whose expiration date is earlier than the supplied threshold.  
**Parameters** – `expiryUtc`: The cutoff time (in UTC); keys with `ExpiresAt` < `expiryUtc` are returned.  
**Return value** – A `Task` that completes with a list of matching `ApiKey` objects; may be empty.  
**Exceptions** – Throws `ArgumentOutOfRangeException` if `expiryUtc` is earlier than `DateTimeOffset.MinValue` or later than `DateTimeOffset.MaxValue`; store errors become `DbException`.

### GetExpiredKeysAsync
```csharp
public Task<List<ApiKey>> GetExpiredKeysAsync();
```
**Purpose** – Retrieves all API keys that have already expired relative to the current system time.  
**Parameters** – None.  
**Return value** – A `Task` that completes with a list of `ApiKey` instances whose `ExpiresAt` is in the past; returns an empty list if none are expired.  
**Exceptions** – Propagates any store‑level exceptions as `DbException`.

## Usage

### Example 1: Creating and storing a new API key
```csharp
var repo = new ApiKeyRepository(connectionString);

var newKey = new ApiKey
{
    ConsumerId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    KeyValue   = "plain-text-secret-123",   // will be hashed by the repository
    ExpiresAt  = DateTimeOffset.UtcNow.AddDays(30),
    Description = "Test key for demo"
};

ApiKey stored = await repo.CreateAsync(newKey);
Console.WriteLine($"Created key with Id {stored.Id}");
```

### Example 2: Finding expired keys for cleanup
```csharp
var repo = new ApiKeyRepository(connectionString);

List<ApiKey> expired = await repo.GetExpiredKeysAsync();
foreach (var key in expired)
{
    await repo.DeleteAsync(key.Id);
    Console.WriteLine($"Deleted expired key {key.Id}");
}
```

## Notes

- All methods that accept an identifier (`Guid`) validate that the value is not `Guid.Empty` and will throw an `ArgumentException` otherwise.  
- Methods that receive an `ApiKey` instance (`CreateAsync`, `UpdateAsync`) perform null‑reference checks; passing `null` results in an `ArgumentNullException`.  
- The repository does **not** cache entities; each call results in a fresh query against the underlying data store. Consequently, callers must handle potential race conditions—for example, between reading a key with `GetByIdAsync` and subsequently updating it with `UpdateAsync`. In high‑concurrency scenarios, consider using optimistic concurrency tokens or transactional scopes provided by the storage layer.  
- The repository is stateless aside from its connection/context; therefore, multiple threads can safely invoke its methods concurrently provided the underlying `DbContext` or connection implementation is thread‑safe (as is typical for EF Core’s `DbContext` when used with a pool or per‑operation instance). If a shared mutable context is used, external synchronization is required.  
- Exceptions originating from the data access layer (e.g., `DbUpdateException`, `DbException`) are not caught and will bubble up to the caller; callers should handle them according to the application’s error‑handling policy.  
- The `GetByHashAsync` method expects a pre‑computed hash; supplying a plain‑text key will never match a stored record and will return `null`.  
- When using `GetKeysExpiringBeforeAsync` or `GetExpiredKeysAsync`, the comparison is performed in UTC; ensure that any `DateTimeOffset` values supplied or stored are correctly normalized to avoid off‑by‑errors due to local time zones.
