// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    /// <param name="apiKeyId">ID of the API key that exceeded its rate limit.</param>
    /// <param name="limit">Configured limit that was exceeded.</param>
    /// <param name="windowInSeconds">Duration of the rate limit window in seconds.</param>
    public RateLimitExceededException(string apiKeyId, int limit, int windowInSeconds)
        : base(string.Format(Domain.Constants.ErrorMessages.RateLimitExceeded, limit, GetTimeUnit(windowInSeconds)))
    {
        ApiKeyId = apiKeyId;
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
