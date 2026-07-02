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
    public string? ParameterName { get; init; }
    public object? AttemptedValue { get; init; }
    public IEnumerable<string>? ValidationErrors { get; init; }

    public ValidationException(string message) : base(message)
    {
        ErrorCode = "VALIDATION_ERROR";
    }

    public ValidationException(string message, string parameterName, object? attemptedValue) : base(message)
    {
        ErrorCode = "VALIDATION_ERROR";
        ParameterName = parameterName;
        AttemptedValue = attemptedValue;
    }

    public ValidationException(string message, IEnumerable<string> validationErrors) : base(message)
    {
        ErrorCode = "VALIDATION_ERROR";
        ValidationErrors = validationErrors;
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "VALIDATION_ERROR";
    }
}