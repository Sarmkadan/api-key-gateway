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
    public string ApiKeyId { get; init; }
    public int Limit { get; init; }
    public int WindowInSeconds { get; init; }
    public DateTime? RetryAfter { get; init; }

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
