// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Handles webhook subscriptions and deliveries for domain events.
// When certain events occur (key created, quota exceeded), webhooks
// are delivered to configured endpoints. This enables real-time integrations.
// =============================================================================

using System.Net;
using System.Text.Json;
using ApiKeyGateway.Events;
using ApiKeyGateway.Utilities;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;

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

/// <summary>
/// Production webhook handler with retry logic and HMAC signing.
/// Uses exponential backoff for retries to avoid overwhelming target services.
/// </summary>
public sealed class WebhookHandler : IWebhookHandler
{
    private readonly ILogger<WebhookHandler> _logger;
    private readonly List<WebhookSubscription> _subscriptions = new();
    private readonly GatewayConfiguration _gatewayConfig;

    public WebhookHandler(ILogger<WebhookHandler> logger, GatewayConfiguration gatewayConfig)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gatewayConfig = gatewayConfig ?? throw new ArgumentNullException(nameof(gatewayConfig));
    }

    public Task<string> RegisterWebhookAsync(string url, string[] eventTypes, string? secret = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        if (eventTypes == null || eventTypes.Length == 0)
            throw new ArgumentException("At least one event type must be specified", nameof(eventTypes));

        var subscription = new WebhookSubscription
        {
            Id = Guid.NewGuid().ToString(),
            Url = url,
            EventTypes = eventTypes,
            Secret = secret,
            RegisteredAt = DateTime.UtcNow,
            IsActive = true
        };

        _subscriptions.Add(subscription);
        _logger.LogInformation(
            "Webhook registered: {Id} for events {Events} at {Url}",
            subscription.Id,
            string.Join(",", eventTypes),
            url);

        return Task.FromResult(subscription.Id);
    }

    public async Task DeliverWebhookAsync<T>(T @event) where T : ApiKeyEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var eventTypeName = typeof(T).Name;

        // Find all subscriptions interested in this event
        var targetSubscriptions = _subscriptions
            .Where(s => s.IsActive && s.EventTypes.Contains(eventTypeName))
            .ToList();

        if (!targetSubscriptions.Any())
        {
            _logger.LogDebug("No webhook subscriptions for event type {EventType}", eventTypeName);
            return;
        }

        var payload = JsonSerializationHelper.SerializeCompact(@event);
        var tasks = targetSubscriptions
            .Select(s => DeliverToEndpointAsync(s, payload, @event.EventId))
            .ToList();

        await Task.WhenAll(tasks);
    }

    private async Task DeliverToEndpointAsync(WebhookSubscription subscription, string payload, Guid eventId)
    {
        if (subscription == null)
            throw new ArgumentNullException(nameof(subscription));

        if (string.IsNullOrWhiteSpace(subscription.Url))
            throw new InvalidOperationException("Webhook subscription has no URL");

        using var client = HttpClientFactory.CreateWebhookClient();

        // Build retry policy using configurable settings from GatewayConfiguration
        var retryPolicy = new RetryPolicyBuilder()
            .WithMaxRetries(_gatewayConfig.WebhookMaxRetries)
            .WithInitialDelay(_gatewayConfig.WebhookBaseDelayMs)
            .WithBackoffMultiplier(_gatewayConfig.WebhookBackoffMultiplier)
            .WithMaxDelay(_gatewayConfig.WebhookMaxDelayMs)
            .RetryOn<HttpRequestException>()
            .RetryOn<TaskCanceledException>()
            .Build<HttpResponseMessage>();

        var request = new HttpRequestMessage(HttpMethod.Post, subscription.Url)
        {
            Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
        };

        // Add HMAC signature if secret configured
        if (!string.IsNullOrEmpty(subscription.Secret))
        {
            var signature = ComputeHmacSignature(payload, subscription.Secret);
            request.Headers.Add("X-Webhook-Signature", signature);
        }

        request.Headers.Add("X-Event-Id", eventId.ToString());
        request.Headers.Add("X-Delivery-Attempt", "1");

        var response = await retryPolicy(async () => await client.SendAsync(request));

        if (response.IsSuccessStatusCode)
        {
            subscription.TotalDeliveries++;
            subscription.LastDeliveryAt = DateTime.UtcNow;
            _logger.LogInformation(
                "Webhook delivered successfully: {SubscriptionId} to {Url}",
                subscription.Id,
                subscription.Url);
        }
        else
        {
            subscription.FailedDeliveries++;
            _logger.LogWarning(
                "Webhook delivery failed: {SubscriptionId} - Status {StatusCode}",
                subscription.Id,
                response.StatusCode);
            _logger.LogError(
                "Webhook delivery failed after {Attempts} attempts: {SubscriptionId}",
                _gatewayConfig.WebhookMaxRetries + 1,
                subscription.Id);
        }
    }

    private static string ComputeHmacSignature(string payload, string secret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
        return $"sha256={Convert.ToHexString(hash).ToLowerInvariant()}";
    }

    private record WebhookSubscription
    {
        public string Id { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string[] EventTypes { get; set; } = null!;
        public string? Secret { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastDeliveryAt { get; set; }
        public int TotalDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
    }
}
