# AnalyticsController
The `AnalyticsController` class is designed to provide analytical data and insights for the API key gateway. It offers various methods to retrieve summary information, top endpoints, and trend data, enabling developers to monitor and optimize the performance of their API keys.

## API
The `AnalyticsController` class exposes the following public members:
* `public AnalyticsController`: The constructor for the `AnalyticsController` class.
* `public async Task<ActionResult<AnalyticsSummary>> GetSummary`: Retrieves a summary of analytical data. This method returns an `ActionResult` containing an `AnalyticsSummary` object, which provides an overview of the current state of the API key gateway. It does not take any parameters and throws exceptions if there are issues with data retrieval or processing.
* `public async Task<ActionResult<List<EndpointStat>>> GetTopEndpoints`: Returns a list of the top endpoints based on their usage. The method returns an `ActionResult` containing a list of `EndpointStat` objects, which represent the top endpoints. It does not take any parameters and throws exceptions if there are issues with data retrieval or processing.
* `public async Task<ActionResult<List<HourlyBucket>>> GetHourlyTrend`: Retrieves the hourly trend data for the API key gateway. This method returns an `ActionResult` containing a list of `HourlyBucket` objects, which represent the hourly trend data. It does not take any parameters and throws exceptions if there are issues with data retrieval or processing.
* `public async Task<ActionResult<List<DailyBucket>>> GetDailyTrend`: Returns the daily trend data for the API key gateway. The method returns an `ActionResult` containing a list of `DailyBucket` objects, which represent the daily trend data. It does not take any parameters and throws exceptions if there are issues with data retrieval or processing.

## Usage
Here are two examples of using the `AnalyticsController` class:
```csharp
// Example 1: Retrieving summary data
var analyticsController = new AnalyticsController();
var summary = await analyticsController.GetSummary();
if (summary.Value != null)
{
    Console.WriteLine($"Total requests: {summary.Value.TotalRequests}");
    Console.WriteLine($"Average response time: {summary.Value.AverageResponseTime}");
}

// Example 2: Retrieving top endpoints
var analyticsController = new AnalyticsController();
var topEndpoints = await analyticsController.GetTopEndpoints();
if (topEndpoints.Value != null)
{
    foreach (var endpoint in topEndpoints.Value)
    {
        Console.WriteLine($"Endpoint: {endpoint.Endpoint}, Requests: {endpoint.Requests}");
    }
}
```

## Notes
When using the `AnalyticsController` class, consider the following edge cases and thread-safety remarks:
* The `GetSummary`, `GetTopEndpoints`, `GetHourlyTrend`, and `GetDailyTrend` methods are asynchronous and may throw exceptions if there are issues with data retrieval or processing.
* The `AnalyticsController` class is designed to be thread-safe, but it is still important to ensure that the class is properly synchronized when accessing shared resources.
* The `ActionResult` return type of the methods allows for flexible error handling and response formatting.
* The `AnalyticsSummary`, `EndpointStat`, `HourlyBucket`, and `DailyBucket` classes are designed to provide a structured representation of the analytical data, making it easier to work with the data in the application.
