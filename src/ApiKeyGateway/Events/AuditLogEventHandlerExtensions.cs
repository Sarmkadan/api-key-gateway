// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Provides extension methods for <see cref="AuditLogEventHandler"/> that enable
// bulk operations and common audit logging patterns.
// =====================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiKeyGateway.Events;

/// <summary>
/// Extension methods for <see cref="AuditLogEventHandler"/> that provide bulk operations
/// and integration helpers for audit logging scenarios.
/// </summary>
/// <remarks>
/// These extensions simplify common scenarios like:
/// <list type="bullet">
/// <item>Handling multiple events in bulk operations</item>
/// <item>Creating unified event delegates for event processing pipelines</item>
/// </list>
/// </remarks>
public static class AuditLogEventHandlerExtensions
{
    /// <summary>
    /// Asynchronously handles multiple <see cref="ApiKeyCreatedEvent"/> instances.
    /// </summary>
    /// <param name="handler">The audit log handler.</param>
    /// <param name="events">The events to handle.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> or <paramref name="events"/> is <c>null</c>.</exception>
    public static async Task HandleApiKeyCreatedAsync(
        this AuditLogEventHandler handler,
        IEnumerable<ApiKeyCreatedEvent> events)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(events);

        foreach (var @event in events)
        {
            await handler.HandleApiKeyCreatedAsync(@event).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously handles multiple <see cref="ApiKeyRotatedEvent"/> instances.
    /// </summary>
    /// <param name="handler">The audit log handler.</param>
    /// <param name="events">The events to handle.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> or <paramref name="events"/> is <c>null</c>.</exception>
    public static async Task HandleApiKeyRotatedAsync(
        this AuditLogEventHandler handler,
        IEnumerable<ApiKeyRotatedEvent> events)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(events);

        foreach (var @event in events)
        {
            await handler.HandleApiKeyRotatedAsync(@event).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously handles multiple <see cref="ApiKeyDisabledEvent"/> instances.
    /// </summary>
    /// <param name="handler">The audit log handler.</param>
    /// <param name="events">The events to handle.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> or <paramref name="events"/> is <c>null</c>.</exception>
    public static async Task HandleApiKeyDisabledAsync(
        this AuditLogEventHandler handler,
        IEnumerable<ApiKeyDisabledEvent> events)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(events);

        foreach (var @event in events)
        {
            await handler.HandleApiKeyDisabledAsync(@event).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Creates a delegate that can handle multiple event types in a single method.
    /// Useful for integrating with event processing pipelines and middleware.
    /// </summary>
    /// <param name="handler">The audit log handler.</param>
    /// <returns>A delegate that handles all three main audit event types.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <c>null</c>.</exception>
    public static Func<object, Task> CreateEventDelegate(this AuditLogEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return async @event =>
        {
            switch (@event)
            {
                case ApiKeyCreatedEvent created:
                    await handler.HandleApiKeyCreatedAsync(created).ConfigureAwait(false);
                    break;

                case ApiKeyRotatedEvent rotated:
                    await handler.HandleApiKeyRotatedAsync(rotated).ConfigureAwait(false);
                    break;

                case ApiKeyDisabledEvent disabled:
                    await handler.HandleApiKeyDisabledAsync(disabled).ConfigureAwait(false);
                    break;

                default:
                    // Ignore unsupported event types
                    break;
            }
        };
    }
}