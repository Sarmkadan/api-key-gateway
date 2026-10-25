// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Extensions;

/// <summary>
/// Extension methods for HTTP status codes used by the gateway.
/// Codes are plain integers, matching the values in <c>HttpStatusCodeConstants</c>.
/// </summary>
public static class HttpStatusCodeExtensions
{
    /// <summary>
    /// Determines whether the status code is informational (1xx).
    /// </summary>
    /// <param name="code">The HTTP status code to check.</param>
    /// <returns><c>true</c> if the code is in the 100-199 range; otherwise <c>false</c>.</returns>
    public static bool IsInformational(this int code) => code is >= 100 and < 200;

    /// <summary>
    /// Determines whether the status code indicates success (2xx).
    /// </summary>
    /// <param name="code">The HTTP status code to check.</param>
    /// <returns><c>true</c> if the code is in the 200-299 range; otherwise <c>false</c>.</returns>
    public static bool IsSuccess(this int code) => code is >= 200 and < 300;

    /// <summary>
    /// Determines whether the status code indicates a redirection (3xx).
    /// </summary>
    /// <param name="code">The HTTP status code to check.</param>
    /// <returns><c>true</c> if the code is in the 300-399 range; otherwise <c>false</c>.</returns>
    public static bool IsRedirection(this int code) => code is >= 300 and < 400;

    /// <summary>
    /// Determines whether the status code indicates a client error (4xx).
    /// </summary>
    /// <param name="code">The HTTP status code to check.</param>
    /// <returns><c>true</c> if the code is in the 400-499 range; otherwise <c>false</c>.</returns>
    public static bool IsClientError(this int code) => code is >= 400 and < 500;

    /// <summary>
    /// Determines whether the status code indicates a server error (5xx).
    /// </summary>
    /// <param name="code">The HTTP status code to check.</param>
    /// <returns><c>true</c> if the code is in the 500-599 range; otherwise <c>false</c>.</returns>
    public static bool IsServerError(this int code) => code is >= 500 and < 600;

    /// <summary>
    /// Determines whether the status code indicates an error (4xx or 5xx).
    /// </summary>
    /// <param name="code">The HTTP status code to check.</param>
    /// <returns><c>true</c> if the code is in the 400-599 range; otherwise <c>false</c>.</returns>
    public static bool IsError(this int code) => code is >= 400 and < 600;
}
