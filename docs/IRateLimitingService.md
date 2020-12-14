# IRateLimitingService

The `IRateLimitingService` interface defines the contract for managing request throttling and quota enforcement within the `api-key-gateway`. It provides asynchronous operations to validate incoming requests against configured limits, record usage statistics, retrieve current limit configurations, dynamically update thresholds, and reset sliding time windows. This service is central to preventing resource exhaustion and ensuring fair usage distribution across API consumers.

## API

### Constructors

**`public RateLimitingService`**
Initializes a new instance of the `RateLimitingService` class. This constructor typically injects required dependencies such as distributed caching providers, configuration readers, and logging facilities to enable stateful rate limiting across multiple gateway instances.

**`public RateLimitingService`**
Represents an overloaded constructor for the `RateLimitingService` class. This variant allows for alternative initialization scenarios, potentially accepting explicit configuration objects or mockable dependencies for specialized testing environments or non-standard deployment topologies.

### Methods

**`public async Task<bool> CheckLimitAsync`**
Evaluates whether the current request count for a specific identifier (e.g., API key or IP address) has exceeded the defined threshold within the active time window.
*   **Parameters**: Accepts a string identifier representing the client and optional context parameters defining the scope of the limit.
*   **Return Value**: Returns `true` if the request is allowed; `false` if the limit has been exceeded.
*   **Exceptions**: Throws `InvalidOperationException` if the service is not initialized or if the underlying storage provider is unavailable.

**`public async Task RecordRequestAsync`**
Increments the request counter for the specified identifier and updates the associated metadata in the backing store. This method should be invoked immediately after `CheckLimitAsync` returns `true`.
*   **Parameters**: Takes the client identifier and a timestamp indicating when the request occurred.
*   **Return Value**: Returns a completed `Task` upon successful persistence of the usage record.
*   **Exceptions**: Throws `TimeoutException` if the write operation to the distributed cache exceeds the configured timeout period.

**`public async Task<RateLimit?> GetLimitAsync`**
Retrieves the current rate limit configuration and status for a given identifier.
*   **Parameters**: Requires the unique client identifier.
*   **Return Value**: Returns a `RateLimit` object containing the maximum allowed requests, the current count, and the window reset time. Returns `null` if no limit is configured for the identifier.
*   **Exceptions**: Throws `ArgumentException` if the provided identifier is null or empty.

**`public async Task<bool> UpdateLimitAsync`**
Dynamically modifies the rate limit thresholds for a specific identifier without requiring a service restart.
*   **Parameters**: Accepts the client identifier and a new `RateLimit` configuration object.
*   **Return Value**: Returns `true` if the update was successfully applied; `false` if the identifier was not found or the update failed due to concurrency conflicts.
*   **Exceptions**: Throws `ValidationException` if the new limit configuration contains invalid values (e.g., negative counts or zero-duration windows).

**`public async Task ResetWindowAsync`**
Manually clears the request count and resets the time window for a specific identifier, effectively granting a fresh quota immediately.
*   **Parameters**: Takes the client identifier to reset.
*   **Return Value**: Returns a completed `Task` when the reset operation is finished.
*   **Exceptions**: Throws `KeyNotFoundException` if the identifier does not exist in the current tracking set.

## Usage

### Example 1: Standard Request Validation Pipeline
This example demonstrates the typical pattern of checking a limit before processing a request and recording the usage only upon success.

```csharp
public async Task<IActionResult> HandleRequest(string apiKey, HttpRequest request)
{
    // Check if the client is within their allowed quota
    bool isAllowed = await _rateLimitingService.CheckLimitAsync(apiKey);

    if (!isAllowed)
    {
        return new TooManyRequestsResult();
    }

    // Record the successful check as a consumed request
    await _rateLimitingService.RecordRequestAsync(apiKey, DateTime.UtcNow);

    // Proceed with business logic
    return Ok(await _businessService.ProcessAsync(request));
}
```

### Example 2: Dynamic Limit Adjustment for Premium Users
This example shows how to retrieve current limits and upgrade a user's quota dynamically based on external events, such as a subscription change.

```csharp
public async Task UpgradeUserQuota(string userId, int newMaxRequests)
{
    // Fetch current configuration
    var currentLimit = await _rateLimitingService.GetLimitAsync(userId);
    
    if (currentLimit == null)
    {
        throw new InvalidOperationException("User limit configuration not found.");
    }

    // Create updated configuration preserving the existing window duration
    var updatedConfig = new RateLimit 
    { 
        MaxRequests = newMaxRequests, 
        WindowDuration = currentLimit.WindowDuration 
    };

    // Apply the new limit immediately
    bool success = await _rateLimitingService.UpdateLimitAsync(userId, updatedConfig);

    if (!success)
    {
        // Handle concurrency conflict or failure
        throw new ServiceException("Failed to apply quota upgrade.");
    }
}
```

## Notes

*   **Concurrency and Race Conditions**: The separation of `CheckLimitAsync` and `RecordRequestAsync` implies that the implementation must handle potential race conditions where multiple simultaneous requests might pass the check before any are recorded. Implementations should utilize atomic operations (e.g., Lua scripts in Redis) or distributed locks internally to ensure accuracy.
*   **Null Handling**: Consumers of `GetLimitAsync` must explicitly handle the `null` return case, which indicates that no rate limiting policy is currently applied to the specified identifier.
*   **Thread Safety**: As all public methods are asynchronous and likely interact with shared distributed state, the service implementation is designed to be thread-safe. However, callers should avoid caching the results of `GetLimitAsync` for extended periods, as the underlying limits may change via `UpdateLimitAsync` or external processes.
*   **Reset Behavior**: Invoking `ResetWindowAsync` does not alter the configured maximum request count; it only zeroes the current counter and restarts the time window. This is useful for administrative overrides but should be used cautiously to prevent abuse.
