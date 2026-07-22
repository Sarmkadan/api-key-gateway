# Usage Examples for Resilient Event Publishing

## Basic Usage

The resilient event publishing infrastructure is automatically configured when you call `AddEventPublishing()`. No additional code is needed in your application.

### Default Configuration (Automatic)

```csharp
// In your Program.cs or Startup.cs
builder.Services.AddEventPublishing(builder.Configuration);

// Later, when setting up the app
app.SubscribeEventHandlers();
```

That's it! Your application now has:
- Automatic retry on transient failures
- Dead-letter queue for failed events
- Comprehensive logging
- Never throws exceptions into the request pipeline

## Advanced Configuration

### Custom Configuration in appsettings.json

```json
{
  "EventPublishing": {
    "MaxRetryAttempts": 5,
    "InitialRetryDelayMs": 200,
    "MaxRetryDelayMs": 10000,
    "MaxDeadLetterQueueSize": 5000,
    "IncludeEventDetailsInDeadLetter": true
  }
}
```

### Programmatic Configuration

```csharp
// In your Program.cs
builder.Services.Configure<EventPublisherOptions>(options => {
    options.MaxRetryAttempts = 5;
    options.InitialRetryDelayMs = 200;
    options.MaxRetryDelayMs = 10000;
    options.MaxDeadLetterQueueSize = 5000;
    options.IncludeEventDetailsInDeadLetter = true;
});

builder.Services.AddEventPublishing(builder.Configuration);
```

## Accessing Dead-Letter Queue

You can access the dead-letter queue for monitoring and recovery:

```csharp
// In a controller or background service
public class MonitoringController : ControllerBase
{
    private readonly IDeadLetterQueue _deadLetterQueue;

    public MonitoringController(IDeadLetterQueue deadLetterQueue)
    {
        _deadLetterQueue = deadLetterQueue;
    }

    [HttpGet("dead-letter/entries")]
    public IActionResult GetDeadLetterEntries()
    {
        var entries = _deadLetterQueue.GetAll();
        return Ok(new {
            Count = entries.Count,
            Entries = entries.Select(e => new {
                e.Id,
                e.FailedAt,
                e.EventType,
                e.FailureReason,
                e.RetryAttempts
            })
        });
    }

    [HttpPost("dead-letter/clear")]
    public IActionResult ClearDeadLetterQueue()
    {
        _deadLetterQueue.Clear();
        return Ok("Dead-letter queue cleared");
    }

    [HttpGet("dead-letter/count")]
    public IActionResult GetDeadLetterCount()
    {
        return Ok(new { Count = _deadLetterQueue.Count });
    }
}
```

## Monitoring and Recovery

### Background Service for Dead-Letter Processing

```csharp
public class DeadLetterProcessor : BackgroundService
{
    private readonly IDeadLetterQueue _deadLetterQueue;
    private readonly ILogger<DeadLetterProcessor> _logger;

    public DeadLetterProcessor(
        IDeadLetterQueue deadLetterQueue,
        ILogger<DeadLetterProcessor> logger)
    {
        _deadLetterQueue = deadLetterQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Process failed events
                var entry = _deadLetterQueue.Take();
                if (entry != null)
                {
                    await ProcessFailedEvent(entry);
                    _logger.LogInformation("Processed dead-letter entry: {EventType}", entry.EventType);
                }
                else
                {
                    // Queue is empty, wait a bit
                    await Task.Delay(5000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing dead-letter entry");
                await Task.Delay(10000, stoppingToken); // Wait longer on error
            }
        }
    }

    private async Task ProcessFailedEvent(DeadLetterEntry entry)
    {
        // Implement your recovery logic here
        // For example: retry with different parameters, notify admin, etc.
        
        switch (entry.EventType)
        {
            case var _ when entry.EventType.Contains("ApiKeyCreatedEvent"):
                // Handle API key creation failure
                break;
                
            case var _ when entry.EventType.Contains("RateLimitExceededEvent"):
                // Handle rate limit event failure
                break;
                
            // Add more event types as needed
        }
    }
}

// Register in DI
builder.Services.AddHostedService<DeadLetterProcessor>();
```

