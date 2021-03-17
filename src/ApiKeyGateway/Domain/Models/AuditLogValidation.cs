// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="AuditLog"/> instances
/// </summary>
public static class AuditLogValidation
{
    /// <summary>
    /// Validates an <see cref="AuditLog"/> instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The audit log to validate</param>
    /// <returns>An enumerable of validation error messages; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this AuditLog value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id is required and cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.ResourceId))
        {
            errors.Add("ResourceId is required and cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.ResourceType))
        {
            errors.Add("ResourceType is required and cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.PerformedBy))
        {
            errors.Add("PerformedBy is required and cannot be empty or whitespace.");
        }

        // Validate Action enum
        if (value.Action == default)
        {
            errors.Add("Action is required and cannot be the default value.");
        }

        // Validate PerformedAt
        if (value.PerformedAt == default)
        {
            errors.Add("PerformedAt is required and cannot be the default DateTime value.");
        }

        // Validate HttpStatusCode if set
        if (value.HttpStatusCode.HasValue)
        {
            if (value.HttpStatusCode is < 100 or > 599)
            {
                errors.Add("HttpStatusCode, if provided, must be a valid HTTP status code (100-599).");
            }
        }

        // Validate Changes dictionary
        if (value.Changes is null)
        {
            errors.Add("Changes dictionary cannot be null.");
        }

        // Validate optional string properties if set
        if (value.Reason is not null && string.IsNullOrWhiteSpace(value.Reason))
        {
            errors.Add("Reason cannot be empty or whitespace if provided.");
        }

        if (value.SourceIp is not null && string.IsNullOrWhiteSpace(value.SourceIp))
        {
            errors.Add("SourceIp cannot be empty or whitespace if provided.");
        }

        if (value.ErrorMessage is not null && string.IsNullOrWhiteSpace(value.ErrorMessage))
        {
            errors.Add("ErrorMessage cannot be empty or whitespace if provided.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="AuditLog"/> instance is valid
    /// </summary>
    /// <param name="value">The audit log to check</param>
    /// <returns>True if valid; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this AuditLog value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="AuditLog"/> instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The audit log to validate</param>
    /// <exception cref="ArgumentException">Thrown if value is null or invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static void EnsureValid(this AuditLog value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"AuditLog validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}