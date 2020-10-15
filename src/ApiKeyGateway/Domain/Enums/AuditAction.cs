// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Enums;

/// <summary>
/// Represents auditable actions within the gateway
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// API key was created
    /// </summary>
    KeyCreated = 1,

    /// <summary>
    /// API key was used (accessed)
    /// </summary>
    KeyUsed = 2,

    /// <summary>
    /// API key was disabled
    /// </summary>
    KeyDisabled = 3,

    /// <summary>
    /// API key was enabled
    /// </summary>
    KeyEnabled = 4,

    /// <summary>
    /// API key was revoked
    /// </summary>
    KeyRevoked = 5,

    /// <summary>
    /// Rate limit was exceeded for key
    /// </summary>
    RateLimitExceeded = 6,

    /// <summary>
    /// Configuration was updated
    /// </summary>
    ConfigurationUpdated = 7,

    /// <summary>
    /// Unauthorized access attempt
    /// </summary>
    UnauthorizedAttempt = 8
}
