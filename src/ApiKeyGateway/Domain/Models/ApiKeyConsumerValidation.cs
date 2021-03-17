// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="ApiKeyConsumer"/>.
/// </summary>
public static class ApiKeyConsumerValidation
{
    /// <summary>
    /// Validates the <see cref="ApiKeyConsumer"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The consumer instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ApiKeyConsumer value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrEmpty(value.Id))
        {
            errors.Add("Id must not be empty.");
        }

        if (string.IsNullOrEmpty(value.Name))
        {
            errors.Add("Name must not be empty.");
        }

        if (string.IsNullOrEmpty(value.Email))
        {
            errors.Add("Email must not be empty.");
        }
        else if (!value.Email.Contains('@'))
        {
            errors.Add("Email must contain '@'.");
        }

        if (string.IsNullOrEmpty(value.Organization))
        {
            errors.Add("Organization must not be empty.");
        }

        if (string.IsNullOrEmpty(value.Tier))
        {
            errors.Add("Tier must not be empty.");
        }

        if (value.IsActive && value.InactiveSince.HasValue)
        {
            errors.Add("Consumer is active but has an inactive since date.");
        }

        if (!value.IsActive && !value.InactiveSince.HasValue)
        {
            errors.Add("Consumer is inactive but has no inactive since date.");
        }

        if (value.TotalApiKeys < 0)
        {
            errors.Add("Total API keys must be non-negative.");
        }

        if (value.LastActivityAt.HasValue && value.LastActivityAt.Value < value.CreatedAt)
        {
            errors.Add("Last activity date must be after creation date.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the <see cref="ApiKeyConsumer"/> instance is valid.
    /// </summary>
    /// <param name="value">The consumer instance to check.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ApiKeyConsumer value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the <see cref="ApiKeyConsumer"/> instance is invalid.
    /// </summary>
    /// <param name="value">The consumer instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this ApiKeyConsumer value)
    {
        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", errors), nameof(value));
        }
    }
}
