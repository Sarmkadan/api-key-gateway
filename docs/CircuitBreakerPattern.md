# CircuitBreakerPattern

`src/ApiKeyGateway/Utilities/CircuitBreakerPattern.cs` contains a small, in-process circuit breaker. It wraps calls to a dependency that can fail, counts consecutive failures, and stops forwarding calls once a threshold is reached. After a timeout it lets calls through again to test whether the dependency has recovered.

The file declares three public types in the `ApiKeyGateway.Utilities` namespace:

- `ICircuitBreaker`, the interface.
- `CircuitBreakerState`, the enum of states.
- `CircuitBreaker`, the sealed production implementation.

```csharp
namespace ApiKeyGateway.Utilities;

public interface ICircuitBreaker
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation);
    void RecordSuccess();
    void RecordFailure();
    CircuitBreakerState GetState();
}

public enum CircuitBreakerState
{
    Closed,
    Open,
    HalfOpen
}

public sealed class CircuitBreaker : ICircuitBreaker
{
    public CircuitBreaker(
        int failureThreshold = 5,
        TimeSpan? timeout = null,
        ILogger<CircuitBreaker>? logger = null);

    public Task<T> ExecuteAsync<T>(Func<Task<T>> operation);
    public void RecordSuccess();
    public void RecordFailure();
    public CircuitBreakerState GetState();
}
```

## States

| State | Meaning | `ExecuteAsync` behavior |
|-------|---------|-------------------------|
| `Closed` | Normal operation. Calls pass through. | Runs the operation. A failure increments the failure count. |
| `Open` | The dependency is considered failing. | Throws `InvalidOperationException("Circuit breaker is open")` without running the operation, until the timeout has passed since the last failure. |
| `HalfOpen` | Trial mode after the timeout. | Runs the operation. Success closes the circuit. Failures are counted and re-open it once the count reaches the threshold. |

### Transitions

- **Closed → Open.** A failure is recorded and the failure count reaches `failureThreshold`.
- **Open → HalfOpen.** The next call to `ExecuteAsync` arrives after more than `timeout` has passed since the last recorded failure. The failure count is reset to `0` at this point. The call itself is then executed as a trial.
- **HalfOpen → Closed.** A success is recorded (through `ExecuteAsync` or `RecordSuccess`).
- **HalfOpen → Open.** Failures accumulate until the count reaches `failureThreshold` again.

`GetState()` returns the stored state as it is. It does not perform the Open → HalfOpen transition. The transition happens only inside `ExecuteAsync`.

## Constructor

```csharp
public CircuitBreaker(
    int failureThreshold = 5,
    TimeSpan? timeout = null,
    ILogger<CircuitBreaker>? logger = null)
```

| Parameter | Default | Description |
|-----------|---------|-------------|
| `failureThreshold` | `5` | Number of failures (since the last success or the last half-open reset) that opens the circuit. |
| `timeout` | `30` seconds | How long the circuit stays open after the last failure before a trial call is allowed. |
| `logger` | `NullLogger<CircuitBreaker>` | Receives state transitions and failure events. |

The constructor does not validate its arguments. A `failureThreshold` of `0` or less makes the first recorded failure open the circuit. Pass only positive values. The gateway's rate limiter does not use this class. `RateLimitingService` has its own inline breaker, configured through `RateLimitingOptions`.

## Public members

### ExecuteAsync

```csharp
public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
```

Runs `operation` if the circuit allows it.

1. If the state is `Open` and the timeout has not elapsed, throws `InvalidOperationException` and does not call `operation`.
2. If the state is `Open` and the timeout has elapsed, moves to `HalfOpen` and resets the failure count.
3. Awaits `operation`. On success it calls `RecordSuccess`. If `operation` throws, it calls `RecordFailure` and rethrows the original exception.

Exceptions from `operation` are rethrown unchanged, including `OperationCanceledException`. Every exception counts as a failure.

`HalfOpen` does not gate concurrent callers. The code comment says a single request is allowed, but the code does not enforce that. Several calls in `HalfOpen` can run at the same time, and each one records its own outcome.

