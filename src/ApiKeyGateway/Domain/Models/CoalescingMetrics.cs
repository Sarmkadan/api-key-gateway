// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// A point-in-time snapshot of request coalescing statistics for monitoring and diagnostics.
/// </summary>
public sealed class CoalescingMetrics
{
    /// <summary>
    /// Total number of <see cref="IRequestCoalescingService.ExecuteAsync{T}"/> invocations since
    /// the service was last instantiated.
    /// </summary>
    public long TotalRequests { get; init; }

    /// <summary>
    /// Number of invocations that joined an existing in-flight request rather than executing the
    /// operation independently, saving one upstream call each.
    /// </summary>
    public long CoalescedRequests { get; init; }

    /// <summary>
    /// Number of operations currently executing or being awaited by follower callers.
    /// </summary>
    public int ActiveRequests { get; init; }

    /// <summary>
    /// Fraction of total invocations that were served by coalescing.
    /// Returns <c>0.0</c> when no requests have been processed yet.
    /// </summary>
    public double CoalescingRatio =>
        TotalRequests == 0 ? 0.0 : (double)CoalescedRequests / TotalRequests;
}
