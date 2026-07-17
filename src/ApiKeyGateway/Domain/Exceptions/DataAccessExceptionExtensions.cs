using System;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Extension methods for <see cref="DataAccessException"/> that provide
/// additional diagnostics and helper functionality.
/// </summary>
public static class DataAccessExceptionExtensions
{
    /// <summary>
    /// Returns a formatted string that combines the exception message with
    /// the <c>Operation</c> and <c>Entity</c> details, if present.
    /// </summary>
    /// <param name="exception">The exception to format.</param>
    /// <returns>A detailed message suitable for logging.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    public static string ToDetailedMessage(this DataAccessException exception) =>
        $"{exception.Message} (Operation: {exception.Operation ?? "N/A"}, Entity: {exception.Entity ?? "N/A"})";

    /// <summary>
    /// Retrieves the <c>Operation</c> value, or returns <paramref name="fallback"/>
    /// when the property is <c>null</c> or empty.
    /// </summary>
    /// <param name="exception">The exception whose operation is queried.</param>
    /// <param name="fallback">The value to return when <c>Operation</c> is not set.</param>
    /// <returns>The operation name or the fallback value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fallback"/> is <c>null</c> or empty.</exception>
    public static string GetOperationOrDefault(this DataAccessException exception, string fallback = "unknown")
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(fallback);
        return string.IsNullOrEmpty(exception.Operation) ? fallback : exception.Operation;
    }

    /// <summary>
    /// Retrieves the <c>Entity</c> value, or returns <paramref name="fallback"/>
    /// when the property is <c>null</c> or empty.
    /// </summary>
    /// <param name="exception">The exception whose entity is queried.</param>
    /// <param name="fallback">The value to return when <c>Entity</c> is not set.</param>
    /// <returns>The entity name or the fallback value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fallback"/> is <c>null</c> or empty.</exception>
    public static string GetEntityOrDefault(this DataAccessException exception, string fallback = "unspecified")
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(fallback);
        return string.IsNullOrEmpty(exception.Entity) ? fallback : exception.Entity;
    }

    /// <summary>
    /// Determines whether the exception lacks entity context, indicating a potentially broader/system-level failure
    /// rather than a specific entity operation failure.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="true"/> when <see cref="DataAccessException.Entity"/> is <see langword="null"/> or empty,
    /// indicating the failure may not be tied to a specific entity type.
    /// </remarks>
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns><see langword="true"/> when entity context is missing; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static bool LacksEntityContext(this DataAccessException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return string.IsNullOrEmpty(exception.Entity);
    }
}