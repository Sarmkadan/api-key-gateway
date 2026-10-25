# AnalyticsSummaryValidation

Static validation helpers for [`AnalyticsSummary`](AnalyticsSummary.md) (`ApiKeyGateway.Services`). The class lives in `src/ApiKeyGateway/Services/AnalyticsSummaryValidation.cs`. It is a `public static class` whose methods are extensions on `AnalyticsSummary`, so they can be called as `summary.Validate()`, `summary.IsValid()` or `summary.EnsureValid()`.

| Method | Returns | Behavior when the summary has problems |
|---|---|---|
| `Validate()` | `IReadOnlyList<string>` | Returns every problem message. An empty list means the summary is valid. |
| `IsValid()` | `bool` | Returns `false`. Equivalent to `Validate().Count == 0`. |
| `EnsureValid()` | `void` | Throws `ArgumentException` (`paramName: value`). The message joins all problem messages with `"; "`. |

Validation is exhaustive within a call. Every rule is evaluated, and each failing rule adds its own message. Messages are returned in the order the rules are checked (see [Validation rules](#validation-rules)). The only early exit is the null check, which runs before any rule (see [Null handling](#null-handling)).

The validator checks the summary's internal consistency. It does not query the database or check that the `ApiKeyId` exists.

## Constants

Both tolerances are `private const` values. They cannot be changed without editing the source.

| Constant | Value | Used by |
|---|---|---|
| `DateTimeToleranceMinutes` | `5` | `From` and `To` may be at most 5 minutes ahead of `DateTime.UtcNow`. |
| `PercentageTolerance` | `0.001` | Allowed absolute difference between `SuccessRatePercent + ErrorRatePercent` and `100`. |

## Validation rules

Rules are listed in the order the code checks them. Messages are copied verbatim from the source.

| Field | Rule | Problem message |
|---|---|---|
| `ApiKeyId` | Must not be `null` or empty. Whitespace-only values pass. | `ApiKeyId must not be empty.` |
| `From` | Must not equal `default(DateTime)`. | `From date must be set to a valid date.` |
| `From` | Must not be later than `DateTime.UtcNow + 5 minutes`. Checked only when `From` is set. | `From date cannot be in the future.` |
| `To` | Must not equal `default(DateTime)`. | `To date must be set to a valid date.` |
| `To` | Must not be later than `DateTime.UtcNow + 5 minutes`. | `To date cannot be in the future.` |
| `To` | Must not be earlier than `From`. Checked only when `To` is set and is not in the future. | `To date must be after or equal to From date.` |
| `TotalRequests` | Must be `>= 0`. | `TotalRequests must be non-negative.` |
| `SuccessfulRequests` | Must be `>= 0`. | `SuccessfulRequests must be non-negative.` |
| `FailedRequests` | Must be `>= 0`. | `FailedRequests must be non-negative.` |
| Request counts | If `TotalRequests > 0`: `SuccessfulRequests + FailedRequests` must equal `TotalRequests`. | `SuccessfulRequests + FailedRequests must equal TotalRequests.` |
| Request counts | If `TotalRequests <= 0`: `SuccessfulRequests` and `FailedRequests` must both be `0`. | `SuccessfulRequests and FailedRequests must be 0 when TotalRequests is 0.` |
| `SuccessRatePercent` | Must be between `0` and `100` inclusive. | `SuccessRatePercent must be between 0 and 100 inclusive.` |
| `ErrorRatePercent` | Must be between `0` and `100` inclusive. | `ErrorRatePercent must be between 0 and 100 inclusive.` |
| Percentages | If `TotalRequests > 0`: `\|SuccessRatePercent + ErrorRatePercent - 100\|` must be `<= 0.001`. | `SuccessRatePercent + ErrorRatePercent must equal 100 within tolerance.` |
| `AverageResponseTimeMs` | Must be `>= 0`. | `AverageResponseTimeMs must be non-negative.` |
| `TotalBytesTransferred` | Must be `>= 0`. | `TotalBytesTransferred must be non-negative.` |
| `UniqueEndpoints` | Must be `>= 0`. | `UniqueEndpoints must be non-negative.` |
| `UniqueSourceIps` | Must be `>= 0`. | `UniqueSourceIps must be non-negative.` |

### Rule notes

- **Date rules are skipped when the value is not set.** If `To` is `default`, only the `To` "must be set" message is reported. The future check and the ordering check against `From` are not run.
- **The ordering rule is an `else if`.** If `To` is in the future, only `To date cannot be in the future.` is reported. `To` is not compared with `From` in that case, even if it is also earlier than `From`.
- **The future-date limit is relative to the current time.** `From` and `To` are compared with `DateTime.UtcNow`, and the 5-minute tolerance absorbs small clock skew. `DateTime.Kind` is not checked. A `DateTime` with `Kind = Local` is compared by its clock value as if it were UTC. Pass UTC values.
- **Count consistency only applies when there is traffic.** With `TotalRequests > 0`, `SuccessfulRequests + FailedRequests` must equal the total. With `TotalRequests <= 0`, both per-outcome counts must be `0`. A negative `TotalRequests` therefore produces two messages when the other counts are non-zero: the non-negative message and the "must be 0 when TotalRequests is 0" message.
- **Percentages are checked on their own, and together when there is traffic.** Each percentage must be in `[0, 100]` regardless of `TotalRequests`. The sum rule (`SuccessRatePercent + ErrorRatePercent == 100`) only runs when `TotalRequests > 0`. When `TotalRequests == 0`, the percentages can be any value in `[0, 100]`, including values that do not add up to 100.
- **Non-finite doubles.** The percentage and response-time checks use plain comparisons. `double.NaN` makes every comparison false, so `NaN` passes all percentage and `AverageResponseTimeMs` checks, including the sum rule. `double.PositiveInfinity` and `double.NegativeInfinity` are rejected by the range and sign checks. Callers that can produce `NaN` (for example, a division by zero during aggregation) must check for it themselves.

## Null handling

A `null` summary is not reported as a problem. All three methods throw `ArgumentNullException` before any rule runs.

| Method | Throws |
|---|---|
| `Validate(this AnalyticsSummary value)` | `ArgumentNullException` (`paramName: value`) |
| `IsValid(this AnalyticsSummary value)` | `ArgumentNullException` (via `Validate`) |
| `EnsureValid(this AnalyticsSummary value)` | `ArgumentNullException` (via `Validate`) |

`EnsureValid` throws `ArgumentException`, not `ArgumentNullException`, for invalid data. Code that catches one should also consider the other.

## Examples

```csharp
using ApiKeyGateway.Services;

var summary = new AnalyticsSummary
{
    ApiKeyId = "key-1",
    From = DateTime.UtcNow.AddDays(-1),
    To = DateTime.UtcNow,
    TotalRequests = 10,
    SuccessfulRequests = 7,
    FailedRequests = 3,
    SuccessRatePercent = 70,
    ErrorRatePercent = 30,
    AverageResponseTimeMs = 42.5
};

if (summary.IsValid())
{
    // Safe to report.
}

IReadOnlyList<string> problems = summary.Validate();
```

Invalid input with several problems returns several messages:

```csharp
var broken = new AnalyticsSummary
{
    ApiKeyId = "key-1",
    From = DateTime.UtcNow,
    To = DateTime.UtcNow,
    TotalRequests = 10,
    SuccessfulRequests = 7,
    FailedRequests = 2   // 7 + 2 != 10
};

IReadOnlyList<string> problems = broken.Validate();
// [ "SuccessfulRequests + FailedRequests must equal TotalRequests.",
//   "SuccessRatePercent + ErrorRatePercent must equal 100 within tolerance." ]
```

The percentage fields are left at `0`, which is inside the range check, so only the percentage sum rule fails. Both messages come from rules that run because `TotalRequests > 0`.

`EnsureValid` joins the same messages with `"; "`:

```csharp
try
{
    broken.EnsureValid();
}
catch (ArgumentException ex)
{
    // ex.ParamName == "value"
    // ex.Message contains every problem message, separated by "; "
}
```

## Related

- [`AnalyticsSummary`](AnalyticsSummary.md): the model being validated.
- [`AnalyticsSummaryExtensions`](AnalyticsSummaryExtensions.md): companion helpers for the same type.
