// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Globalization;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="InvalidApiKeyException"/> to simplify common operations
/// </summary>
public static class InvalidApiKeyExceptionExtensions
{
    /// <summary>
    /// Determines whether the exception represents an expired API key.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception represents an expired key; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static bool IsKeyExpired(this InvalidApiKeyException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.IsExpired;
    }

    /// <summary>
    /// Gets the hash of the invalid API key if available.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>The API key hash if available; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static string? GetApiKeyHash(this InvalidApiKeyException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.ApiKeyHash;
    }

    /// <summary>
    /// Gets the timestamp when the exception occurred.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>The timestamp when the exception occurred.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static DateTime GetOccurredAt(this InvalidApiKeyException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.OccurredAt;
    }

    /// <summary>
    /// Formats the exception details as a string for logging purposes.
    /// </summary>
    /// <param name="exception">The exception to format.</param>
    /// <returns>A formatted string containing exception details.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static string FormatForLogging(this InvalidApiKeyException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var parts = new List<string>();
        parts.Add($"InvalidApiKeyException: {exception.Message}");

        if (!string.IsNullOrEmpty(exception.ApiKeyHash))
        {
            parts.Add($"ApiKeyHash: {exception.ApiKeyHash}");
        }

        parts.Add($"IsExpired: {exception.IsExpired}");
        parts.Add($"OccurredAt: {exception.OccurredAt:yyyy-MM-dd HH:mm:ss UTC}");

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Determines whether the exception represents a disabled API key (not expired).
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception represents a disabled key; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static bool IsKeyDisabled(this InvalidApiKeyException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return !exception.IsExpired;
    }
}