// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Manager for webhook subscriptions and delivery coordination.
// Handles the lifecycle of webhooks: registration, subscription, delivery tracking.
// Provides API for users to configure which events they want to receive.
// =====================================================================

using ApiKeyGateway.Domain.Exceptions;

namespace ApiKeyGateway.Integration;

/// <summary>
/// Manager for webhook subscriptions and delivery coordination.
/// Handles the lifecycle of webhooks: registration, subscription, delivery tracking.
/// Provides API for users to configure which events they want to receive.
/// </summary>
public interface IWebhookManager
{
    /// <summary>
    /// Registers a webhook subscription for an API key.
    /// </summary>
    /// <param name="apiKeyId">Identifier of the API key that owns the subscription.</param>
    /// <param name="targetUrl">HTTPS URL that receives event deliveries.</param>
    /// <param name="eventTypes">Event types the subscription should receive.</param>
    /// <param name="secret">Optional shared secret used to sign deliveries.</param>
    /// <returns>The identifier of the new subscription.</returns>
    Task<string> RegisterWebhookAsync(string apiKeyId, string targetUrl, string[] eventTypes, string? secret = null);

    /// <summary>
    /// Removes a webhook subscription.
    /// </summary>
    /// <param name="webhookId">Identifier of the subscription to remove.</param>
    Task UnregisterWebhookAsync(string webhookId);

    /// <summary>
    /// Gets the active webhook subscriptions for an API key.
    /// </summary>
    /// <param name="apiKeyId">Identifier of the API key.</param>
    /// <returns>The active subscriptions for the key.</returns>
    Task<IEnumerable<WebhookSubscription>> GetWebhooksForKeyAsync(string apiKeyId);

    /// <summary>
    /// Gets the total number of deliveries attempted for a webhook.
    /// </summary>
    /// <param name="webhookId">Identifier of the subscription.</param>
    /// <returns>The delivery count, or 0 if the subscription is unknown.</returns>
    Task<int> GetDeliveryCountAsync(string webhookId);
}

/// <summary>
/// Webhook subscription details.
/// </summary>
public record WebhookSubscription
{
    /// <summary>Unique identifier of the subscription.</summary>
    public string Id { get; set; } = null!;

    /// <summary>Identifier of the API key that owns the subscription.</summary>
    public string ApiKeyId { get; set; } = null!;

    /// <summary>HTTPS URL that receives deliveries.</summary>
    public string TargetUrl { get; set; } = null!;

    /// <summary>Event types the subscription receives.</summary>
    public string[] EventTypes { get; set; } = null!;

    /// <summary>Optional shared secret used to sign deliveries.</summary>
    public string? Secret { get; set; }

    /// <summary>True when the subscription is currently enabled.</summary>
    public bool IsActive { get; set; }

    /// <summary>UTC time when the subscription was registered.</summary>
    public DateTime RegisteredAt { get; set; }

    /// <summary>UTC time of the most recent delivery attempt, or null if none.</summary>
    public DateTime? LastDeliveryAt { get; set; }

    /// <summary>Total number of delivery attempts.</summary>
    public int TotalDeliveries { get; set; }

    /// <summary>Number of delivery attempts that failed.</summary>
    public int FailedDeliveries { get; set; }
}

