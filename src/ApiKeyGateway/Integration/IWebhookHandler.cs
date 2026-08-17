// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Handles webhook subscriptions and deliveries for domain events.
// When certain events occur (key created, quota exceeded), webhooks
// are delivered to configured endpoints. This enables real-time integrations.
// =============================================================================

using ApiKeyGateway.Events;

namespace ApiKeyGateway.Integration;

/// <summary>
/// Handles webhook subscriptions and deliveries for domain events.
/// When certain events occur (key created, quota exceeded), webhooks
/// are delivered to configured endpoints. This enables real-time integrations.
/// </summary>
public interface IWebhookHandler
{
    /// <summary>
    /// Registers a webhook endpoint to receive notifications.
    /// </summary>
    Task<string> RegisterWebhookAsync(string url, string[] eventTypes, string? secret = null);

    /// <summary>
    /// Delivers an event to registered webhook endpoints.
    /// Handles retries, timeouts, and signature verification.
    /// </summary>
    Task DeliverWebhookAsync<T>(T @event) where T : ApiKeyEvent;
}
