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

## AdminControllerExtensions

The `AdminControllerExtensions` class provides a set of extension methods for the `AdminController`, enabling administrative operations such as fetching detailed statistics, exporting data, and performing diagnostic checks. These extensions facilitate maintenance tasks and help monitor system performance by exposing enriched data and operational utilities.

### Example Usage

```csharp
using ApiKeyGateway.Controllers;
using Microsoft.AspNetCore.Mvc;

// Assuming 'adminController' is an instance of AdminController injected in your controller
var controller = adminController;

// 1. Get detailed system statistics
IActionResult statsResult = AdminControllerExtensions.GetDetailedStats(controller);

// 2. Export API keys in JSON format
await AdminControllerExtensions.ExportApiKeysAsync(controller, "json");

// 3. Export audit logs since a specific date
await AdminControllerExtensions.ExportAuditLogsAsync(controller, "csv", DateTime.UtcNow.AddDays(-7));

// 4. Run comprehensive system diagnostics
await AdminControllerExtensions.RunComprehensiveDiagnosticsAsync(controller);

// 5. Reset rate limits for a specific API key
await AdminControllerExtensions.ResetRateLimitsForKeyAsync(controller, "api-key-12345");
```