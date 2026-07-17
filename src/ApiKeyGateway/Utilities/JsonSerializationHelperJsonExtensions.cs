// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// System.Text.Json serialization helpers for JsonSerializationHelper configuration.
// Provides extension-style API for serializing and deserializing JsonSerializationHelper
// serialization settings.
// =============================================================================

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for JsonSerializationHelper configuration.
/// </summary>
public static class JsonSerializationHelperJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    /// <summary>
    /// Represents serialization configuration compatible with JsonSerializationHelper.
    /// This type can be serialized and deserialized to persist JsonSerializationHelper settings.
    /// </summary>
    public sealed record JsonSerializationSettings
    {
        /// <summary>
        /// Gets the property naming policy used for serialization.
        /// </summary>
        public JsonNamingPolicy PropertyNamingPolicy { get; init; } = JsonNamingPolicy.CamelCase;

        /// <summary>
        /// Gets the condition for ignoring properties during serialization.
        /// </summary>
        public JsonIgnoreCondition DefaultIgnoreCondition { get; init; } = JsonIgnoreCondition.WhenWritingNull;

        /// <summary>
        /// Gets whether to write JSON with indentation.
        /// </summary>
        public bool WriteIndented { get; init; } = false;
    }

    /// <summary>
    /// Serializes a JsonSerializationSettings instance to a JSON string.
    /// </summary>
    /// <param name="value">The settings to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the settings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this JsonSerializationSettings value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented || value.WriteIndented
        };

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a JsonSerializationSettings instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A JsonSerializationSettings instance if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty.</exception>
    public static JsonSerializationSettings? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<JsonSerializationSettings>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a JsonSerializationSettings instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized JsonSerializationSettings instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty.</exception>
    public static bool TryFromJson(string json, [NotNullWhen(true)] out JsonSerializationSettings? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<JsonSerializationSettings>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}