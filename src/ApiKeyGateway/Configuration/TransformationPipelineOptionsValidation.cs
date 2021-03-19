// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="TransformationPipelineOptions"/> instances.
/// </summary>
public static class TransformationPipelineOptionsValidation
{
    /// <summary>
    /// Validates a <see cref="TransformationPipelineOptions"/> instance and returns any validation errors.
    /// </summary>
    /// <param name="value">The transformation pipeline options to validate.</param>
    /// <returns>A list of validation error messages, or empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this TransformationPipelineOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate IsEnabled (no validation needed - boolean can always be true or false)

        // Validate MaxRulesPerRequest
        if (value.MaxRulesPerRequest <= 0)
        {
            errors.Add("MaxRulesPerRequest must be a positive integer greater than zero.");
        }

        // Validate StopOnError (no validation needed - boolean can always be true or false)

        // Validate EnableBodyCapture (no validation needed - boolean can always be true or false)

        // Validate MaxBodySizeBytes
        if (value.MaxBodySizeBytes <= 0)
        {
            errors.Add("MaxBodySizeBytes must be a positive integer greater than zero.");
        }

        // Validate RuleCacheTtl
        if (value.RuleCacheTtl < TimeSpan.Zero)
        {
            errors.Add("RuleCacheTtl cannot be negative.");
        }

        // Validate StaticRules
        if (value.StaticRules is null)
        {
            errors.Add("StaticRules cannot be null.");
        }
        else if (value.StaticRules.Count > value.MaxRulesPerRequest)
        {
            errors.Add("StaticRules count cannot exceed MaxRulesPerRequest.");
        }

        // Validate Lua execution options
        if (value.Lua is null)
        {
            errors.Add("Lua execution options cannot be null.");
        }
        else
        {
            // Validate Lua.IsEnabled (no validation needed - boolean can always be true or false)

            // Validate Lua.MaxExecutionMs
            if (value.Lua.MaxExecutionMs <= 0)
            {
                errors.Add("Lua.MaxExecutionMs must be a positive integer greater than zero.");
            }

            // Validate Lua.MaxScriptSizeBytes
            if (value.Lua.MaxScriptSizeBytes <= 0)
            {
                errors.Add("Lua.MaxScriptSizeBytes must be a positive integer greater than zero.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="TransformationPipelineOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The transformation pipeline options to check.</param>
    /// <returns>True if the options are valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static bool IsValid(this TransformationPipelineOptions value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="TransformationPipelineOptions"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The transformation pipeline options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when value contains validation errors.</exception>
    public static void EnsureValid(this TransformationPipelineOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"TransformationPipelineOptions validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}",
                nameof(value));
        }
    }
}