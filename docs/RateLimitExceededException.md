# RateLimitExceededException

The `RateLimitExceededException` is thrown by the API key gateway when a request exceeds the configured rate limit for a given API key. It carries details about the limit that was breached, the time window in which the limit applies, and an optional `RetryAfter` timestamp indicating when the client may retry the request. This exception is typically caught by middleware or client code to implement back-off logic or to return appropriate HTTP 429 responses.

## API

### `public string ApiKeyId`

Gets the identifier of the API key that exceeded the rate limit.

- **Purpose**: Identifies which API key triggered the limit violation.
- **Return value**: A non-null string representing the API key ID.
- **Throws**: Never throws.

### `public int Limit`

Gets the maximum number of allowed requests within the time window.

- **Purpose**: Provides the limit that was exceeded (e.g., 100 requests).
- **Return value**: An integer greater than zero.
- **Throws**: Never throws.

### `public int WindowInSeconds`

Gets the duration of the rate limit window in seconds.

- **Purpose**: Indicates the time window over which the limit is measured (e.g., 60 seconds).
- **Return value**: An integer greater than zero.
- **Throws**: Never throws.

### `public DateTime? RetryAfter`

Gets the optional date and time after which the client may retry the request.

- **Purpose**: When set, provides a UTC timestamp indicating when the rate limit is expected to reset. A `null` value means no specific retry time is available.
- **Return value**: A `DateTime?` (nullable).
- **Throws**: Never throws.

### `public RateLimitExceededException()`

Initializes a new instance of the `RateLimitExceededException` class.

- **Purpose**: Creates an exception instance. Properties such as `ApiKeyId`, `Limit`, `WindowInSeconds`, and `RetryAfter` can be set via object initializers after construction.
- **Parameters**: None.
- **Return value**: A new `RateLimitExceededException` instance.
- **Throws**: Does not throw.

## Usage

### Example 1: Throwing the exception in a rate limiter

```csharp
public void CheckRateLimit(string apiKeyId, int requestCount, int limit, int windowSeconds)
{
    if (requestCount > limit)
    {
        throw new RateLimitExceededException
        {
            ApiKeyId = apiKeyId,
            Limit = limit,
            WindowInSeconds = windowSeconds,
            RetryAfter = DateTime.UtcNow.AddSeconds(windowSeconds)
        };
    }
}
```

### Example 2: Catching and handling the exception in middleware

```csharp
public async Task InvokeAsync(HttpContext context, RequestDelegate next)
{
    try
    {
        await next(context);
    }
    catch (RateLimitExceededException ex)
    {
        context.Response.StatusCode = 429;
        context.Response.Headers["Retry-After"] = ex.RetryAfter?.ToString("R") ?? "60";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Rate limit exceeded",
            apiKeyId = ex.ApiKeyId,
            limit = ex.Limit,
            windowSeconds = ex.WindowInSeconds,
            retryAfter = ex.RetryAfter
        });
    }
}
```

## Notes

- **Edge case – null RetryAfter**: When `RetryAfter` is `null`, consumers should fall back to a default retry interval (e.g., the full window duration) or use the `WindowInSeconds` value to compute a reasonable back-off.
- **Edge case – zero or negative values**: Although the properties are typed as `int`, the exception is typically constructed with positive values. Code that inspects these properties should guard against unexpected non-positive values if the exception is manually constructed with invalid data.
- **Thread safety**: The exception instance is immutable after construction (properties are read-only). Reading its properties from multiple threads concurrently is safe. The constructor itself is not thread-safe, but that is irrelevant because instances are created on a single thread before being thrown.
- **Inheritance**: `RateLimitExceededException` inherits from `System.Exception`. Inherited members (e.g., `Message`, `InnerException`, `StackTrace`) are not documented here but are available for standard exception handling patterns.
