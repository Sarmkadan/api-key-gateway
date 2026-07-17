# AdminControllerExtensions

Extension methods for `AdminController` that expose administrative operations for API key gateway management, including statistics retrieval, diagnostics, and bulk exports.

## API

### `GetDetailedStats`

Retrieves a comprehensive set of statistics about the API key gateway's current operational state.

- **Returns**: `IActionResult` containing a structured response with detailed statistics.
- **Throws**: May throw if underlying services (e.g., metrics, audit logs) are unavailable.

### `ExportApiKeysAsync`

Exports all API keys in the system as a downloadable file.

- **Returns**: `Task<IActionResult>` representing the asynchronous export operation.
- **Throws**: May throw if key retrieval or serialization fails.

### `ExportAuditLogsAsync`

Exports all audit logs in the system as a downloadable file.

- **Returns**: `Task<IActionResult>` representing the asynchronous export operation.
- **Throws**: May throw if log retrieval or serialization fails.

### `RunComprehensiveDiagnosticsAsync`

Executes a full suite of diagnostic checks on the API key gateway and returns a report.

- **Returns**: `Task<IActionResult>` containing a diagnostics report with performance, connectivity, and configuration insights.
- **Throws**: May throw if any diagnostic check encounters an unrecoverable error.

### `ResetRateLimitsForKeyAsync`

Resets the rate limit counters for a specific API key.

- **Parameters**: Accepts a key identifier (e.g., key hash or ID).
- **Returns**: `Task<IActionResult>` indicating success or failure.
- **Throws**: May throw if the key does not exist or rate limit service is unavailable.

### `Timestamp` (property)

The timestamp when the statistics or report were generated.

- **Type**: `string`
- **Access**: Read-only

### `StatsSummary` (property)

A high-level summary of gateway statistics.

- **Type**: `Total`
- **Access**: Read-only
- **Structure**:
  - `Total`: `long` – Total number of requests processed.

### `EndpointBreakdown` (property)

A dictionary mapping endpoint paths to the number of requests handled.

- **Type**: `IReadOnlyDictionary<string, long>?`
- **Access**: Read-only
- **Note**: May be `null` if breakdown is unavailable.

### `ErrorBreakdown` (property)

A dictionary mapping HTTP status codes to the number of occurrences.

- **Type**: `IReadOnlyDictionary<int, long>?`
- **Access**: Read-only
- **Note**: May be `null` if breakdown is unavailable.

### `PerformanceMetrics` (property)

Aggregated performance metrics for the gateway.

- **Type**: `Performance`
- **Access**: Read-only
- **Structure**:
  - `Uptime`: `TimeSpan?` – Duration since last restart.
  - `Requests`: `long?` – Total requests processed.
  - `ActiveKeys`: `int?` – Number of active API keys.
  - `Errors`: `double?` – Error rate (0.0 to 1.0).
  - `RateLimits`: `int?` – Number of rate-limited requests.
  - `AverageLatencyMs`: `double?` – Average request latency in milliseconds.
  - `P95LatencyMs`: `double?` – 95th percentile latency in milliseconds.
  - `ErrorRate`: `double?` – Ratio of errors to total requests.
  - `RequestsPerSecond`: `double` – Current requests per second.

## Usage

### Retrieving and exporting statistics

```csharp
[HttpGet("stats")]
public async Task<IActionResult> GetSystemStats()
{
    var result = AdminControllerExtensions.GetDetailedStats(this);
    if (result is OkObjectResult okResult)
    {
        var stats = (okResult.Value as dynamic).Stats;
        return Ok(stats);
    }
    return BadRequest("Failed to retrieve statistics.");
}

[HttpGet("export/keys")]
public async Task<IActionResult> ExportKeys()
{
    return await AdminControllerExtensions.ExportApiKeysAsync(this);
}
```

### Running diagnostics and resetting rate limits

```csharp
[HttpPost("diagnostics")]
public async Task<IActionResult> RunDiagnostics()
{
    return await AdminControllerExtensions.RunComprehensiveDiagnosticsAsync(this);
}

[HttpPost("reset/{keyId}")]
public async Task<IActionResult> ResetKeyLimits(string keyId)
{
    return await AdminControllerExtensions.ResetRateLimitsForKeyAsync(this, keyId);
}
```

## Notes

- **Thread Safety**: All methods are designed to be thread-safe under normal ASP.NET Core request handling. External state changes (e.g., concurrent key deletions during export) may still cause inconsistencies.
- **Large Exports**: `ExportApiKeysAsync` and `ExportAuditLogsAsync` may consume significant memory and I/O bandwidth for large datasets. Consider streaming or pagination for production use.
- **Diagnostics Overhead**: `RunComprehensiveDiagnosticsAsync` performs intensive checks and may temporarily increase system load.
- **Null Safety**: Properties like `EndpointBreakdown` and `ErrorBreakdown` may be `null` if the underlying data source is unavailable or the feature is disabled. Always check for `null` before enumeration.
- **Timestamp Precision**: `Timestamp` reflects the time of generation and may differ slightly from the actual time of data collection due to asynchronous processing.
