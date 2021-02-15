// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Represents the extracted request context information from HTTP requests.
// Used for serialization and logging purposes.
// =====================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Represents the extracted request context information from HTTP requests.
/// Contains details about the API key, client, correlation tracking, and request metadata.
/// </summary>
public sealed record RequestContext
{
    /// <summary>
    /// The extracted API key from the request headers.
    /// Returns null or empty string if no API key was provided.
    /// </summary>
    public string? ApiKey { get; init; }


    /// <summary>
    /// The correlation ID for tracking the request across services.
    /// </summary>
    public string CorrelationId { get; init; } = string.Empty;

    /// <summary>
    /// The client's IP address extracted from the request.
    /// </summary>
    public string ClientIpAddress { get; init; } = "unknown";

    /// <summary>
    /// The request scope (API key ID or "anonymous" if no API key was provided).
    /// </summary>
    public string RequestScope { get; init; } = "anonymous";

    /// <summary>
    /// Indicates whether the request accepts JSON responses.
    /// </summary>
    public bool AcceptsJson { get; init; }

    /// <summary>
    /// The page number extracted from query parameters (defaults to 1).
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// The page size extracted from query parameters (defaults to 50, capped at 1000).
    /// </summary>
    public int PageSize { get; init; } = 50;
}
