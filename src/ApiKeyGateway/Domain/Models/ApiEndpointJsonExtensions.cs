// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="ApiEndpoint"/>.
/// </summary>
public static class ApiEndpointJsonExtensions
{
    /// <summary>
    /// Gets the default JSON serialization options for <see cref="ApiEndpoint"/>.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes a <see cref="ApiEndpoint"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The endpoint to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the endpoint.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ApiEndpoint value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        
        var options = indented 
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } 
             : _jsonOptions;
        
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to <see cref="ApiEndpoint"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized endpoint.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    public static ApiEndpoint? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<ApiEndpoint>(json, _jsonOptions);
    }

/// <summary>
/// Attempts to deserialize a JSON string to a <see cref="ApiEndpoint"/> instance.
/// </summary>
/// <param name="json">The JSON string to deserialize.</param>
/// <param name="value">Receives the deserialized endpoint if successful, otherwise null.</param>
/// <returns>True if deserialization succeeded; otherwise, false.</returns>
public static bool TryFromJson(string json, out ApiEndpoint? value)
{
    value = null;

    if (string.IsNullOrWhiteSpace(json))
    {
        return false;
    }

    try
    {
        value = JsonSerializer.Deserialize<ApiEndpoint>(json, _jsonOptions);
        return value is not null;
    }
    catch (JsonException)
    {
        return false;
    }
}

}
