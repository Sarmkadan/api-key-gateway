// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Outcome of a single rate-limit check for one API key.
/// </summary>
[DebuggerDisplay("RateLimitDecision {ApiKeyId,nq} Allowed={Allowed,nq} Remaining={Remaining,nq}/{Limit,nq}")]
public sealed class RateLimitDecision
{
    /// <summary>ID of the API key that was checked</summary>
    public string ApiKeyId { get; init; } = string.Empty;

    /// <summary>True if the request was admitted and a permit was consumed</summary>
    public bool Allowed { get; init; }

    /// <summary>Maximum number of requests permitted in the window</summary>
    public int Limit { get; init; }

    /// <summary>Permits left in the current window after this check</summary>
    public int Remaining { get; init; }

    /// <summary>When the oldest counted request leaves the window (UTC)</summary>
    public DateTime ResetAtUtc { get; init; }

    /// <summary>
    /// Returns a concise representation of the decision. Contains no key material.
    /// </summary>
    public override string ToString() =>
        $"RateLimitDecision {{ ApiKeyId = {ApiKeyId}, Allowed = {Allowed}, Limit = {Limit}, Remaining = {Remaining}, ResetAtUtc = {ResetAtUtc:O} }}";
}
