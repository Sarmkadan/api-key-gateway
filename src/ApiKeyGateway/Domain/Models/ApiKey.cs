// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Represents an API key entity with authentication and metadata
/// </summary>
[DebuggerDisplay("ApiKey {Id,nq} {MaskedKey,nq} Status={Status,nq}")]
public class ApiKey
{
    /// <summary>
    /// Unique identifier of the API key.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Identifier of the consumer that owns the key.
    /// </summary>
    public string ConsumerId { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable name of the key.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Hash of the secret key value. Never exposed in logs or string output.
    /// </summary>
    public string KeyHash { get; init; } = string.Empty;

    /// <summary>
    /// Public prefix of the key used for identification.
    /// </summary>
    public string Prefix { get; init; } = string.Empty;

    /// <summary>
    /// Current lifecycle status of the key.
    /// </summary>
    public Enums.ApiKeyStatus Status { get; set; } = Enums.ApiKeyStatus.Active;

    /// <summary>
    /// Creation time of the key in UTC.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Expiration time of the key in UTC, or null if it does not expire.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Time the key was last used in UTC, or null if never used.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Time the key was disabled in UTC, or null if it is not disabled.
    /// </summary>
    public DateTime? DisabledAt { get; set; }

    /// <summary>
    /// Optional free-text description of the key.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Arbitrary key-value metadata attached to the key.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    /// <summary>
    /// Total number of requests made with the key.
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// Total bytes transferred with the key.
    /// </summary>
    public long BytesTransferred { get; set; }

    /// <summary>
    /// Optional comma-separated list of allowed source IP addresses.
    /// </summary>
    public string? IpWhitelist { get; set; }

    /// <summary>
    /// Optional identifier of the rate limit policy applied to the key.
    /// </summary>
    public string? RateLimitId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the key is active and not expired.
    /// </summary>
    public bool IsActive => Status == Enums.ApiKeyStatus.Active && !IsExpired;

    /// <summary>
    /// Gets a value indicating whether the key has passed its expiration time.
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;

    /// <summary>
    /// Log-safe representation of the key: the public prefix followed by a mask.
    /// The stored hash is never exposed through this property or <see cref="ToString"/>.
    /// </summary>
    public string MaskedKey => string.IsNullOrEmpty(Prefix) ? "****" : $"{Prefix}****";

    /// <summary>
    /// Comma-separated list of scope strings that restrict which upstream routes
    /// this key may access. An empty or null value means the key is unrestricted
    /// (can access all routes). Example: "/api/metrics,/api/stats"
    /// </summary>
    public string? AllowedScopes { get; set; }

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
        {
            return true;
        }

        var allowedIps = IpWhitelist.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ip => ip.Trim())
            .ToList();

        return allowedIps.Contains(requestIp);
    }

    /// <summary>
    /// Checks if the given request path is permitted by this key's scope list.
    /// A key with no scopes is unrestricted. A key with scopes can only access
    /// paths that start with at least one of those scope prefixes.
    /// </summary>
    public bool IsScopeAllowed(string requestPath)
    {
        if (string.IsNullOrWhiteSpace(AllowedScopes))
        {
            return true;
        }

        var scopes = AllowedScopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s));

        return scopes.Any(scope =>
            requestPath.StartsWith(scope, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns a concise representation of the API key. Shows the masked key only; the hash is omitted.
    /// </summary>
    public override string ToString() =>
        $"ApiKey {{ Id = {Id}, Key = {MaskedKey}, ConsumerId = {ConsumerId}, Status = {Status}, ExpiresAt = {ExpiresAt}, LastUsedAt = {LastUsedAt}, DisabledAt = {DisabledAt}, RequestCount = {RequestCount} }}";
}
