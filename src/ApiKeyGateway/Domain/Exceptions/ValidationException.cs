// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Thrown when validation of input parameters fails
// =============================================================================

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Thrown when validation of input parameters fails
/// </summary>
public class ValidationException : ApiKeyGatewayException
{
    /// <summary>Name of the parameter that failed validation</summary>
    public string? ParameterName { get; init; }

    /// <summary>Value that was attempted to be used</summary>
    public object? AttemptedValue { get; init; }

    /// <summary>Collection of validation error messages</summary>
    public IEnumerable<string>? ValidationErrors { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="ValidationException"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    public ValidationException(string message) : base(message)
    {
        ErrorCode = "VALIDATION_ERROR";
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ValidationException"/> with parameter context
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="parameterName">Name of the parameter that failed validation.</param>
    /// <param name="attemptedValue">Value that was attempted to be used.</param>
    public ValidationException(string message, string parameterName, object? attemptedValue) : base(message)
    {
        ErrorCode = "VALIDATION_ERROR";
        ParameterName = parameterName;
        AttemptedValue = attemptedValue;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ValidationException"/> with validation errors
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="validationErrors">Collection of validation error messages.</param>
    public ValidationException(string message, IEnumerable<string> validationErrors) : base(message)
    {
        ErrorCode = "VALIDATION_ERROR";
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ValidationException"/> with inner exception
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "VALIDATION_ERROR";
    }
}