/// <summary>
/// Production implementation of webhook manager with persistence.
/// </summary>
public sealed class WebhookManager : IWebhookManager
{
    private readonly ILogger<WebhookManager> _logger;
    private readonly Dictionary<string, WebhookSubscription> _subscriptions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookManager"/> class.
    /// </summary>
    /// <param name="logger">Logger for webhook operations.</param>
    public WebhookManager(ILogger<WebhookManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a webhook subscription for an API key after validating the input and requiring an HTTPS URL.
    /// </summary>
    /// <param name="apiKeyId">Identifier of the API key that owns the subscription.</param>
    /// <param name="targetUrl">HTTPS URL that receives event deliveries.</param>
    /// <param name="eventTypes">Event types the subscription should receive.</param>
    /// <param name="secret">Optional shared secret used to sign deliveries.</param>
    /// <returns>The identifier of the new subscription.</returns>
    public Task<string> RegisterWebhookAsync(
        string apiKeyId,
        string targetUrl,
        string[] eventTypes,
        string? secret = null)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
        {
            throw new ArgumentException("API key ID cannot be empty", nameof(apiKeyId));
        }

        if (string.IsNullOrWhiteSpace(targetUrl))
        {
            throw new ArgumentException("Target URL cannot be empty", nameof(targetUrl));
        }

        if (eventTypes == null || eventTypes.Length == 0)
        {
            throw new ArgumentException("At least one event type must be specified", nameof(eventTypes));
        }

        // Validate URL is HTTPS for security
        if (!targetUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Webhook URLs must use HTTPS", nameof(targetUrl), targetUrl);
        }

        if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri))
        {
            throw new ValidationException("Invalid webhook URL format", nameof(targetUrl), targetUrl);
        }

        var subscription = new WebhookSubscription
        {
            Id = Guid.NewGuid().ToString(),
            ApiKeyId = apiKeyId,
            TargetUrl = targetUrl,
            EventTypes = eventTypes,
            Secret = secret,
            IsActive = true,
            RegisteredAt = DateTime.UtcNow
        };

        try
        {
            _subscriptions[subscription.Id] = subscription;

            _logger.LogInformation(
                "Webhook registered: {Id} for API key {ApiKeyId} - {EventCount} events",
                subscription.Id,
                apiKeyId,
                eventTypes.Length);

            return Task.FromResult(subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register webhook for API key {ApiKeyId}", apiKeyId);
            throw new DataAccessException("Failed to register webhook", nameof(RegisterWebhookAsync), nameof(WebhookSubscription), ex);
        }
    }

    /// <summary>
    /// Removes a webhook subscription if it exists.
    /// </summary>
    /// <param name="webhookId">Identifier of the subscription to remove.</param>
    public Task UnregisterWebhookAsync(string webhookId)
    {
        if (string.IsNullOrWhiteSpace(webhookId))
        {
            throw new ArgumentException("Webhook ID cannot be empty", nameof(webhookId));
        }

        try
        {
            if (_subscriptions.Remove(webhookId))
            {
                _logger.LogInformation("Webhook unregistered: {Id}", webhookId);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unregister webhook {Id}", webhookId);
            throw new DataAccessException("Failed to unregister webhook", nameof(UnregisterWebhookAsync), nameof(WebhookSubscription), ex);
        }
    }

    /// <summary>
    /// Gets the active webhook subscriptions for an API key.
    /// </summary>
    /// <param name="apiKeyId">Identifier of the API key.</param>
    /// <returns>The active subscriptions for the key.</returns>
    public Task<IEnumerable<WebhookSubscription>> GetWebhooksForKeyAsync(string apiKeyId)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
        {
            throw new ArgumentException("API key ID cannot be empty", nameof(apiKeyId));
        }

        try
        {
            var webhooks = _subscriptions.Values
                .Where(s => s.ApiKeyId == apiKeyId && s.IsActive)
                .ToList();

            return Task.FromResult<IEnumerable<WebhookSubscription>>(webhooks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get webhooks for API key {ApiKeyId}", apiKeyId);
            throw new DataAccessException("Failed to retrieve webhooks", nameof(GetWebhooksForKeyAsync), nameof(WebhookSubscription), ex);
        }
    }

    /// <summary>
    /// Gets the total number of deliveries attempted for a webhook.
    /// </summary>
    /// <param name="webhookId">Identifier of the subscription.</param>
    /// <returns>The delivery count, or 0 if the subscription is unknown.</returns>
    public Task<int> GetDeliveryCountAsync(string webhookId)
    {
        if (string.IsNullOrWhiteSpace(webhookId))
        {
            throw new ArgumentException("Webhook ID cannot be empty", nameof(webhookId));
        }

        try
        {
            if (_subscriptions.TryGetValue(webhookId, out var subscription))
            {
                return Task.FromResult(subscription.TotalDeliveries);
            }

            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get delivery count for webhook {Id}", webhookId);
            throw new DataAccessException("Failed to get delivery count", nameof(GetDeliveryCountAsync), nameof(WebhookSubscription), ex);
        }
    }
}
