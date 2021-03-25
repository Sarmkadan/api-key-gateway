// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Provides validation helpers for API key models used in the ApiKeysController.
/// </summary>
public static class ApiKeysControllerValidation
{
    /// <summary>
    /// Validates an <see cref="ApiKey"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The API key instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ApiKey value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            problems.Add("Id must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(value.ConsumerId))
        {
            problems.Add("ConsumerId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Name must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(value.KeyHash))
        {
            problems.Add("KeyHash must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(value.Prefix))
        {
            problems.Add("Prefix must not be empty.");
        }

        // Validate dates
        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt must be set to a valid date.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("CreatedAt cannot be in the future.");
        }

        if (value.ExpiresAt.HasValue)
        {
            if (value.ExpiresAt.Value < value.CreatedAt)
            {
                problems.Add("ExpiresAt cannot be before CreatedAt.");
            }

            if (value.ExpiresAt.Value > DateTime.UtcNow.AddYears(10))
            {
                problems.Add("ExpiresAt cannot be more than 10 years in the future.");
            }
        }

        if (value.LastUsedAt.HasValue)
        {
            if (value.LastUsedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("LastUsedAt cannot be in the future.");
            }

            if (value.LastUsedAt.Value < value.CreatedAt)
            {
                problems.Add("LastUsedAt cannot be before CreatedAt.");
            }
        }

        if (value.DisabledAt.HasValue)
        {
            if (value.DisabledAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("DisabledAt cannot be in the future.");
            }

            if (value.DisabledAt.Value < value.CreatedAt)
            {
                problems.Add("DisabledAt cannot be before CreatedAt.");
            }
        }

        // Validate numeric properties
        if (value.RequestCount < 0)
        {
            problems.Add("RequestCount must be non-negative.");
        }

        if (value.BytesTransferred < 0)
        {
            problems.Add("BytesTransferred must be non-negative.");
        }

        // Validate status - enum values are always valid by definition
        // No additional validation needed for enum values

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an <see cref="ApiKey"/> instance is valid.
    /// </summary>
    /// <param name="value">The API key instance to check.</param>
    /// <returns><c>true</c> if the instance is valid; <c>false</c> otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ApiKey value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="ApiKey"/> instance is valid, throwing an <see cref="ArgumentException"/> if it's not.
    /// </summary>
    /// <param name="value">The API key instance to ensure.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this ApiKey value)
    {
        var problems = new List<string>(Validate(value));

        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid ApiKey instance: {string.Join("; ", problems)}", nameof(value));
        }
    }

    /// <summary>
    /// Validates a <see cref="CreateKeyRequest"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The create key request instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Controllers.CreateKeyRequest value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.ConsumerId))
        {
            problems.Add("ConsumerId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Name must not be empty.");
        }

        if (value.ExpirationDays.HasValue && value.ExpirationDays <= 0)
        {
            problems.Add("ExpirationDays must be a positive integer when specified.");
        }

        if (value.ExpirationDays.HasValue && value.ExpirationDays > 3650)
        {
            problems.Add("ExpirationDays cannot exceed 3650 days (10 years).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="CreateKeyRequest"/> instance is valid.
    /// </summary>
    /// <param name="value">The create key request instance to check.</param>
    /// <returns><c>true</c> if the instance is valid; <c>false</c> otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Controllers.CreateKeyRequest value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="CreateKeyRequest"/> instance is valid, throwing an <see cref="ArgumentException"/> if it's not.
    /// </summary>
    /// <param name="value">The create key request instance to ensure.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this Controllers.CreateKeyRequest value)
    {
        var problems = new List<string>(Validate(value));

        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid CreateKeyRequest instance: {string.Join("; ", problems)}", nameof(value));
        }
    }

    /// <summary>
    /// Validates a <see cref="CreateKeyResponse"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The create key response instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Controllers.CreateKeyResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.KeyId))
        {
            problems.Add("KeyId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(value.ConsumerId))
        {
            problems.Add("ConsumerId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Name must not be empty.");
        }

        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt must be set to a valid date.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("CreatedAt cannot be in the future.");
        }

        if (value.ExpiresAt.HasValue)
        {
            if (value.ExpiresAt.Value < value.CreatedAt)
            {
                problems.Add("ExpiresAt cannot be before CreatedAt.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="CreateKeyResponse"/> instance is valid.
    /// </summary>
    /// <param name="value">The create key response instance to check.</param>
    /// <returns><c>true</c> if the instance is valid; <c>false</c> otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Controllers.CreateKeyResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="CreateKeyResponse"/> instance is valid, throwing an <see cref="ArgumentException"/> if it's not.
    /// </summary>
    /// <param name="value">The create key response instance to ensure.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this Controllers.CreateKeyResponse value)
    {
        var problems = new List<string>(Validate(value));

        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid CreateKeyResponse instance: {string.Join("; ", problems)}", nameof(value));
        }
    }

    /// <summary>
    /// Validates a <see cref="GetKeyResponse"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The get key response instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Controllers.GetKeyResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.KeyId))
        {
            problems.Add("KeyId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(value.ConsumerId))
        {
            problems.Add("ConsumerId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Name must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(value.Status))
        {
            problems.Add("Status must not be empty.");
        }

        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt must be set to a valid date.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("CreatedAt cannot be in the future.");
        }

        if (value.ExpiresAt.HasValue)
        {
            if (value.ExpiresAt.Value < value.CreatedAt)
            {
                problems.Add("ExpiresAt cannot be before CreatedAt.");
            }
        }

        if (value.LastUsedAt.HasValue)
        {
            if (value.LastUsedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("LastUsedAt cannot be in the future.");
            }

            if (value.LastUsedAt.Value < value.CreatedAt)
            {
                problems.Add("LastUsedAt cannot be before CreatedAt.");
            }
        }

        if (value.RequestCount < 0)
        {
            problems.Add("RequestCount must be non-negative.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="GetKeyResponse"/> instance is valid.
    /// </summary>
    /// <param name="value">The get key response instance to check.</param>
    /// <returns><c>true</c> if the instance is valid; <c>false</c> otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Controllers.GetKeyResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="GetKeyResponse"/> instance is valid, throwing an <see cref="ArgumentException"/> if it's not.
    /// </summary>
    /// <param name="value">The get key response instance to ensure.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this Controllers.GetKeyResponse value)
    {
        var problems = new List<string>(Validate(value));

        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid GetKeyResponse instance: {string.Join("; ", problems)}", nameof(value));
        }
    }

    /// <summary>
    /// Validates a <see cref="RotateKeyResponse"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The rotate key response instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Controllers.RotateKeyResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.OldKeyId))
        {
            problems.Add("OldKeyId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(value.NewKeyId))
        {
            problems.Add("NewKeyId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(value.ConsumerId))
        {
            problems.Add("ConsumerId must not be empty.");
        }

        if (value.NewKeyExpiresAt.HasValue)
        {
            if (value.NewKeyExpiresAt.Value < DateTime.UtcNow)
            {
                problems.Add("NewKeyExpiresAt cannot be in the past.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="RotateKeyResponse"/> instance is valid.
    /// </summary>
    /// <param name="value">The rotate key response instance to check.</param>
    /// <returns><c>true</c> if the instance is valid; <c>false</c> otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Controllers.RotateKeyResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="RotateKeyResponse"/> instance is valid, throwing an <see cref="ArgumentException"/> if it's not.
    /// </summary>
    /// <param name="value">The rotate key response instance to ensure.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this Controllers.RotateKeyResponse value)
    {
        var problems = new List<string>(Validate(value));

        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid RotateKeyResponse instance: {string.Join("; ", problems)}", nameof(value));
        }
    }
}