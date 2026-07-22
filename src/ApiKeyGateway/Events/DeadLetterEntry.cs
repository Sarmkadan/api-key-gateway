// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Events;

/// <summary>
/// Represents a failed event that has been moved to the dead-letter queue.
/// Contains metadata about the failure and optionally the original event.
/// </summary>
public sealed class DeadLetterEntry
{
    /// <summary>
    /// Unique identifier for this dead-letter entry.
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when the event failed and was moved to dead-letter queue.
    /// </summary>
    public DateTimeOffset FailedAt { get; } = DateTimeOffset.UtcNow;


    /// <summary>
    /// Type name of the event that failed.
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// JSON representation of the event payload.
    /// </summary>
    public string? EventPayload { get; init; }

    /// <summary>
    /// Exception message describing the failure reason.
    /// </summary>
    public string FailureReason { get; init; } = string.Empty;

    /// <summary>
    /// Exception stack trace if available.
    /// </summary>
    public string? StackTrace { get; init; }

    /// <summary>
    /// Number of retry attempts that were made before failure.
    /// </summary>
    public int RetryAttempts { get; init; }

    /// <summary>
    /// Creates a dead-letter entry from a failed event publishing attempt.
    /// </summary>
    /// <param name="eventType">Type of the event that failed.</param>
    /// <param name="eventPayload">The event payload that failed to publish.</param>
    /// <param name="failureReason">Exception message describing the failure.</param>
    /// <param name="stackTrace">Exception stack trace.</param>
    /// <param name="retryAttempts">Number of retry attempts made.</param>
    /// <param name="includeEventDetails">Whether to include event details in the entry.</param>
    public static DeadLetterEntry Create<T>(
        T @event,
        Exception failureReason,
        int retryAttempts,
        bool includeEventDetails = true) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(failureReason);

        var eventType = typeof(T).FullName ?? typeof(T).Name;

        if (includeEventDetails)
        {
            var payload = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            return new DeadLetterEntry
            {
                EventType = eventType,
                EventPayload = payload,
                FailureReason = failureReason.Message,
                StackTrace = failureReason.StackTrace,
                RetryAttempts = retryAttempts
            };
        }
        else
        {
            return new DeadLetterEntry
            {
                EventType = eventType,
                EventPayload = null,
                FailureReason = failureReason.Message,
                StackTrace = failureReason.StackTrace,
                RetryAttempts = retryAttempts
            };
        }
    }

    /// <summary>
    /// Creates a minimal dead-letter entry with only metadata (no event payload).
    /// </summary>
    public static DeadLetterEntry CreateMinimal<T>(
        Exception failureReason,
        int retryAttempts) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(failureReason);

        var eventType = typeof(T).FullName ?? typeof(T).Name;

        return new DeadLetterEntry
        {
            EventType = eventType,
            EventPayload = null,
            FailureReason = failureReason.Message,
            StackTrace = failureReason.StackTrace,
            RetryAttempts = retryAttempts
        };
    }
}