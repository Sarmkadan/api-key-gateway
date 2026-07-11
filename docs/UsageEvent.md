# UsageEvent

The `UsageEvent` type captures a single usage event recorded by the API key gateway. It combines details of an individual API request (endpoint, status code, response time, size) with the current state of the API key’s rate‑limit window (current usage, limit, time until reset, percentage used, and the exact reset time). These events are used for auditing, real‑time monitoring, and enforcing rate limits.

## API

| Member | Type | Description |
|--------|------|-------------|
| `EventId` | `Guid` | A unique identifier for this usage event. |
| `Timestamp` | `DateTime` | The date and time (UTC) when the request was processed. |
| `ApiKeyId` | `string` | The API key that was used to make the request. |
| `Endpoint` | `string` | The requested endpoint path (e.g., `/api/v1/resource`). |
| `HttpStatusCode` | `int` | The HTTP status code returned to the client. |
| `ResponseTimeMs` | `long` | The server‑side response time in milliseconds. |
| `ResponseSizeBytes` | `long` | The size of the response body in bytes. |
| `CurrentUsage` | `int` | The number of requests already counted in the current rate‑limit window for this API key. |
| `Limit` | `int` | The maximum number of requests allowed in the current rate‑limit window. |
| `SecondsUntilReset` | `int` | The number of seconds remaining until the current usage window resets. |
| `PercentageUsed` | `int` | The percentage of the limit that has been consumed (`CurrentUsage / Limit * 100`). |
| `WindowResetTime` | `DateTime` | The exact date and time (UTC) when the current usage window will reset. |

All members are public fields. No methods or constructors are exposed; instances are typically created by the gateway’s internal logic and made available for inspection or serialization.

## Usage

### Example 1: Logging a usage event

```csharp
var usageEvent = new UsageEvent
{
    EventId = Guid.NewGuid(),
    Timestamp = DateTime.UtcNow,
    ApiKeyId = "key-abc123",
    Endpoint = "/api/v1/users",
    HttpStatusCode = 200,
    ResponseTimeMs = 45,
    ResponseSizeBytes = 2048,
    CurrentUsage = 87,
    Limit = 100,
    SecondsUntilReset = 120,
    PercentageUsed = 87,
    WindowResetTime = DateTime.UtcNow.AddSeconds(120)
};

// Serialize to JSON for storage or transmission
string json = JsonSerializer.Serialize(usageEvent);
logger.LogInformation("Usage event recorded: {Event}", json);
```

### Example 2: Checking rate‑limit status before processing a request

```csharp
public bool IsRateLimited(UsageEvent lastEvent)
{
    // If the window has already reset, usage is no longer relevant
    if (lastEvent.WindowResetTime <= DateTime.UtcNow)
        return false;

    // If current usage exceeds the limit, the request should be rejected
    return lastEvent.CurrentUsage >= lastEvent.Limit;
}
```

## Notes

- **Division by zero**: When `Limit` is `0`, the `PercentageUsed` field is undefined. Consumers should check for `Limit == 0` before using `PercentageUsed` or computing derived values.
- **Thread safety**: `UsageEvent` is a plain data object with no built‑in synchronization. If instances are shared across threads (e.g., in a concurrent cache or event pipeline), external locking or immutable copies should be used to avoid race conditions.
- **Time precision**: All `DateTime` values are expected to be in UTC. The `Timestamp` and `WindowResetTime` should be compared using `DateTime.UtcNow` to avoid time‑zone discrepancies.
- **Duplicate members**: The public surface includes multiple fields named `CurrentUsage` and `Limit` (as listed in the type definition). They represent the same underlying values; only the first occurrence of each is documented above.
