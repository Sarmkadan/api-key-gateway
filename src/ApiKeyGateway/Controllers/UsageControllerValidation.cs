using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Validation helpers for <see cref="UsageController"/>.
/// </summary>
public static class UsageControllerValidation
{
    /// <summary>
    /// Validates a <see cref="UsageController"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The <see cref="UsageController"/> instance to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    public static IReadOnlyList<string> Validate(this UsageController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrEmpty(value.ApiKeyId))
            problems.Add("API Key ID is required.");

        if (value.StartDate == default)
            problems.Add("Start date is required.");

        if (value.EndDate == default)
            problems.Add("End date is required.");

        if (value.EndDate < value.StartDate)
            problems.Add("End date must be after start date.");

        if (value.TotalRequests < 0)
            problems.Add("Total requests must be a non-negative integer.");

        if (value.SuccessfulRequests < 0)
            problems.Add("Successful requests must be a non-negative integer.");

        if (value.FailedRequests < 0)
            problems.Add("Failed requests must be a non-negative integer.");

        if (value.SuccessRate < 0 || value.SuccessRate > 100)
            problems.Add("Success rate must be between 0 and 100.");

        if (value.TotalBytesTransferred < 0)
            problems.Add("Total bytes transferred must be a non-negative integer.");

        if (value.AverageResponseTimeMs < 0)
            problems.Add("Average response time must be a non-negative integer.");

        if (value.UniqueEndpoints < 0)
            problems.Add("Unique endpoints must be a non-negative integer.");

        if (string.IsNullOrEmpty(value.Id))
            problems.Add("Id is required.");

        if (value.RecordedAt == default)
            problems.Add("Recorded at is required.");

        if (string.IsNullOrEmpty(value.Endpoint))
            problems.Add("Endpoint is required.");

        if (string.IsNullOrEmpty(value.Method))
            problems.Add("Method is required.");

        if (value.StatusCode < 0)
            problems.Add("Status code must be a non-negative integer.");

        if (value.RequestBytes < 0)
            problems.Add("Request bytes must be a non-negative integer.");

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="UsageController"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="UsageController"/> instance to check.</param>
    /// <returns><c>true</c> if the instance is valid; <c>false</c> otherwise.</returns>
    public static bool IsValid(this UsageController value)
    {
        return !Validate(value).Any();
    }

    /// <summary>
    /// Ensures that a <see cref="UsageController"/> instance is valid, throwing an <see cref="ArgumentException"/> if it's not.
    /// </summary>
    /// <param name="value">The <see cref="UsageController"/> instance to ensure.</param>
    /// <exception cref="ArgumentException">If the instance is not valid.</exception>
    public static void EnsureValid(this UsageController value)
    {
        var problems = Validate(value).ToList();

        if (problems.Any())
            throw new ArgumentException($"Invalid UsageController instance: {string.Join(", ", problems)}");
    }
}
