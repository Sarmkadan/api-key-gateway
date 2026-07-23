// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Policy applied by the rate limiter when the backing limit store
/// (database, cache) cannot be reached.
/// </summary>
public enum RateLimitFailurePolicy
{
    /// <summary>
    /// Allow the request through without a rate-limit check and publish a
    /// warning event. Prioritises availability over strict limit enforcement.
    /// </summary>
    FailOpen,

    /// <summary>
    /// Reject the request with 503 Service Unavailable until the limit store
    /// recovers. Prioritises strict limit enforcement over availability.
    /// </summary>
    FailClosed
}

/// <summary>
/// Options controlling rate limiter behaviour when the limit store fails,
/// including a simple circuit breaker that stops hammering a down backend.
/// </summary>
public class RateLimitingOptions
{
    /// <summary>Configuration section name the options are bound from.</summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// What the gateway does when the limit store throws a data-access error.
    /// Defaults to <see cref="RateLimitFailurePolicy.FailClosed"/> so an outage
    /// cannot be exploited to bypass limits unless explicitly opted into.
    /// </summary>
    public RateLimitFailurePolicy FailurePolicy { get; set; } = RateLimitFailurePolicy.FailClosed;

    /// <summary>
    /// Number of consecutive store failures within
    /// <see cref="CircuitBreakerFailureWindow"/> that trips the circuit breaker.
    /// Once tripped, the store is skipped entirely for
    /// <see cref="CircuitBreakerCooldown"/>.
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// Sliding window within which consecutive failures are counted towards
    /// the circuit breaker threshold. Failures spaced further apart than this
    /// restart the count.
    /// </summary>
    public TimeSpan CircuitBreakerFailureWindow { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How long the limit store is skipped after the circuit breaker trips.
    /// While open, requests are handled per <see cref="FailurePolicy"/>
    /// without touching the store.
    /// </summary>
    public TimeSpan CircuitBreakerCooldown { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Validates the option values.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <see cref="CircuitBreakerFailureThreshold"/> is less than 1,
    /// or <see cref="CircuitBreakerFailureWindow"/> or
    /// <see cref="CircuitBreakerCooldown"/> is not positive.
    /// </exception>
    public void Validate()
    {
        if (CircuitBreakerFailureThreshold < 1)
            throw new ArgumentOutOfRangeException(nameof(CircuitBreakerFailureThreshold), CircuitBreakerFailureThreshold, "Threshold must be at least 1.");
        if (CircuitBreakerFailureWindow <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(CircuitBreakerFailureWindow), CircuitBreakerFailureWindow, "Window must be positive.");
        if (CircuitBreakerCooldown <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(CircuitBreakerCooldown), CircuitBreakerCooldown, "Cooldown must be positive.");
    }
}
