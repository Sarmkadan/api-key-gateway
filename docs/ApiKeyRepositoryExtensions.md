# ApiKeyRepositoryExtensions

Extension methods for `ApiKeyRepository` providing common query patterns and domain-specific operations without modifying the core repository implementation.

**Namespace:** `ApiKeyGateway.Repositories`
**File:** `src/ApiKeyGateway/Repositories/ApiKeyRepositoryExtensions.cs`

## Public Methods

### ExistsByIdAsync

```csharp
public static async Task<bool> ExistsByIdAsync(this ApiKeyRepository repository, string id)
```

Checks if an API key with the specified ID exists in the repository.

- **Parameters:**
  - `repository` - The `ApiKeyRepository` instance.
  - `id` - The ID of the API key to check.
- **Returns:** `true` if the API key exists, `false` otherwise.
- **Throws:** `ArgumentNullException` when `repository` is null or `id` is null/empty.

### GetActiveKeysByConsumerAsync

```csharp
public static async Task<IReadOnlyList<ApiKey>> GetActiveKeysByConsumerAsync(this ApiKeyRepository repository, string consumerId)
```

Retrieves all active API keys for a given consumer. Filters results to only include keys with `ApiKeyStatus.Active`.

- **Parameters:**
  - `repository` - The `ApiKeyRepository` instance.
  - `consumerId` - The consumer ID to filter by.
- **Returns:** A read-only list of active `ApiKey` instances.
- **Throws:** `ArgumentNullException` when `repository` is null or `consumerId` is null/empty.

### GetActiveKeyCountByConsumerAsync

```csharp
public static async Task<int> GetActiveKeyCountByConsumerAsync(this ApiKeyRepository repository, string consumerId)
```

Counts the number of active API keys for a given consumer.

- **Parameters:**
  - `repository` - The `ApiKeyRepository` instance.
  - `consumerId` - The consumer ID to count for.
- **Returns:** The number of active API keys.
- **Throws:** `ArgumentNullException` when `repository` is null or `consumerId` is null/empty.
