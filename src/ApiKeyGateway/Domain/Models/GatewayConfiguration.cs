namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Application-wide configuration for the gateway
/// </summary>
public class GatewayConfiguration
{
    /// <summary>Identifier of this configuration record</summary>
    public string Id { get; init; } = "default";

    /// <summary>Whether requests must be made over SSL</summary>
    public bool RequireSsl { get; set; } = true;

    /// <summary>Whether every request is logged</summary>
    public bool LogAllRequests { get; set; } = true;

    /// <summary>Maximum allowed length of an API key</summary>
    public int MaxKeyLength { get; set; } = 256;

    /// <summary>Minimum required length of an API key</summary>
    public int MinKeyLength { get; set; } = 16;

    /// <summary>Default lifetime of a new API key, in days</summary>
    public int DefaultKeyExpirationDays { get; set; } = 365;

    /// <summary>Number of days audit log entries are retained</summary>
    public int AuditLogRetentionDays { get; set; } = 90;

    /// <summary>Whether rate limiting is enforced</summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>Default rate limit applied to keys, in requests per hour</summary>
    public int DefaultRateLimitPerHour { get; set; } = 10000;

    /// <summary>Whether IP whitelisting is enforced</summary>
    public bool EnableIpWhitelisting { get; set; } = false;

    /// <summary>Maximum number of requests processed concurrently</summary>
    public int MaxConcurrentRequests { get; set; } = 1000;

    /// <summary>Secret used to sign and validate JWT tokens</summary>
    public string JwtSecret { get; set; } = string.Empty;

    /// <summary>Connection string for the backing database</summary>
    public string DatabaseConnectionString { get; set; } = string.Empty;

    /// <summary>Maximum number of retry attempts for a failed webhook delivery</summary>
    public int WebhookMaxRetries { get; set; } = 3;

    /// <summary>Initial delay before the first webhook retry, in milliseconds</summary>
    public int WebhookBaseDelayMs { get; set; } = 1000;

    /// <summary>Multiplier applied to the webhook retry delay after each attempt</summary>
    public double WebhookBackoffMultiplier { get; set; } = 2.0;

    /// <summary>Upper bound on the webhook retry delay, in milliseconds</summary>
    public int WebhookMaxDelayMs { get; set; } = 30000;

    /// <summary>Additional application-defined settings</summary>
    public Dictionary<string, string> CustomSettings { get; set; } = [];

    /// <summary>When the configuration was last updated (UTC)</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Identity of the user who last updated the configuration</summary>
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
        ArgumentException.ThrowIfNullOrEmpty(key);
        return CustomSettings.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Sets or updates a custom configuration setting
    /// </summary>
    public void SetSetting(string key, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);
        CustomSettings[key] = value;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns a short text representation of the key configuration values.
    /// </summary>
    /// <returns>A string describing the main configuration settings.</returns>
    public override string ToString() =>
        $"GatewayConfiguration {{ RequireSsl = {RequireSsl}, LogAllRequests = {LogAllRequests}, MaxKeyLength = {MaxKeyLength}, MinKeyLength = {MinKeyLength}, DefaultKeyExpirationDays = {DefaultKeyExpirationDays}, AuditLogRetentionDays = {AuditLogRetentionDays} }}";
}
