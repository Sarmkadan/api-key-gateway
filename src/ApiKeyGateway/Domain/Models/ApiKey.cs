// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Represents an API key entity with authentication and metadata
/// </summary>
public class ApiKey
{
    public string Id { get; init; } = string.Empty;
    public string ConsumerId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string KeyHash { get; init; } = string.Empty;
    public string Prefix { get; init; } = string.Empty;
    public Enums.ApiKeyStatus Status { get; set; } = Enums.ApiKeyStatus.Active;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? DisabledAt { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];
    public int RequestCount { get; set; }
    public long BytesTransferred { get; set; }
    public string? IpWhitelist { get; set; }
    public string? RateLimitId { get; set; }
    public bool IsActive => Status == Enums.ApiKeyStatus.Active && !IsExpired;
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;

    /// <summary>
    /// Validates that the API key can be used for authentication
    /// </summary>
    public bool CanBeUsed()
    {
        return IsActive && !IsExpired && Status is not (Enums.ApiKeyStatus.Revoked or Enums.ApiKeyStatus.Suspended);
    }

    /// <summary>
    /// Records a successful API key usage
    /// </summary>
    public void RecordUsage(long bytes = 0)
    {
        RequestCount++;
        BytesTransferred += bytes;
        LastUsedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Disables the API key
    /// </summary>
    public void Disable()
    {
        Status = Enums.ApiKeyStatus.Disabled;
        DisabledAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Enables a previously disabled API key
    /// </summary>
    public void Enable()
    {
        if (Status == Enums.ApiKeyStatus.Disabled)
        {
            Status = Enums.ApiKeyStatus.Active;
            DisabledAt = null;
        }
    }

    /// <summary>
    /// Revokes the API key permanently
    /// </summary>
    public void Revoke()
    {
        Status = Enums.ApiKeyStatus.Revoked;
    }

    /// <summary>
    /// Checks if the request source IP is allowed
    /// </summary>
    public bool IsIpAllowed(string requestIp)
    {
        if (string.IsNullOrWhiteSpace(IpWhitelist))
            return true;

        var allowedIps = IpWhitelist.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ip => ip.Trim())
            .ToList();

        return allowedIps.Contains(requestIp);
    }
}
