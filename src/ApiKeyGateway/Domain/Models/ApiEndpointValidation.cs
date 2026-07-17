// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="ApiEndpoint"/> instances
/// </summary>
public static class ApiEndpointValidation
{
    /// <summary>
    /// Validates an <see cref="ApiEndpoint"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The endpoint to validate</param>
    /// <returns>A read-only list of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this ApiEndpoint value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            problems.Add("Id cannot be null or whitespace");
        }

        // Validate Path
        if (string.IsNullOrWhiteSpace(value.Path))
        {
            problems.Add("Path cannot be null or whitespace");
        }
        else if (!value.Path.StartsWith('/'))
        {
            problems.Add("Path must start with '/'");
        }

        // Validate Method
        if (string.IsNullOrWhiteSpace(value.Method))
        {
            problems.Add("Method cannot be null or whitespace");
        }
        else if (!IsValidHttpMethod(value.Method))
        {
            problems.Add($"Method '{value.Method}' is not a valid HTTP method (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS)");
        }

        // Validate TargetUrl
        if (string.IsNullOrWhiteSpace(value.TargetUrl))
        {
            problems.Add("TargetUrl cannot be null or whitespace");
        }
        else if (!Uri.IsWellFormedUriString(value.TargetUrl, UriKind.Absolute))
        {
            problems.Add($"TargetUrl '{value.TargetUrl}' is not a valid absolute URI");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt cannot be default(DateTime)");
        }
        else if (value.CreatedAt > DateTime.UtcNow)
        {
            problems.Add("CreatedAt cannot be in the future");
        }

        // Validate TimeoutMs
        if (value.TimeoutMs <= 0)
        {
            problems.Add("TimeoutMs must be a positive number");
        }
        else if (value.TimeoutMs > 300000) // 5 minutes max
        {
            problems.Add("TimeoutMs cannot exceed 300000 (5 minutes)");
        }

        // Validate MaxPayloadBytes
        if (value.MaxPayloadBytes <= 0)
        {
            problems.Add("MaxPayloadBytes must be a positive number");
        }
        else if (value.MaxPayloadBytes > 104857600) // 100 MB max
        {
            problems.Add("MaxPayloadBytes cannot exceed 104857600 (100 MB)");
        }

        // Validate CacheTtlSeconds
        if (value.CacheTtlSeconds < 0)
        {
            problems.Add("CacheTtlSeconds cannot be negative");
        }
        else if (value.CacheTtlSeconds > 86400) // 24 hours max
        {
            problems.Add("CacheTtlSeconds cannot exceed 86400 (24 hours)");
        }

        // Validate Headers
        if (value.Headers == null)
        {
            problems.Add("Headers dictionary cannot be null");
        }

        // Validate AllowedConsumers
        if (value.AllowedConsumers == null)
        {
            problems.Add("AllowedConsumers list cannot be null");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="ApiEndpoint"/> instance is valid.
    /// </summary>
    /// <param name="value">The endpoint to check</param>
    /// <returns>True if the endpoint is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static bool IsValid(this ApiEndpoint? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="ApiEndpoint"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The endpoint to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the endpoint is invalid, containing the validation problems</exception>
    public static void EnsureValid(this ApiEndpoint value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ApiEndpoint is invalid. Problems:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}"
            );
        }
    }

    private static bool IsValidHttpMethod(string method)
    {
        return method is "GET" or "POST" or "PUT" or "DELETE" or "PATCH" or "HEAD" or "OPTIONS";
    }
}
