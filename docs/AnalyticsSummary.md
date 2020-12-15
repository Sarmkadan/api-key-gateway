# AnalyticsSummary

`AnalyticsSummary` is a data transfer object that encapsulates aggregated usage and performance metrics for API key traffic over a specified time window. It serves as the primary output of analytics queries, providing both overall summaries and per-dimension breakdowns (by endpoint, HTTP method, or hourly interval) for request counts, success/error rates, response times, and data transfer volumes.

## API

### Overall Summary Members

- **`public string ApiKeyId`**  
  The unique identifier of the API key for which the analytics were computed. This field is always populated and identifies the subject of the summary.

- **`public DateTime From`**  
  The inclusive start of the time window covered by this summary. All metrics are derived from requests with timestamps greater than or equal to this value.

- **`public DateTime To`**  
  The exclusive end of the time window covered by this summary. All metrics are derived from requests with timestamps strictly less than this value.

- **`public int TotalRequests`**  
  The total number of requests made using the API key within the `From`–`To` window, regardless of outcome.

- **`public int SuccessfulRequests`**  
  The number of requests that completed with a successful HTTP status code (typically 2xx) within the window.

- **`public int FailedRequests`**  
  The number of requests that completed with a failed HTTP status code (typically 4xx or 5xx) within the window.

- **`public double SuccessRatePercent`**  
  The percentage of requests that were successful, calculated as `(SuccessfulRequests / TotalRequests) * 100`. Returns `0` when `TotalRequests` is zero.

- **`public double ErrorRatePercent`**  
  The percentage of requests that failed, calculated as `(FailedRequests / TotalRequests) * 100`. Returns `0` when `TotalRequests` is zero.

- **`public double AverageResponseTimeMs`**  
  The mean response time in milliseconds across all requests within the window. Returns `0` when there are no requests.

- **`public long TotalBytesTransferred`**  
  The cumulative sum of bytes transferred (both request and response bodies) for all requests within the window.

- **`public int UniqueEndpoints`**  
  The count of distinct endpoint paths accessed during the window. Duplicate requests to the same path are counted once.

- **`public int UniqueSourceIps`**  
  The count of distinct client IP addresses from which requests originated during the window.

### Per-Endpoint Breakdown Members

- **`public string Endpoint`**  
  The specific endpoint path (e.g., `/api/v1/users`) for which this breakdown row applies. This field is populated only in per-endpoint summary views.

- **`public string Method`**  
  The HTTP method (e.g., `GET`, `POST`) for which this breakdown row applies. This field is populated only in per-method summary views.

- **`public int RequestCount`**  
  The number of requests for the specific dimension (endpoint, method, or hour) within the window.

- **`public double AverageResponseTimeMs`**  
  The mean response time in milliseconds for requests matching the specific dimension.

- **`public int ErrorCount`**  
  The number of failed requests for the specific dimension.

- **`public double ErrorRatePercent`**  
  The percentage of requests that failed for the specific dimension, calculated as `(ErrorCount / RequestCount) * 100`. Returns `0` when `RequestCount` is zero.

### Per-Hour Breakdown Members

- **`public DateTime Hour`**  
  The start of the hourly bucket (truncated to the hour) for which this breakdown row applies. This field is populated only in hourly time-series summary views.

- **`public int RequestCount`**  
  The number of requests within the specific hourly bucket.

## Usage

### Example 1: Retrieving an Overall Summary

```csharp
IAnalyticsService analyticsService = /* injected */;
string apiKeyId = "key_abc123";
DateTime from = DateTime.UtcNow.AddDays(-7);
DateTime to = DateTime.UtcNow;

AnalyticsSummary overallSummary = await analyticsService.GetOverallSummaryAsync(
    apiKeyId, from, to);

Console.WriteLine($"Total requests: {overallSummary.TotalRequests}");
Console.WriteLine($"Success rate: {overallSummary.SuccessRatePercent:F2}%");
Console.WriteLine($"Avg response time: {overallSummary.AverageResponseTimeMs:F2} ms");
Console.WriteLine($"Unique endpoints: {overallSummary.UniqueEndpoints}");
Console.WriteLine($"Unique source IPs: {overallSummary.UniqueSourceIps}");
```

### Example 2: Retrieving Per-Endpoint Breakdowns

```csharp
IAnalyticsService analyticsService = /* injected */;
string apiKeyId = "key_abc123";
DateTime from = DateTime.UtcNow.AddDays(-1);
DateTime to = DateTime.UtcNow;

IReadOnlyList<AnalyticsSummary> endpointBreakdowns =
    await analyticsService.GetEndpointBreakdownAsync(apiKeyId, from, to);

foreach (AnalyticsSummary row in endpointBreakdowns)
{
    Console.WriteLine(
        $"Endpoint: {row.Endpoint} | " +
        $"Requests: {row.RequestCount} | " +
        $"Errors: {row.ErrorCount} | " +
        $"Error rate: {row.ErrorRatePercent:F2}% | " +
        $"Avg latency: {row.AverageResponseTimeMs:F2} ms");
}
```

## Notes

- **Zero-request windows:** When `TotalRequests` is `0`, rate-based fields (`SuccessRatePercent`, `ErrorRatePercent`, `AverageResponseTimeMs`) return `0` rather than `NaN` or throwing. Callers do not need to guard against division by zero.
- **Dimension-specific fields:** The `Endpoint`, `Method`, and `Hour` fields are mutually exclusive in practice. A summary row represents exactly one dimension type; fields not relevant to that dimension remain at their default values (`null` for strings, `DateTime.MinValue` for `Hour`). Callers should check which field is populated to determine the row type.
- **Immutability:** `AnalyticsSummary` is a plain data object. Once constructed by the analytics service, its values do not change. It is safe to pass between threads without synchronization.
- **Thread safety:** Reading properties from an instance is thread-safe since the object is effectively immutable after creation. No internal state is mutated post-construction.
- **Time zone handling:** The `From`, `To`, and `Hour` fields use UTC. Callers are responsible for converting local time ranges to UTC before querying and converting back for display if needed.
- **Precision:** `AverageResponseTimeMs` and `ErrorRatePercent` are stored as `double`. Accumulated rounding errors over large datasets are possible but negligible for display purposes. For billing or quota calculations, use the raw integer counters (`TotalRequests`, `SuccessfulRequests`, `FailedRequests`, `TotalBytesTransferred`).
