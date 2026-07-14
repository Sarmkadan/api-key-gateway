// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Validation helpers for ValidationHelpersTests instances
/// </summary>
public static class ValidationHelpersTestsValidation
{
    /// <summary>
    /// Validates all aspects of a ValidationHelpersTests instance
    /// </summary>
    /// <param name="value">The instance to validate</param>
    /// <returns>List of human-readable problems, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this ValidationHelpersTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate IsValidEmail_VariousFormats_ReturnsExpectedResult method exists
        try
        {
            var method = typeof(ValidationHelpersTests).GetMethod("IsValidEmail_VariousFormats_ReturnsExpectedResult");
            if (method is null)
            {
                problems.Add("IsValidEmail_VariousFormats_ReturnsExpectedResult method not found");
            }
        }
        catch
        {
            problems.Add("IsValidEmail_VariousFormats_ReturnsExpectedResult method access failed");
        }

        // Validate IsValidApiKeyFormat_ValidSkPrefixWith32Chars_ReturnsTrue method exists
        try
        {
            var method = typeof(ValidationHelpersTests).GetMethod("IsValidApiKeyFormat_ValidSkPrefixWith32Chars_ReturnsTrue");
            if (method is null)
            {
                problems.Add("IsValidApiKeyFormat_ValidSkPrefixWith32Chars_ReturnsTrue method not found");
            }
        }
        catch
        {
            problems.Add("IsValidApiKeyFormat_ValidSkPrefixWith32Chars_ReturnsTrue method access failed");
        }

        // Validate IsValidApiKeyFormat_InvalidFormats_ReturnsFalse method exists
        try
        {
            var method = typeof(ValidationHelpersTests).GetMethod("IsValidApiKeyFormat_InvalidFormats_ReturnsFalse");
            if (method is null)
            {
                problems.Add("IsValidApiKeyFormat_InvalidFormats_ReturnsFalse method not found");
            }
        }
        catch
        {
            problems.Add("IsValidApiKeyFormat_InvalidFormats_ReturnsFalse method access failed");
        }

        // Validate IsValidIpAddress_VariousAddresses_ReturnsExpectedResult method exists
        try
        {
            var method = typeof(ValidationHelpersTests).GetMethod("IsValidIpAddress_VariousAddresses_ReturnsExpectedResult");
            if (method is null)
            {
                problems.Add("IsValidIpAddress_VariousAddresses_ReturnsExpectedResult method not found");
            }
        }
        catch
        {
            problems.Add("IsValidIpAddress_VariousAddresses_ReturnsExpectedResult method access failed");
        }

        // Validate SanitizeInput_StringExceedingMaxLength_TruncatesToLimit method exists
        try
        {
            var method = typeof(ValidationHelpersTests).GetMethod("SanitizeInput_StringExceedingMaxLength_TruncatesToLimit");
            if (method is null)
            {
                problems.Add("SanitizeInput_StringExceedingMaxLength_TruncatesToLimit method not found");
            }
        }
        catch
        {
            problems.Add("SanitizeInput_StringExceedingMaxLength_TruncatesToLimit method access failed");
        }

        // Validate SanitizeInput_StringWithLeadingAndTrailingWhitespace_ReturnsTrimmedValue method exists
        try
        {
            var method = typeof(ValidationHelpersTests).GetMethod("SanitizeInput_StringWithLeadingAndTrailingWhitespace_ReturnsTrimmedValue");
            if (method is null)
            {
                problems.Add("SanitizeInput_StringWithLeadingAndTrailingWhitespace_ReturnsTrimmedValue method not found");
            }
        }
        catch
        {
            problems.Add("SanitizeInput_StringWithLeadingAndTrailingWhitespace_ReturnsTrimmedValue method access failed");
        }

        // Validate ValidateKeyFormat_KeyWithSufficientEntropy_ReturnsValid method exists
        try
        {
            var method = typeof(ValidationHelpersTests).GetMethod("ValidateKeyFormat_KeyWithSufficientEntropy_ReturnsValid");
            if (method is null)
            {
                problems.Add("ValidateKeyFormat_KeyWithSufficientEntropy_ReturnsValid method not found");
            }
        }
        catch
        {
            problems.Add("ValidateKeyFormat_KeyWithSufficientEntropy_ReturnsValid method access failed");
        }

        // Validate ValidateKeyFormat_KeyTooShort_ReturnsInvalidWithMessage method exists
        try
        {
            var method = typeof(ValidationHelpersTests).GetMethod("ValidateKeyFormat_KeyTooShort_ReturnsInvalidWithMessage");
            if (method is null)
            {
                problems.Add("ValidateKeyFormat_KeyTooShort_ReturnsInvalidWithMessage method not found");
            }
        }
        catch
        {
            problems.Add("ValidateKeyFormat_KeyTooShort_ReturnsInvalidWithMessage method access failed");
        }

        // Validate ValidateKeyName_ValidName_ReturnsValid method exists
        try
        {
            var method = typeof(ValidationHelpersTests).GetMethod("ValidateKeyName_ValidName_ReturnsValid");
            if (method is null)
            {
                problems.Add("ValidateKeyName_ValidName_ReturnsValid method not found");
            }
        }
        catch
        {
            problems.Add("ValidateKeyName_ValidName_ReturnsValid method access failed");
        }

        // Validate ValidateKeyName_NameWithSpecialChars_ReturnsInvalid method exists
        try
        {
            var method = typeof(ValidationHelpersTests).GetMethod("ValidateKeyName_NameWithSpecialChars_ReturnsInvalid");
            if (method is null)
            {
                problems.Add("ValidateKeyName_NameWithSpecialChars_ReturnsInvalid method not found");
            }
        }
        catch
        {
            problems.Add("ValidateKeyName_NameWithSpecialChars_ReturnsInvalid method access failed");
        }

        // Validate ValidateQuotaLimit_VariousLimits_ReturnsExpectedValidity method exists
        try
        {
            var method = typeof(ValidationHelpersTests).GetMethod("ValidateQuotaLimit_VariousLimits_ReturnsExpectedValidity");
            if (method is null)
            {
                problems.Add("ValidateQuotaLimit_VariousLimits_ReturnsExpectedValidity method not found");
            }
        }
        catch
        {
            problems.Add("ValidateQuotaLimit_VariousLimits_ReturnsExpectedValidity method access failed");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a ValidationHelpersTests instance is valid
    /// </summary>
    /// <param name="value">The instance to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this ValidationHelpersTests value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a ValidationHelpersTests instance is valid, throwing if not
    /// </summary>
    /// <param name="value">The instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, with problem details</exception>
    public static void EnsureValid(this ValidationHelpersTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ValidationHelpersTests instance is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}
