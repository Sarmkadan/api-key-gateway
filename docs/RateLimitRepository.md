# RateLimitRepository

The `RateLimitRepository` provides data access operations for managing rate limit configurations associated with API keys within the `api-key-gateway` system. It handles the persistence, retrieval, modification, and removal of `RateLimit` entities, ensuring that throttling rules are consistently stored and available for enforcement by upstream services. This repository acts as the primary interface between the application's rate limiting logic and the underlying data store.

## API

### `public RateLimitRepository`
Initializes a new instance of the `RateLimitRepository` class. This constructor typically injects necessary dependencies, such as database connections or configuration settings, required to perform data operations.

### `public async Task<RateLimit?> GetByApiKeyIdAsync`
Retrieves the rate limit configuration associated with a specific API key.
*   **Parameters**: Accepts the unique identifier of the API key (usually a `Guid` or `string`, depending on the domain model definition) as an argument.
*   **Return Value**: Returns a `Task` that resolves to a `RateLimit` object if a configuration exists for the provided key, or `null` if no record is found.
*   **Exceptions**: May throw database-related exceptions if the underlying storage connection fails or if a timeout occurs during the query.

### `public async Task<RateLimit> CreateAsync`
Persists a new rate limit configuration to the data store.
*   **Parameters**: Accepts a `RateLimit` entity containing the initial configuration details (e.g., limit count, time window, associated API key ID).
*   **Return Value**: Returns a `Task` that resolves to the created `RateLimit` object, potentially including system-generated fields such as the primary key or creation timestamp.
*   **Exceptions**: Throws an exception if a record with the same unique constraint (such as the API key ID) already exists, or if the database transaction fails.

### `public async Task UpdateAsync`
Updates an existing rate limit configuration in the data store.
*   **Parameters**: Accepts a `RateLimit` entity containing the updated values. The entity must include the valid identifier of the existing record.
*   **Return Value**: Returns a `Task` that completes when the update operation is finished. It does not return a value.
*   **Exceptions**: Throws an exception if the specified record does not exist, if concurrency checks fail (e.g., optimistic locking), or if the database transaction fails.

### `public async Task DeleteAsync`
Removes a rate limit configuration from the data store.
*   **Parameters**: Accepts the unique identifier of the rate limit record or the `RateLimit` entity to be deleted.
*   **Return Value**: Returns a `Task` that completes when the deletion operation is finished. It does not return a value.
*   **Exceptions**: Throws an exception if the record does not exist or if foreign key constraints prevent the deletion.

## Usage

### Retrieving and Updating a Rate Limit
This example demonstrates fetching an existing rate limit for a specific API key, modifying the request cap, and persisting the changes.

```csharp
public async Task AdjustRateLimitAsync(IRateLimitRepository repository, Guid apiKeyId, int newLimit)
{
    var rateLimit = await repository.GetByApiKeyIdAsync(apiKeyId);

    if (rateLimit == null)
    {
        throw new InvalidOperationException($"No rate limit found for API key {apiKeyId}.");
    }

    rateLimit.MaxRequests = newLimit;
    rateLimit.WindowInSeconds = 60;
    
    await repository.UpdateAsync(rateLimit);
}
```

### Creating a New Rate Limit Configuration
This example shows how to initialize a new rate limit rule for a newly registered API key.

```csharp
public async Task InitializeKeyRateLimitAsync(IRateLimitRepository repository, Guid apiKeyId)
{
    var newRateLimit = new RateLimit
    {
        ApiKeyId = apiKeyId,
        MaxRequests = 100,
        WindowInSeconds = 3600,
        Strategy = RateLimitStrategy.SlidingWindow
    };

    var createdLimit = await repository.CreateAsync(newRateLimit);
    
    Console.WriteLine($"Created rate limit with ID: {createdLimit.Id}");
}
```

## Notes

*   **Null Handling**: Consumers of `GetByApiKeyIdAsync` must explicitly handle the `null` return case, as the absence of a rate limit record may imply either an unconfigured key or a key that should be rejected by default policy.
*   **Concurrency**: As an asynchronous repository interacting with a shared data store, concurrent calls to `UpdateAsync` for the same entity may result in race conditions. Implementations should rely on database-level locking or optimistic concurrency tokens to ensure data integrity.
*   **Existence Guarantees**: `CreateAsync` assumes the provided entity does not already exist in the store based on unique constraints. Attempting to create a duplicate entry for the same `ApiKeyId` will result in an exception.
*   **Transaction Scope**: Individual methods like `UpdateAsync` and `DeleteAsync` typically execute within their own implicit transaction scope. For operations requiring atomicity across multiple repositories, external transaction management may be required.
