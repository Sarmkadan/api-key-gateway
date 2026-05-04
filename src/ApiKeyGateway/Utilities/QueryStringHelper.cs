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
    public static string BuildQueryString(IDictionary<string, string> parameters)
    {
        if (parameters == null || parameters.Count == 0)
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
    public static string RemoveParameter(string url, params string[] parameterNames)
    {
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
