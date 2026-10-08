// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// A named rate limit policy (for example "free" or "pro") that API keys can be
/// assigned to. Declared under <c>RateLimiting:Policies</c>.
/// </summary>
[DebuggerDisplay("RateLimitPolicy {RequestsPerUnit,nq}/{WindowSeconds,nq}s")]
public class RateLimitPolicyOptions
{
    /// <summary>Maximum number of requests allowed within one window. Must be positive.</summary>
    public int RequestsPerUnit { get; set; }

    /// <summary>
    /// Length of the rate limit window in seconds. Must be within
    /// [<see cref="RateLimitingOptionsValidation.MinWindowSeconds"/>,
    /// <see cref="RateLimitingOptionsValidation.MaxWindowSeconds"/>] and match a supported unit
    /// (1, 60, 3600 or 86400) because limits are stored as a unit.
    /// </summary>
    public int WindowSeconds { get; set; } = 3600;

    /// <summary>Returns the policy's request count and window, e.g. "RateLimitPolicy { RequestsPerUnit = 100, WindowSeconds = 3600 }".</summary>
    public override string ToString() => $"RateLimitPolicy {{ RequestsPerUnit = {RequestsPerUnit}, WindowSeconds = {WindowSeconds} }}";
}
