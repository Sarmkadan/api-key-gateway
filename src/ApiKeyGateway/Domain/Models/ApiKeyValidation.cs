// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="ApiKey"/>.
/// </summary>
public static class ApiKeyValidation
{
    /// <summary>
    /// Validates the <see cref="ApiKey"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The API key instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ApiKey value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrEmpty(value.Id))
        {
            errors.Add("Id must not be empty.");
        }

        if (string.IsNullOrEmpty(value.ConsumerId))
        {
            errors.Add("ConsumerId must not be empty.");
        }

        if (string.IsNullOrEmpty(value.Name))
        {
            errors.Add("Name must not be empty.");
        }
        else if (value.Name.Length > 100)
        {
            errors.Add("Name must not exceed 100 characters.");
        }

        if (string.IsNullOrEmpty(value.KeyHash))
        {
            errors.Add("KeyHash must not be empty.");
        }
        else if (value.KeyHash.Length < 8)
        {
            errors.Add("KeyHash must be at least 8 characters long.");
        }

        if (string.IsNullOrEmpty(value.Prefix))
        {
            errors.Add("Prefix must not be empty.");
        }
        else if (value.Prefix.Length != 8)
        {
            errors.Add("Prefix must be exactly 8 characters long.");
        }

        if (!Enum.IsDefined(value.Status))
        {
            errors.Add("Status must be a valid ApiKeyStatus value.");
        }

        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid date.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        if (value.ExpiresAt.HasValue)
        {
            if (value.ExpiresAt.Value == default)
            {
                errors.Add("ExpiresAt must be a valid date if set.");
            }
            else if (value.ExpiresAt.Value < value.CreatedAt)
            {
                errors.Add("ExpiresAt cannot be before CreatedAt.");
            }
        }

        if (value.LastUsedAt.HasValue)
        {
            if (value.LastUsedAt.Value == default)
            {
                errors.Add("LastUsedAt must be a valid date if set.");
            }
            else if (value.LastUsedAt.Value < value.CreatedAt)
            {
                errors.Add("LastUsedAt cannot be before CreatedAt.");
            }
            else if (value.ExpiresAt.HasValue && value.LastUsedAt.Value > value.ExpiresAt.Value)
            {
                errors.Add("LastUsedAt cannot be after ExpiresAt.");
            }
        }

        if (value.DisabledAt.HasValue)
        {
            if (value.DisabledAt.Value == default)
            {
                errors.Add("DisabledAt must be a valid date if set.");
            }
            else if (value.DisabledAt.Value < value.CreatedAt)
            {
                errors.Add("DisabledAt cannot be before CreatedAt.");
            }
        }

        if (value.Description?.Length > 500)
        {
            errors.Add("Description must not exceed 500 characters.");
        }

        if (value.Metadata == null)
        {
            errors.Add("Metadata must not be null.");
        }
        else if (value.Metadata.Count > 50)
        {
            errors.Add("Metadata must not contain more than 50 entries.");
        }

        if (value.RequestCount < 0)
        {
            errors.Add("RequestCount must be non-negative.");
        }

        if (value.BytesTransferred < 0)
        {
            errors.Add("BytesTransferred must be non-negative.");
        }

        if (value.IpWhitelist?.Length > 2000)
        {
            errors.Add("IpWhitelist must not exceed 2000 characters.");
        }

        if (value.RateLimitId?.Length > 100)
        {
            errors.Add("RateLimitId must not exceed 100 characters.");
        }

        if (value.AllowedScopes?.Length > 2000)
        {
            errors.Add("AllowedScopes must not exceed 2000 characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the <see cref="ApiKey"/> instance is valid.
    /// </summary>
    /// <param name="value">The API key instance to check.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ApiKey value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the <see cref="ApiKey"/> instance is invalid.
    /// </summary>
    /// <param name="value">The API key instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this ApiKey value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", errors), nameof(value));
        }
    }
}