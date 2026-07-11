# RetryPolicyBuilder

The `RetryPolicyBuilder` is a fluent configuration class designed to construct resilient retry policies for asynchronous operations within the `api-key-gateway` project. It enables developers to define specific retry behaviors, including the maximum number of attempts, delay strategies (fixed or exponential backoff), and the specific exception types that should trigger a retry. Once configured, the builder compiles these settings into an executable function that wraps target tasks, automatically handling transient failures according to the defined rules.

## API

### `WithMaxRetries`
Configures the maximum number of retry attempts the policy will execute after the initial failure.
- **Parameters**: `int maxRetries` – The number of additional attempts allowed. Must be non-negative.
- **Returns**: `RetryPolicyBuilder` – The current builder instance to allow method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `maxRetries` is negative.

### `WithInitialDelay`
Sets the base duration to wait before the first retry attempt.
- **Parameters**: `TimeSpan initialDelay` – The time to wait before the first retry. Must be non-negative.
- **Returns**: `RetryPolicyBuilder` – The current builder instance to allow method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `initialDelay` is less than `TimeSpan.Zero`.

### `WithBackoffMultiplier`
Defines the multiplier applied to the delay duration for each subsequent retry, enabling exponential backoff strategies.
- **Parameters**: `double multiplier` – The factor by which the previous delay is multiplied. Must be greater than or equal to 1.0.
- **Returns**: `RetryPolicyBuilder` – The current builder instance to allow method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `multiplier` is less than 1.0.

### `WithMaxDelay`
Specifies the upper limit for the delay duration between retries, preventing wait times from growing indefinitely when using exponential backoff.
- **Parameters**: `TimeSpan maxDelay` – The maximum allowable wait time between attempts. Must be non-negative.
- **Returns**: `RetryPolicyBuilder` – The current builder instance to allow method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `maxDelay` is less than `TimeSpan.Zero`.

### `RetryOn<TException>`
Registers a specific exception type that should trigger the retry logic. Exceptions not matching any registered types will cause the operation to fail immediately.
- **Parameters**: None (generic type parameter `TException` defines the type).
- **Returns**: `RetryPolicyBuilder` – The current builder instance to allow method chaining.
- **Throws**: No exceptions thrown during configuration; invalid type constraints are handled by the C# compiler.

### `Build<T>`
Finalizes the configuration and generates the executable retry wrapper function.
- **Parameters**: None.
- **Returns**: `Func<Func<Task<T>>, Task<T>>` – A higher-order function that accepts a factory delegate (`Func<Task<T>>`) representing the operation to execute. When invoked, this returned function runs the operation and applies the configured retry logic.
- **Throws**: `InvalidOperationException` if the builder has not been configured with at least one exception type via `RetryOn<TException>`.

## Usage

### Example 1: Exponential Backoff for Network Errors
This example configures a policy that retries up to 3 times on `HttpRequestException`, starting with a 1-second delay and doubling the wait time for each subsequent attempt, capped at 30 seconds.

```csharp
var policyFunc = new RetryPolicyBuilder()
    .WithMaxRetries(3)
    .WithInitialDelay(TimeSpan.FromSeconds(1))
    .WithBackoffMultiplier(2.0)
    .WithMaxDelay(TimeSpan.FromSeconds(30))
    .RetryOn<HttpRequestException>()
    .Build<string>();

// Usage within an async context
string result = await policyFunc(async () => 
{
    return await httpClient.GetStringAsync("https://api.example.com/data");
});
```

### Example 2: Fixed Delay for Database Transients
This example sets up a policy to handle transient database errors with a fixed 500ms delay between up to 5 retries, specifically targeting `SqlException`.

```csharp
var policyFunc = new RetryPolicyBuilder()
    .WithMaxRetries(5)
    .WithInitialDelay(TimeSpan.FromMilliseconds(500))
    .WithBackoffMultiplier(1.0) // Fixed delay
    .RetryOn<SqlException>()
    .Build<int>();

// Usage within an async context
int recordCount = await policyFunc(async () => 
{
    return await databaseContext.Records.CountAsync();
});
```

## Notes

- **Exception Filtering**: The policy strictly adheres to the types registered via `RetryOn<TException>`. If an operation throws an exception type that was not explicitly registered, the policy will not retry, and the exception will propagate immediately to the caller. Multiple calls to `RetryOn<TException>` with different types are additive.
- **Delay Calculation**: When `WithBackoffMultiplier` is greater than 1.0, the delay for attempt $n$ is calculated as $\min(\text{initialDelay} \times \text{multiplier}^{(n-1)}, \text{maxDelay})$. If `WithMaxDelay` is not called, delays may grow indefinitely based on the multiplier.
- **Thread Safety**: The `RetryPolicyBuilder` instance itself is **not thread-safe** during the configuration phase. Do not modify the builder instance from multiple threads simultaneously. However, the delegate returned by `Build<T>` is stateless regarding the builder's configuration and is safe to be invoked concurrently from multiple threads.
- **Configuration Validation**: All numeric and time-span parameters are validated at configuration time. Attempting to build a policy without registering at least one exception type will result in an `InvalidOperationException` at the time `Build<T>` is called, not during the individual setter calls.
