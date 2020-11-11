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
    public const string InvalidApiKeyFormat = "API key format is invalid. Expected format: {prefix}_{randomString}";
    public const string ApiKeyNotFound = "The specified API key does not exist or has been revoked.";
    public const string ApiKeyExpired = "The API key has expired and is no longer valid.";
    public const string ApiKeyDisabled = "The API key is disabled and cannot be used.";
    public const string RateLimitExceeded = "Rate limit exceeded. Maximum {0} requests per {1} allowed.";
    public const string UnauthorizedAccess = "Unauthorized access. Valid API key required.";
    public const string InvalidConfiguration = "Invalid gateway configuration: {0}";
    public const string DatabaseConnectionFailed = "Failed to establish database connection.";
    public const string ServiceNotInitialized = "{0} service is not properly initialized.";
    public const string InvalidEndpointConfiguration = "Invalid endpoint configuration: {0}";
    public const string UsageTrackingFailed = "Failed to track API usage for key {0}.";
    public const string AuditLogFailed = "Failed to write audit log entry.";
    public const string KeyGenerationFailed = "Failed to generate API key.";
    public const string UserNotFound = "API consumer not found with identifier {0}.";
    public const string DuplicateKeyName = "An API key with name {0} already exists for this consumer.";
    public const string InvalidTimeRange = "Invalid time range: start time must be before end time.";
    public const string KeyStoreUnavailable = "The authentication service is temporarily unavailable. Please retry shortly.";
    public const string DataAccessFailed = "Failed to access data store.";
    public const string KeyUpdateFailed = "Failed to update API key.";
}
