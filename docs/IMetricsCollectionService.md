# IMetricsCollectionService

Central service for recording and retrieving API gateway metrics such as request counts, error rates, and latency statistics. Used by middleware and controllers to track operational health and enforce rate limiting policies.

## API

### `MetricsCollectionService`

Initializes a new instance of the metrics collection service with default counters and empty snapshots.

### `void RecordRequest()`

Increments the total request counter and updates endpoint-specific request counts. No parameters or return value; never throws.

### `void RecordRateLimitExceeded()`

Increments the counter tracking exceeded rate limits. No parameters or return value; never throws.

### `void RecordError()`

Increments the total error counter and updates error-code-specific counts. No parameters or return value; never throws.

### `MetricsSnapshot GetSnapshot()`

Retrieves a read-only snapshot of current metrics including counts, rates, and latency percentiles. Returns a `MetricsSnapshot` instance populated at the time of the call. Never throws.

### `DateTime Timestamp`

Gets the UTC timestamp when the current snapshot was generated. Read-only property; never throws.

### `long TotalRequests`

Gets the total number of requests recorded since service initialization. Read-only property; never throws.

### `long TotalErrors`

Gets the total number of errors recorded since service initialization. Read-only property; never throws.

### `double ErrorRate`

Gets the ratio of errors to total requests expressed as a value between 0.0 and 1.0. Read-only property; never throws.

### `double AverageLatencyMs`

Gets the average request latency in milliseconds over the recorded samples. Read-only property; never throws.

### `long P95LatencyMs`

Gets the 95th percentile request latency in milliseconds. Read-only property; never throws.

### `long TotalRateLimitExceeded`

Gets the total number of rate-limit exceeded events recorded since service initialization. Read-only property; never throws.

### `Dictionary<string, long> RequestsByEndpoint`

Gets a dictionary mapping endpoint names to the number of requests received for each endpoint. Read-only property; never throws.

### `Dictionary<string, long> ErrorsByCode`

Gets a dictionary mapping HTTP status codes to the number of errors recorded for each code. Read-only property; never throws.

## Usage
