// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Base exception class for all api-key-gateway specific exceptions
// =============================================================================

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Base exception class for all api-key-gateway specific exceptions
/// </summary>
public class ApiKeyGatewayException : Exception
{
    public string? ErrorCode { get; init; }
    public DateTime OccurredAt { get; init; }

    public ApiKeyGatewayException(string message) : base(message)
    {
        OccurredAt = DateTime.UtcNow;
    }

    public ApiKeyGatewayException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
        OccurredAt = DateTime.UtcNow;
    }

    public ApiKeyGatewayException(string message, Exception innerException) : base(message, innerException)
    {
        OccurredAt = DateTime.UtcNow;
    }

    public ApiKeyGatewayException(string message, string errorCode, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
        OccurredAt = DateTime.UtcNow;
    }
}