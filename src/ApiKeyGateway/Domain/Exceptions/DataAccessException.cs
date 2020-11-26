// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Thrown when database or repository operations fail
/// </summary>
public class DataAccessException : ApiKeyGatewayException
{
    public string? Operation { get; init; }
    public string? Entity { get; init; }

    public DataAccessException(string message) : base(message) { }

    public DataAccessException(string message, string operation) : base(message)
    {
        Operation = operation;
    }

    public DataAccessException(string message, string operation, string entity)
        : base(message)
    {
        Operation = operation;
        Entity = entity;
    }

    public DataAccessException(string message, Exception innerException)
        : base(message, innerException) { }
}
