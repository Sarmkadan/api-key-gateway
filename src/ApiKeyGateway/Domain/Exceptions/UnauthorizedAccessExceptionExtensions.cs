// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="UnauthorizedAccessException"/> to simplify common operations
/// </summary>
public static class UnauthorizedAccessExceptionExtensions
{
    /// <summary>
    /// Creates a new <see cref="UnauthorizedAccessException"/> with the same properties as the source exception
    /// </summary>
    /// <param name="exception">The source exception (cannot be null)</param>
    /// <returns>A new exception with the same message, reason, and source IP</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null</exception>
    public static UnauthorizedAccessException WithSourceIp(this UnauthorizedAccessException exception, string sourceIp)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(sourceIp);

        return new UnauthorizedAccessException(exception.Message, exception.Reason, sourceIp)
        {
            Source = exception.Source
        };
    }

    /// <summary>
    /// Creates a new <see cref="UnauthorizedAccessException"/> with an updated reason
    /// </summary>
    /// <param name="exception">The source exception (cannot be null)</param>
    /// <param name="newReason">The new reason value</param>
    /// <returns>A new exception with the updated reason</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null</exception>
    public static UnauthorizedAccessException WithReason(this UnauthorizedAccessException exception, string newReason)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return new UnauthorizedAccessException(exception.Message, newReason, exception.SourceIp)
        {
            Source = exception.Source
        };
    }

    /// <summary>
    /// Creates a new <see cref="UnauthorizedAccessException"/> with an updated message
    /// </summary>
    /// <param name="exception">The source exception (cannot be null)</param>
    /// <param name="newMessage">The new message value</param>
    /// <returns>A new exception with the updated message</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null</exception>
    public static UnauthorizedAccessException WithMessage(this UnauthorizedAccessException exception, string newMessage)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(newMessage);

        return new UnauthorizedAccessException(newMessage, exception.Reason, exception.SourceIp)
        {
            Source = exception.Source
        };
    }

    /// <summary>
    /// Gets a formatted string representation of the exception including reason and source IP if available
    /// </summary>
    /// <param name="exception">The source exception (cannot be null)</param>
    /// <returns>A formatted string suitable for logging or error responses</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null</exception>
    public static string ToFormattedString(this UnauthorizedAccessException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception.Reason is null && exception.SourceIp is null)
        {
            return exception.Message;
        }

        var parts = new List<string>();
        parts.Add(exception.Message);

        if (exception.Reason is not null)
        {
            parts.Add($"Reason: {exception.Reason}");
        }

        if (exception.SourceIp is not null)
        {
            parts.Add($"Source IP: {exception.SourceIp}");
        }

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Checks if the exception has a specific reason
    /// </summary>
    /// <param name="exception">The source exception (cannot be null)</param>
    /// <param name="reasonToMatch">The reason to check for (case-insensitive)</param>
    /// <returns>True if the exception has the specified reason; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="reasonToMatch"/> is null or empty</exception>
    public static bool HasReason(this UnauthorizedAccessException exception, string reasonToMatch)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(reasonToMatch);

        return string.Equals(exception.Reason, reasonToMatch, StringComparison.OrdinalIgnoreCase);
    }
}