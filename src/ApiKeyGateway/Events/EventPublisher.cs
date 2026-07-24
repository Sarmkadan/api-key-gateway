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
    private readonly ReaderWriterLockSlim _subscribersLock = new();
    private readonly ILogger<InMemoryEventPublisher> _logger;

    public InMemoryEventPublisher(ILogger<InMemoryEventPublisher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a handler for a specific event type.
    /// Handlers are called in registration order.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when handler is null.</exception>
    public void Subscribe<T>(Func<T, Task> handler) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(T);
        _subscribersLock.EnterUpgradeableReadLock();
        try
        {
            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribersLock.EnterWriteLock();
                try
                {
                    _subscribers[eventType] = new();
                }
                finally
                {
                    _subscribersLock.ExitWriteLock();
                }
            }

            _subscribersLock.EnterWriteLock();
            try
            {
                _subscribers[eventType].Add(handler);
            }
            finally
            {
                _subscribersLock.ExitWriteLock();
            }

            _logger.LogInformation("Event subscriber registered for {EventType}", eventType.Name);
        }
        finally
        {
            _subscribersLock.ExitUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Publishes event to all registered subscribers.
    /// If a subscriber throws, we log and continue (fail-open pattern).
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when event is null.</exception>
    public async Task PublishAsync<T>(T @event) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(@event);

        var eventType = typeof(T);

        var handlers = Array.Empty<Func<T, Task>>();
        _subscribersLock.EnterReadLock();
        try
        {
            if (_subscribers.TryGetValue(eventType, out var handlerList))
            {
                // Take a snapshot of handlers to avoid holding lock during async handler execution
                // and to prevent issues if subscribers are modified during iteration
                handlers = handlerList.Cast<Func<T, Task>>().ToArray();
                _logger.LogInformation(
                    "Publishing {EventType} to {SubscriberCount} subscribers",
                    eventType.Name,
                    handlers.Length);
            }
            else
            {
                _logger.LogDebug("No subscribers for event type {EventType}", eventType.Name);
            }
        }
        finally
        {
            _subscribersLock.ExitReadLock();
        }

        // Call all handlers in sequence outside the lock to avoid holding it during async operations
        // error in one doesn't stop others. Invoke through the typed delegate rather than DynamicInvoke
        // so handler exceptions surface directly instead of wrapped in TargetInvocationException.
        foreach (var handler in handlers)
        {
            try
            {
                if (handler is Func<T, Task> typedHandler)
                {
                    await typedHandler(@event);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in event handler for {EventType}", eventType.Name);
                // Continue to next handler despite error
            }
        }
    }

    /// <summary>
    /// Unregisters a handler for a specific event type.
    /// If the handler was not registered, this method does nothing.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe<T>(Func<T, Task> handler) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(T);
        _subscribersLock.EnterUpgradeableReadLock();
        try
        {
            if (!_subscribers.ContainsKey(eventType))
            {
                return;
            }

            _subscribersLock.EnterWriteLock();
            try
            {
                var handlers = _subscribers[eventType];
                handlers.Remove(handler);
                _logger.LogInformation("Event subscriber unregistered for {EventType}", eventType.Name);

                // Clean up empty lists to prevent memory leaks
                if (handlers.Count == 0)
                {
                    _subscribers.Remove(eventType);
                }
            }
            finally
            {
                _subscribersLock.ExitWriteLock();
            }
        }
        finally
        {
            _subscribersLock.ExitUpgradeableReadLock();
        }
    }
}
