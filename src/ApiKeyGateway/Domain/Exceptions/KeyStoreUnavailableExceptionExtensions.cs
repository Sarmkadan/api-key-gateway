// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Diagnostics;
using System.Globalization;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Extension methods for <see cref="KeyStoreUnavailableException"/> that provide
/// convenient ways to create, analyze, and format exception instances.
/// </summary>
public static class KeyStoreUnavailableExceptionExtensions
{
    /// <summary>
    /// Creates a new <see cref="KeyStoreUnavailableException"/> with a standardized message
    /// indicating the key store is unavailable for the specified operation.
    /// </summary>
    /// <param name="exception">The source exception instance.</param>
    /// <param name="operation">The operation that failed due to unavailable key store.</param>
    /// <returns>A new exception instance with a descriptive message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static KeyStoreUnavailableException WithOperation(
        this KeyStoreUnavailableException exception,
        string operation)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(operation);

        return new KeyStoreUnavailableException(
            $"Key store is unavailable during operation: {operation}",
            operation,
            exception);
    }

    /// <summary>
    /// Creates a new <see cref="KeyStoreUnavailableException"/> indicating a cache miss
    /// for the specified key, suggesting the key may not exist in the store.
    /// </summary>
    /// <param name="exception">The source exception instance.</param>
    /// <param name="key">The API key that was not found in the store.</param>
    /// <returns>A new exception instance indicating a cache miss scenario.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static KeyStoreUnavailableException WithCacheMiss(
        this KeyStoreUnavailableException exception,
        string key)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return new KeyStoreUnavailableException(
            $"API key '{key}' not found in key store (cache miss)",
            nameof(WithCacheMiss),
            exception);
    }

    /// <summary>
    /// Creates a new <see cref="KeyStoreUnavailableException"/> with a message
    /// formatted for logging purposes, including operation context.
    /// </summary>
    /// <param name="exception">The source exception instance.</param>
    /// <param name="context">Additional context about the failure scenario.</param>
    /// <returns>A new exception instance with formatted message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static KeyStoreUnavailableException WithContext(
        this KeyStoreUnavailableException exception,
        string context)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(context);

        return new KeyStoreUnavailableException(
            $"Key store unavailable: {context}",
            exception);
    }

    /// <summary>
    /// Gets a collection of all operation names associated with this exception
    /// and its inner exceptions, suitable for diagnostic reporting.
    /// </summary>
    /// <param name="exception">The source exception instance.</param>
    /// <returns>An enumerable of operation names, or empty if none available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static IEnumerable<string> GetAllOperations(
        this KeyStoreUnavailableException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var operations = new List<string>();
        CollectOperations(exception, operations);
        return operations.AsReadOnly();
    }

    private static void CollectOperations(
        Exception? ex,
        List<string> operations)
    {
        if (ex is KeyStoreUnavailableException kse && !string.IsNullOrEmpty(kse.Operation))
        {
            operations.Add(kse.Operation);
        }

        if (ex?.InnerException is not null)
        {
            CollectOperations(ex.InnerException, operations);
        }
    }

    /// <summary>
    /// Determines whether this exception represents a transient failure that
    /// might succeed on retry, based on the exception type and message content.
    /// </summary>
    /// <param name="exception">The source exception instance.</param>
    /// <returns>
    /// True if the failure appears transient (network issues, timeouts);
    /// false if it's likely a persistent problem (schema issues, permissions).
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static bool IsLikelyTransient(this KeyStoreUnavailableException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            { Message: var msg } when msg.Contains("timeout", StringComparison.OrdinalIgnoreCase) => true,
            { Message: var msg } when msg.Contains("unavailable", StringComparison.OrdinalIgnoreCase) => true,
            { Message: var msg } when msg.Contains("network", StringComparison.OrdinalIgnoreCase) => true,
            { Message: var msg } when msg.Contains("connection", StringComparison.OrdinalIgnoreCase) => true,
            { Message: var msg } when msg.Contains("temporarily", StringComparison.OrdinalIgnoreCase) => true,
            _ => false
        };
    }

    /// <summary>
    /// Creates a diagnostic string representation of this exception,
    /// including all operations and inner exception details.
    /// </summary>
    /// <param name="exception">The source exception instance.</param>
    /// <returns>A formatted diagnostic string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static string ToDiagnosticString(this KeyStoreUnavailableException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        writer.WriteLine("KeyStoreUnavailableException Diagnostic Report:");
        writer.WriteLine("==========================================");
        writer.WriteLine($"Message: {exception.Message}");

        if (!string.IsNullOrEmpty(exception.Operation))
        {
            writer.WriteLine($"Operation: {exception.Operation}");
        }

        if (exception.InnerException is not null)
        {
            writer.WriteLine($"Inner Exception: {exception.InnerException.GetType().Name}");
            writer.WriteLine($"Inner Message: {exception.InnerException.Message}");
        }

        writer.WriteLine("==========================================");
        return writer.ToString();
    }
}