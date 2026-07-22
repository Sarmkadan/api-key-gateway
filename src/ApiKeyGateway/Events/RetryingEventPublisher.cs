// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace ApiKeyGateway.Events;

/// <summary>
/// A resilient event publisher decorator that wraps any inner <see cref="IEventPublisher"/>
/// with bounded exponential-backoff retry logic and dead-letter queue handling.
///
/// <para>
/// This implementation ensures that transient failures do not cause event loss by:
/// <list type="bullet">
/// <item><description>Retrying failed publishes with exponential backoff</description></item>
/// <item><description>Moving permanently failed events to a dead-letter queue</description></item>
/// <item><description>Swallowing exceptions to prevent them from bubbling up to the caller</description></item>
/// </list>
/// </para>
///
/// <para>
/// The publisher never throws exceptions into the request pipeline - all failures are logged
/// and either retried or moved to the dead-letter queue.
/// </para>
/// </summary>
/// <remarks>
/// This is a decorator that can wrap any <see cref="IEventPublisher"/> implementation,
/// including <see cref="InMemoryEventPublisher"/> or future distributed implementations.
/// </remarks>
public sealed class RetryingEventPublisher : IEventPublisher
{
    private readonly IEventPublisher _innerPublisher;
    private readonly IDeadLetterQueue _deadLetterQueue;
    private readonly EventPublisherOptions _options;
    private readonly ILogger<RetryingEventPublisher> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryingEventPublisher"/> class.
    /// </summary>
    /// <param name="innerPublisher">The inner event publisher to wrap with retry logic.</param>
    /// <param name="deadLetterQueue">The dead-letter queue for storing failed events.</param>
    /// <param name="options">Configuration options for retry and dead-letter behavior.</param>
    /// <param name="logger">Logger for recording retry attempts and failures.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public RetryingEventPublisher(
        IEventPublisher innerPublisher,
        IDeadLetterQueue deadLetterQueue,
        EventPublisherOptions options,
        ILogger<RetryingEventPublisher> logger)
    {
        _innerPublisher = innerPublisher ?? throw new ArgumentNullException(nameof(innerPublisher));
        _deadLetterQueue = deadLetterQueue ?? throw new ArgumentNullException(nameof(deadLetterQueue));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation(
            "RetryingEventPublisher initialized with {MaxRetryAttempts} max retries, " +
            "{InitialRetryDelayMs}ms initial delay, {MaxRetryDelayMs}ms max delay, " +
            "{MaxDeadLetterQueueSize} max dead-letter queue size",
            _options.MaxRetryAttempts,
            _options.InitialRetryDelayMs,
            _options.MaxRetryDelayMs,
            _options.MaxDeadLetterQueueSize);
    }

    /// <summary>
    /// Publishes an event to all registered subscribers with retry and dead-letter handling.
    ///
    /// <para>
    /// This method will:
    /// <list type="number">
    /// <item><description>Attempt to publish the event immediately</description></item>
    /// <item><description>If it fails, retry with exponential backoff up to MaxRetryAttempts times</description></item>
    /// <item><description>If all retries fail, move the event to the dead-letter queue</description></item>
    /// <item><description>Never throw exceptions - all failures are logged and handled internally</description></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <typeparam name="T">The event type.</typeparam>
    /// <param name="@event">The event to publish.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PublishAsync<T>(T @event) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(@event);

        var eventType = typeof(T).Name;
        var retryAttempts = 0;
        var lastException = (Exception?)null;

        _logger.LogDebug("Attempting to publish {EventType} (attempt 1)", eventType);

        // Try to publish with retry logic
        while (retryAttempts <= _options.MaxRetryAttempts)
        {
            try
            {
                await _innerPublisher.PublishAsync(@event);
                _logger.LogInformation("Successfully published {EventType} after {RetryAttempts} attempts",
                    eventType, retryAttempts);
                return; // Success - exit the retry loop
            }
            catch (Exception ex) when (retryAttempts < _options.MaxRetryAttempts)
            {
                lastException = ex;
                retryAttempts++;

                if (retryAttempts <= _options.MaxRetryAttempts)
                {
                    // Calculate delay with exponential backoff and jitter
                    var delayMs = CalculateRetryDelayMs(retryAttempts);
                    _logger.LogWarning(ex,
                        "Failed to publish {EventType} (attempt {RetryAttempt}/{MaxRetryAttempts}), " +
                        "retrying in {DelayMs}ms",
                        eventType, retryAttempts, _options.MaxRetryAttempts, delayMs);

                    await Task.Delay(delayMs);
                }
            }
            catch (Exception ex)
            {
                // This is the final attempt and it still failed
                lastException = ex;
                retryAttempts++;
                break;
            }
        }

        // All retry attempts exhausted - move to dead-letter queue
        HandlePublishFailure(@event, lastException!, retryAttempts);
    }

    private int CalculateRetryDelayMs(int retryAttempt)
    {
        // Exponential backoff with jitter: delay * 2^(attempt-1)
        // Plus a small random jitter to prevent thundering herd problems
        var exponentialDelay = _options.InitialRetryDelayMs * Math.Pow(2, retryAttempt - 1);
        var jitter = Random.Shared.Next(0, Math.Max(50, (int)exponentialDelay / 4));
        var delayMs = (int)exponentialDelay + jitter;

        // Cap at maximum delay
        return Math.Min(delayMs, _options.MaxRetryDelayMs);
    }

    private void HandlePublishFailure<T>(T @event, Exception failureReason, int retryAttempts) where T : notnull
    {
        _logger.LogError(failureReason,
            "All {MaxRetryAttempts} retry attempts exhausted for {EventType}. " +
            "Moving event to dead-letter queue",
            _options.MaxRetryAttempts,
            typeof(T).Name);

        // Create dead-letter entry
        var deadLetterEntry = _options.IncludeEventDetailsInDeadLetter
            ? DeadLetterEntry.Create(@event, failureReason, retryAttempts)
            : DeadLetterEntry.CreateMinimal<T>(failureReason, retryAttempts);

        // Add to dead-letter queue
        _deadLetterQueue.Add(deadLetterEntry);

        _logger.LogWarning(
            "Event {EventType} moved to dead-letter queue. " +
            "Queue size: {DeadLetterQueueSize}. " +
            "Failure reason: {FailureMessage}",
            typeof(T).Name,
            _deadLetterQueue.Count,
            failureReason.Message);
    }
}