### RecordSuccess

```csharp
public void RecordSuccess()
```

Resets the failure count to `0`. If the state is `HalfOpen`, it changes the state to `Closed`. In `Closed` or `Open`, only the failure count is reset. Calling it while `Open` does not close the circuit.

Use it when the protected call happens outside `ExecuteAsync`.

### RecordFailure

```csharp
public void RecordFailure()
```

Increments the failure count and updates the time of the last failure. If the count is at or above `failureThreshold`, the state becomes `Open`.

Each call also extends the open period, because the timeout is measured from the last failure. Use it when the protected call happens outside `ExecuteAsync`.

### GetState

```csharp
public CircuitBreakerState GetState()
```

Returns the current state. This is a plain read with no lock. Use it for diagnostics or health reporting. Do not use it to decide whether to call the dependency, because the answer can be stale. Calling `ExecuteAsync` is the correct way to make that decision.

## Thread safety

State changes and counter updates are guarded by a private lock. The lock is released before `operation` runs, so long-running operations do not block other callers. `GetState()` reads without the lock.

## Logging

With a logger supplied, the class writes:

| Event | Level | Message |
|-------|-------|---------|
| Each recorded failure | Warning | `Circuit breaker failure recorded: {FailureCount}/{Threshold}` |
| Circuit opens | Error | `Circuit breaker opening after {FailureCount} failures` |
| Open → HalfOpen | Information | `Circuit breaker transitioning to Half-Open` |
| HalfOpen → Closed | Information | `Circuit breaker transitioning to Closed` |

## Usage

The class is not registered in the dependency injection container. Create it where it is needed, or register it yourself. Keep one instance per protected dependency, because the state is per instance.

```csharp
using ApiKeyGateway.Utilities;

public sealed class ExternalPricingClient
{
    private readonly HttpClient _httpClient;
    private readonly ICircuitBreaker _breaker;
    private readonly ILogger<ExternalPricingClient> _logger;

    public ExternalPricingClient(
        HttpClient httpClient,
        ILogger<ExternalPricingClient> logger,
        ILogger<CircuitBreaker> breakerLogger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _breaker = new CircuitBreaker(
            failureThreshold: 3,
            timeout: TimeSpan.FromSeconds(15),
            logger: breakerLogger);
    }

    public async Task<decimal?> GetPriceAsync(string sku, CancellationToken cancellationToken)
    {
        try
        {
            return await _breaker.ExecuteAsync(async () =>
            {
                using var response = await _httpClient.GetAsync($"/prices/{sku}", cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<decimal>(cancellationToken);
            });
        }
        catch (InvalidOperationException ex) when (_breaker.GetState() == CircuitBreakerState.Open)
        {
            // Circuit is open: fail fast instead of waiting on a dependency that is down.
            _logger.LogWarning(ex, "Pricing service unavailable, skipping lookup for {Sku}", sku);
            return null;
        }
    }
}
```

In this example, three consecutive failures open the circuit. For the next 15 seconds after the last failure, calls return `null` immediately. After that, the next call is a trial. A success closes the circuit. Each failure is rethrown to the caller, so the caller still sees the original error.

`InvalidOperationException` is also a type that other code can throw. The `when` filter checks the circuit state to make sure the exception came from the breaker. Matching on the exception type alone is not enough.

### Inspecting state

```csharp
if (_breaker.GetState() == CircuitBreakerState.Open)
{
    // Report degraded status. Do not skip the call here: ExecuteAsync decides on the transition.
}
```

## Limitations

- The state is in memory. Each process has its own circuit, and the state is lost on restart.
- Only consecutive failures are counted. Any success resets the count.
- Every exception counts as a failure, including cancellation and programming errors. Filter at the call site if that is not wanted.
- `HalfOpen` does not limit the number of trial calls.
- The timeout is measured from the last failure, not from the moment the circuit opened.
- A failed trial in `HalfOpen` does not re-open the circuit immediately. Because the count was reset on entering `HalfOpen`, the circuit re-opens only after `failureThreshold` failures again.
- The constructor does not validate its arguments.
