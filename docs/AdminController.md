# AdminController

Centralizes administrative operations for the API Key Gateway, including usage analytics, configuration inspection, diagnostics, and system reset capabilities. Provides endpoints typically protected by elevated permissions to monitor and manage gateway behavior.

## API

### `AdminController`

Entry point for administrative endpoints. No public constructor parameters are exposed; dependencies are injected via constructor injection.

### `IActionResult GetStats()`

Retrieves high-level usage statistics for the API Key Gateway.

- **Returns**: `IActionResult` with HTTP 200 and a JSON payload containing request counts, error rates, and active key counts.
- **Throws**: May throw `InvalidOperationException` if the statistics service is unavailable or data retrieval fails.

### `async Task<IActionResult> ExportUsageData()`

Exports historical usage data in a structured format (e.g., CSV or JSON) for external analysis.

- **Returns**: `Task<IActionResult>` resolving to HTTP 200 with a file stream, or HTTP 404 if no data exists.
- **Throws**: `IOException` if file generation or stream writing fails; `UnauthorizedAccessException` if export is disabled by policy.

### `IActionResult GetConfiguration()`

Returns the current runtime configuration of the API Key Gateway as a JSON-serialized object.

- **Returns**: `IActionResult` with HTTP 200 and the configuration object; HTTP 500 if serialization fails.
- **Throws**: `JsonException` if configuration cannot be serialized due to circular references or unsupported types.

### `async Task<IActionResult> RunDiagnostics()`

Executes a suite of diagnostic checks (e.g., connectivity, rate limit health, circuit breaker state) and returns a report.

- **Returns**: `Task<IActionResult>` with HTTP 200 and a diagnostic report; HTTP 503 if critical checks fail.
- **Throws**: `OperationCanceledException` if diagnostics are aborted due to timeout.

### `async Task<IActionResult> ResetRateLimits()`

Resets all active rate limit counters and sliding windows across the system, effectively clearing transient throttling state.

- **Returns**: `Task<IActionResult>` with HTTP 204 on success; HTTP 403 if the caller lacks permission.
- **Throws**: `InvalidOperationException` if the rate limit store is unreachable or the operation cannot be completed atomically.

## Usage
