// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Constants;

/// <summary>
/// Centralized error messages for the application
/// </summary>
public static class ErrorMessages
{
    /// <summary>
    /// Message returned when an API key has an invalid format
    /// </summary>
    public const string InvalidApiKeyFormat = "API key format is invalid. Expected format: {prefix}_{randomString}";

    /// <summary>
    /// Message returned when an API key does not exist or has been revoked
    /// </summary>
    public const string ApiKeyNotFound = "The specified API key does not exist or has been revoked.";

    /// <summary>
    /// Message returned when an API key has expired
    /// </summary>
    public const string ApiKeyExpired = "The API key has expired and is no longer valid.";

    /// <summary>
    /// Message returned when an API key is disabled
    /// </summary>
    public const string ApiKeyDisabled = "The API key is disabled and cannot be used.";

    /// <summary>
    /// Message returned when a rate limit has been exceeded
    /// </summary>
    public const string RateLimitExceeded = "Rate limit exceeded. Maximum {0} requests per {1} allowed.";

    /// <summary>
    /// Message returned when a request is not authorized
    /// </summary>
    public const string UnauthorizedAccess = "Unauthorized access. Valid API key required.";

    /// <summary>
    /// Message describing an invalid gateway configuration
    /// </summary>
    public const string InvalidConfiguration = "Invalid gateway configuration: {0}";

    /// <summary>
    /// Message returned when a database connection cannot be established
    /// </summary>
    public const string DatabaseConnectionFailed = "Failed to establish database connection.";

    /// <summary>
    /// Message returned when a service has not been initialized
    /// </summary>
    public const string ServiceNotInitialized = "{0} service is not properly initialized.";

    /// <summary>
    /// Message describing an invalid endpoint configuration
    /// </summary>
    public const string InvalidEndpointConfiguration = "Invalid endpoint configuration: {0}";

    /// <summary>
    /// Message returned when usage tracking fails for a key
    /// </summary>
    public const string UsageTrackingFailed = "Failed to track API usage for key {0}.";

    /// <summary>
    /// Message returned when an audit log entry cannot be written
    /// </summary>
    public const string AuditLogFailed = "Failed to write audit log entry.";

    /// <summary>
    /// Message returned when API key generation fails
    /// </summary>
    public const string KeyGenerationFailed = "Failed to generate API key.";

    /// <summary>
    /// Message returned when an API consumer cannot be found
    /// </summary>
    public const string UserNotFound = "API consumer not found with identifier {0}.";

    /// <summary>
    /// Message returned when an API key name is already in use for a consumer
    /// </summary>
    public const string DuplicateKeyName = "An API key with name {0} already exists for this consumer.";

    /// <summary>
    /// Message returned when a time range is invalid
    /// </summary>
    public const string InvalidTimeRange = "Invalid time range: start time must be before end time.";

    /// <summary>
    /// Message returned when the key store is temporarily unavailable
    /// </summary>
    public const string KeyStoreUnavailable = "The authentication service is temporarily unavailable. Please retry shortly.";

    /// <summary>
    /// Message returned when the data store cannot be accessed
    /// </summary>
    public const string DataAccessFailed = "Failed to access data store.";

    /// <summary>
    /// Message returned when an API key update fails
    /// </summary>
    public const string KeyUpdateFailed = "Failed to update API key.";
}
