// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Events;

/// <summary>
/// Interface for publishing domain events across the application.
/// Implementations handle different event routing strategies
/// (in-memory, message queue, distributed event bus).
/// This abstraction allows swapping implementations without changing publishers.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to all registered subscribers.
    /// Subscribers are notified synchronously (ordered by registration).
    /// </summary>
    Task PublishAsync<T>(T @event) where T : notnull;
}

/// <summary>
/// In-memory event publisher using a simple subscriber list.
/// Suitable for monolithic deployments. For distributed systems,
/// consider replacing with message queue implementation.
/// </summary>
public class InMemoryEventPublisher : IEventPublisher
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private readonly ILogger<InMemoryEventPublisher> _logger;

    public InMemoryEventPublisher(ILogger<InMemoryEventPublisher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a handler for a specific event type.
    /// Handlers are called in registration order.
    /// </summary>
    public void Subscribe<T>(Func<T, Task> handler) where T : notnull
    {
        var eventType = typeof(T);
        if (!_subscribers.ContainsKey(eventType))
        {
            _subscribers[eventType] = new();
        }

        _subscribers[eventType].Add(handler);
        _logger.LogInformation("Event subscriber registered for {EventType}", eventType.Name);
    }

    /// <summary>
    /// Publishes event to all registered subscribers.
    /// If a subscriber throws, we log and continue (fail-open pattern).
    /// </summary>
    public async Task PublishAsync<T>(T @event) where T : notnull
    {
        var eventType = typeof(T);

        if (!_subscribers.ContainsKey(eventType))
        {
            _logger.LogDebug("No subscribers for event type {EventType}", eventType.Name);
            return;
        }

        var handlers = _subscribers[eventType];
        _logger.LogInformation(
            "Publishing {EventType} to {SubscriberCount} subscribers",
            eventType.Name,
            handlers.Count);

        // Call all handlers in sequence, error in one doesn't stop others
        foreach (var handler in handlers)
        {
            try
            {
                if (handler is Delegate del)
                {
                    await (Task)del.DynamicInvoke(@event)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in event handler for {EventType}", eventType.Name);
                // Continue to next handler despite error
            }
        }
    }
}
