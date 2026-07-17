using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="ApiResponseBuilder{T}" /> instances.
/// </summary>
public static class ApiResponseBuilderValidation
{
    /// <summary>
    /// Validates a <see cref="ApiResponseBuilder{T}" /> instance and returns any validation problems.
    /// </summary>
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="value">The API response builder to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value" /> is null.</exception>
    public static IReadOnlyList<string> Validate<T>(this ApiResponseBuilder<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that either Success or Error was called by checking the message field
        // Success sets message to "Success" or provided message, Error sets message to error message
        // If neither was called, message would remain null
        var messageField = value.GetType().GetField("_message", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var messageValue = messageField?.GetValue(value) as string;

        if (string.IsNullOrEmpty(messageValue))
        {
            problems.Add("Either Success() or Error() must be called before building.");
        }

        // Validate metadata keys are not empty
        var metadataField = value.GetType().GetField("_metadata", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var metadataValue = metadataField?.GetValue(value) as Dictionary<string, object>;

        if (metadataValue != null)
        {
            foreach (var key in metadataValue.Keys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    problems.Add("WithMetadata() requires a non-empty key.");
                    break;
                }
            }
        }

        // Validate error messages are not empty
        var errorsField = value.GetType().GetField("_errors", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var errorsValue = errorsField?.GetValue(value) as List<string>;

        if (errorsValue != null)
        {
            foreach (var error in errorsValue)
            {
                if (string.IsNullOrEmpty(error))
                {
                    problems.Add("AddError() requires a non-empty error message.");
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ApiResponseBuilder{T}" /> instance is valid.
    /// </summary>
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="value">The API response builder to check.</param>
    /// <returns>True if the builder is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value" /> is null.</exception>
    public static bool IsValid<T>(this ApiResponseBuilder<T> value)
    {
        return value is not null && value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ApiResponseBuilder{T}" /> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="value">The API response builder to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the builder contains validation problems.</exception>
    public static void EnsureValid<T>(this ApiResponseBuilder<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ApiResponseBuilder is not valid. Problems: {string.Join(" ", problems)}",
                nameof(value));
        }
    }
}