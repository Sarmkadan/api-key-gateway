// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Text.Json;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="GatewayConfiguration"/>.
/// </summary>
public static class ServiceCollectionExtensionsJsonExtensions
{
    /// <summary>
    /// Caches JSON serialization options with camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="GatewayConfiguration"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The gateway configuration to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the gateway configuration.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public static string ToJson(this GatewayConfiguration value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(JsonSerializerOptions) { WriteIndented = true }
            : JsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="GatewayConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized gateway configuration, or <see langword="null"/> if JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="json"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="JsonException">
    /// Thrown if JSON is invalid or cannot be deserialized.
    /// </exception>
    public static GatewayConfiguration? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<GatewayConfiguration>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="GatewayConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized gateway configuration if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="json"/> is <see langword="null"/>.
    /// </exception>
    public static bool TryFromJson(string json, out GatewayConfiguration? value)
    {
        value = null;

        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<GatewayConfiguration>(json, JsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}