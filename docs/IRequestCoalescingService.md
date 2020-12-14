# IRequestCoalescingService

A service that coalesces concurrent identical requests into a single execution to reduce redundant processing and improve efficiency in high-load scenarios. It is particularly useful for API gateways where multiple identical requests may arrive simultaneously for the same resource or operation.

## API

### `RequestCoalescingService`

The default implementation of `IRequestCoalescingService` that manages request coalescing logic. This class is thread-safe and designed for concurrent use.

### `public async Task<T> ExecuteAsync<T>(Func<Task<T>> requestFactory, string requestKey, CancellationToken cancellationToken = default)`

Executes a request, coalescing concurrent calls with the same `requestKey` into a single execution.

- **requestFactory**: A delegate that produces the asynchronous request to execute.
- **requestKey**: A unique identifier for the request used to determine coalescing eligibility.
- **cancellationToken**: Optional token to monitor for cancellation requests.
- **Return value**: A task that represents the asynchronous operation. The task result contains the value returned by `requestFactory`.
- **Exceptions**: Throws `ArgumentNullException` if `requestFactory` is null. Throws `OperationCanceledException` if `cancellationToken` is triggered.

### `public CoalescingMetrics GetMetrics()`

Retrieves current metrics about coalescing activity.

- **Return value**: A `CoalescingMetrics` object containing statistics such as the number of coalesced requests, total executions, and active coalescing groups.
- **Exceptions**: None.

### `public void Dispose()`

Releases all resources used by the service.

- **Exceptions**: None.

### `public bool TrySetResult(string requestKey, object result)`

Attempts to set the result for a coalesced request group identified by `requestKey`.

- **requestKey**: The key identifying the request group.
- **result**: The result to set.
- **Return value**: `true` if the result was successfully set; otherwise, `false` (e.g., if the group was already completed or canceled).
- **Exceptions**: None.

### `public bool TrySetException(string requestKey, Exception exception)`

Attempts to set an exception for a coalesced request group identified by `requestKey`.

- **requestKey**: The key identifying the request group.
- **exception**: The exception to set.
- **Return value**: `true` if the exception was successfully set; otherwise, `false` (e.g., if the group was already completed or canceled).
- **Exceptions**: None.

### `public bool TrySetCanceled(string requestKey, CancellationToken cancellationToken)`

Attempts to mark a coalesced request group as canceled.

- **requestKey**: The key identifying the request group.
- **cancellationToken**: The cancellation token that triggered the cancellation.
- **Return value**: `true` if the group was successfully marked as canceled; otherwise, `false` (e.g., if the group was already completed).
- **Exceptions**: None.

## Usage

### Example: Coalescing identical API requests
