// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Application-wide configuration for the gateway
/// </summary>
public class GatewayConfiguration
{
    public string Id { get; init; } = "default";
    public bool RequireSsl { get; set; } = true;
    public bool LogAllRequests { get; set; } = true;
    public int MaxKeyLength { get; set; } = 256;
    public int MinKeyLength { get; set; } = 16;
    public int DefaultKeyExpirationDays { get; set; } = 365;
    public int AuditLogRetentionDays { get; set; } = 90;
    public bool EnableRateLimiting { get; set; } = true;
    public int DefaultRateLimitPerHour { get; set; } = 10000;
    public bool EnableIpWhitelisting { get; set; } = false;
    public int MaxConcurrentRequests { get; set; } = 1000;
    public string JwtSecret { get; set; } = string.Empty;
    public string DatabaseConnectionString { get; set; } = string.Empty;
    public Dictionary<string, string> CustomSettings { get; set; } = [];
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Validates the critical configuration settings
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(JwtSecret) &&
               !string.IsNullOrWhiteSpace(DatabaseConnectionString) &&
               MinKeyLength > 0 &&
               MaxKeyLength > MinKeyLength &&
               DefaultKeyExpirationDays > 0;
    }

    /// <summary>
    /// Gets a configuration value with fallback to default
    /// </summary>
    public string? GetSetting(string key)
    {
        return CustomSettings.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Sets or updates a custom configuration setting
    /// </summary>
    public void SetSetting(string key, string value)
    {
        CustomSettings[key] = value;
        UpdatedAt = DateTime.UtcNow;
    }
}
