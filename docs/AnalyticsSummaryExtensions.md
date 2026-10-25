# AnalyticsSummaryExtensions

Static extension methods for `AnalyticsSummary` (`ApiKeyGateway.Services`) that turn the numeric fields of an analytics summary into strings for logs, digests and quick diagnostics. The class lives in `src/ApiKeyGateway/Services/AnalyticsSummaryExtensions.cs`, next to the `AnalyticsSummary` model that it extends (declared in `UsageAnalyticsService.cs`).

The methods only format data. None of them modify the summary, query the database or touch background state.

Every method validates its `summary` argument with `ArgumentNullException.ThrowIfNull`, so a `null` summary throws `ArgumentNullException`.

## API

### `ToSummaryString(this AnalyticsSummary summary)` → `string`

Builds a single-line description of the summary: API key id, date range, request counts, success and error rates and average response time.

Output format:

```
API Key: {ApiKeyId}, Period: {From:yyyy-MM-dd} to {To:yyyy-MM-dd}, Requests: {TotalRequests} (Success: {SuccessfulRequests}, Failed: {FailedRequests}), Success Rate: {SuccessRatePercent:F2}%, Error Rate: {ErrorRatePercent:F2}%, Avg Response Time: {AverageResponseTimeMs:F2}ms
```

- Dates use the `yyyy-MM-dd` pattern. Percentages and the response time use two decimal places (`F2`).
- The numeric parts are formatted with the **current culture**, so a culture with a comma decimal separator produces `12,50%`. Use the `Format*` methods below when the output must be culture-independent.
- The method does not include `TotalBytesTransferred`, `UniqueEndpoints` or `UniqueSourceIps`.
- **Throws** `ArgumentNullException` if `summary` is `null`.

```csharp
using ApiKeyGateway.Services;

var summary = new AnalyticsSummary
{
    ApiKeyId = "key-42",
    From = new DateTime(2026, 10, 1),
    To = new DateTime(2026, 10, 7),
    TotalRequests = 1000,
    SuccessfulRequests = 950,
    FailedRequests = 50,
    SuccessRatePercent = 95.0,
    ErrorRatePercent = 5.0,
    AverageResponseTimeMs = 42.5
};

string line = summary.ToSummaryString();
// API Key: key-42, Period: 2026-10-01 to 2026-10-07, Requests: 1000 (Success: 950, Failed: 50), Success Rate: 95.00%, Error Rate: 5.00%, Avg Response Time: 42.50ms
```

### `FormatAverageResponseTime(this AnalyticsSummary summary, string? format = null)` → `string`

Formats `AverageResponseTimeMs` with the invariant culture. When `format` is `null`, the method uses `"F2"`. Any other value is passed straight to `double.ToString`, so standard and custom numeric format strings both work.

- **Throws** `ArgumentNullException` if `summary` is `null`.

```csharp
summary.FormatAverageResponseTime();          // "42.50"
summary.FormatAverageResponseTime("F0");      // "43"
summary.FormatAverageResponseTime("0.###");   // "42.5"
```

### `FormatSuccessRatePercent(this AnalyticsSummary summary, string? format = null)` → `string`

Formats `SuccessRatePercent` with the invariant culture, using the same `format` rules as `FormatAverageResponseTime`. The `%` sign is not added; append it yourself if needed.

- **Throws** `ArgumentNullException` if `summary` is `null`.

```csharp
summary.FormatSuccessRatePercent();           // "95.00"
summary.FormatSuccessRatePercent("F1");       // "95.0"
```

> Note: do not pass a `"P"` (percent) format. That specifier multiplies by 100, but `SuccessRatePercent` already holds a percentage, so the output would be 100 times too large (`95.00` becomes `9,500.00 %`). Use `"F"`, `"0.#"` or similar numeric formats instead.

### `FormatErrorRatePercent(this AnalyticsSummary summary, string? format = null)` → `string`

Formats `ErrorRatePercent` with the invariant culture, using the same `format` rules as the other `Format*` methods. As with the success rate, the `%` sign is not added.

- **Throws** `ArgumentNullException` if `summary` is `null`.

```csharp
summary.FormatErrorRatePercent();             // "5.00"
summary.FormatErrorRatePercent("F1");         // "5.0"
```

## Usage Notes

- `ToSummaryString()` is meant for human-readable log lines. Use it when the current culture is acceptable. For machine-readable or cross-region output, build the string with the `Format*` methods or with explicit `InvariantCulture` formatting.
- Passing an empty string as `format` (`""`) is not the same as passing `null`. `null` selects `"F2"`, but `""` is forwarded to `double.ToString("")`, which uses the general (`G`) format.
- The `Format*` methods never add a unit. Append `ms` or `%` in the calling code when needed.

## Related Types

- `AnalyticsSummary` - the model these methods extend. Defined in `src/ApiKeyGateway/Services/UsageAnalyticsService.cs`. The properties used here are `ApiKeyId`, `From`, `To`, `TotalRequests`, `SuccessfulRequests`, `FailedRequests`, `SuccessRatePercent`, `ErrorRatePercent` and `AverageResponseTimeMs`.
- `AnalyticsSummaryValidation` - validation helpers for the same model (`src/ApiKeyGateway/Services/AnalyticsSummaryValidation.cs`).
- [`AnalyticsControllerExtensions`](AnalyticsControllerExtensions.md) - extension helpers for the analytics controller.
