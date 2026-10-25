# AnalyticsControllerValidation

Static validation helpers for [`AnalyticsController`](AnalyticsController.md) (`ApiKeyGateway.Controllers`). The class lives in `src/ApiKeyGateway/Controllers/AnalyticsControllerValidation.cs` and exposes three extension methods on `AnalyticsController?`:

- `Validate(this AnalyticsController? value)` returns an `IReadOnlyList<string>` of problem messages. An empty list means the instance is valid.
- `IsValid(this AnalyticsController? value)` returns a `bool`.
- `EnsureValid(this AnalyticsController? value)` throws an `ArgumentException` when the instance has problems.

## Validation rules

**There are no validation rules.** `Validate` always returns an empty list for a non-null instance. The source comment gives the reason: the controller's public members are all async action methods (`GetSummary`, `GetTopEndpoints`, `GetHourlyTrend`, `GetDailyTrend`, `GetTopConsumers`), and the controller exposes no settable properties that could hold invalid state. Request inputs for these endpoints are not checked here.

| Method | Rule | Result |
|---|---|---|
| `Validate` | Null `value` | Throws `ArgumentNullException` before any check. |
| `Validate` | Any non-null instance | Returns an empty `IReadOnlyList<string>`. |
| `IsValid` | Null `value` | Throws `ArgumentNullException`. |
| `IsValid` | Any non-null instance | Returns `true`. |
| `EnsureValid` | Null `value` | Throws `ArgumentNullException`. |
| `EnsureValid` | Any non-null instance | Returns normally. No exception is thrown. |

The `ArgumentException` message format in `EnsureValid` (`Invalid AnalyticsController instance: {problems}`) is only reached if a rule is added to `Validate` later. With the current code it is unreachable.

## Null handling

Null is the only input that is rejected. Each method calls `ArgumentNullException.ThrowIfNull(value)` as its first statement, so the exception is thrown before `Validate` runs. `IsValid` and `EnsureValid` throw the same exception because they call the same guard.

```csharp
using ApiKeyGateway.Controllers;

AnalyticsController? controller = null;

controller.Validate();     // throws ArgumentNullException
controller.IsValid();      // throws ArgumentNullException
controller.EnsureValid();  // throws ArgumentNullException
```

## Usage

```csharp
using ApiKeyGateway.Controllers;

var controller = new AnalyticsController(analyticsService, logger);

// Returns an empty list: the controller has no validation rules.
IReadOnlyList<string> problems = controller.Validate();

// True for any non-null instance.
bool valid = controller.IsValid();

// Returns normally for any non-null instance.
controller.EnsureValid();
```

## Notes

- **Extension syntax:** Because the methods are extensions, they can be called as `controller.Validate()`. They can also be called as static methods, for example `AnalyticsControllerValidation.Validate(controller)`.
- **Thread safety:** The methods are static, keep no state, and only read their argument. They are safe to call from multiple threads, provided the controller instance itself is not being mutated.
- **Scope:** These helpers do not validate endpoint query parameters, date ranges, or result limits. Those inputs are not part of this class and are handled by the controller's actions and their callers.
- **Extending:** To add rules, put them in `Validate` and add a message for each failure. `IsValid` and `EnsureValid` will pick them up automatically because both call `Validate`.
