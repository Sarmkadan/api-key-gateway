# IUsageTrackingService

The `IUsageTrackingService` interface defines a contract for recording and querying API usage metrics associated with a specific API key within a defined date range. Implementations typically store usage events and provide aggregated statistics for monitoring, billing, or analytics purposes.

## API

### UsageTrackingService(string apiKeyId, DateTime startDate, DateTime endDate)

**Purpose**  
Initializes a new instance of the service for the supplied API key identifier and date range.

**Parameters**  
- `apiKeyId`: The unique identifier of the API key to track.  
- `startDate`: The inclusive start of the tracking period.  
- `endDate`: The exclusive end of the tracking period.

**Return Value**  
A new `IUsageTrackingService` instance.

**Exceptions**  
- `ArgumentNullException` if `apiKeyId` is `null`.  
- `ArgumentOutOfRangeException` if `startDate` is after `endDate`.

### Task RecordUsageAsync(UsageRecord usageRecord)

**Purpose**  
Asynchronously records a single usage event.

**Parameters**  
- `usageRecord`: The usage details to record.

**Return Value**  
A `Task` that completes when the record has been persisted.

**Exceptions**  
- `ArgumentNullException` if `usageRecord` is `null`.  
- `InvalidOperationException` if the timestamp of `usageRecord` falls outside the service’s start and end dates.  
- `ObjectDisposedException` if the service has been disposed.

### Task<UsageStatistics> GetUsageStatisticsAsync()

**Purpose**  
Asynchronously retrieves aggregated usage statistics for the configured API key and date range.

**Parameters**  
None.

**Return Value**  
A `Task<UsageStatistics>` containing the aggregated metrics.

**Exceptions**  
- `ObjectDisposedException` if the service has been disposed.

### Task<List<UsageRecord>> GetUsageRecordsAsync()

**Purpose**  
Asynchronously retrieves the list of individual usage records for the configured API key and date range.

**Parameters**  
None.

**Return Value**  
A `Task<List<UsageRecord>>` containing all recorded usage events.

**Exceptions**  
- `ObjectDisposedException` if the service has been disposed.

### Task<long> GetTotalBytesUsedAsync()

**Purpose**  
Asynchronously returns the total number of bytes transferred for the configured API key and date range.

**Parameters**  
None.

**Return Value**  
A `Task<long>` representing the cumulative bytes transferred.

**Exceptions**  
- `ObjectDisposedException` if the service has been disposed.

### string ApiKeyId

**Purpose**  
Gets the API key identifier associated with this service instance.

**Return Value**  
The API key string.

**Exceptions**  
None.

### DateTime StartDate

**Purpose**  
Gets the inclusive start date of the tracking period.

**Return Value**  
The start date.

**Exceptions**  
None.

### DateTime EndDate

**Purpose**  
Gets the exclusive end date of the tracking period.

**Return Value**  
The end date.

**Exceptions**  
None.

### int TotalRequests

**Purpose**  
Gets the total number of requests recorded for the API key within the date range.

**Return Value**  
The request count.

**Exceptions**  
None.

### int SuccessfulRequests

**Purpose**  
Gets the number of requests that completed successfully.

**Return Value**  
The successful request count.

**Exceptions**  
None.

### int FailedRequests

**Purpose**  
Gets the number of requests that failed.

**Return Value**  
The failed request count.

**Exceptions**  
None.

### long TotalBytesTransferred

**Purpose**  
Gets the cumulative number of bytes transferred across all requests.

**Return Value**  
The total bytes transferred.

**Exceptions**  
None.

### double AverageResponseTimeMs

**Purpose**  
Gets the average response time of requests in milliseconds.

**Return Value**  
The average response time.

**Exceptions**  
None.

### int UniqueEndpoints

**Purpose**  
Gets the count of distinct API endpoints that have been invoked.

**Return Value**  
The unique endpoint count.

**Exceptions**  
None.

## Usage

```csharp
// Example 1: Creating a service and recording usage
var service = new UsageTrackingService(
    apiKeyId: "abc-123-key",
    startDate: new DateTime(2025, 1, 1),
    endDate: new DateTime(2025, 2, 1));

var record = new UsageRecord
{
    ApiKeyId = "abc-123-key",
    Timestamp = DateTime.UtcNow,
    Endpoint = "/api/resource",
    ResponseTimeMs = 124.5,
    BytesSent = 1024,
    BytesReceived = 2048,
    IsSuccessful = true
};

await service.RecordUsageAsync(record);
```

```csharp
// Example 2: Retrieving statistics and raw records
var stats = await service.GetUsageStatisticsAsync();
Console.WriteLine($"Total requests: {stats.TotalRequests}");
Console.WriteLine($"Average response time: {stats.AverageResponseTimeMs} ms");

var records = await service.GetUsageRecordsAsync();
foreach (var r in records.Take(5))
{
    Console.WriteLine($"{r.Timestamp:O} {r.Endpoint} {r.ResponseTimeMs}ms");
}
```

## Notes

- The constructor validates that `apiKeyId` is not null and that `startDate` precedes `endDate`; invalid arguments result in immediate exceptions.  
- `RecordUsageAsync` rejects records whose timestamp lies outside the service’s date range, ensuring that scoped usage data remains consistent.  
- All query methods (`GetUsageStatisticsAsync`, `GetUsageRecordsAsync`, `GetTotalBytesUsedAsync`) return fresh data reflecting the state at the moment of invocation; concurrent calls may observe interleaved updates if the underlying implementation does not provide internal synchronization.  
- The interface itself does not impose thread‑safety guarantees; callers should consult the specific implementation’s documentation to determine whether external locking is required for concurrent access.  
- After an instance has been disposed (if the implementation implements `IDisposable`), any further member invocation will throw `ObjectDisposedException`.  
- The `UsageStatistics` type returned by `GetUsageStatisticsAsync` aggregates the same values exposed through the individual properties (`TotalRequests`, `SuccessfulRequests`, etc.) and may include additional fields such as `FailedRequests` or `UniqueEndpoints` depending on the implementation.  
- Property getters are lightweight and do not modify state; they simply return the current cached or computed values.  
- Implementations may choose to persist data asynchronously; therefore, there may be a delay between a successful call to `RecordUsageAsync` and the visibility of that record in subsequent query calls.
