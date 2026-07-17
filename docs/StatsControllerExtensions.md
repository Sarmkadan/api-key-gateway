# StatsControllerExtensions

The `StatsControllerExtensions` class provides a set of static helper methods and data transfer object (DTO) definitions designed to standardize the construction of statistical responses within the `api-key-gateway` project. It encapsulates the logic for generating usage statistics, rate limit statuses, and endpoint-specific metrics, ensuring consistent data shaping before serialization to API consumers. By utilizing sealed records for its return types, the extension guarantees immutability and value-based equality for all statistical data payloads.

## API

### `UsageStatsDto GetUsageStatisticsDto`
Generates a data transfer object containing aggregate usage statistics for a specific API key or time window.
*   **Purpose**: Constructs a snapshot of total requests, successful calls, failed calls, and data transfer metrics.
*   **Parameters**: Accepts internal aggregation parameters derived from repository data (specific signature depends on injected context, typically source metrics).
*   **Return Value**: Returns a `UsageStatsDto` record containing the calculated totals.
*   **Exceptions**: Throws an exception if the underlying metric source is null or if arithmetic overflow occurs during aggregation.

### `RateLimitStatusDto GetRateLimitStatusDto`
Constructs a status object representing the current rate limiting state for a requester.
*   **Purpose**: Provides immediate feedback on remaining quota, reset times, and current limit thresholds.
*   **Parameters**: Requires current count, maximum limit, and reset timestamp information.
*   **Return Value**: Returns a `RateLimitStatusDto` record. Note that this record may contain a nested `RateLimitDto` defining the policy configuration.
*   **Exceptions**: Throws if the reset timestamp is invalid or if the current count exceeds integer bounds.

### `IReadOnlyList<EndpointStatDto> GetEndpointStatisticsList`
Retrieves a collection of statistics broken down by individual API endpoints.
*   **Purpose**: Enables granular analysis of traffic patterns per route or method.
*   **Parameters**: Accepts a collection of raw endpoint logs or aggregated counters.
*   **Return Value**: Returns an `IReadOnlyList<EndpointStatDto>`, ensuring the consumer cannot modify the underlying collection.
*   **Exceptions**: Throws if the input data contains duplicate endpoint keys that cannot be resolved or if the input list is null.

### `sealed record UsageStatsDto`
An immutable record representing high-level usage metrics.
*   **Properties**: Typically includes `TotalRequests`, `SuccessCount`, `FailureCount`, and `BytesTransferred`.
*   **Behavior**: Supports deconstruction and value-based equality checks.

### `sealed record RateLimitDto`
An immutable record defining the configuration of a rate limit policy.
*   **Properties**: Includes `Limit`, `WindowDuration`, and `PolicyType`.
*   **Behavior**: Used as a nested property within `RateLimitStatusDto` to describe the active rules.

### `sealed record RateLimitStatusDto`
An immutable record representing the dynamic state of rate limiting.
*   **Properties**: Includes `Remaining`, `ResetAt`, `LimitExceeded`, and the associated `RateLimitDto` configuration.
*   **Behavior**: Provides the client with necessary headers or body content to handle throttling.

### `sealed record EndpointStatDto`
An immutable record holding metrics for a single endpoint.
*   **Properties**: Includes `EndpointPath`, `HttpMethod`, `RequestCount`, and `AverageLatencyMs`.
*   **Behavior**: Elements of the list returned by `GetEndpointStatisticsList`.

## Usage

The following examples demonstrate how to utilize these extensions within a controller or service layer to format statistical responses.

### Example 1: Generating Aggregate Usage Report
This example shows how to construct a usage summary for a specific API key to return to an administrative dashboard.

```csharp
public async Task<ActionResult<UsageStatsDto>> GetKeyUsage(string apiKeyId)
{
    var rawMetrics = await _usageRepository.GetRawMetricsAsync(apiKeyId);
    
    // Utilize the extension method to shape the data
    var statsDto = StatsControllerExtensions.GetUsageStatisticsDto(rawMetrics);
    
    // The returned record is immutable and ready for serialization
    return Ok(statsDto);
}
```

### Example 2: Constructing Rate Limit Headers and Body
This example illustrates creating a detailed rate limit status object that includes both the current state and the policy definition.

```csharp
public IActionResult CheckRateLimit(string clientId, int currentCount, int maxLimit, DateTime resetTime)
{
    // Generate the status DTO including the nested policy definition
    var statusDto = StatsControllerExtensions.GetRateLimitStatusDto(
        currentCount, 
        maxLimit, 
        resetTime
    );

    // Access nested RateLimitDto for policy details if needed
    var policy = statusDto.Policy; 

    Response.Headers.Append("X-RateLimit-Remaining", statusDto.Remaining.ToString());
    
    return Ok(statusDto);
}
```

## Notes

*   **Immutability**: All DTOs (`UsageStatsDto`, `RateLimitDto`, `RateLimitStatusDto`, `EndpointStatDto`) are defined as `sealed record` types. This ensures that once a statistical snapshot is created by the extension methods, its state cannot be altered, preventing race conditions where data might change between creation and serialization.
*   **Thread Safety**: The extension methods themselves are stateless and rely solely on their input parameters. They are inherently thread-safe provided that the input collections (e.g., the source data for `GetEndpointStatisticsList`) are not mutated concurrently by other threads during the method execution.
*   **Read-Only Collections**: The `GetEndpointStatisticsList` method returns an `IReadOnlyList<T>`. Consumers must treat this list as read-only; attempting to cast it back to a mutable list and modify it may result in runtime errors depending on the internal implementation of the list construction.
*   **Null Handling**: While the extension methods aim to produce valid DTOs, they do not silently swallow data errors. Passing null collections or invalid date ranges (e.g., a reset time in the past for a new window) will result in exceptions rather than default values, enforcing data integrity at the boundary.