### Health Check Integration

```csharp
public class EventPublishingHealthCheck : IHealthCheck
{
    private readonly IDeadLetterQueue _deadLetterQueue;

    public EventPublishingHealthCheck(IDeadLetterQueue deadLetterQueue)
    {
        _deadLetterQueue = deadLetterQueue;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_deadLetterQueue.Count > 100) // Threshold
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Dead-letter queue has {_deadLetterQueue.Count} entries (threshold: 100)"));
        }

        return Task.FromResult(HealthCheckResult.Healthy());
    }
}

// Register in DI
builder.Services.AddHealthChecks()
    .AddCheck<EventPublishingHealthCheck>("event_publishing");
```

## Testing Your Event Handlers

```csharp
public class EventPublisherTests
{
    [Fact]
    public async Task TestEventHandler_WithRetryingPublisher()
    {
        // Arrange
        var innerPublisher = new InMemoryEventPublisher(logger);
        var deadLetterQueue = new InMemoryDeadLetterQueue(100);
        var options = new EventPublisherOptions { MaxRetryAttempts = 3 };
        var retryingPublisher = new RetryingEventPublisher(
            innerPublisher, deadLetterQueue, options, logger);

        var handler = new MyEventHandler();
        retryingPublisher.Subscribe<MyEvent>(handler.HandleAsync);

        // Act
        await retryingPublisher.PublishAsync(new MyEvent());

        // Assert
        Assert.True(handler.WasCalled);
    }
}
```

## Migration from Old Code

If you were previously using `InMemoryEventPublisher` directly, no changes are needed! The `RetryingEventPublisher` is automatically registered as the default implementation.


### Before (still works!)
```csharp
// This still works exactly as before
services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();
```

### After (automatic)
```csharp
// Just call AddEventPublishing and everything is configured
services.AddEventPublishing(configuration);
```

## Best Practices

### 1. Monitor Dead-Letter Queue Size
Always monitor the dead-letter queue size in production. A growing queue indicates persistent failures that need attention.

### 2. Configure Appropriate Retry Delays
- Start with conservative delays (100-500ms)
- Increase based on your system's characteristics
- Cap maximum delay to prevent unbounded growth

### 3. Include Event Details in Dead-Letter (Default)
Keep `IncludeEventDetailsInDeadLetter = true` during development for debugging.
Set to `false` in production if events contain sensitive data.

### 4. Process Dead-Letter Entries
Set up a background service to process failed events. Don't let them accumulate indefinitely.

### 5. Log Retry Attempts
The `RetryingEventPublisher` automatically logs all retry attempts. Monitor these logs for patterns in failures.

### 6. Test Failure Scenarios
Write tests that simulate failures to ensure your handlers are resilient.

## Troubleshooting

### Issue: Events not being published
**Check:**
- Is `AddEventPublishing()` called during startup?
- Are event handlers subscribed via `SubscribeEventHandlers()`?
- Check logs for retry attempts or dead-letter entries

### Issue: Dead-letter queue growing too large
**Solutions:**
- Increase `MaxDeadLetterQueueSize` if needed
- Set up background processing to handle failed events
- Investigate why events are failing
- Check application logs for error patterns

### Issue: Too many retry attempts
**Solutions:**
- Increase `MaxRetryAttempts` if your system is temporarily overloaded
- Increase `MaxRetryDelayMs` to give systems more time to recover
- Check if downstream systems are healthy

### Issue: Performance degradation
**Solutions:**
- Review retry delays - too many retries too quickly can hurt performance
- Consider reducing `MaxRetryAttempts` if failures are permanent
- Check if dead-letter queue operations are blocking

## Summary

The resilient event publishing infrastructure is designed to work automatically with minimal configuration. Just call `AddEventPublishing()` and you get:
- ✅ Automatic retry on failures
- ✅ Dead-letter queue for failed events
- ✅ Comprehensive logging
- ✅ Never throws exceptions into request pipeline
- ✅ Easy monitoring and recovery
- ✅ Backward compatible with existing code

Set up monitoring for the dead-letter queue and you're done!
