# IUsageQuotaService

The `IUsageQuotaService` interface defines the contract for managing and enforcing usage quotas within the `api-key-gateway` project. It provides mechanisms to retrieve current quota configurations, update limits dynamically, and perform atomic checks that both validate remaining allowance and record consumption in a single operation. This service is critical for preventing resource exhaustion and ensuring fair usage policies are applied to API keys or tenants.

## API

### `UsageQuotaResult`
A public record type representing the outcome of a quota check and recording operation. It encapsulates the status of the request (e.g., allowed or denied), the remaining quota count, and any relevant metadata regarding the enforcement decision.

### `UsageQuotaService`
The concrete implementation class of the `IUsageQuotaService` interface. This class handles the underlying logic for interacting with the quota storage backend, managing concurrency, and executing the business rules defined by the interface methods.

### `CheckAndRecordAsync`
```csharp
public async Task<UsageQuotaResult> CheckAndRecordAsync(...)
```
Atomically verifies if sufficient quota remains for a specific operation and, if successful, decrements the available count.
*   **Purpose**: To enforce limits while simultaneously tracking usage to prevent race conditions between checking and recording.
*   **Parameters**: Accepts context necessary to identify the quota subject (typically an API key or tenant ID) and the cost of the operation.
*   **Return Value**: Returns a `UsageQuotaResult` indicating whether the operation was permitted and detailing the remaining balance.
*   **Exceptions**: May throw exceptions if the underlying storage is unavailable or if the provided context is invalid.

### `GetQuotaAsync`
```csharp
public async Task<UsageQuota?> GetQuotaAsync(...)
```
Retrieves the current quota configuration and consumption status for a specific entity without modifying the count.
*   **Purpose**: To inspect current limits and usage levels for reporting or pre-flight validation.
*   **Parameters**: Accepts an identifier to locate the specific quota record.
*   **Return Value**: Returns a `UsageQuota` object containing limit and current usage details, or `null` if no quota is configured for the target.
*   **Exceptions**: Throws if the data store is unreachable.

### `SetQuotaAsync`
```csharp
public async Task<bool> SetQuotaAsync(...)
```
Updates or creates a quota definition for a specific entity.
*   **Purpose**: To dynamically adjust usage limits or initialize quota records for new entities.
*   **Parameters**: Accepts the target identifier and the new quota configuration values (limit, reset policy, etc.).
*   **Return Value**: Returns `true` if the operation succeeded, or `false` if the update failed due to concurrency conflicts or validation errors.
*   **Exceptions**: May throw if the configuration data is malformed.

## Usage

### Example 1: Enforcing a Request Limit
The following example demonstrates how to use `CheckAndRecordAsync` at the entry point of an API request to ensure the caller has not exceeded their allocated limit before processing.

```csharp
public async Task<IActionResult> ProcessRequest(string apiKey, IUsageQuotaService quotaService)
{
    // Attempt to consume one unit of quota
    var result = await quotaService.CheckAndRecordAsync(apiKey, cost: 1);

    if (!result.IsAllowed)
    {
        return StatusCode(429, "Quota exceeded. Please try again later.");
    }

    // Proceed with business logic
    return Ok(new { Message = "Request processed successfully", Remaining = result.Remaining });
}
```

### Example 2: Dynamically Updating Quota Limits
This example illustrates how an administrative function might use `SetQuotaAsync` to increase a tenant's limit and then verify the change using `GetQuotaAsync`.

```csharp
public async Task UpdateTenantLimit(string tenantId, int newLimit, IUsageQuotaService quotaService)
{
    var success = await quotaService.SetQuotaAsync(tenantId, newLimit);

    if (!success)
    {
        throw new InvalidOperationException("Failed to update quota configuration.");
    }

    var currentQuota = await quotaService.GetQuotaAsync(tenantId);
    
    if (currentQuota.HasValue)
    {
        Console.WriteLine($"Quota updated for {tenantId}. New Limit: {currentQuota.Value.Limit}");
    }
}
```

## Notes

*   **Concurrency**: The `CheckAndRecordAsync` method is designed to be atomic. Implementations must handle concurrent requests to ensure that two simultaneous calls do not both succeed when only one unit of quota remains.
*   **Null Handling**: Consumers of `GetQuotaAsync` must explicitly handle the `null` return case, which indicates that no quota policy is currently assigned to the requested identifier. This should typically be treated as either "unlimited" or "denied" depending on the system's default security posture.
*   **Thread Safety**: The `UsageQuotaService` implementation is expected to be thread-safe and suitable for registration as a singleton or scoped service within the dependency injection container.
*   **Latency**: As all public methods are asynchronous and likely involve I/O operations with a distributed cache or database, callers should await these tasks promptly and avoid blocking threads.
