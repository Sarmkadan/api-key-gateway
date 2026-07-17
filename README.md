## RateLimitCalculationHelperJsonExtensions

The `RateLimitCalculationHelperJsonExtensions` class provides JSON serialization extensions for serializing and deserializing metadata about the `RateLimitCalculationHelper` static class. Since `RateLimitCalculationHelper` is a static class with no state, this class serializes metadata containing the type name and public method names for documentation and reflection purposes.

### Example Usage

```csharp
using ApiKeyGateway.Utilities;

// Serialize RateLimitCalculationHelper metadata to a JSON string
string json = RateLimitCalculationHelperJsonExtensions.ToJson(indented: true);

// Deserialize the JSON string back to a metadata object
RateLimitCalculationHelperJsonExtensions.RateLimitCalculationHelperMetadata? metadata =
  RateLimitCalculationHelperJsonExtensions.FromJson(json);
if (metadata != null)
{
  // Access the type name and method names
  string? typeName = metadata.TypeName;
  IReadOnlyList<string>? methods = metadata.Methods;
}
```
// Attempt to deserialize using TryFromJson method
bool success = RateLimitCalculationHelperJsonExtensions.TryFromJson(json, out var parsedMetadata);
if (success && parsedMetadata != null)
{
  // Use parsed metadata
  string? typeName = parsedMetadata.TypeName;
  IReadOnlyList<string>? methods = parsedMetadata.Methods;
}
```

## UsageRecordValidation

The `UsageRecordValidation` class provides validation extension methods for usage record DTOs, allowing you to validate usage records and usage statistics before processing or storing them. These methods help ensure data integrity by checking required fields and enforcing business rules.

### Public Members

- `Validate(this UsageRecordResponse value)` - Validates a usage record and returns a list of human-readable problems
- `IsValid(this UsageRecordResponse value)` - Checks if a usage record is valid
- `EnsureValid(this UsageRecordResponse value)` - Ensures a usage record is valid, throwing an `ArgumentException` if it's not

### Example Usage

```csharp
using ApiKeyGateway.Controllers;
using ApiKeyGateway.Models;

// Create a sample usage record
var usageRecord = new UsageRecordResponse
{
    Id = "rec-12345",
    RecordedAt = DateTime.UtcNow,
    Endpoint = "/api/users",
    Method = "GET",
    StatusCode = 200,
    RequestBytes = 1024,
    ResponseBytes = 2048,
    ResponseTimeMs = 42,
    SourceIp = "192.168.1.1"
};

// Validate the usage record
IReadOnlyList<string> validationErrors = usageRecord.Validate();

