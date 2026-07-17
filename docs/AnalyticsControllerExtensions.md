# AnalyticsControllerExtensions

The `AnalyticsControllerExtensions` class provides a set of extension methods for ASP.NET Core controllers that handle analytics endpoints, as well as a collection of properties that represent the result of a period-over-period comparison. The static methods enable asynchronous retrieval of aggregated metrics, error‑rate‑sorted endpoints, hourly trends with error filtering, and weekly trends. The instance properties expose the detailed breakdown of a comparison between two time periods, including request counts, success/error rates, response times, and the relative change between the periods.

## API

### Static Methods

#### `ComparePeriodsAsync`
```csharp
public static async Task<ActionResult<PeriodComparison>> ComparePeriodsAsync(
    this ControllerBase controller,
    PeriodMetrics currentPeriod,
    PeriodMetrics comparisonPeriod)
```
Compares two time periods and returns a `PeriodComparison` object containing the aggregated metrics and the computed change.

- **Parameters**  
  - `controller`: The controller instance on which the extension method is invoked.  
  - `currentPeriod`: The primary period to analyze.  
  - `comparisonPeriod`: The baseline period to compare against.
- **Returns**  
  An `ActionResult<PeriodComparison>` that, on success, contains a `PeriodComparison` with the comparison data.
- **Throws**  
  - `ArgumentNullException` if `currentPeriod` or `comparisonPeriod` is `null`.  
  - `InvalidOperationException` if the underlying data source is unavailable or the periods are invalid.

#### `GetEndpointsByErrorRateAsync`
```csharp
public static async Task<ActionResult<IReadOnlyList<EndpointStat>>> GetEndpointsByErrorRateAsync(
    this ControllerBase controller,
    DateTime from,
    DateTime to,
    int? top = null)
```
Retrieves a list of endpoints sorted by error rate (descending) within the specified time range.

- **Parameters**  
  - `controller`: The controller instance.  
  - `from`: Start of the time window.  
  - `to`: End of the time window.  
  - `top`: Optional limit on the number of endpoints returned.
- **Returns**  
  An `ActionResult` containing a read‑only list of `EndpointStat` objects, each representing an endpoint’s aggregated metrics.
- **Throws**  
  - `ArgumentOutOfRangeException` if `from` is later than `to`.  
  - `InvalidOperationException` if the data source fails to respond.

#### `GetHourlyTrendWithErrorFilterAsync`
```csharp
public static async Task<ActionResult<IReadOnlyList<HourlyBucket>>> GetHourlyTrendWithErrorFilterAsync(
    this ControllerBase controller,
    DateTime from,
    DateTime to,
    int? minErrorCount = null)
```
Returns hourly buckets of request data, optionally filtered to include only hours where the error count meets or exceeds a threshold.

- **Parameters**  
  - `controller`: The controller instance.  
  - `from`: Start of the time window.  
  - `to`: End of the time window.  
  - `minErrorCount`: Minimum number of errors required for an hour to be included. If `null`, all hours are returned.
- **Returns**  
  An `ActionResult` containing a read‑only list of `HourlyBucket` objects.
- **Throws**  
  - `ArgumentOutOfRangeException` if `from` is later than `to`.  
  - `InvalidOperationException` if the data source is unavailable.

#### `GetWeeklyTrendAsync`
```csharp
public static async Task<ActionResult<IReadOnlyList<WeeklyBucket>>> GetWeeklyTrendAsync(
    this ControllerBase controller,
    DateTime from,
    DateTime to)
```
Retrieves weekly buckets of aggregated request metrics over the specified date range.

- **Parameters**  
  - `controller`: The controller instance.  
  - `from`: Start of the time window.  
  - `to`: End of the time window.
- **Returns**  
  An `ActionResult` containing a read‑only list of `WeeklyBucket` objects.
- **Throws**  
  - `ArgumentOutOfRangeException` if `from` is later than `to`.  
  - `InvalidOperationException` if the data source cannot be queried.

### Instance Properties

The following properties are available on an instance of `AnalyticsControllerExtensions` (typically obtained as the result of a comparison operation).

#### `CurrentPeriod`
```csharp
public required PeriodMetrics CurrentPeriod { get; set; }
```
The metrics for the primary (current) period being analyzed.

#### `ComparisonPeriod`
```csharp
public required PeriodMetrics ComparisonPeriod { get; set; }
```
The metrics for the baseline (comparison) period.

#### `Change`
```csharp
public required PeriodChange Change { get; set; }
```
An object describing the absolute and relative differences between the two periods.

#### `Period`
```csharp
public required string Period { get; set; }
```
A human‑readable label for the time period (e.g., "Last 7 days").

#### `From`
```csharp
public required DateTime From { get; set; }
```
The start of the current period.

#### `To`
```csharp
public required DateTime To { get; set; }
```
The end of the current period.

#### `TotalRequests`
```csharp
public int TotalRequests { get; set; }
```
Total number of requests in the current period.

#### `SuccessfulRequests`
```csharp
public int SuccessfulRequests { get; set; }
```
Number of requests that completed successfully (HTTP 2xx).

