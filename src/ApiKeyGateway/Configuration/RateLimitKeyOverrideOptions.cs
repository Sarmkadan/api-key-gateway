// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Per-key rate limit override. References a policy by name and may replace the
/// policy's request count and/or window. Declared under <c>RateLimiting:KeyOverrides</c>
/// keyed by API key id, and also used as the request body of the admin override endpoint.
/// </summary>
public class RateLimitKeyOverrideOptions
{
    /// <summary>Name of the policy under <c>RateLimiting:Policies</c>. Required.</summary>
    public string? Policy { get; set; }

    /// <summary>Optional request count that replaces the policy's <see cref="RateLimitPolicyOptions.RequestsPerUnit"/>.</summary>
    public int? RequestsPerUnit { get; set; }

    /// <summary>Optional window that replaces the policy's <see cref="RateLimitPolicyOptions.WindowSeconds"/>.</summary>
    public int? WindowSeconds { get; set; }
}
