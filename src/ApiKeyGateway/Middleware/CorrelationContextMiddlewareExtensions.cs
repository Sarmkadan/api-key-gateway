using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ApiKeyGateway.Middleware;

/// <summary>
/// Extension methods for <see cref="CorrelationContextMiddleware"/> that provide convenient access to correlation context values
/// and middleware configuration helpers.
/// </summary>
public static class CorrelationContextMiddlewareExtensions
{
    /// <summary>
    /// Gets the correlation ID from the HTTP context items set by the correlation context middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The correlation ID, or <see langword="null"/> if not set.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/>.</exception>
    public static string? GetCorrelationId(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Items.TryGetValue("CorrelationId", out var value)
            ? value as string
            : null;
    }

    /// <summary>
    /// Gets the API key ID from the HTTP context items set by the correlation context middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The API key ID, or <see langword="null"/> if not set.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/>.</exception>
    public static string? GetApiKeyId(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Items.TryGetValue("ApiKeyId", out var value)
            ? value as string
            : null;
    }

    /// <summary>
    /// Gets the client IP address from the HTTP context items set by the correlation context middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The client IP address, or <see langword="null"/> if not set.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/>.</exception>
    public static string? GetClientIp(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Items.TryGetValue("ClientIp", out var value)
            ? value as string
            : null;
    }

    /// <summary>
    /// Extracts all correlation context values as a dictionary for logging or diagnostics purposes.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A read-only dictionary containing all correlation context values.
    /// The dictionary may be empty if no correlation context values have been set.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/>.</exception>
    public static IReadOnlyDictionary<string, object?> GetCorrelationContext(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = new Dictionary<string, object?>(StringComparer.Ordinal);

        if (context.Items.TryGetValue("CorrelationId", out var correlationId))
        {
            result["CorrelationId"] = correlationId;
        }

        if (context.Items.TryGetValue("ApiKeyId", out var apiKeyId))
        {
            result["ApiKeyId"] = apiKeyId;
        }

        if (context.Items.TryGetValue("ClientIp", out var clientIp))
        {
            result["ClientIp"] = clientIp;
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the correlation context has been initialized in the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns><see langword="true"/> if correlation context values are present; otherwise, <see langword="false"/>.
    /// This is determined by the presence of a correlation ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <see langword="null"/>.</exception>
    public static bool HasCorrelationContext(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Items.ContainsKey("CorrelationId");
    }
}