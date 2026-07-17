// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="ApiEndpoint"/> to enhance endpoint management capabilities.
/// </summary>
public static class ApiEndpointExtensions
{
    /// <summary>
    /// Validates that the endpoint is accessible and the payload size is within limits.
    /// </summary>
    /// <param name="endpoint">The endpoint to validate.</param>
    /// <param name="payloadBytes">The payload size in bytes to validate against the endpoint's maximum.</param>
    /// <returns>True if the endpoint is accessible and the payload size is valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="endpoint"/> is null.</exception>
    public static bool IsValid(this ApiEndpoint endpoint, long payloadBytes)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        return endpoint.IsAccessible() && endpoint.IsPayloadSizeValid(payloadBytes);
    }

    /// <summary>
    /// Retrieves the endpoint's configuration headers as a read-only dictionary.
    /// </summary>
    /// <param name="endpoint">The endpoint to retrieve configuration for.</param>
    /// <returns>A read-only dictionary containing the endpoint's configuration headers.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="endpoint"/> is null.</exception>
    /// <remarks>Returns an empty dictionary if the Headers collection is null or empty.</remarks>
    public static IReadOnlyDictionary<string, string> GetConfiguration(this ApiEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        return endpoint.Headers?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>();
    }
}
