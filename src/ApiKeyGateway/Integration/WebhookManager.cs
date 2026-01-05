// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Integration;

/// <summary>
/// Manager for webhook subscriptions and delivery coordination.
/// Handles the lifecycle of webhooks: registration, subscription, delivery tracking.
/// Provides API for users to configure which events they want to receive.
/// </summary>
public interface IWebhookManager
{
    Task<string> RegisterWebhookAsync(string apiKeyId, string targetUrl, string[] eventTypes, string? secret = null);
    Task UnregisterWebhookAsync(string webhookId);
    Task<IEnumerable<WebhookSubscription>> GetWebhooksForKeyAsync(string apiKeyId);
    Task<int> GetDeliveryCountAsync(string webhookId);
}

/// <summary>
/// Webhook subscription details.
/// </summary>
public record WebhookSubscription
{
    public string Id { get; set; } = null!;
    public string ApiKeyId { get; set; } = null!;
    public string TargetUrl { get; set; } = null!;
    public string[] EventTypes { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastDeliveryAt { get; set; }
    public int TotalDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
}

/// <summary>
/// Production implementation of webhook manager with persistence.
/// </summary>
public sealed class WebhookManager : IWebhookManager
{
    private readonly ILogger<WebhookManager> _logger;
    private readonly Dictionary<string, WebhookSubscription> _subscriptions = new();

    public WebhookManager(ILogger<WebhookManager> logger)
    {
        _logger = logger;
    }

    public Task<string> RegisterWebhookAsync(
        string apiKeyId,
        string targetUrl,
        string[] eventTypes,
        string? secret = null)
    {
        // Validate URL is HTTPS for security
        if (!targetUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Webhook URLs must use HTTPS");
        }

        var subscription = new WebhookSubscription
        {
            Id = Guid.NewGuid().ToString(),
            ApiKeyId = apiKeyId,
            TargetUrl = targetUrl,
            EventTypes = eventTypes,
            IsActive = true,
            RegisteredAt = DateTime.UtcNow
        };

        _subscriptions[subscription.Id] = subscription;

        _logger.LogInformation(
            "Webhook registered: {Id} for API key {ApiKeyId} - {EventCount} events",
            subscription.Id,
            apiKeyId,
            eventTypes.Length);

        return Task.FromResult(subscription.Id);
    }

    public Task UnregisterWebhookAsync(string webhookId)
    {
        if (_subscriptions.Remove(webhookId))
        {
            _logger.LogInformation("Webhook unregistered: {Id}", webhookId);
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<WebhookSubscription>> GetWebhooksForKeyAsync(string apiKeyId)
    {
        var webhooks = _subscriptions.Values
            .Where(s => s.ApiKeyId == apiKeyId && s.IsActive)
            .ToList();

        return Task.FromResult<IEnumerable<WebhookSubscription>>(webhooks);
    }

    public Task<int> GetDeliveryCountAsync(string webhookId)
    {
        if (_subscriptions.TryGetValue(webhookId, out var subscription))
        {
            return Task.FromResult(subscription.TotalDeliveries);
        }

        return Task.FromResult(0);
    }
}
