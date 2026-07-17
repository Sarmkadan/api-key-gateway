# RateLimitingServiceTestsExtensions

Provides factory methods and execution helpers for testing rate limiting behavior. This static class simplifies the creation of `RateLimitingService` instances with controlled configurations, generates predefined rate limits, and offers utilities to run concurrent requests and assert their outcomes in unit tests.

## API

### CreateService

```csharp
public static RateLimitingService CreateService(...)
```

Constructs a `RateLimitingService` configured for test scenarios. Accepts parameters that control the underlying rate limit store, window size, and maximum request counts. Returns a fully initialized service ready for use in concurrent or sequential test execution. Does not throw under normal configuration inputs.

### CreateRateLimit

```csharp
public static RateLimit CreateRateLimit(...)
```

Produces a `RateLimit` instance with test-friendly defaults. Parameters allow overriding the request limit, window duration, and associated API key identifier. Returns a populated `RateLimit` record. Does not throw.

### ExecuteConcurrentRequestsAsync

```csharp
public static async Task<ConcurrentBag<RateLimitResult>> ExecuteConcurrentRequestsAsync(...)
```

Executes a specified number of requests against a `RateLimitingService` concurrently. Accepts the service instance, the API key to use, and the total request count. Returns a `ConcurrentBag<RateLimitResult>` containing the outcome of each individual request. The bag preserves no ordering guarantees, which is expected for concurrent workloads. Does not throw directly; individual request exceptions are captured in the result records.

### ShouldAllThrowRateLimitExceededAsync

```csharp
public static async Task ShouldAllThrowRateLimitExceededAsync(...)
```

Asserts that every request in a concurrent batch fails with a rate-limit-exceeded condition. Accepts the service, API key, and request count. Internally executes the requests and validates that all results indicate a rate limit violation. Throws an assertion exception if any request succeeds or fails for a different reason.

### ShouldAllSucceedAsync

```csharp
public static async Task ShouldAllSucceedAsync(...)
```

Asserts that every request in a concurrent batch succeeds without hitting a rate limit. Accepts the service, API key, and request count. Internally executes the requests and validates that all results indicate success. Throws an assertion exception if any request is rate-limited or fails unexpectedly.

### RateLimitResult

```csharp
public record RateLimitResult(...)
```

Immutable record capturing the outcome of a single rate-limited request. Contains fields indicating whether the request succeeded, whether a rate limit was exceeded, any exception encountered, and the API key used. Designed for inspection and assertion in test code.

## Usage

**Example 1: Verify that exceeding the limit causes failures**

```csharp
var service = RateLimitingServiceTestsExtensions.CreateService(maxRequests: 5, windowSeconds: 60);
var apiKey = "test-key-123";

// First 5 requests should all succeed
await RateLimitingServiceTestsExtensions.ShouldAllSucceedAsync(service, apiKey, requestCount: 5);

// The 6th request onward should be rate-limited
await RateLimitingServiceTestsExtensions.ShouldAllThrowRateLimitExceededAsync(service, apiKey, requestCount: 1);
```

**Example 2: Inspect individual results from a mixed concurrent burst**

```csharp
var rateLimit = RateLimitingServiceTestsExtensions.CreateRateLimit(maxRequests: 3, windowSeconds: 10);
var service = RateLimitingServiceTestsExtensions.CreateService(rateLimit);

var results = await RateLimitingServiceTestsExtensions.ExecuteConcurrentRequestsAsync(
    service, apiKey: "burst-key", requestCount: 10);

int succeeded = results.Count(r => r.Succeeded);
int exceeded = results.Count(r => r.RateLimitExceeded);

Assert.Equal(3, succeeded);
Assert.Equal(7, exceeded);
```

## Notes

- `ExecuteConcurrentRequestsAsync` uses `ConcurrentBag<T>`, which is thread-safe for writes but does not preserve insertion order. Tests relying on ordering must sort or index results by a secondary field.
- `ShouldAllThrowRateLimitExceededAsync` and `ShouldAllSucceedAsync` are assertion helpers intended for xUnit or similar frameworks. They throw on first mismatch, so a single unexpected result fails the entire check.
- The factory methods `CreateService` and `CreateRateLimit` do not enforce that the window size or max request counts are positive; invalid values may cause downstream failures in the rate limiter itself rather than in the extension methods.
- No internal synchronization guards are provided across multiple calls to the same service instance from different tests running in parallel. Each test should use its own isolated service instance to avoid cross-test contamination.
