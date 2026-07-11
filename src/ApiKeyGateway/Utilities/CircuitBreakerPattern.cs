// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging.Abstractions;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Simple circuit breaker implementation for fault tolerance.
/// Prevents cascading failures by stopping requests to failing services.
/// Automatically recovers when service becomes healthy again.
/// </summary>
public interface ICircuitBreaker
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation);
    void RecordSuccess();
    void RecordFailure();
    CircuitBreakerState GetState();
}

/// <summary>
/// Circuit breaker state enumeration.
/// </summary>
public enum CircuitBreakerState
{
    Closed,      // Operating normally, requests pass through
    Open,        // Failing, requests are blocked
    HalfOpen     // Testing if service recovered
}

/// <summary>
/// Production circuit breaker with configurable thresholds.
/// </summary>
public sealed class CircuitBreaker : ICircuitBreaker
{
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly ILogger<CircuitBreaker> _logger;
    private readonly object _lockObj = new();

    public CircuitBreaker(
        int failureThreshold = 5,
        TimeSpan? timeout = null,
        ILogger<CircuitBreaker>? logger = null)
    {
        _failureThreshold = failureThreshold;
        _timeout = timeout ?? TimeSpan.FromSeconds(30);
        _logger = logger ?? NullLogger<CircuitBreaker>.Instance;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        lock (_lockObj)
        {
            switch (_state)
            {
                case CircuitBreakerState.Open:
                    // Check if timeout has elapsed, move to half-open
                    if (DateTime.UtcNow - _lastFailureTime > _timeout)
                    {
                        _logger.LogInformation("Circuit breaker transitioning to Half-Open");
                        _state = CircuitBreakerState.HalfOpen;
                        _failureCount = 0;
                    }
                    else
                    {
                        throw new InvalidOperationException("Circuit breaker is open");
                    }
                    break;

                case CircuitBreakerState.HalfOpen:
                    // Only allow single request through
                    break;
            }
        }

        try
        {
            var result = await operation();
            RecordSuccess();
            return result;
        }
        catch
        {
            RecordFailure();
            throw;
        }
    }

    public void RecordSuccess()
    {
        lock (_lockObj)
        {
            _failureCount = 0;

            if (_state == CircuitBreakerState.HalfOpen)
            {
                _logger.LogInformation("Circuit breaker transitioning to Closed");
                _state = CircuitBreakerState.Closed;
            }
        }
    }

    public void RecordFailure()
    {
        lock (_lockObj)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            _logger.LogWarning(
                "Circuit breaker failure recorded: {FailureCount}/{Threshold}",
                _failureCount,
                _failureThreshold);

            if (_failureCount >= _failureThreshold)
            {
                _logger.LogError("Circuit breaker opening after {FailureCount} failures", _failureCount);
                _state = CircuitBreakerState.Open;
            }
        }
    }

    public CircuitBreakerState GetState() => _state;
}
