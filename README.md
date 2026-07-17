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

## AnalyticsControllerExtensions

The `AnalyticsControllerExtensions` class provides extension methods for `AnalyticsController` that enable advanced analytics operations by composing existing controller actions. These extensions allow you to compare time periods, filter endpoint statistics by error rates, analyze hourly trends with error filtering, and aggregate daily data into weekly buckets for easier trend analysis.

### Public Members

- `ComparePeriodsAsync(this AnalyticsController controller, string keyId, string period = "30d", string comparisonPeriod = null)` - Returns a comparison of current vs previous period metrics for an API key
- `GetEndpointsByErrorRateAsync(this AnalyticsController controller, string keyId, double minErrorRate = 0, int limit = 20, DateTime? from = null, DateTime? to = null)` - Returns endpoint statistics with error rate filtering
- `GetHourlyTrendWithErrorFilterAsync(this AnalyticsController controller, string keyId, double minErrorRate = 0, DateTime? from = null, DateTime? to = null)` - Returns hourly trend data with error rate threshold filtering
- `GetWeeklyTrendAsync(this AnalyticsController controller, string keyId, int weeks = 4, DateTime? from = null, DateTime? to = null)` - Returns daily trend data aggregated by week

### Data Transfer Objects

The extension methods return the following DTO types:

```csharp
public sealed class PeriodComparison
{
  public required PeriodMetrics CurrentPeriod { get; set; }
  public required PeriodMetrics ComparisonPeriod { get; set; }
  public required PeriodChange Change { get; set; }
}

public sealed class PeriodMetrics
{
  public required string Period { get; set; }
  public required DateTime From { get; set; }
  public required DateTime To { get; set; }
  public int TotalRequests { get; set; }
  public int SuccessfulRequests { get; set; }
  public int FailedRequests { get; set; }
  public double SuccessRatePercent { get; set; }
  public double ErrorRatePercent { get; set; }
  public double AverageResponseTimeMs { get; set; }
  public long TotalBytesTransferred { get; set; }
}

public sealed class PeriodChange
{
  public double RequestsChangePercent { get; set; }
  public double SuccessRateChangePercent { get; set; }
  public double ErrorRateChangePercent { get; set; }
  public double AvgResponseTimeChangePercent { get; set; }
  public double BytesTransferredChangePercent { get; set; }
}

public sealed class WeeklyBucket
{
  public int WeekNumber { get; set; }
  public int Year { get; set; }
  public DateTime StartDate { get; set; }
  public DateTime EndDate { get; set; }
  public int RequestCount { get; set; }
  public int ErrorCount { get; set; }
  public double AverageResponseTimeMs { get; set; }
  public long TotalBytes { get; set; }
}
```

### Example Usage