if (usageRecord.IsValid())
{
    // Process the valid usage record
    Console.WriteLine("Usage record is valid and ready for processing.");
}
else
{
    // Handle validation errors
    foreach (string error in validationErrors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}

// Alternative: Use EnsureValid to throw an exception if invalid
try
{
    usageRecord.EnsureValid();
    Console.WriteLine("Usage record passed validation successfully.");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

## AdminControllerExtensions

The `AdminControllerExtensions` class provides a set of extension methods for the `AdminController`, enabling administrative operations such as fetching detailed statistics, exporting data, performing diagnostic checks, and managing API keys. These extensions facilitate maintenance tasks and help monitor system performance by exposing enriched data and operational utilities.

### Public Members

- `GetDetailedStats(this AdminController controller)` - Gets detailed system statistics including endpoint breakdown and error rates
- `ExportApiKeysAsync(this AdminController controller, string format = "csv")` - Exports API keys in the specified format (csv, json, xml)
- `ExportAuditLogsAsync(this AdminController controller, string format = "csv", DateTime? since = null)` - Exports audit logs in the specified format
- `RunComprehensiveDiagnosticsAsync(this AdminController controller)` - Runs comprehensive system diagnostics and returns a health check report
- `ResetRateLimitsForKeyAsync(this AdminController controller, string apiKeyId)` - Resets rate limits for a specific API key

### Data Transfer Objects

The extension methods return data transfer objects with the following structure:

```csharp
public string Timestamp { get; init; }
public StatsSummary Total { get; init; }
public IReadOnlyDictionary<string, long>? EndpointBreakdown { get; init; }
public IReadOnlyDictionary<int, long>? ErrorBreakdown { get; init; }
public PerformanceMetrics Performance { get; init; }
public TimeSpan? Uptime { get; init; }

public class StatsSummary
{
    public long? Requests { get; init; }
    public int? ActiveKeys { get; init; }
    public double? Errors { get; init; }
    public int? RateLimits { get; init; }
}

public class PerformanceMetrics
{
    public double? AverageLatencyMs { get; init; }
    public double? P95LatencyMs { get; init; }
    public double? ErrorRate { get; init; }
    public double RequestsPerSecond { get; init; }
}
```

### Example Usage

```csharp
using ApiKeyGateway.Controllers;
using Microsoft.AspNetCore.Mvc;

// Assuming 'adminController' is an instance of AdminController injected in your controller
var controller = adminController;

// 1. Get detailed system statistics
IActionResult statsResult = controller.GetDetailedStats();

// 2. Export API keys in JSON format
await controller.ExportApiKeysAsync("json");

// 3. Export audit logs since a specific date
await controller.ExportAuditLogsAsync("csv", DateTime.UtcNow.AddDays(-7));

// 4. Run comprehensive system diagnostics
await controller.RunComprehensiveDiagnosticsAsync();

// 5. Reset rate limits for a specific API key
await controller.ResetRateLimitsForKeyAsync("api-key-12345");
```

## StatsControllerExtensions

The `StatsControllerExtensions` class provides strongly-typed extension methods for the `StatsController`, enabling easy access to usage statistics, rate-limit status, and endpoint-specific metrics. These extensions deserialize the controller's anonymous object responses into concrete DTOs, eliminating the need for reflection or dynamic typing when working with statistics data.

### Public Members

- `GetUsageStatisticsDto(this StatsController controller, string period = "day")` - Retrieves usage statistics for a specified period (hour/day/month) as a strongly-typed `UsageStatsDto`
- `GetRateLimitStatusDto(this StatsController controller)` - Retrieves the current rate-limit status for the authenticated API key as a `RateLimitStatusDto`
- `GetEndpointStatisticsList(this StatsController controller)` - Retrieves endpoint-specific statistics as a read-only list of `EndpointStatDto` objects

### Data Transfer Objects

The extension methods return the following DTO types:

```csharp
public sealed record UsageStatsDto(
    string Period,
    int Requests,
    int Errors,
    double TotalDataTransferred,
    double AverageResponseTime);

public sealed record RateLimitDto(
    int Limit,
    int Current,
    int Remaining,
    string ResetIn);

public sealed record RateLimitStatusDto(
    string ApiKeyId,
    RateLimitDto Hourly,
    RateLimitDto Daily,
    RateLimitDto Monthly,
    string Status);

public sealed record EndpointStatDto(
    string Path,
    int Requests,
    int AvgResponseTime,
    int ErrorCount);
```

### Example Usage

```csharp
using ApiKeyGateway.Controllers;
using Microsoft.AspNetCore.Mvc;

// Assuming 'statsController' is an instance of StatsController injected in your controller
var controller = statsController;

// 1. Get usage statistics for the current day
UsageStatsDto usageStats = controller.GetUsageStatisticsDto("day");
Console.WriteLine($"Period: {usageStats.Period}, Requests: {usageStats.Requests}, Errors: {usageStats.Errors}");

// 2. Get usage statistics for the current hour
UsageStatsDto hourlyStats = controller.GetUsageStatisticsDto("hour");
Console.WriteLine($"Hourly requests: {hourlyStats.Requests}, Data transferred: {usageStats.TotalDataTransferred} bytes");

// 3. Get rate limit status for the authenticated API key
RateLimitStatusDto rateLimitStatus = controller.GetRateLimitStatusDto();
Console.WriteLine($"API Key: {rateLimitStatus.ApiKeyId}, Status: {rateLimitStatus.Status}");
Console.WriteLine($"Hourly limit: {rateLimitStatus.Hourly.Limit} requests, {rateLimitStatus.Hourly.Remaining} remaining");

// 4. Get endpoint-specific statistics
IReadOnlyList<EndpointStatDto> endpointStats = controller.GetEndpointStatisticsList();
foreach (var endpointStat in endpointStats)
{
    Console.WriteLine($"Endpoint: {endpointStat.Path}, Requests: {endpointStat.Requests}, Errors: {endpointStat.ErrorCount}");
}
```

## AnalyticsControllerJsonExtensions

The `AnalyticsControllerJsonExtensions` class provides System.Text.Json serialization extensions for analytics response types returned by `AnalyticsController` actions. It enables serialization and deserialization of analytics summary data, endpoint statistics, hourly buckets, and daily buckets with support for both compact and indented JSON formatting.

### Public Members

- `ToJson(this AnalyticsSummary value, bool indented = false)` - Serializes an analytics summary to a JSON string
- `ToJson(this List<EndpointStat> value, bool indented = false)` - Serializes a list of endpoint statistics to a JSON string
- `ToJson(this List<HourlyBucket> value, bool indented = false)` - Serializes a list of hourly buckets to a JSON string
- `ToJson(this List<DailyBucket> value, bool indented = false)` - Serializes a list of daily buckets to a JSON string
- `FromJson(string json)` - Deserializes a JSON string to an analytics summary
- `FromJsonToEndpointStats(string json)` - Deserializes a JSON string to a list of endpoint statistics
- `FromJsonToHourlyBuckets(string json)` - Deserializes a JSON string to a list of hourly buckets
- `FromJsonToDailyBuckets(string json)` - Deserializes a JSON string to a list of daily buckets
- `TryFromJson(string json, out AnalyticsSummary? value)` - Attempts to deserialize a JSON string to an analytics summary
- `TryFromJson(string json, out List<EndpointStat>? value)` - Attempts to deserialize a JSON string to a list of endpoint statistics
- `TryFromJson(string json, out List<HourlyBucket>? value)` - Attempts to deserialize a JSON string to a list of hourly buckets
- `TryFromJson(string json, out List<DailyBucket>? value)` - Attempts to deserialize a JSON string to a list of daily buckets

### Example Usage

```csharp
using ApiKeyGateway.Controllers;
using ApiKeyGateway.Services;

// Create sample analytics data
var analyticsSummary = new AnalyticsSummary
{
    TotalRequests = 1000,
    TotalErrors = 15,
    AverageResponseTimeMs = 42.5,
    TopEndpoints = new List<EndpointStat>
    {
        new EndpointStat { Path = "/api/users", Requests = 500, Errors = 2 },
        new EndpointStat { Path = "/api/products", Requests = 300, Errors = 5 }
    },
    HourlyStats = new List<HourlyBucket>
    {
        new HourlyBucket { Hour = 10, Requests = 150, Errors = 1 },
        new HourlyBucket { Hour = 11, Requests = 200, Errors = 2 }
    },
    DailyStats = new List<DailyBucket>
    {
        new DailyBucket { Date = DateTime.Today, Requests = 800, Errors = 10 },
        new DailyBucket { Date = DateTime.Today.AddDays(-1), Requests = 200, Errors = 5 }
    }
};

// Serialize analytics summary to JSON string
string jsonSummary = analyticsSummary.ToJson(indented: true);

// Serialize endpoint statistics to JSON string
string jsonEndpoints = analyticsSummary.TopEndpoints.ToJson();

// Serialize hourly buckets to JSON string
string jsonHourly = analyticsSummary.HourlyStats.ToJson();

// Serialize daily buckets to JSON string
string jsonDaily = analyticsSummary.DailyStats.ToJson();

// Deserialize analytics summary from JSON
AnalyticsSummary? deserializedSummary = AnalyticsControllerJsonExtensions.FromJson(jsonSummary);

// Deserialize endpoint statistics from JSON
List<EndpointStat>? deserializedEndpoints = AnalyticsControllerJsonExtensions.FromJsonToEndpointStats(jsonEndpoints);

// Deserialize hourly buckets from JSON
List<HourlyBucket>? deserializedHourly = AnalyticsControllerJsonExtensions.FromJsonToHourlyBuckets(jsonHourly);

// Deserialize daily buckets from JSON
List<DailyBucket>? deserializedDaily = AnalyticsControllerJsonExtensions.FromJsonToDailyBuckets(jsonDaily);

// Try to deserialize with error handling
if (AnalyticsControllerJsonExtensions.TryFromJson(jsonSummary, out var parsedSummary))
{
    Console.WriteLine("Successfully deserialized analytics summary");
}
else
{
    Console.WriteLine("Failed to deserialize analytics summary");
}
```