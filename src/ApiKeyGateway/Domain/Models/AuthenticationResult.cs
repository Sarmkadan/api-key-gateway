// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using ApiKeyGateway.Domain.Enums;

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Represents the result of an authentication attempt
/// </summary>
public record AuthenticationResult
{
    /// <summary>
    /// Indicates whether authentication was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The failure reason when Success is false
    /// </summary>
    public AuthenticationFailureReason? FailureReason { get; init; }

    /// <summary>
    /// The authenticated API key when Success is true
    /// </summary>
    public ApiKey? ApiKey { get; init; }

    /// <summary>
    /// Creates a successful authentication result
    /// </summary>
    /// <param name="apiKey">The authenticated API key</param>
    public static AuthenticationResult SuccessResult(ApiKey apiKey) =>
        new AuthenticationResult { Success = true, ApiKey = apiKey };

    /// <summary>
    /// Creates a failed authentication result with a specific reason
    /// </summary>
    /// <param name="failureReason">The reason for authentication failure</param>
    public static AuthenticationResult FailureResult(AuthenticationFailureReason failureReason) =>
        new AuthenticationResult { Success = false, FailureReason = failureReason };
}

/// <summary>
/// Enumeration of possible authentication failure reasons
/// </summary>
public enum AuthenticationFailureReason
{
    /// <summary>Missing API key in request</summary>
    MissingApiKey,

    /// <summary>Invalid API key format</summary>
    InvalidApiKeyFormat,

    /// <summary>API key not found or revoked</summary>
    ApiKeyNotFound,

    /// <summary>API key has expired</summary>
    ApiKeyExpired,

    /// <summary>API key is disabled</summary>
    ApiKeyDisabled,

    /// <summary>Source IP address not whitelisted</summary>
    IpNotWhitelisted,

    /// <summary>Authentication service unavailable</summary>
    ServiceUnavailable
}