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
    public string? Operation { get; init; }

    public KeyStoreUnavailableException(string message) : base(message) { }

    public KeyStoreUnavailableException(string message, string operation) : base(message)
    {
        Operation = operation;
    }

    public KeyStoreUnavailableException(string message, Exception innerException)
        : base(message, innerException) { }

    public KeyStoreUnavailableException(string message, string operation, Exception innerException)
        : base(message, innerException)
    {
        Operation = operation;
    }
}
