// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Factory for creating HTTP clients with standard configuration.
/// Ensures all outbound HTTP requests use consistent timeouts, headers,
/// and error handling. This prevents socket exhaustion and connection issues.
/// </summary>
public static class HttpClientFactory
{
    private const int DefaultTimeoutSeconds = 30;
    private const int MaxRetries = 3;

    /// <summary>
    /// Creates a configured HttpClient with default settings.
    /// Includes standard headers and reasonable timeouts.
    /// </summary>
    public static HttpClient CreateClient(string userAgent = "ApiKeyGateway/1.0")
    {
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            // Connection pooling prevents socket exhaustion
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            MaxConnectionsPerServer = 10
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds)
        };

        client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");

        return client;
    }

    /// <summary>
    /// Creates a client specifically for webhook deliveries.
    /// Uses extended timeout since webhooks may take longer to process.
    /// </summary>
    public static HttpClient CreateWebhookClient()
    {
        var client = CreateClient("ApiKeyGateway-WebhookDelivery/1.0");
        client.Timeout = TimeSpan.FromSeconds(60);
        return client;
    }

    /// <summary>
    /// Creates a client for external API calls with retry support.
    /// </summary>
    public static HttpClient CreateExternalApiClient(string apiName)
    {
        var client = CreateClient($"ApiKeyGateway/{apiName}/1.0");
        // External APIs may have rate limiting, so we use slightly higher timeout
        client.Timeout = TimeSpan.FromSeconds(45);
        return client;
    }
}
