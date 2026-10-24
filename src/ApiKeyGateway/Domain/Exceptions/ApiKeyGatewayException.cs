// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Base exception class for all api-key-gateway specific exceptions
// =============================================================================

using System.Text.Json.Serialization;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Base exception class for all api-key-gateway specific exceptions
/// </summary>
public class ApiKeyGatewayException : Exception
{
    /// <summary>Human-readable error message.</summary>
    [JsonPropertyOrder(-10)]
    public new string Message => base.Message;

    /// <summary>Optional machine-readable error code.</summary>
    [JsonPropertyOrder(10)]
    public string? ErrorCode { get; init; }

    /// <summary>UTC time when the exception was created.</summary>
    [JsonPropertyOrder(20)]
    public DateTime OccurredAt { get; init; }

    /// <summary>Parameterless constructor for JSON deserialization</summary>
    public ApiKeyGatewayException() : base("An error occurred")
    {
        OccurredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    [JsonConstructor]
    public ApiKeyGatewayException(string message) : base(message)
    {
        OccurredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance with the specified message and error code.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The machine-readable error code.</param>
    public ApiKeyGatewayException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
        OccurredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this error.</param>
    public ApiKeyGatewayException(string message, Exception innerException) : base(message, innerException)
    {
        OccurredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance with the specified message, error code, and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The machine-readable error code.</param>
    /// <param name="innerException">The exception that caused this error.</param>
    public ApiKeyGatewayException(string message, string errorCode, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
        OccurredAt = DateTime.UtcNow;
    }
}