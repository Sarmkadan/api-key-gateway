# UsageController

The `UsageController` serves as the primary interface for retrieving detailed usage metrics and historical records associated with specific API keys within the `api-key-gateway` system. It aggregates request data to provide statistical summaries, including success rates, bandwidth consumption, and latency averages, while also exposing granular logs of individual transactions. This controller is designed to support auditing, billing analysis, and performance monitoring by exposing endpoints that filter data based on configurable time ranges and key identifiers.

## API

The following members are exposed by the `UsageController`. Note that several properties listed in the project context appear to be data model components returned by the controller methods rather than direct properties of the controller instance itself; they are documented below in the context of their likely response structures.

### `UsageController`
The public constructor initializes a new instance of the controller. It typically relies on dependency injection to resolve underlying data repositories or services required to fetch usage data.

### `GetKeyStatistics`
```csharp
public async Task<ActionResult<UsageStatisticsResponse>> GetKeyStatistics
```
Retrieves an aggregated summary of usage statistics for a specific API key over a defined period.
*   **Purpose**: To provide high-level metrics such as total requests, success rates, and average response times.
*   **Parameters**: Implicitly utilizes the `ApiKeyId`, `StartDate`, and `EndDate` context (likely bound from route or query parameters) to filter the dataset.
*   **Return Value**: Returns an `ActionResult` wrapping a `UsageStatisticsResponse` object containing aggregated fields like `TotalRequests`, `SuccessfulRequests`, `FailedRequests`, `SuccessRate`, `TotalBytesTransferred`, `AverageResponseTimeMs`, and `UniqueEndpoints`.
*   **Exceptions**: May throw or return a 404 status if the `ApiKeyId` does not exist, or a 400 status if the `StartDate` is later than the `EndDate`.

### `GetKeyRecords`
```csharp
public async Task<ActionResult<List<UsageRecordResponse>>> GetKeyRecords
```
Fetches a detailed list of individual usage records for a specific API key.
*   **Purpose**: To enable granular auditing of specific requests, including endpoints hit, HTTP methods used, and status codes returned.
*   **Parameters**: Uses the `ApiKeyId` and time range (`StartDate`, `EndDate`) to scope the results.
*   **Return Value**: Returns an `ActionResult` wrapping a `List<UsageRecordResponse>`. Each item in the list contains properties such as `Id`, `RecordedAt`, `Endpoint`, `Method`, `StatusCode`, `RequestBytes`, and potentially response size details.
*   **Exceptions**: May return a 400 status if the requested date range exceeds system retention policies or if pagination parameters (if implied) are invalid.

### `GetConsumerUsage`
```csharp
public async Task<ActionResult<ConsumerUsageResponse>> GetConsumerUsage
```
Retrieves usage data aggregated at the consumer level, potentially spanning multiple API keys associated with a single consumer entity.
*   **Purpose**: To provide a holistic view of a consumer's activity across the gateway.
*   **Parameters**: Likely accepts a consumer identifier alongside the standard `StartDate` and `EndDate`.
*   **Return Value**: Returns an `ActionResult` wrapping a `ConsumerUsageResponse`, which aggregates data similar to `UsageStatisticsResponse` but scoped to the consumer.
*   **Exceptions**: Throws or returns an error if the consumer identity cannot be resolved or if the user lacks permission to view aggregate consumer data.

### Data Model Properties
The following properties represent the schema of the response objects (`UsageStatisticsResponse`, `UsageRecordResponse`) returned by the controller methods, rather than state held directly by the controller instance:

*   **`ApiKeyId`** (`string`): The unique identifier of the API key associated with the usage data.
*   **`StartDate`** (`DateTime`): The beginning of the time window for the reported statistics.
*   **`EndDate`** (`DateTime`): The end of the time window for the reported statistics.
*   **`TotalRequests`** (`int`): The total count of requests processed within the period.
*   **`SuccessfulRequests`** (`int`): The count of requests resulting in a 2xx or 3xx status code.
*   **`FailedRequests`** (`int`): The count of requests resulting in a 4xx or 5xx status code.
*   **`SuccessRate`** (`double`): The calculated percentage of successful requests (0.0 to 100.0).
*   **`TotalBytesTransferred`** (`long`): The cumulative size of request and response payloads in bytes.
*   **`AverageResponseTimeMs`** (`double`): The mean latency for requests in milliseconds.
*   **`UniqueEndpoints`** (`int`): The count of distinct API endpoints accessed.
*   **`Id`** (`string`): The unique identifier for a specific usage record entry.
*   **`RecordedAt`** (`DateTime`): The precise timestamp when the request was processed.
*   **`Endpoint`** (`string`): The relative path of the API endpoint accessed.
*   **`Method`** (`string`): The HTTP method used (e.g., GET, POST, PUT).
*   **`StatusCode`** (`int`): The HTTP response status code returned to the client.
*   **`RequestBytes`** (`long`): The size of the incoming request payload in bytes.

