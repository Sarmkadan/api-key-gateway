// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Validation;

/// <summary>
/// Validator for API key format, strength, and metadata.
/// Ensures API keys meet security and format requirements before creation.
/// Separates validation logic from business logic for reusability.
/// </summary>
public static class ApiKeyValidator
{
    private const int MinKeyLength = 32;
    private const int MaxKeyLength = 256;
    private const int MinNameLength = 3;
    private const int MaxNameLength = 100;

    /// <summary>
    /// Validates API key format and strength.
    /// Keys should be sufficiently random to resist guessing.
    /// </summary>
    public static ValidationResult ValidateKeyFormat(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return new ValidationResult { IsValid = false, Message = "API key cannot be empty" };

        if (key.Length < MinKeyLength)
            return new ValidationResult
            {
                IsValid = false,
                Message = $"API key must be at least {MinKeyLength} characters long"
            };

        if (key.Length > MaxKeyLength)
            return new ValidationResult
            {
                IsValid = false,
                Message = $"API key cannot exceed {MaxKeyLength} characters"
            };

        // Single-pass span scan for character-type entropy.
        // Short-circuits as soon as all four categories are found, avoiding
        // up to three redundant full traversals over the key string.
        bool hasUppercase = false, hasLowercase = false, hasDigits = false, hasSpecial = false;
        foreach (char c in key.AsSpan())
        {
            if (char.IsUpper(c)) hasUppercase = true;
            else if (char.IsLower(c)) hasLowercase = true;
            else if (char.IsDigit(c)) hasDigits = true;
            else hasSpecial = true;

            if (hasUppercase && hasLowercase && hasDigits && hasSpecial) break;
        }

        var characterTypesUsed = 0;
        if (hasUppercase) characterTypesUsed++;
        if (hasLowercase) characterTypesUsed++;
        if (hasDigits) characterTypesUsed++;
        if (hasSpecial) characterTypesUsed++;

        if (characterTypesUsed < 3)
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "API key should contain mix of uppercase, lowercase, digits, and special characters"
            };
        }

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Validates API key name/description.
    /// </summary>
    public static ValidationResult ValidateKeyName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new ValidationResult { IsValid = false, Message = "API key name is required" };

        if (name.Length < MinNameLength)
            return new ValidationResult
            {
                IsValid = false,
                Message = $"API key name must be at least {MinNameLength} characters"
            };

        if (name.Length > MaxNameLength)
            return new ValidationResult
            {
                IsValid = false,
                Message = $"API key name cannot exceed {MaxNameLength} characters"
            };

        // Check for valid characters
        if (!name.All(c => char.IsLetterOrDigit(c) || c is ' ' or '_' or '-'))
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "API key name can only contain letters, digits, spaces, underscores, and hyphens"
            };
        }

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Validates quota limits are reasonable.
    /// </summary>
    public static ValidationResult ValidateQuotaLimit(int limit)
    {
        if (limit <= 0)
            return new ValidationResult { IsValid = false, Message = "Quota limit must be greater than 0" };

        if (limit > 1_000_000_000)
            return new ValidationResult { IsValid = false, Message = "Quota limit cannot exceed 1 billion" };

        return new ValidationResult { IsValid = true };
    }
}

/// <summary>
/// Result of validation operation.
/// </summary>
public record ValidationResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}
