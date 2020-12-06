# GatewayConfiguration

`GatewayConfiguration` represents the centralized runtime configuration for the API key gateway service. It holds all operational parameters—security constraints, rate limiting, logging behavior, key lifecycle defaults, and infrastructure connection strings—along with a flexible dictionary for custom extensions. The type also tracks metadata about when and by whom the configuration was last modified, and exposes a validation flag and methods to retrieve or inject arbitrary settings.

## API

### `public string Id`
A unique identifier for this configuration instance. Typically a GUID or a logical name used to distinguish configurations in multi-tenant or multi-environment deployments.

### `public bool RequireSsl`
When `true`, the gateway rejects any request that does not arrive over an encrypted connection (HTTPS). Set to `false` only for development or internal-only networks where TLS is terminated externally.

### `public bool LogAllRequests`
Controls whether every inbound request—regardless of authentication outcome—is written to the audit log. When `false`, only rejected or anomalous requests are logged.

### `public int MaxKeyLength`
The maximum permitted length, in characters, for an API key string. Keys longer than this value are rejected during creation or validation.

### `public int MinKeyLength`
The minimum permitted length, in characters, for an API key string. Keys shorter than this value are rejected during creation or validation.

### `public int DefaultKeyExpirationDays`
The lifespan, in days, assigned to newly created API keys when no explicit expiration is provided. After this period the key is considered expired and will fail validation.

### `public int AuditLogRetentionDays`
The number of days audit log records are preserved before being purged by the retention job. A value of zero means indefinite retention.

### `public bool EnableRateLimiting`
Master switch for rate limiting. When `false`, per-key and global rate limits are not enforced, regardless of other rate-limit settings.

### `public int DefaultRateLimitPerHour`
The default maximum number of requests an individual API key may make per hour when rate limiting is enabled. This value is used for keys that do not have a custom limit assigned.

### `public bool EnableIpWhitelisting`
When `true`, the gateway evaluates IP whitelist rules associated with each API key. Requests from non-whitelisted addresses are denied even if the key is otherwise valid.

### `public int MaxConcurrentRequests`
The hard ceiling on the total number of in-flight requests the gateway will process simultaneously. Requests exceeding this limit receive an immediate rejection (typically HTTP 503).

### `public string JwtSecret`
The symmetric signing key used to validate JWT tokens when the gateway is configured to accept JWT-based authentication alongside or instead of API keys. Must be kept secret.

### `public string DatabaseConnectionString`
The connection string used by the gateway to reach its backing data store for key metadata, audit logs, and configuration persistence.

### `public Dictionary<string, string> CustomSettings`
A string-to-string dictionary holding arbitrary configuration extensions. Consumers can store feature flags, integration endpoints, or environment-specific overrides without modifying the core schema.

### `public DateTime UpdatedAt`
The UTC timestamp of the most recent modification to this configuration object. Set automatically when the configuration is persisted.

### `public string? UpdatedBy`
An optional identifier—such as a username, service account, or system name—of the principal who last modified the configuration. `null` if the modifier is unknown or not recorded.

### `public bool IsValid`
Indicates whether the current configuration passes all internal consistency checks (e.g., `MinKeyLength` ≤ `MaxKeyLength`, non-negative retention values, non-empty connection string when persistence is required). Invalid configurations cause the gateway to refuse startup.

### `public string? GetSetting(string key)`
Retrieves the value associated with the given `key` from `CustomSettings`. Returns `null` if the key is not present.  
**Parameters:** `key` — the case-sensitive setting name.  
**Returns:** the corresponding value as a `string`, or `null`.  
**Throws:** `ArgumentNullException` when `key` is `null`.

### `public void SetSetting(string key, string value)`
Adds or updates an entry in `CustomSettings`. If `key` already exists, its value is overwritten.  
**Parameters:** `key` — the case-sensitive setting name; `value` — the setting value.  
**Throws:** `ArgumentNullException` when `key` is `null`; `ArgumentException` when `key` is empty or consists only of whitespace.

## Usage

### Example 1: Building a default configuration and validating it
```csharp
var config = new GatewayConfiguration
{
    Id = "prod-primary",
    RequireSsl = true,
    LogAllRequests = false,
    MaxKeyLength = 128,
    MinKeyLength = 32,
    DefaultKeyExpirationDays = 365,
    AuditLogRetentionDays = 90,
    EnableRateLimiting = true,
    DefaultRateLimitPerHour = 1000,
    EnableIpWhitelisting = false,
    MaxConcurrentRequests = 500,
    JwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET"),
    DatabaseConnectionString = "Server=db.internal;Database=ApiKeyGateway;User Id=gw;Password=***;"
};

if (!config.IsValid)
{
    throw new InvalidOperationException("Gateway configuration is invalid and cannot be applied.");
}

// Store an environment-specific override
config.SetSetting("notification.webhook.url", "https://alerts.internal/gateway-events");
```

### Example 2: Reading and reacting to custom settings at runtime
```csharp
public void ConfigureAlerts(GatewayConfiguration config)
{
    string? webhookUrl = config.GetSetting("notification.webhook.url");
    string? slackChannel = config.GetSetting("notification.slack.channel");

    if (webhookUrl is not null)
    {
        RegisterWebhookAlerting(webhookUrl);
    }

    if (slackChannel is not null)
    {
        RegisterSlackAlerting(slackChannel);
    }

    // Fallback when no alerting is configured
    if (webhookUrl is null && slackChannel is null)
    {
        Log.Warning("No notification channels configured in GatewayConfiguration.CustomSettings");
    }
}
```

## Notes

- **Validation consistency:** `IsValid` checks that `MinKeyLength` is strictly less than or equal to `MaxKeyLength`, that all integer fields with retention/length semantics are non-negative, and that `DatabaseConnectionString` is non-empty when persistence is required. It does not verify that the connection string actually reaches a live database.
- **CustomSettings thread safety:** `GetSetting` and `SetSetting` operate on the underlying `Dictionary<string, string>`, which is not thread-safe by default. Concurrent reads and writes from multiple threads must be externally synchronized if the configuration is mutated at runtime.
- **JwtSecret sensitivity:** The `JwtSecret` property holds a raw secret value. Avoid logging or serializing this property into insecure channels. Consider using secure string handling or masking in `ToString`-like outputs if implemented.
- **Nullable UpdatedBy:** `UpdatedBy` is a nullable reference type. Code consuming this member must perform null checks before dereferencing, especially when generating audit trails.
- **Rate limiting dependency:** `DefaultRateLimitPerHour` is only honored when `EnableRateLimiting` is `true`. Setting a limit while the master switch is off has no effect on request processing.
- **IpWhitelisting dependency:** `EnableIpWhitelisting` gates all IP enforcement. Individual key whitelists are ignored when this flag is `false`, even if populated.
- **Configuration immutability after startup:** The type itself does not enforce immutability, but typical gateway implementations load the configuration once at startup and treat it as read-only thereafter. Mutating fields on a live instance may not propagate to already-initialized subsystems.
