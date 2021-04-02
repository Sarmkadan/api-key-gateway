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
    /// <summary>Name of the operation that failed</summary>
    public string? Operation { get; init; }

    /// <summary>Name of the entity type involved in the operation</summary>
    public string? Entity { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="DataAccessException"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    public DataAccessException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of <see cref="DataAccessException"/> with operation context
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="operation">Name of the operation that failed.</param>
    public DataAccessException(string message, string operation) : base(message)
    {
        Operation = operation;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DataAccessException"/> with operation and entity context
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="operation">Name of the operation that failed.</param>
    /// <param name="entity">Name of the entity type involved in the operation.</param>
    public DataAccessException(string message, string operation, string entity)
        : base(message)
    {
        Operation = operation;
        Entity = entity;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DataAccessException"/> with operation, entity, and inner exception
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="operation">Name of the operation that failed.</param>
    /// <param name="entity">Name of the entity type involved in the operation.</param>
    /// <param name="innerException">The inner exception.</param>
    public DataAccessException(string message, string operation, string entity, Exception innerException)
        : base(message, innerException)
    {
        Operation = operation;
        Entity = entity;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DataAccessException"/> with inner exception
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DataAccessException(string message, Exception innerException) : base(message, innerException) { }
}
