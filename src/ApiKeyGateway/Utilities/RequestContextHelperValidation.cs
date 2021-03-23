// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Validation helpers for RequestContextHelper to ensure data integrity
// =============================================================================

using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Provides validation helpers for RequestContextHelper to ensure request data integrity.
/// Validates extracted values like correlation IDs, pagination parameters, and IP addresses.
/// </summary>
public static class RequestContextHelperValidation
{
    /// <summary>
    /// Validates the values extracted from a request using RequestContextHelper.
    /// Returns any validation problems found in the extracted values.
    /// </summary>
    /// <param name="request">The HTTP request to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown if request is null.</exception>
    public static IReadOnlyList<string> Validate(this HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new List<string>();

        // Validate correlation ID
        var correlationId = RequestContextHelper.GetOrCreateCorrelationId(request);
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            errors.Add("Correlation ID cannot be null or whitespace.");
        }
        else if (!IsValidCorrelationId(correlationId))
        {
            errors.Add("Correlation ID must be a valid GUID format when provided.");
        }

        // Validate pagination parameters
        var (pageNumber, pageSize) = RequestContextHelper.ExtractPaginationParams(request);
        if (pageNumber < 1)
        {
            errors.Add("Pagination pageNumber must be at least 1.");
        }

        if (pageSize < 1 || pageSize > 1000)
        {
            errors.Add("Pagination pageSize must be between 1 and 1000 inclusive.");
        }

        // Validate client IP address
        var clientIp = RequestContextHelper.GetClientIpAddress(request);
        if (string.IsNullOrWhiteSpace(clientIp) || clientIp == "unknown" || clientIp == "::1" || clientIp == "127.0.0.1")
        {
            errors.Add("Client IP address must be a valid non-localhost address.");
        }
        else if (!IsValidIpAddress(clientIp))
        {
            errors.Add("Client IP address must be a valid IPv4 or IPv6 address.");
        }

        // Validate request scope
        var scope = RequestContextHelper.GetRequestScope(request);
        if (string.IsNullOrWhiteSpace(scope))
        {
            errors.Add("Request scope cannot be null or whitespace.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the request values extracted using RequestContextHelper are valid.
    /// </summary>
    /// <param name="request">The HTTP request to check.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid(this HttpRequest request)
    {
        return request.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the request values extracted using RequestContextHelper are valid.
    /// Throws an exception if validation fails.
    /// </summary>
    /// <param name="request">The HTTP request to validate.</param>
    /// <exception cref="ArgumentException">Thrown if request contains invalid values with a list of validation errors.</exception>
    public static void EnsureValid(this HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = request.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException("RequestContextHelper validation failed: " + string.Join(" ", errors));
        }
    }

    /// <summary>
    /// Validates if a string is a valid correlation ID (GUID format).
    /// </summary>
    /// <param name="correlationId">The correlation ID to validate.</param>
    /// <returns>True if valid GUID format, false otherwise.</returns>
    private static bool IsValidCorrelationId(string correlationId)
    {
        return Guid.TryParse(correlationId, out _);
    }

    /// <summary>
    /// Validates if a string is a valid IP address (IPv4 or IPv6).
    /// </summary>
    /// <param name="ipAddress">The IP address to validate.</param>
    /// <returns>True if valid IP address, false otherwise.</returns>
    private static bool IsValidIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        // Handle IPv4
        if (ipAddress.Contains('.') && !ipAddress.Contains(':'))
        {
            var parts = ipAddress.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (!byte.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out _))
                    return false;
            }

            return true;
        }

        // Handle IPv6
        if (ipAddress.Contains(':'))
        {
            return System.Net.IPAddress.TryParse(ipAddress, out _);
        }

        return false;
    }
}