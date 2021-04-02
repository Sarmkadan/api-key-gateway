// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Thrown when an API key is invalid, expired, or disabled
/// </summary>
/// <summary>
/// Thrown when an API key is invalid, expired, or disabled
/// </summary>
public class InvalidApiKeyException : ApiKeyGatewayException
{
    /// <summary>Hash of the invalid API key</summary>
    public string? ApiKeyHash { get; init; }

    /// <summary>Timestamp when the exception occurred</summary>
    public new DateTime OccurredAt { get; init; }

    /// <summary>Whether the key was expired when this exception was thrown</summary>
    public bool IsExpired { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="InvalidApiKeyException"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    public InvalidApiKeyException(string message) : base(message)
    {
        OccurredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InvalidApiKeyException"/> with the key hash
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="apiKeyHash">Hash of the invalid API key.</param>
    public InvalidApiKeyException(string message, string apiKeyHash) : base(message)
    {
        ApiKeyHash = apiKeyHash;
        OccurredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InvalidApiKeyException"/> with an inner exception
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public InvalidApiKeyException(string message, Exception innerException)
        : base(message, innerException)
    {
        OccurredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InvalidApiKeyException"/> indicating expiration
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="isExpired">Whether the key was expired.</param>
    public InvalidApiKeyException(string message, bool isExpired) : base(message)
    {
        IsExpired = isExpired;
        OccurredAt = DateTime.UtcNow;
    }
}
