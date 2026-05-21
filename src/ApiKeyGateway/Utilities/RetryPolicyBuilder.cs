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
    /// Sets maximum number of retry attempts.
    /// </summary>
    public RetryPolicyBuilder WithMaxRetries(int maxRetries)
    {
        _maxRetries = maxRetries;
        return this;
    }

    /// <summary>
    /// Sets initial delay before first retry in milliseconds.
    /// </summary>
    public RetryPolicyBuilder WithInitialDelay(int delayMs)
    {
        _initialDelayMs = delayMs;
        return this;
    }

    /// <summary>
    /// Sets exponential backoff multiplier (default: 2.0).
    /// Each retry waits longer: delay = delay * multiplier.
    /// </summary>
    public RetryPolicyBuilder WithBackoffMultiplier(double multiplier)
    {
        _backoffMultiplier = multiplier;
        return this;
    }

    /// <summary>
    /// Sets maximum delay to prevent waiting too long.
    /// </summary>
    public RetryPolicyBuilder WithMaxDelay(int delayMs)
    {
        _maxDelayMs = delayMs;
        return this;
    }

    /// <summary>
    /// Adds exception type that should trigger a retry.
    /// </summary>
    public RetryPolicyBuilder RetryOn<TException>() where TException : Exception
    {
        _retryableExceptions.Add(typeof(TException));
        return this;
    }

    /// <summary>
    /// Builds and returns the retry policy function.
    /// </summary>
    public Func<Func<Task<T>>, Task<T>> Build<T>()
    {
        return async (operation) =>
        {
            var currentDelay = _initialDelayMs;

            for (int attempt = 0; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    return await operation().ConfigureAwait(false);
                }
                catch (Exception ex) when (attempt < _maxRetries && ShouldRetry(ex))
                {
                    await Task.Delay(currentDelay).ConfigureAwait(false);
                    currentDelay = (int)Math.Min(currentDelay * _backoffMultiplier, _maxDelayMs);
                }
            }

            // All retries exhausted, throw original exception
            return await operation().ConfigureAwait(false);
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