#### `FailedRequests`
```csharp
public int FailedRequests { get; set; }
```
Number of requests that failed (HTTP 4xx or 5xx).

#### `SuccessRatePercent`
```csharp
public double SuccessRatePercent { get; set; }
```
Percentage of requests that were successful, calculated as `(SuccessfulRequests / TotalRequests) * 100`.

#### `ErrorRatePercent`
```csharp
public double ErrorRatePercent { get; set; }
```
Percentage of requests that resulted in an error, calculated as `(FailedRequests / TotalRequests) * 100`.

#### `AverageResponseTimeMs`
```csharp
public double AverageResponseTimeMs { get; set; }
```
Average response time for all requests in the current period, in milliseconds.

#### `TotalBytesTransferred`
```csharp
public long TotalBytesTransferred { get; set; }
```
Total bytes transferred (both sent and received) during the current period.

#### `RequestsChangePercent`
```csharp
public double RequestsChangePercent { get; set; }
```
Percentage change in total requests between the comparison period and the current period. A positive value indicates an increase.

#### `SuccessRateChangePercent`
```csharp
public double SuccessRateChangePercent { get; set; }
```
Percentage change in success rate between the two periods.

#### `ErrorRateChangePercent`
```csharp
public double ErrorRateChangePercent { get; set; }
```
Percentage change in error rate between the two periods.

## Usage

### Example 1: Comparing Two Periods and Inspecting the Result

```csharp
[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    [HttpGet("compare")]
    public async Task<IActionResult> ComparePeriods(
        [FromQuery] DateTime currentFrom,
        [FromQuery] DateTime currentTo,
        [FromQuery] DateTime comparisonFrom,
        [FromQuery] DateTime comparisonTo)
    {
        var currentPeriod = new PeriodMetrics { From = currentFrom, To = currentTo };
        var comparisonPeriod = new PeriodMetrics { From = comparisonFrom, To = comparisonTo };

        var result = await this.ComparePeriodsAsync(currentPeriod, comparisonPeriod);

        if (result.Result is not OkObjectResult okResult)
            return result.Result;

        var comparison = okResult.Value as AnalyticsControllerExtensions;

        Console.WriteLine($"Total requests: {comparison.TotalRequests}");
        Console.WriteLine($"Error rate change: {comparison.ErrorRateChangePercent}%");
        Console.WriteLine($"Average response time: {comparison.AverageResponseTimeMs} ms");

        return Ok(comparison);
    }
}
```

### Example 2: Retrieving Endpoints by Error Rate and Filtering Hourly Trends

```csharp
[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    [HttpGet("troubleshoot")]
    public async Task<IActionResult> Troubleshoot(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        // Get top 5 endpoints with highest error rate
        var endpointsResult = await this.GetEndpointsByErrorRateAsync(from, to, top: 5);
        if (endpointsResult.Result is not OkObjectResult okEndpoints)
            return endpointsResult.Result;

        var topEndpoints = okEndpoints.Value as IReadOnlyList<EndpointStat>;
        foreach (var ep in topEndpoints)
        {
            Console.WriteLine($"{ep.Endpoint}: {ep.ErrorRate}% errors");
        }

        // Get hourly trend for hours with at least 10 errors
        var hourlyResult = await this.GetHourlyTrendWithErrorFilterAsync(from, to, minErrorCount: 10);
        if (hourlyResult.Result is not OkObjectResult okHourly)
            return hourlyResult.Result;

        var hourlyBuckets = okHourly.Value as IReadOnlyList<HourlyBucket>;
        foreach (var bucket in hourlyBuckets)
        {
            Console.WriteLine($"{bucket.Timestamp}: {bucket.Errors} errors");
        }

        return Ok(new { topEndpoints, hourlyBuckets });
    }
}
```

## Notes

- **Edge Cases**  
  - When `TotalRequests` is zero, `SuccessRatePercent` and `ErrorRatePercent` are defined as `0.0` to avoid division by zero.  
  - The `top` parameter in `GetEndpointsByErrorRateAsync` must be non‑negative; a value of `0` returns an empty list.  
  - If `minErrorCount` is set to a value greater than the maximum error count in any hour, `GetHourlyTrendWithErrorFilterAsync` returns an empty list.  
  - The `From` and `To` properties must represent a valid time range (`From` ≤ `To`); otherwise the static methods throw `ArgumentOutOfRangeException`.

- **Thread Safety**  
  - The static extension methods are thread‑safe as they do not modify shared state.  
  - Instance properties of `AnalyticsControllerExtensions` are not inherently thread‑safe. If an instance is shared across multiple threads, external synchronization (e.g., locking) is required when reading or writing its properties.  
  - The `required` modifier on several properties enforces that they are set during object initialization; after construction, they behave like ordinary mutable properties.

- **Dependencies**  
  - All static methods rely on an underlying data store (e.g., a database or cache). Transient failures may result in `InvalidOperationException`. Callers should implement appropriate retry or fallback logic.
