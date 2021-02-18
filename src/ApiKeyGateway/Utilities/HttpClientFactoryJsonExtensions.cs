// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Configuration for HTTP client creation.
/// This record represents the configuration used by <see cref="HttpClientFactory"/> to create HttpClient instances.
/// </summary>
/// <param name="DefaultTimeoutSeconds">The default timeout in seconds for HTTP requests.</param>
/// <param name="MaxRetries">The maximum number of retry attempts for failed requests.</param>
public sealed record HttpClientConfiguration(
    int DefaultTimeoutSeconds = 30,
    int MaxRetries = 3);

/// <summary>
/// Provides JSON serialization extensions for <see cref="HttpClientConfiguration"/>.
/// </summary>
public static class HttpClientFactoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the <see cref="HttpClientConfiguration"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="HttpClientConfiguration"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the <see cref="HttpClientConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this HttpClientConfiguration value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="HttpClientConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="HttpClientConfiguration"/> instance if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static HttpClientConfiguration? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<HttpClientConfiguration>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="HttpClientConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="HttpClientConfiguration"/> instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out HttpClientConfiguration? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<HttpClientConfiguration>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}