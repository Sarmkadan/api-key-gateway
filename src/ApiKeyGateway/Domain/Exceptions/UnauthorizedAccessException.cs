// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Thrown when authentication fails or credentials are missing
/// </summary>
public class UnauthorizedAccessException : ApiKeyGatewayException
{
    public string? Reason { get; init; }
    public string? SourceIp { get; init; }

    public UnauthorizedAccessException(string message) : base(message) { }

    public UnauthorizedAccessException(string message, string reason) : base(message)
    {
        Reason = reason;
    }

    public UnauthorizedAccessException(string message, string reason, string sourceIp)
        : base(message)
    {
        Reason = reason;
        SourceIp = sourceIp;
    }
}
