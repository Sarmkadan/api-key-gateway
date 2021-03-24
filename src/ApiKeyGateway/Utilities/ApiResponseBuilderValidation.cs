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
    public static IReadOnlyList<string> Validate<T>(this ApiResponseBuilder<T>? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Success property by attempting to set it to null state
        try
        {
            value.Success(null);
        }
        catch
        {
            problems.Add("Success() method must be called before building.");
        }

        // Validate Error property by attempting to set it to null state
        try
        {
            value.Error(200, string.Empty);
        }
        catch
        {
            problems.Add("Error() method must be called before building.");
        }

        // Validate Data property by attempting to set it to null for reference types
        try
        {
            var builder = new ApiResponseBuilder<object?>();
            _ = builder.WithData(null);
        }
        catch
        {
            problems.Add("WithData() must be called with valid data.");
        }

        // Validate WithMetadata by attempting to add empty metadata
        try
        {
            var builder = new ApiResponseBuilder<object>();
            _ = builder.WithMetadata(string.Empty, null!);
        }
        catch
        {
            problems.Add("WithMetadata() requires a non-empty key.");
        }

        // Validate AddError by attempting to add empty error
        try
        {
            var builder = new ApiResponseBuilder<object>();
            _ = builder.AddError(string.Empty);
        }
        catch
        {
            problems.Add("AddError() requires a non-empty error message.");
        }

        // Validate that at least one of Success or Error was called
        try
        {
            var builder = new ApiResponseBuilder<object>();
            _ = builder.Build();
        }
        catch
        {
            problems.Add("Either Success() or Error() must be called before building.");
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
    public static bool IsValid<T>(this ApiResponseBuilder<T>? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ApiResponseBuilder{T}" /> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="value">The API response builder to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the builder contains validation problems.</exception>
    public static void EnsureValid<T>(this ApiResponseBuilder<T>? value)
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