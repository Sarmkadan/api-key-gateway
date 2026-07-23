// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ApiKeyGateway.Validation;

/// <summary>
/// Validation helpers for API key validator parameters.
/// Provides comprehensive validation for inputs to ApiKeyValidator static methods.
/// </summary>
public static class ApiKeyValidatorValidation
{
    /// <summary>
    /// Validates an API key string format.
    /// Returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="key">The API key to validate.</param>
    /// <returns>An IReadOnlyList of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if key is null.</exception>
    public static IReadOnlyList<string> ValidateKeyFormat(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(key))
        {
            problems.Add("API key cannot be null, empty, or whitespace.");
            return problems.AsReadOnly();
        }

        const int MinKeyLength = 32;
        const int MaxKeyLength = 256;

        if (key.Length < MinKeyLength)
        {
            problems.Add(string.Format(CultureInfo.InvariantCulture,
                "API key must be at least {0} characters long.", MinKeyLength));
        }

        if (key.Length > MaxKeyLength)
        {
            problems.Add(string.Format(CultureInfo.InvariantCulture,
                "API key cannot exceed {0} characters.", MaxKeyLength));
        }

        // Check character type entropy using pattern matching
        bool hasUppercase = key.Any(c => char.IsUpper(c));
        bool hasLowercase = key.Any(c => char.IsLower(c));
        bool hasDigits = key.Any(c => char.IsDigit(c));
        bool hasSpecial = key.Any(c => !char.IsLetterOrDigit(c));

        var characterTypesUsed = 0;
        if (hasUppercase) characterTypesUsed++;
        if (hasLowercase) characterTypesUsed++;
        if (hasDigits) characterTypesUsed++;
        if (hasSpecial) characterTypesUsed++;

        if (characterTypesUsed < 3)
        {
            problems.Add("API key should contain a mix of uppercase letters, lowercase letters, digits, and special characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates an API key name/description.
    /// </summary>
    /// <param name="name">The API key name to validate.</param>
    /// <returns>An IReadOnlyList of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if name is null.</exception>
    public static IReadOnlyList<string> ValidateKeyName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var problems = new List<string>();

        const int MinNameLength = 3;
        const int MaxNameLength = 100;

        if (string.IsNullOrWhiteSpace(name))
        {
            problems.Add("API key name cannot be null, empty, or whitespace.");
            return problems.AsReadOnly();
        }

        if (name.Length < MinNameLength)
        {
            problems.Add(string.Format(CultureInfo.InvariantCulture,
                "API key name must be at least {0} characters.", MinNameLength));
        }

        if (name.Length > MaxNameLength)
        {
            problems.Add(string.Format(CultureInfo.InvariantCulture,
                "API key name cannot exceed {0} characters.", MaxNameLength));
        }

        // Check for valid characters using pattern matching for clarity
        if (name.Any(c => !char.IsLetterOrDigit(c) && c is not (' ' or '_' or '-')))
        {
            problems.Add("API key name can only contain letters, digits, spaces, underscores, and hyphens.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a quota limit value against the shared <see cref="Domain.Models.QuotaLimit"/> contract:
    /// the <see cref="Domain.Models.QuotaLimit.Unlimited"/> sentinel (-1) is accepted,
    /// otherwise the limit must be between 1 and <see cref="Domain.Models.QuotaLimit.MaxValue"/>.
    /// </summary>
    /// <param name="limit">The quota limit to validate.</param>
    /// <returns>An IReadOnlyList of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateQuotaLimit(int limit)
    {
        var problems = new List<string>();

        if (limit is Domain.Models.QuotaLimit.Unlimited)
        {
            return problems.AsReadOnly();
        }

        if (limit <= 0)
        {
            problems.Add(string.Format(CultureInfo.InvariantCulture,
                "Quota limit must be greater than 0, or {0} for unlimited.",
                Domain.Models.QuotaLimit.Unlimited));
        }
        else if (limit > Domain.Models.QuotaLimit.MaxValue)
        {
            problems.Add("Quota limit cannot exceed 1 billion.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the API key string format is valid.
    /// </summary>
    /// <param name="key">The API key to check.</param>
    /// <returns>True if the key format is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if key is null.</exception>
    public static bool IsValidKeyFormat(string key) => ValidateKeyFormat(key).Count == 0;

    /// <summary>
    /// Determines whether the API key name is valid.
    /// </summary>
    /// <param name="name">The API key name to check.</param>
    /// <returns>True if the name is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if name is null.</exception>
    public static bool IsValidKeyName(string name) => ValidateKeyName(name).Count == 0;

    /// <summary>
    /// Determines whether the quota limit is valid.
    /// </summary>
    /// <param name="limit">The quota limit to check.</param>
    /// <returns>True if the limit is valid; otherwise, false.</returns>
    public static bool IsValidQuotaLimit(int limit) => ValidateQuotaLimit(limit).Count == 0;

    /// <summary>
    /// Ensures that the API key string format is valid, throwing if not.
    /// </summary>
    /// <param name="key">The API key to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if key is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void EnsureValidKeyFormat(string key)
    {
        var problems = ValidateKeyFormat(key);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that the API key name is valid, throwing if not.
    /// </summary>
    /// <param name="name">The API key name to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if name is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void EnsureValidKeyName(string name)
    {
        var problems = ValidateKeyName(name);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that the quota limit is valid, throwing if not.
    /// The <see cref="Domain.Models.QuotaLimit.Unlimited"/> sentinel (-1) is accepted;
    /// zero, other negatives, and values above <see cref="Domain.Models.QuotaLimit.MaxValue"/> are rejected.
    /// </summary>
    /// <param name="limit">The quota limit to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the limit is out of range (zero, a negative other than -1, or above the maximum).</exception>
    public static void EnsureValidQuotaLimit(int limit)
    {
        var problems = ValidateQuotaLimit(limit);
        if (problems.Count > 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), limit, string.Join(" ", problems));
        }
    }
}
