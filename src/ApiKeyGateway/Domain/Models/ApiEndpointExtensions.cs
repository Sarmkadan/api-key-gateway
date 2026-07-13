// =============================================================================
// Author: [Your Name]
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Extension methods for <see cref="ApiEndpoint"/> to enhance endpoint management capabilities.
/// </summary>
public static class ApiEndpointExtensions
{
    /// <summary>
    /// Validates that the endpoint is accessible and the payload size is valid.
    /// </summary>
    /// <param name="endpoint">The endpoint to validate.</param>
    /// <param name="payloadBytes">The payload size to validate.</param>
    /// <returns>True if the endpoint is accessible and the payload size is valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="endpoint"/> is null.</exception>
    public static bool IsValid(this ApiEndpoint endpoint, long payloadBytes)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        return endpoint.IsAccessible() && endpoint.IsPayloadSizeValid(payloadBytes);
    }

    /// <summary>
    /// Retrieves the endpoint's configuration as a dictionary.
    /// </summary>
    /// <param name="endpoint">The endpoint to retrieve configuration for.</param>
    /// <returns>A dictionary containing the endpoint's configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="endpoint"/> is null.</exception>
    public static IReadOnlyDictionary<string, string> GetConfiguration(this ApiEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        return endpoint.Headers.ToDictionary(x => x.Key, x => x.Value);
    }
}
