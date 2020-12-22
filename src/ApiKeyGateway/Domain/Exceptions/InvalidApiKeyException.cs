// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Thrown when an API key is invalid, expired, or disabled
/// </summary>
public class InvalidApiKeyException : ApiKeyGatewayException
{
    public string? ApiKeyHash { get; init; }
    public new DateTime OccurredAt { get; init; }
    public bool IsExpired { get; init; }

    public InvalidApiKeyException(string message) : base(message)
    {
        OccurredAt = DateTime.UtcNow;
    }

    public InvalidApiKeyException(string message, string apiKeyHash) : base(message)
    {
        ApiKeyHash = apiKeyHash;
        OccurredAt = DateTime.UtcNow;
    }

    public InvalidApiKeyException(string message, Exception innerException)
        : base(message, innerException)
    {
        OccurredAt = DateTime.UtcNow;
    }

    public InvalidApiKeyException(string message, bool isExpired) : base(message)
    {
        IsExpired = isExpired;
        OccurredAt = DateTime.UtcNow;
    }
}