```csharp
using ApiKeyGateway.Controllers;
using Microsoft.AspNetCore.Mvc;

// Assuming 'analyticsController' is an instance of AnalyticsController injected in your controller
var controller = analyticsController;

// 1. Compare current 30-day period with previous 30-day period
ActionResult<PeriodComparison> periodComparison = await controller.ComparePeriodsAsync(
    keyId: "api-key-12345",
    period: "30d",
    comparisonPeriod: "30d"
);

if (periodComparison.Result is OkObjectResult okResult)
{
    PeriodComparison comparison = (PeriodComparison)okResult.Value!;
    Console.WriteLine($"Current period: {comparison.CurrentPeriod.Period} ({comparison.CurrentPeriod.From:yyyy-MM-dd} to {comparison.CurrentPeriod.To:yyyy-MM-dd})");
    Console.WriteLine($"Comparison period: {comparison.ComparisonPeriod.Period} ({comparison.ComparisonPeriod.From:yyyy-MM-dd} to {comparison.ComparisonPeriod.To:yyyy-MM-dd})");
    Console.WriteLine($"Request change: {comparison.Change.RequestsChangePercent}%");
    Console.WriteLine($"Success rate change: {comparison.Change.SuccessRateChangePercent}%");
}

// 2. Get endpoints with error rate of at least 5%
ActionResult<IReadOnlyList<EndpointStat>> endpointsResult = await controller.GetEndpointsByErrorRateAsync(
    keyId: "api-key-12345",
    minErrorRate: 5.0,
    limit: 10
);

if (endpointsResult.Result is OkObjectResult endpointsOkResult)
{
    IReadOnlyList<EndpointStat> endpoints = endpointsOkResult.Value!;
    foreach (var endpoint in endpoints)
    {
        Console.WriteLine($"Endpoint: {endpoint.Path}, Requests: {endpoint.RequestCount}, Errors: {endpoint.ErrorCount}, Error Rate: {endpoint.ErrorRatePercent}%");
    }
}

// 3. Get hourly trend data with error rate filtering (minimum 2% error rate)
ActionResult<IReadOnlyList<HourlyBucket>> hourlyResult = await controller.GetHourlyTrendWithErrorFilterAsync(
    keyId: "api-key-12345",
    minErrorRate: 2.0,
    from: DateTime.UtcNow.AddDays(-7),
    to: DateTime.UtcNow
);

if (hourlyResult.Result is OkObjectResult hourlyOkResult)
{
    IReadOnlyList<HourlyBucket> hourlyBuckets = hourlyOkResult.Value!;
    foreach (var bucket in hourlyBuckets)
    {
        Console.WriteLine($"Hour {bucket.Hour}: {bucket.RequestCount} requests, {bucket.ErrorCount} errors ({bucket.ErrorRatePercent}% error rate)");
    }
}

// 4. Get weekly aggregated trend data for the last 8 weeks
ActionResult<IReadOnlyList<WeeklyBucket>> weeklyResult = await controller.GetWeeklyTrendAsync(
    keyId: "api-key-12345",
    weeks: 8
);

if (weeklyResult.Result is OkObjectResult weeklyOkResult)
{
    IReadOnlyList<WeeklyBucket> weeklyBuckets = weeklyOkResult.Value!;
    foreach (var week in weeklyBuckets)
    {
        Console.WriteLine($"Week {week.Year}-W{week.WeekNumber}: {week.RequestCount} requests, {week.ErrorCount} errors");
    }
}
```

## RequestValidatorValidation

The `RequestValidatorValidation` class provides instance-based validation helpers that wrap the static methods of `RequestValidator`. It offers a convenient way to validate common data types like email addresses, URLs, IP addresses, string lengths, numeric ranges, and GUIDs with both validation-returning and exception-throwing methods.

### Public Members

- `ValidateEmail(string email)` - Validates an email address and returns a list of validation problems
- `ValidateUrl(string url, bool requireHttps)` - Validates a URL and returns a list of validation problems
- `ValidateIpAddress(string ip)` - Validates an IP address and returns a list of validation problems
- `ValidateLength(string value, int minLength, int maxLength, string fieldName)` - Validates string length and returns a list of validation problems
- `ValidateRange(int value, int minimum, int maximum, string fieldName)` - Validates a numeric range and returns a list of validation problems
- `ValidateGuid(Guid value, string fieldName)` - Validates a GUID and returns a list of validation problems
- `IsValidEmail(string email)` - Checks if an email address is valid
- `IsValidUrl(string url, bool requireHttps)` - Checks if a URL is valid
- `IsValidIpAddress(string ip)` - Checks if an IP address is valid
- `IsValidLength(string value, int minLength, int maxLength, string fieldName)` - Checks if a string length is valid
- `IsValidRange(int value, int minimum, int maximum, string fieldName)` - Checks if a number is within range
- `IsValidGuid(Guid value, string fieldName)` - Checks if a GUID is valid
- `EnsureValidEmail(string email)` - Ensures an email address is valid, throwing if not
- `EnsureValidUrl(string url, bool requireHttps)` - Ensures a URL is valid, throwing if not
- `EnsureValidIpAddress(string ip)` - Ensures an IP address is valid, throwing if not
- `EnsureValidLength(string value, int minLength, int maxLength, string fieldName)` - Ensures a string length is valid, throwing if not
- `EnsureValidRange(int value, int minimum, int maximum, string fieldName)` - Ensures a number is within range, throwing if not
- `EnsureValidGuid(Guid value, string fieldName)` - Ensures a GUID is valid, throwing if not

### Example Usage

