// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Represents an API endpoint protected by the gateway
/// </summary>
public class ApiEndpoint
{
    /// <summary>
    /// Unique identifier of the endpoint.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Public request path of the endpoint.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// HTTP method the endpoint accepts. Defaults to GET.
    /// </summary>
    public string Method { get; init; } = "GET";

    /// <summary>
    /// Upstream URL that requests are forwarded to.
    /// </summary>
    public string TargetUrl { get; init; } = string.Empty;

    /// <summary>
    /// Whether the endpoint is enabled.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Creation time of the endpoint in UTC.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Whether requests to the endpoint must carry a valid API key.
    /// </summary>
    public bool RequireApiKey { get; set; } = true;

    /// <summary>
    /// Upstream request timeout in milliseconds.
    /// </summary>
    public int TimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Maximum allowed request payload size in bytes.
    /// </summary>
    public int MaxPayloadBytes { get; set; } = 10485760;

    /// <summary>
    /// Optional free-text description of the endpoint.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Consumer identifiers allowed to access the endpoint. An empty list allows all consumers.
    /// </summary>
    public List<string> AllowedConsumers { get; set; } = [];

    /// <summary>
    /// Additional headers sent with forwarded requests.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>
    /// Whether responses from the endpoint are cached.
    /// </summary>
    public bool CacheEnabled { get; set; } = false;

    /// <summary>
    /// Cache entry lifetime in seconds.
    /// </summary>
    public int CacheTtlSeconds { get; set; } = 300;

    /// <summary>
    /// Checks if an endpoint is accessible
    /// </summary>
    public bool IsAccessible()
    {
        return IsActive && !string.IsNullOrWhiteSpace(TargetUrl);
    }

    /// <summary>
    /// Determines if a consumer is allowed to access this endpoint
    /// </summary>
    public bool IsConsumerAllowed(string consumerId)
    {
        if (!AllowedConsumers.Any())
        {
            return true;
        }

        return AllowedConsumers.Contains(consumerId);
    }

    /// <summary>
    /// Validates that the payload is within size limits
    /// </summary>
    public bool IsPayloadSizeValid(long payloadBytes)
    {
        return payloadBytes <= MaxPayloadBytes;
    }

    /// <summary>
    /// Gets the full endpoint identifier
    /// </summary>
    public string GetEndpointSignature()
    {
        return $"{Method} {Path}";
    }

    /// <summary>
    /// Returns a diagnostic string with the endpoint's key settings.
    /// </summary>
    /// <returns>A formatted description of the endpoint.</returns>
    public override string ToString() => $"ApiEndpoint {{ IsActive = {IsActive}, RequireApiKey = {RequireApiKey}, TimeoutMs = {TimeoutMs}, MaxPayloadBytes = {MaxPayloadBytes}, Description = {Description}, AllowedConsumers = {AllowedConsumers} }}";
}
