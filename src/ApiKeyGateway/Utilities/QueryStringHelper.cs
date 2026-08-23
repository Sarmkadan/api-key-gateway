// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Web;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Helper for building and parsing query strings safely.
/// Handles URL encoding/decoding and parameter management.
/// </summary>
public static class QueryStringHelper
{
    /// <summary>
    /// Builds a query string from a dictionary of parameters.
    /// Properly encodes values for safe URL usage.
    /// </summary>
    /// <param name="parameters">The dictionary of parameter names and values. Cannot be null.</param>
    /// <returns>A query string including the encoded parameters, or an empty string if parameters is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameters"/> is null.</exception>
    /// <param name="parameters">The dictionary of parameter names and values. Cannot be null.</param>
    /// <returns>A query string including the encoded parameters, or an empty string if parameters is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameters"/> is null.</exception>
    public static string BuildQueryString(IDictionary<string, string> parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null");

        if (parameters.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        var first = true;

        foreach (var kvp in parameters.Where(p => !string.IsNullOrEmpty(p.Value)))
        {
            if (!first)
                sb.Append('&');

            sb.Append(HttpUtility.UrlEncode(kvp.Key));
            sb.Append('=');
            sb.Append(HttpUtility.UrlEncode(kvp.Value));
            first = false;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parses a query string into a dictionary.
    /// Handles URL decoding automatically.
    /// </summary>
    public static Dictionary<string, string> ParseQueryString(string queryString)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(queryString))
            return parameters;

        var cleanQuery = queryString.TrimStart('?');
        var pairs = cleanQuery.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            var key = HttpUtility.UrlDecode(parts[0]);
            var value = parts.Length > 1 ? HttpUtility.UrlDecode(parts[1]) : string.Empty;

            parameters[key] = value;
        }

        return parameters;
    }

    /// <summary>
    /// Appends parameters to an existing URL, handling whether URL already has query string.
    /// </summary>
    public static string AppendParameters(string baseUrl, IDictionary<string, string> parameters)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseUrl);
        
        if (parameters == null || parameters.Count == 0)
            return baseUrl;

        var queryString = BuildQueryString(parameters);
        if (string.IsNullOrEmpty(queryString))
            return baseUrl;

        var separator = baseUrl.Contains('?') ? '&' : '?';
        return $"{baseUrl}{separator}{queryString}";
    }

    /// <summary>
    /// Removes specific parameters from a URL's query string.
    /// </summary>
    /// <param name="url">The URL to remove parameters from. Cannot be null or empty.</param>
    /// <param name="parameterNames">The names of the parameters to remove. Cannot be null.</param>
    /// <returns>The URL with the specified parameters removed from the query string.</returns>
    public static string RemoveParameter(string url, params string[] parameterNames)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);
        ArgumentNullException.ThrowIfNull(parameterNames);

        var uriBuilder = new UriBuilder(url);
        var parameters = ParseQueryString(uriBuilder.Query);

        foreach (var paramName in parameterNames)
        {
            parameters.Remove(paramName);
        }

        uriBuilder.Query = BuildQueryString(parameters);
        return uriBuilder.Uri.ToString();
    }
}
