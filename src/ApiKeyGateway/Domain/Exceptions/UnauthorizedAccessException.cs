// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Thrown when authentication fails or credentials are missing
/// </summary>
public class UnauthorizedAccessException : Exception
{
    /// <summary>Reason for the unauthorized access</summary>
    public string? Reason { get; init; }

    /// <summary>Source IP address where the unauthorized request originated</summary>
    public string? SourceIp { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="UnauthorizedAccessException"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    public UnauthorizedAccessException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of <see cref="UnauthorizedAccessException"/> with reason
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="reason">Reason for the unauthorized access.</param>
    public UnauthorizedAccessException(string message, string reason) : base(message)
    {
        Reason = reason;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="UnauthorizedAccessException"/> with reason and source IP
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="reason">Reason for the unauthorized access.</param>
    /// <param name="sourceIp">Source IP address where the unauthorized request originated.</param>
    public UnauthorizedAccessException(string message, string reason, string sourceIp)
        : base(message)
    {
        Reason = reason;
        SourceIp = sourceIp;
    }
}
