// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Thrown when the key store (database or cache) is temporarily unreachable
/// and the gateway cannot verify API key authenticity.
/// </summary>
public class KeyStoreUnavailableException : ApiKeyGatewayException
{
    /// <summary>Name of the operation that failed</summary>
    public string? Operation { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="KeyStoreUnavailableException"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    public KeyStoreUnavailableException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of <see cref="KeyStoreUnavailableException"/> with operation context
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="operation">Name of the operation that failed.</param>
    public KeyStoreUnavailableException(string message, string operation) : base(message)
    {
        Operation = operation;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="KeyStoreUnavailableException"/> with inner exception
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public KeyStoreUnavailableException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of <see cref="KeyStoreUnavailableException"/> with operation and inner exception
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="operation">Name of the operation that failed.</param>
    /// <param name="innerException">The inner exception.</param>
    public KeyStoreUnavailableException(string message, string operation, Exception innerException) : base(message, innerException)
    {
        Operation = operation;
    }
}
