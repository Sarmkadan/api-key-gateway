# UsageQuotaRepository

`UsageQuotaRepository` provides persistence operations for `UsageQuota` entities within the API key gateway. It encapsulates the data-access logic required to retrieve, create, update, and delete usage quota records associated with API keys, serving as the concrete implementation behind the `IUsageQuotaService` abstraction.

## API

### UsageQuotaRepository

Instantiates a new repository instance. The constructor is expected to receive an injected database connection or data-context dependency (not shown in the public surface), which it uses internally for all subsequent operations.

### GetByApiKeyIdAsync

```csharp
public async Task<UsageQuota?> GetByApiKeyIdAsync(string apiKeyId)
```

Retrieves the `UsageQuota` record bound to the specified API key identifier.

**Parameters:**
- `apiKeyId` — The unique identifier of the API key whose quota is being requested. Must not be null or empty.

**Returns:** The matching `UsageQuota` instance, or `null` if no quota has been configured for the given API key.

**Throws:** `ArgumentNullException` when `apiKeyId` is null. May throw data-access exceptions (e.g., `InvalidOperationException`, provider-specific connection faults) if the underlying store is unreachable or the query fails.

### CreateAsync

```csharp
public async Task<UsageQuota> CreateAsync(UsageQuota quota)
```

Persists a new `UsageQuota` record to the backing store.

**Parameters:**
- `quota` — A fully populated `UsageQuota` object containing the API key association, limit values, and any metadata required for creation. Must not be null.

**Returns:** The created `UsageQuota` instance, reflecting any server-generated values (such as an assigned identifier or timestamp) after successful persistence.

**Throws:** `ArgumentNullException` when `quota` is null. Throws a concurrency or constraint-violation exception (e.g., `DbUpdateException`) if a quota for the same API key already exists or a required foreign-key relationship is missing.

### UpdateAsync

```csharp
public async Task UpdateAsync(UsageQuota quota)
```

Updates an existing `UsageQuota` record in the backing store.

**Parameters:**
- `quota` — The `UsageQuota` object containing the modified fields and the identifier of the record to update. Must not be null.

**Returns:** No value (task completes when the update has been committed).

**Throws:** `ArgumentNullException` when `quota` is null. Throws a concurrency or not-found exception if the record no longer exists in the store or if optimistic-concurrency checks fail.

### DeleteAsync

```csharp
public async Task DeleteAsync(string apiKeyId)
```

Removes the `UsageQuota` record associated with the given API key identifier from the backing store.

**Parameters:**
- `apiKeyId` — The unique identifier of the API key whose quota should be deleted. Must not be null or empty.

**Returns:** No value (task completes when the deletion has been committed).

**Throws:** `ArgumentNullException` when `apiKeyId` is null. May throw a not-found exception if no quota exists for the specified API key, depending on the implementation’s delete semantics.

## Usage

### Example 1: Creating and immediately retrieving a quota

```csharp
var repository = new UsageQuotaRepository(dbConnection);

var newQuota = new UsageQuota
{
    ApiKeyId = "key-abc123",
    DailyLimit = 10_000,
    BurstLimit = 500
};

UsageQuota created = await repository.CreateAsync(newQuota);

// Later, fetch the persisted record
UsageQuota? fetched = await repository.GetByApiKeyIdAsync("key-abc123");
if (fetched is not null)
{
    Console.WriteLine($"Daily limit: {fetched.DailyLimit}");
}
```

### Example 2: Updating a quota and cleaning up

```csharp
var repository = new UsageQuotaRepository(dbConnection);

UsageQuota? existing = await repository.GetByApiKeyIdAsync("key-xyz789");
if (existing is null)
{
    return;
}

existing.DailyLimit = 20_000;
await repository.UpdateAsync(existing);

// When the API key is revoked, remove its quota
await repository.DeleteAsync("key-xyz789");
```

## Notes

- **Null returns from `GetByApiKeyIdAsync`:** Callers must guard against a `null` result, which indicates the absence of a quota configuration rather than a fault. Treating `null` as “unlimited” or “default” is a policy decision left to the consuming service layer.
- **Idempotency of `DeleteAsync`:** The implementation may throw if no matching record exists, or it may silently succeed. Callers should not assume idempotent behavior unless explicitly documented by the underlying data provider.
- **Thread safety:** This repository does not guarantee thread safety across independent instances or concurrent operations on the same record. Concurrent updates to the same quota row may result in optimistic-concurrency failures or lost updates unless an external synchronization mechanism (or the store’s own concurrency tokens) is employed.
- **Transactional boundaries:** Each method executes within its own implicit transaction scope unless wrapped in an ambient transaction by the caller. Batch operations spanning multiple methods should be orchestrated within an explicit unit-of-work to preserve atomicity.
