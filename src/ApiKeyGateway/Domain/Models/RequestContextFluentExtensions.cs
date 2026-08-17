// =============================================================================
// Author: ChatGPT (generated extension)
// =============================================================================

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides fluent extension methods for <see cref="RequestContext"/>.
/// </summary>
public static class RequestContextFluentExtensions
{
    /// <summary>
    /// Determines whether the request originates from a localhost address.
    /// </summary>
    /// <param name="context">The <see cref="RequestContext"/> instance.</param>
    /// <returns>
    /// <c>true</c> if <see cref="RequestContext.ClientIpAddress"/> is a loopback address
    /// (e.g., <c>127.0.0.1</c>, <c>::1</c>, or <c>localhost</c>); otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <c>null</c>.</exception>
    public static bool IsFromLocalhost(this RequestContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var ip = context.ClientIpAddress?.Trim().ToLowerInvariant();

        return ip == "127.0.0.1"
            || ip == "::1"
            || ip == "localhost";
    }

    /// <summary>
    /// Checks whether a header with the specified <paramref name="name"/> exists in the request context.
    /// </summary>
    /// <param name="context">The <see cref="RequestContext"/> instance.</param>
    /// <param name="name">The header name to look for.</param>
    /// <returns>
    /// <c>true</c> if a header collection is present and contains the specified name; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="name"/> is <c>null</c>.</exception>
    public static bool HasHeader(this RequestContext context, string name)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (name == null) throw new ArgumentNullException(nameof(name));

        // Attempt to retrieve a property named "Headers" via reflection.
        // The property, if it exists, should be of type IDictionary<string, string>.
        var headersProp = typeof(RequestContext).GetProperty("Headers", BindingFlags.Public | BindingFlags.Instance);
        if (headersProp == null) return false;

        var value = headersProp.GetValue(context);
        if (value is IDictionary<string, string> headers)
        {
            return headers.ContainsKey(name);
        }

        return false;
    }

    /// <summary>
    /// Retrieves the value of a header with the specified <paramref name="name"/> or returns a fallback value.
    /// </summary>
    /// <param name="context">The <see cref="RequestContext"/> instance.</param>
    /// <param name="name">The header name to retrieve.</param>
    /// <param name="fallback">The value to return if the header is not present.</param>
    /// <returns>
    /// The header value if found; otherwise, the provided <paramref name="fallback"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="name"/> is <c>null</c>.</exception>
    public static string GetHeaderOrDefault(this RequestContext context, string name, string fallback)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (name == null) throw new ArgumentNullException(nameof(name));

        var headersProp = typeof(RequestContext).GetProperty("Headers", BindingFlags.Public | BindingFlags.Instance);
        if (headersProp == null) return fallback;

        var value = headersProp.GetValue(context);
        if (value is IDictionary<string, string> headers && headers.TryGetValue(name, out var headerValue))
        {
            return headerValue;
        }

        return fallback;
    }
}
