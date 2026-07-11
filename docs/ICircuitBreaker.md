# ICircuitBreaker

A circuit breaker pattern implementation for managing transient faults in distributed systems. It monitors operations for failures and, when a threshold is exceeded, "trips" the circuit to prevent further calls until recovery criteria are met. Designed for high-throughput scenarios where resilience against downstream failures is critical.

## API

### `public CircuitBreaker`

The default constructor initializes a new circuit breaker with default failure thresholds and recovery parameters.

- **Parameters**: None.
- **Remarks**: Uses default values for failure threshold (5), recovery timeout (30 seconds), and half-open success threshold (3).

---

### `public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)`

Executes the provided asynchronous action while monitoring for failures. If the circuit is open, throws `InvalidOperationException`. If the circuit is half-open, allows a single call through to test downstream health.

- **Parameters**:
  - `action` (Func<Task<T>>): The asynchronous operation to execute.
- **Return Value**: Task<T> representing the result of the operation.
- **Exceptions**:
  - `InvalidOperationException`: Thrown if the circuit is open or in a failed state.
  - `ArgumentNullException`: Thrown if `action` is null.
- **Remarks**: Automatically records success or failure based on the outcome of `action`.

---

### `public void RecordSuccess()`

Records a successful operation, incrementing the success counter and potentially transitioning the circuit from half-open to closed if the success threshold is met.

- **Parameters**: None.
- **Remarks**: Only effective if the circuit is in a half-open state. Otherwise, no action is taken.

---

### `public void RecordFailure()`

Records a failed operation, incrementing the failure counter and potentially transitioning the circuit to open if the failure threshold is exceeded.

- **Parameters**: None.
- **Remarks**: May immediately trip the circuit to open state if the failure threshold is reached.

---
### `public CircuitBreakerState GetState()`

Retrieves the current state of the circuit breaker.

- **Parameters**: None.
- **Return Value**: CircuitBreakerState (enum indicating Open, Closed, or HalfOpen).
- **Remarks**: Useful for monitoring and diagnostic purposes.

## Usage

### Example 1: Basic Usage with Async Operation