```csharp
using ApiKeyGateway.Validation;

// Validate an email address
IReadOnlyList<string> emailErrors = RequestValidatorValidation.ValidateEmail("test@example.com");
if (RequestValidatorValidation.IsValidEmail("test@example.com"))
{
  Console.WriteLine("Email is valid.");
}
else
{
  foreach (string error in emailErrors)
  {
    Console.WriteLine($"Email validation error: {error}");
  }
}

// Validate a URL with HTTPS requirement
IReadOnlyList<string> urlErrors = RequestValidatorValidation.ValidateUrl("https://example.com/api", requireHttps: true);
if (RequestValidatorValidation.IsValidUrl("https://example.com/api", requireHttps: true))
{
  Console.WriteLine("URL is valid.");
}

// Validate an IP address
IReadOnlyList<string> ipErrors = RequestValidatorValidation.ValidateIpAddress("192.168.1.1");
if (RequestValidatorValidation.IsValidIpAddress("192.168.1.1"))
{
  Console.WriteLine("IP address is valid.");
}

// Validate string length
IReadOnlyList<string> lengthErrors = RequestValidatorValidation.ValidateLength("password123", minLength: 8, maxLength: 64, fieldName: "Password");
if (RequestValidatorValidation.IsValidLength("password123", minLength: 8, maxLength: 64))
{
  Console.WriteLine("Password length is valid.");
}

// Validate a numeric range
IReadOnlyList<string> rangeErrors = RequestValidatorValidation.ValidateRange(42, minimum: 0, maximum: 100, fieldName: "Age");
if (RequestValidatorValidation.IsValidRange(42, minimum: 0, maximum: 100))
{
  Console.WriteLine("Age is within valid range.");
}

// Validate a GUID
Guid id = Guid.NewGuid();
IReadOnlyList<string> guidErrors = RequestValidatorValidation.ValidateGuid(id, fieldName: "User ID");
if (RequestValidatorValidation.IsValidGuid(id))
{
  Console.WriteLine("GUID is valid.");
}

// Alternative: Use EnsureValid methods to throw exceptions on failure
try
{
  RequestValidatorValidation.EnsureValidEmail("invalid-email");
  Console.WriteLine("Email validation passed.");
}
catch (ArgumentException ex)
{
  Console.WriteLine($"Validation failed: {ex.Message}");
}

// Validate a URL with HTTPS requirement using EnsureValid
try
{
  RequestValidatorValidation.EnsureValidUrl("http://insecure.com", requireHttps: true);
  Console.WriteLine("URL validation passed.");
}
catch (ArgumentException ex)
{
  Console.WriteLine($"URL validation failed: {ex.Message}");
}
```

## ApiKeysControllerValidation

The `ApiKeysControllerValidation` class provides validation extension methods for API key models used in the `ApiKeysController`. These methods help ensure data integrity by validating required fields, date ranges, and business rules for API key entities including `ApiKey`, `CreateKeyRequest`, `CreateKeyResponse`, `GetKeyResponse`, and `RotateKeyResponse`.

### Public Members

- `Validate(this ApiKey value)` - Validates an API key and returns a list of human-readable problems
- `IsValid(this ApiKey value)` - Checks if an API key is valid
- `EnsureValid(this ApiKey value)` - Ensures an API key is valid, throwing an `ArgumentException` if it's not
- `Validate(this CreateKeyRequest value)` - Validates a create key request and returns a list of human-readable problems
- `IsValid(this CreateKeyRequest value)` - Checks if a create key request is valid
- `EnsureValid(this CreateKeyRequest value)` - Ensures a create key request is valid, throwing an `ArgumentException` if it's not
- `Validate(this CreateKeyResponse value)` - Validates a create key response and returns a list of human-readable problems
- `IsValid(this CreateKeyResponse value)` - Checks if a create key response is valid
- `EnsureValid(this CreateKeyResponse value)` - Ensures a create key response is valid, throwing an `ArgumentException` if it's not
- `Validate(this GetKeyResponse value)` - Validates a get key response and returns a list of human-readable problems
- `IsValid(this GetKeyResponse value)` - Checks if a get key response is valid
- `EnsureValid(this GetKeyResponse value)` - Ensures a get key response is valid, throwing an `ArgumentException` if it's not
- `Validate(this RotateKeyResponse value)` - Validates a rotate key response and returns a list of human-readable problems
- `IsValid(this RotateKeyResponse value)` - Checks if a rotate key response is valid
- `EnsureValid(this RotateKeyResponse value)` - Ensures a rotate key response is valid, throwing an `ArgumentException` if it's not

### Example Usage

