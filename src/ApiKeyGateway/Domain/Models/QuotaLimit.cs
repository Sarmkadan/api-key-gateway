// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Defines the shared contract for quota limit values used across validators,
/// rate limiting, and usage quota checks.
/// </summary>
/// <remarks>
/// A quota limit is either the <see cref="Unlimited"/> sentinel (-1), meaning
/// no quota is enforced, or a positive value between 1 and <see cref="MaxValue"/>.
/// Zero and negative values other than -1 are invalid.
/// </remarks>
public static class QuotaLimit
{
    /// <summary>
    /// Sentinel value indicating that no quota limit applies (unlimited usage).
    /// </summary>
    public const int Unlimited = -1;

    /// <summary>
    /// Maximum allowed finite quota limit (one billion requests).
    /// </summary>
    public const int MaxValue = 1_000_000_000;

    /// <summary>
    /// Determines whether the given limit is the <see cref="Unlimited"/> sentinel.
    /// </summary>
    /// <param name="limit">The quota limit to inspect.</param>
    /// <returns><see langword="true"/> if the limit means unlimited usage; otherwise, <see langword="false"/>.</returns>
    public static bool IsUnlimited(int limit) => limit == Unlimited;

    /// <summary>
    /// Determines whether the given limit satisfies the quota limit contract:
    /// either the <see cref="Unlimited"/> sentinel or a positive value not exceeding <see cref="MaxValue"/>.
    /// </summary>
    /// <param name="limit">The quota limit to inspect.</param>
    /// <returns><see langword="true"/> if the limit is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(int limit) => limit is Unlimited or (> 0 and <= MaxValue);
}