## Usage

### Example 1: Retrieving Aggregated Statistics
This example demonstrates how to call the `GetKeyStatistics` method to retrieve a summary of API key performance for the last 30 days.

```csharp
using Microsoft.AspNetCore.Mvc;
using ApiKeyGateway.Controllers;
using ApiKeyGateway.Models;

public class UsageAnalysisService
{
    private readonly UsageController _usageController;

    public UsageAnalysisService(UsageController usageController)
    {
        _usageController = usageController;
    }

    public async Task AnalyzeKeyPerformance(string apiKeyId)
    {
        // Set context for the controller (simulating model binding)
        _usageController.ApiKeyId = apiKeyId;
        _usageController.StartDate = DateTime.UtcNow.AddDays(-30);
        _usageController.EndDate = DateTime.UtcNow;

        // Invoke the statistics endpoint
        var result = await _usageController.GetKeyStatistics();

        if (result.Result is NotFoundResult)
        {
            Console.WriteLine("API Key not found.");
            return;
        }

        var stats = result.Value;
        Console.WriteLine($"Total Requests: {stats.TotalRequests}");
        Console.WriteLine($"Success Rate: {stats.SuccessRate:F2}%");
        Console.WriteLine($"Avg Response Time: {stats.AverageResponseTimeMs:F2}ms");
    }
}
```

### Example 2: Fetching Detailed Request Logs
This example illustrates retrieving a list of individual records to audit failed requests for a specific endpoint.

```csharp
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ApiKeyGateway.Controllers;
using ApiKeyGateway.Models;

public class AuditLogger
{
    private readonly UsageController _usageController;

    public AuditLogger(UsageController usageController)
    {
        _usageController = usageController;
    }

    public async Task LogFailedRequests(string apiKeyId)
    {
        _usageController.ApiKeyId = apiKeyId;
        _usageController.StartDate = DateTime.UtcNow.Date;
        _usageController.EndDate = DateTime.UtcNow;

        var result = await _usageController.GetKeyRecords();
        
        if (result.Value == null)
        {
            return;
        }

        var failedRecords = result.Value
            .Where(r => r.StatusCode >= 400)
            .OrderByDescending(r => r.RecordedAt);

        foreach (var record in failedRecords)
        {
            Console.WriteLine(
                $"[{record.RecordedAt}] {record.Method} {record.Endpoint} - {record.StatusCode} ({record.RequestBytes} bytes)"
            );
        }
    }
}
```

## Notes

*   **Date Range Validation**: The controller methods assume logical consistency between `StartDate` and `EndDate`. Callers must ensure `StartDate` precedes `EndDate`; otherwise, the underlying query logic may return empty datasets or trigger validation errors resulting in `BadRequest` responses.
*   **Data Volume**: The `GetKeyRecords` method returns a `List<UsageRecordResponse>`. For high-traffic API keys or extensive date ranges, this list may become large, potentially impacting memory usage and serialization times. Implementations should consider pagination mechanisms if the underlying data source supports it, though the current signature suggests a full list return.
*   **Thread Safety**: As with standard ASP.NET Core controllers, `UsageController` instances are typically created per request. However, if the controller holds state via the public properties (`ApiKeyId`, `StartDate`, etc.) before the async operation completes, care must be taken not to reuse the same controller instance across concurrent threads without resetting these properties, as this could lead to race conditions where one request's parameters overwrite another's.
*   **Precision**: The `SuccessRate` and `AverageResponseTimeMs` properties are represented as `double`. Consumers should be aware of floating-point precision limitations when performing further financial or strict SLA calculations based on these values.
*   **Time Zones**: All `DateTime` properties (`StartDate`, `EndDate`, `RecordedAt`) should be treated as UTC unless explicitly documented otherwise by the gateway configuration, to ensure consistent reporting across distributed systems.
