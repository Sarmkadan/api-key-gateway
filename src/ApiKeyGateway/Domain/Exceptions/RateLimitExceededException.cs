// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Thrown when a request exceeds the configured rate limit for an API key
/// </summary>
public class RateLimitExceededException : ApiKeyGatewayException
{
    /// <summary>ID of the API key that exceeded its rate limit</summary>
    public string ApiKeyId { get; init; }

    /// <summary>Configured limit that was exceeded</summary>
    public int Limit { get; init; }

    /// <summary>Duration of the rate limit window in seconds</summary>
    public int WindowInSeconds { get; init; }

    /// <summary>Recommended time to wait before retrying (UTC)</summary>
    public DateTime? RetryAfter { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="RateLimitExceededException"/>
    /// </summary>
    public RateLimitExceededException()
        : base("Rate limit exceeded")
    {
        ApiKeyId = string.Empty;
        Limit = 0;
        WindowInSeconds = 0;
        RetryAfter = null;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RateLimitExceededException"/> with a message
    /// </summary>
    /// <param name="message">The error message.</param>
    public RateLimitExceededException(string message)
        : base(message)
    {
        ApiKeyId = string.Empty;
        Limit = 0;
        WindowInSeconds = 0;
        RetryAfter = null;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RateLimitExceededException"/> with rate limit details
    /// </summary>
    /// <param name="apiKeyId">ID of the API key that exceeded its rate limit.</param>
    /// <param name="limit">Configured limit that was exceeded.</param>
    /// <param name="windowInSeconds">Duration of the rate limit window in seconds.</param>
    public RateLimitExceededException(string apiKeyId, int limit, int windowInSeconds)
        : base(string.Format(Domain.Constants.ErrorMessages.RateLimitExceeded, limit, GetTimeUnit(windowInSeconds)))
    {
        ApiKeyId = apiKeyId ?? throw new ArgumentNullException(nameof(apiKeyId));
        Limit = limit;
        WindowInSeconds = windowInSeconds;
        RetryAfter = DateTime.UtcNow.AddSeconds(windowInSeconds);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RateLimitExceededException"/> with message and inner exception
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public RateLimitExceededException(string message, Exception innerException)
        : base(message, innerException)
    {
        ApiKeyId = string.Empty;
        Limit = 0;
        WindowInSeconds = 0;
        RetryAfter = null;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RateLimitExceededException"/> with all parameters
    /// </summary>
    /// <param name="apiKeyId">ID of the API key that exceeded its rate limit.</param>
    /// <param name="limit">Configured limit that was exceeded.</param>
    /// <param name="windowInSeconds">Duration of the rate limit window in seconds.</param>
    /// <param name="innerException">The inner exception.</param>
    public RateLimitExceededException(string apiKeyId, int limit, int windowInSeconds, Exception innerException)
        : base(string.Format(Domain.Constants.ErrorMessages.RateLimitExceeded, limit, GetTimeUnit(windowInSeconds)), innerException)
    {
        ApiKeyId = apiKeyId ?? throw new ArgumentNullException(nameof(apiKeyId));
        Limit = limit;
        WindowInSeconds = windowInSeconds;
        RetryAfter = DateTime.UtcNow.AddSeconds(windowInSeconds);
    }

    private static string GetTimeUnit(int seconds) => seconds switch
    {
        1 => "second",
        60 => "minute",
        3600 => "hour",
        86400 => "day",
        _ => $"{seconds} seconds"
    };
}