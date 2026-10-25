# CoalescingMetrics

`src/ApiKeyGateway/Domain/Models/CoalescingMetrics.cs` is an immutable, point-in-time snapshot of the statistics kept by [RequestCoalescingService](IRequestCoalescingService.md). It is returned by `IRequestCoalescingService.GetMetrics()` and is intended for monitoring endpoints, logs, and diagnostics. It holds no reference to the service and does not change after it is created.

```csharp
namespace ApiKeyGateway.Domain.Models;

public sealed class CoalescingMetrics
{
    public long TotalRequests { get; init; }
    public long CoalescedRequests { get; init; }
    public int ActiveRequests { get; init; }
    public double CoalescingRatio { get; }   // computed, not settable
}
```

The class is `sealed` and every settable property uses an `init` accessor, so callers can construct it with an object initializer but cannot modify it afterwards.

## Properties

| Property | Type | Source | Meaning |
|----------|------|--------|---------|
| `TotalRequests` | `long` | Incremented by each `ExecuteAsync<T>` call | Number of `ExecuteAsync<T>` invocations since the service instance was created. |
| `CoalescedRequests` | `long` | Incremented when a call joins an existing in-flight request | Number of calls that attached to another caller's execution instead of running the operation themselves. Each one saves one upstream call. |
| `ActiveRequests` | `int` | Count of in-flight entries at the time of the snapshot | Number of coalescing groups (distinct request keys) currently executing or being awaited by followers. This counts keys, not callers. |
| `CoalescingRatio` | `double` | Computed: `CoalescedRequests / TotalRequests` | Fraction of all invocations served by coalescing. Returns `0.0` when `TotalRequests` is `0`. |

### TotalRequests

Counts every call to `ExecuteAsync<T>`, whether the call became a leader, a follower, or ran independently after a race (see the fallback path in `RequestCoalescingService`). It is a running total and is never reset while the singleton lives. Counts are kept with `Interlocked`, so they are safe to read from any thread.

### CoalescedRequests

Counts only follower calls. The leader of a group is never counted here, so for a group of `N` concurrent identical calls the typical contribution is `N - 1`.

### ActiveRequests

Reflects the number of pending entries in the service's internal dictionary. When a group finishes, its entry is removed, so this value falls back toward `0` when the gateway is idle. A high value during steady load means many distinct keys are in flight at once, not that many callers are waiting.

### CoalescingRatio

A derived value, so it cannot be set and does not need to be stored. Use it as a quick gauge of how much duplicated work the gateway is absorbing:

- `0.0` - no requests yet, or no concurrent duplicates have occurred.
- Closer to `1.0` - most calls are duplicates of work already in flight.

The ratio is a lifetime average since the service started, so it changes slowly. For a recent-window view, sample `TotalRequests` and `CoalescedRequests` periodically and compute the difference between samples.

## Consistency of snapshots

`GetMetrics()` reads `TotalRequests`, `CoalescedRequests`, and `ActiveRequests` separately. The values are not captured under a single lock, so a snapshot taken while requests are arriving can be slightly skewed relative to each other (for example, a coalesced increment may be visible before the matching total increment). The values are good enough for dashboards and trend lines; do not use them for exact accounting.

## Usage

### Reading metrics from the service

```csharp
public sealed class CoalescingStatusService
{
    private readonly IRequestCoalescingService _coalescing;

    public CoalescingStatusService(IRequestCoalescingService coalescing)
    {
        _coalescing = coalescing ?? throw new ArgumentNullException(nameof(coalescing));
    }

    public object GetStatus()
    {
        CoalescingMetrics metrics = _coalescing.GetMetrics();

        return new
        {
            metrics.TotalRequests,
            metrics.CoalescedRequests,
            metrics.ActiveRequests,
            CoalescingRatio = Math.Round(metrics.CoalescingRatio, 4)
        };
    }
}
```

### Logging a periodic snapshot

```csharp
CoalescingMetrics metrics = coalescingService.GetMetrics();

logger.LogInformation(
    "Coalescing: total={Total} coalesced={Coalesced} active={Active} ratio={Ratio:P1}",
    metrics.TotalRequests,
    metrics.CoalescedRequests,
    metrics.ActiveRequests,
    metrics.CoalescingRatio);
```

### Constructing an instance in tests

Because the setters are `init`, tests can build any state directly:

```csharp
var metrics = new CoalescingMetrics
{
    TotalRequests = 10,
    CoalescedRequests = 4,
    ActiveRequests = 2
};

metrics.CoalescingRatio.Should().Be(0.4);
```

`CoalescingRatio` is not validated against the other properties. Setting `CoalescedRequests` greater than `TotalRequests` produces a ratio above `1.0`. This cannot happen through the service, but it is possible when the class is built by hand.

## Related

- [IRequestCoalescingService](IRequestCoalescingService.md) - the service that produces these snapshots.
- [RequestCoalescingExtensions](RequestCoalescingExtensions.md) - DI registration of the service as a singleton.
