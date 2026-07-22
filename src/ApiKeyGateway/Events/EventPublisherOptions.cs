// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace ApiKeyGateway.Events;

/// <summary>
/// Configuration options for event publishing behavior including retry policies
/// and dead-letter queue settings.
/// </summary>
public sealed class EventPublisherOptions
{
    /// <summary>
    /// Maximum number of retry attempts when publishing an event fails.
    /// Default: 3 attempts (1 initial + 3 retries = 4 total attempts)
    /// </summary>
    [Range(0, 10, ErrorMessage = "MaxRetryAttempts must be between 0 and 10")]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay in milliseconds for the first retry attempt.
    /// Subsequent retries use exponential backoff: delay * 2^(attempt-1)
    /// Default: 100ms
    /// </summary>
    [Range(1, 10000, ErrorMessage = "InitialRetryDelayMs must be between 1 and 10000")]
    public int InitialRetryDelayMs { get; set; } = 100;

    /// <summary>
    /// Maximum delay in milliseconds between retry attempts.
    /// Prevents unbounded exponential growth.
    /// Default: 5000ms (5 seconds)
    /// </summary>
    [Range(1, 30000, ErrorMessage = "MaxRetryDelayMs must be between 1 and 30000")]
    public int MaxRetryDelayMs { get; set; } = 5000;

    /// <summary>
    /// Maximum number of events to keep in the dead-letter queue.
    /// When exceeded, oldest events are discarded (FIFO).
    /// Default: 1000 events
    /// </summary>
    [Range(0, 10000, ErrorMessage = "MaxDeadLetterQueueSize must be between 0 and 10000")]
    public int MaxDeadLetterQueueSize { get; set; } = 1000;

    /// <summary>
    /// Whether to include event details in dead-letter queue entries.
    /// When false, only metadata (event type, timestamp, failure reason) is stored.
    /// Default: true
    /// </summary>
    public bool IncludeEventDetailsInDeadLetter { get; set; } = true;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <exception cref="ValidationException">Thrown when configuration is invalid.</exception>
    public void Validate()
    {
        if (MaxRetryAttempts < 0)
        {
            throw new ValidationException($"{nameof(MaxRetryAttempts)} must be non-negative");
        }

        if (InitialRetryDelayMs < 1)
        {
            throw new ValidationException($"{nameof(InitialRetryDelayMs)} must be positive");
        }

        if (MaxRetryDelayMs < 1)
        {
            throw new ValidationException($"{nameof(MaxRetryDelayMs)} must be positive");
        }

        if (MaxDeadLetterQueueSize < 0)
        {
            throw new ValidationException($"{nameof(MaxDeadLetterQueueSize)} must be non-negative");
        }
    }
}