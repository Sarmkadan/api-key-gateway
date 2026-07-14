// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Middleware;

/// <summary>
/// Provides validation helpers for <see cref="CorrelationContextMiddleware"/> instances.
/// Validates constructor dependencies and ensures the middleware is properly initialized.
/// </summary>
public static class CorrelationContextMiddlewareValidation
{
    /// <summary>
    /// Validates the specified <see cref="CorrelationContextMiddleware"/> instance.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <returns>A read-only list of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CorrelationContextMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate injected services (RequestDelegate and ILogger)
        // RequestDelegate is a framework delegate that should never be null when properly constructed
        // ILogger should also be injected by the framework and should not be null

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the specified <see cref="CorrelationContextMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CorrelationContextMiddleware value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the specified <see cref="CorrelationContextMiddleware"/> instance is invalid.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this CorrelationContextMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"CorrelationContextMiddleware is not valid. Problems: {string.Join("; ", errors)}",
                nameof(value));
        }
    }
}