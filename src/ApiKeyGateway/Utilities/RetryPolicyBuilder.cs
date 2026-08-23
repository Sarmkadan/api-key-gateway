// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Builder for creating retry policies for resilient operations.
/// Uses exponential backoff to prevent overwhelming failing services.
/// This is useful for external API calls, database operations, etc.
/// </summary>
public sealed class RetryPolicyBuilder
{
    private int _maxRetries = 3;
    private int _initialDelayMs = 100;
    private double _backoffMultiplier = 2.0;
    private int _maxDelayMs = 30000;
    private readonly List<Type> _retryableExceptions = new();

    /// <summary>
    /// Gets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetries => _maxRetries;

    /// <summary>
    /// Gets the initial delay before first retry in milliseconds.
    /// </summary>
    public int InitialDelayMs => _initialDelayMs;

    /// <summary>
    /// Gets the exponential backoff multiplier.
    /// </summary>
    public double BackoffMultiplier => _backoffMultiplier;

    /// <summary>
    /// Gets the maximum delay in milliseconds.
    /// </summary>
    public int MaxDelayMs => _maxDelayMs;

    /// <summary>
    /// Sets maximum number of retry attempts.
    /// </summary>
    /// <param name="maxRetries">The maximum number of retry attempts.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public RetryPolicyBuilder WithMaxRetries(int maxRetries)
    {
        _maxRetries = maxRetries;
        return this;
    }

    /// <summary>
    /// Sets initial delay before first retry in milliseconds.
    /// </summary>
    /// <param name="delayMs">The initial delay before first retry in milliseconds.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public RetryPolicyBuilder WithInitialDelay(int delayMs)
    {
        _initialDelayMs = delayMs;
        return this;
    }

    /// <summary>
    /// Sets exponential backoff multiplier (default: 2.0).
    /// Each retry waits longer: delay = delay * multiplier.
    /// </summary>
    /// <param name="multiplier">The exponential backoff multiplier.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public RetryPolicyBuilder WithBackoffMultiplier(double multiplier)
    {
        _backoffMultiplier = multiplier;
        return this;
    }

    /// <summary>
    /// Sets maximum delay to prevent waiting too long.
    /// </summary>
    /// <param name="delayMs">The maximum delay in milliseconds.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public RetryPolicyBuilder WithMaxDelay(int delayMs)
    {
        _maxDelayMs = delayMs;
        return this;
    }

    /// <summary>
    /// Adds exception type that should trigger a retry.
    /// </summary>
    /// <typeparam name="TException">The type of exception to retry on.</typeparam>
    /// <returns>This instance for fluent chaining.</returns>
    public RetryPolicyBuilder RetryOn<TException>() where TException : Exception
    {
        _retryableExceptions.Add(typeof(TException));
        return this;
    }

    /// <summary>
    /// Builds and returns the retry policy function.
    /// </summary>
    /// <typeparam name="T">The type of the operation result.</typeparam>
    /// <param name="operation">The asynchronous operation to retry.</param>
    /// <returns>A function that executes the operation with retry logic.</returns>
    public Func<Func<Task<T>>, Task<T>> Build<T>()
    {
        return async (operation) =>
        {
            var currentDelay = _initialDelayMs;

            for (int attempt = 0; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex) when (attempt < _maxRetries && ShouldRetry(ex))
                {
                    await Task.Delay(currentDelay);
                    currentDelay = (int)Math.Min(currentDelay * _backoffMultiplier, _maxDelayMs);
                }
            }

            // All retries exhausted, throw original exception
            return await operation();
        };
    }

    /// <summary>
    /// Determines if an exception should trigger a retry.
    /// </summary>
    private bool ShouldRetry(Exception ex)
    {
        if (_retryableExceptions.Count == 0)
        {
            // If no specific exceptions configured, retry on common transient errors
            return ex is HttpRequestException ||
                   ex is TimeoutException ||
                   ex is InvalidOperationException;
        }

        return _retryableExceptions.Any(type => type.IsAssignableFrom(ex.GetType()));
    }
}