# UsageQuotaServiceTestsComprehensive

A comprehensive test suite for the `UsageQuotaService` class, covering limit enforcement, period rollover, disabled quotas, and edge cases at exact limits. All tests are asynchronous and validate that the service correctly returns `QuotaCheckResult` states (`Unlimited`, `WithinLimit`, `Exceeded`) and remaining counts under various configurations.

## API

### `public UsageQuotaServiceTestsComprehensive()`

Default constructor. Initializes the test class with any required mock dependencies (e.g., `IUsageRepository`, `IClock`) via a test fixture or setup method.

### `public async Task CheckAndRecordAsync_UsageExactlyAtLimit_ReturnsExceededWithZeroRemaining`

Verifies that when current usage equals the configured limit, `CheckAndRecordAsync` returns `QuotaCheckResult.Exceeded` with `Remaining` equal to 0.

- **Parameters**: None.
- **Returns**: `Task` (test passes if assertion succeeds).
- **Throws**: `Xunit.Sdk.XunitException` if the result does not match expectations.

### `public async Task CheckAndRecordAsync_UsageExceedsLimit_ReturnsExceededWithZeroRemaining`

Verifies that when current usage exceeds the limit, the result is `Exceeded` with `Remaining` = 0.

- **Parameters**: None.
- **Returns**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` on failure.

### `public async Task CheckAndRecordAsync_DisabledQuotaWithHighUsage_ReturnsUnlimited`

Confirms that when the quota is disabled (limit = 0 or feature flag off), even high usage returns `Unlimited` with `Remaining` = `int.MaxValue` or similar sentinel.

- **Parameters**: None.
- **Returns**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException`.

### `public async Task CheckAndRecordAsync_ExactlyAtLimitDaily_ReturnsCorrectState`

Tests daily quota enforcement: when usage exactly matches the daily limit, the result is `Exceeded` with zero remaining.

- **Parameters**: None.
- **Returns**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException`.

### `public async Task CheckAndRecordAsync_ExactlyAtLimitMonthly_ReturnsCorrectState`

Same as above but for monthly period.

- **Parameters**: None.
- **Returns**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException`.

### `public async Task CheckAndRecordAsync_ExactlyAtLimitHourly_ReturnsCorrectState`

Same as above but for hourly period.

- **Parameters**: None.
- **Returns**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException`.

### `public async Task CheckAndRecordAsync_PeriodRollover_ResetsCounterAndAllowsNewRequests`

Verifies that after a period boundary (e.g., day, month), the usage counter resets and a new request is allowed (`WithinLimit`).

- **Parameters**: None.
- **Returns**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException`.

### `public async Task CheckAndRecordAsync_MultipleRequestsAfterRollover_WorksCorrectly`

Ensures that multiple requests after a rollover are handled correctly, decrementing remaining count appropriately.

- **Parameters**: None.
- **Returns**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException`.

### `public async Task CheckAndRecordAsync_PeriodRolloverWithZeroLimit_ResetsAndAllowsNoRequests`

When the limit is zero, after rollover the counter resets but the service still rejects requests (returns `Exceeded` with zero remaining).

- **Parameters**: None.
- **Returns**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException`.

### `public async Task CheckAndRecordAsync_WeeklyPeriodRollover_ResetsCounter`

Tests that weekly rollover correctly resets the usage counter.

- **Parameters**: None.
- **Returns**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException`.

### `public async Task CheckAndRecordAsync_MonthlyPeriodRollover_ResetsCounter`

Tests that monthly rollover correctly resets the usage counter.

- **Parameters**: None.
- **Returns**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException`.

### `public async Task CheckAndRecordAsync_ExactlyAtLimit_RequestRejectedWithCorrectState`

Verifies that when usage is exactly at the limit, the request is rejected and the returned state contains the correct `Remaining` (0) and `IsExceeded` flag.

- **Parameters**: None.
- **Returns**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException`.

### `public async Task CheckAndRecordAsync_QuotaExceeded_BehaviorConsistentAcrossPeriods`

Ensures that the `Exceeded` behavior is identical for daily, hourly, and monthly periods when usage exceeds the limit.

- **Parameters**: None.
- **Returns**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException`.

## Usage

The test class is designed to be run by a test runner such as `dotnet test` or within an IDE. Below are two realistic examples of how to execute these tests programmatically or integrate them into a CI pipeline.

### Example 1: Running all tests via command line

```bash
dotnet test tests/ApiKeyGateway.Tests --filter "FullyQualifiedName~UsageQuotaServiceTestsComprehensive"
```

### Example 2: Running a specific test in code (e.g., for debugging)

```csharp
using Xunit;
using ApiKeyGateway.Tests;

public class TestRunner
{
    public async Task RunSpecificTest()
    {
        var testClass = new UsageQuotaServiceTestsComprehensive();
        await testClass.CheckAndRecordAsync_ExactlyAtLimit_ReturnsExceededWithZeroRemaining();
        Console.WriteLine("Test passed.");
    }
}
```

## Notes

- **Edge cases covered**: exact limit boundaries (hourly, daily, monthly, weekly), zero-limit quotas, disabled quotas, and period rollover with both positive and zero limits. The tests ensure that the service does not allow requests after the limit is reached and correctly resets counters at period boundaries.
- **Thread safety**: The test class itself is not thread-safe; tests should be run sequentially within a single test session. The underlying `UsageQuotaService` is expected to be thread-safe (e.g., using locks or atomic operations), but these tests do not verify concurrent access. For thread-safety validation, separate integration or stress tests are recommended.
- **Dependencies**: The tests rely on mocked implementations of `IUsageRepository` and `IClock` to control time and storage behavior. No real database or external service is required.
- **Test framework**: The class uses xUnit (`[Fact]` attributes implied) and `async Task` patterns. All tests are independent and can be run in any order.
