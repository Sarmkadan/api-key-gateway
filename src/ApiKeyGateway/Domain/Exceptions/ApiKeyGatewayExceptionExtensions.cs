// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Extension methods for ApiKeyGatewayException providing fluent API and common operations
// =============================================================================

using System.Globalization;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Extension methods for <see cref="ApiKeyGatewayException"/> providing fluent API and common operations
/// </summary>
public static class ApiKeyGatewayExceptionExtensions
{
    /// <summary>
    /// Creates a new <see cref="ApiKeyGatewayException"/> with the same error code and inner exception, but with updated message
    /// </summary>
    /// <param name="exception">The original exception (cannot be null)</param>
    /// <param name="message">The new message to use</param>
    /// <returns>A new exception instance with the updated message</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null</exception>
    public static ApiKeyGatewayException WithMessage(
        this ApiKeyGatewayException exception,
        string message)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(message);

        return exception.ErrorCode is { } errorCode
            ? new ApiKeyGatewayException(message, errorCode, exception)
            : new ApiKeyGatewayException(message, exception);
    }

    /// <summary>
    /// Creates a new <see cref="ApiKeyGatewayException"/> with the same message and inner exception, but with updated error code
    /// </summary>
    /// <param name="exception">The original exception (cannot be null)</param>
    /// <param name="errorCode">The new error code to use</param>
    /// <returns>A new exception instance with the updated error code</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null</exception>
    public static ApiKeyGatewayException WithErrorCode(
        this ApiKeyGatewayException exception,
        string errorCode)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(errorCode);

        return exception.InnerException is { } innerException
            ? new ApiKeyGatewayException(exception.Message, errorCode, innerException)
            : new ApiKeyGatewayException(exception.Message, errorCode);
    }

    /// <summary>
    /// Creates a new <see cref="ApiKeyGatewayException"/> with the same error code and message, but with updated inner exception
    /// </summary>
    /// <param name="exception">The original exception (cannot be null)</param>
    /// <param name="innerException">The new inner exception to use</param>
    /// <returns>A new exception instance with the updated inner exception</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null</exception>
    public static ApiKeyGatewayException WithInnerException(
        this ApiKeyGatewayException exception,
        Exception innerException)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(innerException);

        return exception.ErrorCode is { } errorCode
            ? new ApiKeyGatewayException(exception.Message, errorCode, innerException)
            : new ApiKeyGatewayException(exception.Message, innerException);
    }

    /// <summary>
    /// Determines whether the exception has a specific error code
    /// </summary>
    /// <param name="exception">The exception to check (cannot be null)</param>
    /// <param name="errorCode">The error code to match against</param>
    /// <returns>True if the exception has the specified error code; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null</exception>
    /// <exception cref="ArgumentNullException">Thrown when errorCode is null</exception>
    public static bool HasErrorCode(
        this ApiKeyGatewayException exception,
        string errorCode)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(errorCode);

        return string.Equals(exception.ErrorCode, errorCode, StringComparison.Ordinal);
    }
}