# StatsController

The `StatsController` provides HTTP endpoints for exposing operational metrics and health information of the API gateway. It aggregates data from underlying services such as usage tracking, rate limiting, quota management, and recent activity logs, returning the information in a consumable JSON format.

## API

### GetUsageStatistics
- **Purpose**: Returns aggregated usage statistics (e.g., total requests, bandwidth, error rates) for a configurable time window.
- **Parameters**: None.
- **Return Value**: `IActionResult`. On success, returns an `OkObjectResult` containing a DTO with usage metrics. On failure, returns a non‑200 status code (e.g., `500 Internal Server Error`) or propagates an exception that results in an error response.
- **When it throws**: May throw `InvalidOperationException` if the statistics store is unavailable or misconfigured. Exceptions from data‑access dependencies are also propagated and translated into error responses by the ASP.NET Core pipeline.

### GetRateLimitStatus
- **Purpose**: Provides the current rate‑limit configuration and utilization for the gateway or a specific client.
- **Parameters**: None.
- **Return Value**: `IActionResult`. Successful calls yield an `OkObjectResult` with a DTO detailing limit values, remaining requests, and reset timestamps. Errors produce appropriate error status codes.
- **When it throws**: Throws `InvalidOperationException` when the rate‑limit backing store cannot be accessed. Other unexpected exceptions are bubbled up to the middleware layer.

### GetEndpointStatistics
- **Purpose**: Delivers per‑endpoint metrics such as request count, average latency, and error percentages.
- **Parameters**: None.
- **Return Value**: `IActionResult`. Returns `OkObjectResult` with a collection of endpoint‑specific statistic objects on success; otherwise returns an error result.
- **When it throws**: May throw `InvalidOperationException` if the endpoint telemetry source is inaccessible. Any exception from the underlying telemetry service results in a non‑200 response.

### GetRecentActivity
- **Purpose**: Returns a chronological list of recent gateway activities (e.g., API key validations, quota consumptions, blocked requests).
- **Parameters**: None.
- **Return Value**: `IActionResult`. On success, returns an `OkObjectResult` containing an array of activity log entries. Failure yields an error status code.
- **When it throws**: Throws `InvalidOperationException` when the activity log store cannot be queried. Other exceptions are handled by the global error handling middleware.

### GetQuotaStatus
- **Purpose**: Reports quota consumption and remaining allowance for tracked clients or API keys.
- **Parameters**: None.
- **Return Value**: `IActionResult`. Successful responses are `OkObjectResult` objects with quota details; errors result in non‑200 status codes.
- **When it throws**: May throw `InvalidOperationException` if the quota repository is unavailable. Unexpected exceptions are converted to error responses by the framework.

## Usage

```csharp
// Example 1: Direct instantiation for unit testing
var statsController = new StatsController(mockStatisticsService.Object);
IActionResult result = statsController.GetUsageStatistics();
var okResult = result as OkObjectResult;
var usageDto = okResult?.Value as UsageStatisticsDto;
Assert.NotNull(usageDto);
```

```csharp
// Example 2: Calling the endpoint via HttpClient in an integration test
using var client = new HttpClient { BaseAddress = new Uri("https://gateway.example.com") };
HttpResponseMessage response = await client.GetAsync("/api/stats/ratelimit");
response.EnsureSuccessStatusCode();
string json = await response.Content.ReadAsStringAsync();
var quotaInfo = JsonSerializer.Deserialize<RateLimitStatusDto>(json);
```

## Notes

- The controller itself is stateless; all state is held in injected services. Therefore, the controller is thread‑safe as long as its dependencies (e.g., statistics, rate‑limit, quota services) are thread‑safe or scoped appropriately per request.
- If a particular metric source has no data, the endpoints return default or empty values (e.g., zero counts, empty collections) rather than throwing, unless the source is completely unavailable.
- Consumers should treat any non‑200 response as indicative of a service‑side issue and may retry after a brief back‑off, depending on the error code.
- The methods accept no input parameters; filtering or time‑range selection, if required, must be configured through query strings or request headers handled by action method attributes not shown in the signature. The documentation reflects only the explicitly listed members.
