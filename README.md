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