// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of an API key
/// </summary>
public enum ApiKeyStatus
{
    /// <summary>
    /// Key is active and can be used for requests
    /// </summary>
    Active = 1,

    /// <summary>
    /// Key is disabled but not deleted - can be re-enabled
    /// </summary>
    Disabled = 2,

    /// <summary>
    /// Key has reached expiration date
    /// </summary>
    Expired = 3,

    /// <summary>
    /// Key has been permanently revoked by admin
    /// </summary>
    Revoked = 4,

    /// <summary>
    /// Key is suspended due to abuse or policy violation
    /// </summary>
    Suspended = 5
}
