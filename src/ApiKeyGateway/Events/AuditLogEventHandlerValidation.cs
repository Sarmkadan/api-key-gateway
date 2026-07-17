// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ApiKeyGateway.Events;

/// <summary>
/// Provides validation helpers for <see cref="AuditLogEventHandler"/> instances.
/// Validates constructor dependencies and ensures the handler is properly initialized.
/// </summary>
public static class AuditLogEventHandlerValidation
{
    /// <summary>
    /// Validates the specified <see cref="AuditLogEventHandler"/> instance.
    /// </summary>
    /// <param name="value">The handler to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Public API")]
    public static IReadOnlyList<string> Validate(this AuditLogEventHandler value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Since AuditLogEventHandler is sealed and dependencies are set in constructor,
        // the only validation needed is null check which is already performed above.
        // This method is kept for API consistency with extension methods pattern.

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="AuditLogEventHandler"/> is valid.
    /// </summary>
    /// <param name="value">The handler to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this AuditLogEventHandler value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="AuditLogEventHandler"/> is valid.
    /// </summary>
    /// <param name="value">The handler to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the handler is not valid.</exception>
    public static void EnsureValid(this AuditLogEventHandler value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"AuditLogEventHandler is not valid. Problems: {string.Join("; ", errors)}");
        }
    }
}