// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="RateLimit"/> instances
/// </summary>
public static class RateLimitValidation
{
    /// <summary>
    /// Validates a <see cref="RateLimit"/> instance and returns any validation errors
    /// </summary>
    /// <param name="value">The rate limit to validate</param>
    /// <returns>A list of validation error messages, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this RateLimit? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id cannot be null or whitespace.");
        }

        // Validate ApiKeyId
        if (string.IsNullOrWhiteSpace(value.ApiKeyId))
        {
            errors.Add("ApiKeyId cannot be null or whitespace.");
        }

        // Validate RequestsPerUnit
        if (value.RequestsPerUnit <= 0)
        {
            errors.Add("RequestsPerUnit must be a positive integer greater than zero.");
        }
        else if (value.RequestsPerUnit > 1_000_000) // Reasonable upper bound
        {
            errors.Add("RequestsPerUnit cannot exceed 1,000,000.");
        }

        // Validate Unit (enum validation)
        if (!Enum.IsDefined(typeof(Enums.RateLimitUnit), value.Unit))
        {
            errors.Add("Unit contains an invalid value.");
        }

        // Validate IsEnabled
        // No validation needed - boolean can always be true or false

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be the default DateTime value.");
        }

        // Validate LastResetAt
        if (value.LastResetAt.HasValue && value.LastResetAt.Value == default)
        {
            errors.Add("LastResetAt cannot be the default DateTime value.");
        }

        // Validate CurrentRequestCount
        if (value.CurrentRequestCount < 0)
        {
            errors.Add("CurrentRequestCount cannot be negative.");
        }
        else if (value.CurrentRequestCount > value.RequestsPerUnit)
        {
            errors.Add("CurrentRequestCount cannot exceed RequestsPerUnit.");
        }

        // Validate that CurrentRequestCount doesn't exceed RequestsPerUnit when IsEnabled
        if (value.IsEnabled && value.CurrentRequestCount > value.RequestsPerUnit)
        {
            errors.Add("CurrentRequestCount cannot exceed RequestsPerUnit when IsEnabled is true.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="RateLimit"/> instance is valid
    /// </summary>
    /// <param name="value">The rate limit to check</param>
    /// <returns>True if the rate limit is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this RateLimit? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="RateLimit"/> instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The rate limit to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when value contains validation errors</exception>
    public static void EnsureValid(this RateLimit? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"RateLimit validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}