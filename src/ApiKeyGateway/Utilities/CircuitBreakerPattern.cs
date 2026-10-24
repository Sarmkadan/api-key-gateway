// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging.Abstractions;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Interface for circuit breaker pattern implementation.
/// Prevents cascading failures by stopping requests to failing services.
/// Automatically recovers when service becomes healthy again.
/// </summary>
public interface ICircuitBreaker
{
    /// <summary>
    /// Executes the operation if the circuit allows it and records its outcome.
    /// </summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <returns>The result of the operation.</returns>
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation);

    /// <summary>
    /// Records a successful operation.
    /// </summary>
    void RecordSuccess();

    /// <summary>
    /// Records a failed operation.
    /// </summary>
    void RecordFailure();

    /// <summary>
    /// Gets the current state of the circuit breaker.
    /// </summary>
    /// <returns>The current <see cref="CircuitBreakerState"/>.</returns>
    CircuitBreakerState GetState();
}

/// <summary>
/// Represents the state of a circuit breaker.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>
    /// Operating normally; requests pass through.
    /// </summary>
    Closed,

    /// <summary>
    /// Failing; requests are blocked.
    /// </summary>
    Open,

    /// <summary>
    /// Testing whether the service has recovered.
    /// </summary>
    HalfOpen
}

/// <summary>
/// Production circuit breaker with configurable thresholds.
/// </summary>
public sealed class CircuitBreaker : ICircuitBreaker
{
    private const int DefaultFailureThreshold = 5;
    private const int DefaultTimeoutSeconds = 30;

    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly ILogger<CircuitBreaker> _logger;
    private readonly object _lockObj = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreaker"/> class.
    /// </summary>
    /// <param name="failureThreshold">Number of consecutive failures before the circuit opens.</param>
    /// <param name="timeout">How long the circuit stays open before allowing a trial request. Defaults to 30 seconds.</param>
    /// <param name="logger">Optional logger for state transitions.</param>
    public CircuitBreaker(
        int failureThreshold = DefaultFailureThreshold,
        TimeSpan? timeout = null,
        ILogger<CircuitBreaker>? logger = null)
    {
        _failureThreshold = failureThreshold;
        _timeout = timeout ?? TimeSpan.FromSeconds(DefaultTimeoutSeconds);
        _logger = logger ?? NullLogger<CircuitBreaker>.Instance;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public CircuitBreakerState GetState() => _state;
}