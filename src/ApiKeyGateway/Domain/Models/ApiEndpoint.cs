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
    public string Id { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Method { get; init; } = "GET";
    public string TargetUrl { get; init; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool RequireApiKey { get; set; } = true;
    public int TimeoutMs { get; set; } = 30000;
    public int MaxPayloadBytes { get; set; } = 10485760;
    public string? Description { get; set; }
    public List<string> AllowedConsumers { get; set; } = [];
    public Dictionary<string, string> Headers { get; set; } = [];
    public bool CacheEnabled { get; set; } = false;
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
            return true;

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
}
