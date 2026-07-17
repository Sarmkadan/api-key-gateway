// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace ApiKeyGateway.Services;

/// <summary>
/// Provides validation helpers for <see cref="AnalyticsSummary"/>.
/// </summary>
public static class AnalyticsSummaryValidation
{
    private const double PercentageTolerance = 0.001;
    private const int DateTimeToleranceMinutes = 5;

    /// <summary>
    /// Validates the <see cref="AnalyticsSummary"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The analytics summary instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AnalyticsSummary value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrEmpty(value.ApiKeyId))
        {
            errors.Add("ApiKeyId must not be empty.");
        }

        if (value.From == default)
        {
            errors.Add("From date must be set to a valid date.");
        }
        else if (value.From > DateTime.UtcNow.AddMinutes(DateTimeToleranceMinutes))
        {
            errors.Add("From date cannot be in the future.");
        }

        if (value.To == default)
        {
            errors.Add("To date must be set to a valid date.");
        }
        else if (value.To > DateTime.UtcNow.AddMinutes(DateTimeToleranceMinutes))
        {
            errors.Add("To date cannot be in the future.");
        }
        else if (value.To < value.From)
        {
            errors.Add("To date must be after or equal to From date.");
        }

        if (value.TotalRequests < 0)
        {
            errors.Add("TotalRequests must be non-negative.");
        }

        if (value.SuccessfulRequests < 0)
        {
            errors.Add("SuccessfulRequests must be non-negative.");
        }

        if (value.FailedRequests < 0)
        {
            errors.Add("FailedRequests must be non-negative.");
        }

        if (value.TotalRequests > 0)
        {
            if (value.SuccessfulRequests + value.FailedRequests != value.TotalRequests)
            {
                errors.Add("SuccessfulRequests + FailedRequests must equal TotalRequests.");
            }
        }
        else
        {
            if (value.SuccessfulRequests != 0 || value.FailedRequests != 0)
            {
                errors.Add("SuccessfulRequests and FailedRequests must be 0 when TotalRequests is 0.");
            }
        }

        if (value.SuccessRatePercent < 0 || value.SuccessRatePercent > 100)
        {
            errors.Add("SuccessRatePercent must be between 0 and 100 inclusive.");
        }

        if (value.ErrorRatePercent < 0 || value.ErrorRatePercent > 100)
        {
            errors.Add("ErrorRatePercent must be between 0 and 100 inclusive.");
        }

        if (Math.Abs(value.SuccessRatePercent + value.ErrorRatePercent - 100) > PercentageTolerance)
        {
            errors.Add("SuccessRatePercent + ErrorRatePercent must equal 100 within tolerance.");
        }

        if (value.AverageResponseTimeMs < 0)
        {
            errors.Add("AverageResponseTimeMs must be non-negative.");
        }

        if (value.TotalBytesTransferred < 0)
        {
            errors.Add("TotalBytesTransferred must be non-negative.");
        }

        if (value.UniqueEndpoints < 0)
        {
            errors.Add("UniqueEndpoints must be non-negative.");
        }

        if (value.UniqueSourceIps < 0)
        {
            errors.Add("UniqueSourceIps must be non-negative.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the <see cref="AnalyticsSummary"/> instance is valid.
    /// </summary>
    /// <param name="value">The analytics summary instance to check.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this AnalyticsSummary value) => Validate(value).Count == 0;

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the <see cref="AnalyticsSummary"/> instance is invalid.
    /// </summary>
    /// <param name="value">The analytics summary instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this AnalyticsSummary value)
    {
        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", errors), nameof(value));
        }
    }
}