```csharp
using ApiKeyGateway.Controllers;
using ApiKeyGateway.Domain.Models;

// Create a sample API key
var apiKey = new ApiKey
{
    Id = "key-12345",
    ConsumerId = "consumer-67890",
    Name = "Production API Key",
    KeyHash = "hashed-key-value",
    Prefix = "prod_",
    CreatedAt = DateTime.UtcNow,
    ExpiresAt = DateTime.UtcNow.AddYears(1),
    RequestCount = 0,
    BytesTransferred = 0,
    Status = ApiKeyStatus.Active
};

// Validate the API key
IReadOnlyList<string> validationErrors = apiKey.Validate();

if (apiKey.IsValid())
{
    Console.WriteLine("API key is valid and ready for use.");
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
    apiKey.EnsureValid();
    Console.WriteLine("API key passed validation successfully.");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Validate a create key request
var createRequest = new CreateKeyRequest
{
    ConsumerId = "consumer-67890",
    Name = "New API Key",
    ExpirationDays = 365
};

IReadOnlyList<string> requestErrors = createRequest.Validate();
if (createRequest.IsValid())
{
    Console.WriteLine("Create key request is valid.");
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

## ApiKeyValidatorValidation

The `ApiKeyValidatorValidation` class provides validation helpers for API key validator parameters. It offers comprehensive validation for inputs to the `ApiKeyValidator` static methods, ensuring API key formats, names, and quota limits meet business requirements before key generation or validation.

### Public Members

- `ValidateKeyFormat(string key)` - Validates an API key string format and returns a list of human-readable problems
- `ValidateKeyName(string name)` - Validates an API key name/description and returns a list of human-readable problems
- `ValidateQuotaLimit(int limit)` - Validates a quota limit value and returns a list of human-readable problems
- `IsValidKeyFormat(string key)` - Checks if an API key string format is valid
- `IsValidKeyName(string name)` - Checks if an API key name is valid
- `IsValidQuotaLimit(int limit)` - Checks if a quota limit is valid
- `EnsureValidKeyFormat(string key)` - Ensures an API key string format is valid, throwing if not
- `EnsureValidKeyName(string name)` - Ensures an API key name is valid, throwing if not
- `EnsureValidQuotaLimit(int limit)` - Ensures a quota limit is valid, throwing if not

### Example Usage

```csharp
using ApiKeyGateway.Validation;

// Validate an API key format
string apiKey = "prod_AbCdEfGh1234567890!@#$%^&*()";
IReadOnlyList<string> keyFormatErrors = ApiKeyValidatorValidation.ValidateKeyFormat(apiKey);

if (ApiKeyValidatorValidation.IsValidKeyFormat(apiKey))
{
Console.WriteLine("API key format is valid.");
}
else
{
foreach (string error in keyFormatErrors)
{
Console.WriteLine($"Key format validation error: {error}");
}
}

// Validate an API key name
string keyName = "Production API Key - Service A";
IReadOnlyList<string> nameErrors = ApiKeyValidatorValidation.ValidateKeyName(keyName);

if (ApiKeyValidatorValidation.IsValidKeyName(keyName))
{
Console.WriteLine("API key name is valid.");
}
else
{
foreach (string error in nameErrors)
{
Console.WriteLine($"Name validation error: {error}");
}
}

// Validate a quota limit
int quotaLimit = 10000;
IReadOnlyList<string> quotaErrors = ApiKeyValidatorValidation.ValidateQuotaLimit(quotaLimit);

if (ApiKeyValidatorValidation.IsValidQuotaLimit(quotaLimit))
{
Console.WriteLine("Quota limit is valid.");
}
else
{
foreach (string error in quotaErrors)
{
Console.WriteLine($"Quota validation error: {error}");
}
}

// Alternative: Use EnsureValid methods to throw exceptions on failure
try
{
ApiKeyValidatorValidation.EnsureValidKeyFormat("invalid-key");
Console.WriteLine("Key format validation passed.");
}
catch (ArgumentException ex)
{
Console.WriteLine($"Key format validation failed: {ex.Message}");
}

try
{
ApiKeyValidatorValidation.EnsureValidKeyName("AB");
Console.WriteLine("Key name validation passed.");
}
catch (ArgumentException ex)
{
Console.WriteLine($"Key name validation failed: {ex.Message}");
}

try
{
ApiKeyValidatorValidation.EnsureValidQuotaLimit(-1);
Console.WriteLine("Quota limit validation passed.");
}
catch (ArgumentException ex)
{
Console.WriteLine($"Quota limit validation failed: {ex.Message}");
}
```