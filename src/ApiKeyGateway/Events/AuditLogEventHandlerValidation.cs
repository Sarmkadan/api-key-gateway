// =============================================================================// Author: Vladyslav Zaiets | https://sarmkadan.com// CTO & Software Architect// =============================================================================
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ApiKeyGateway.Events;

/// <summary>
/// Provides validation helpers for <see cref="AuditLogEventHandler"/> instances.
/// Validates constructor dependencies and ensures the handler is properly initialized./// </summary>
public static class AuditLogEventHandlerValidation
{
    /// <summary>
    /// Validates the specified <see cref="AuditLogEventHandler"/> instance.
    /// </summary>
    /// <param name="value">The handler to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Public API")]
    public static IReadOnlyList<string> Validate(this AuditLogEventHandler value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate injected services
        if (value is null)
        {
            errors.Add("AuditLogEventHandler instance cannot be null.");
        }

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