// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for query string data.
/// Enables serialization and deserialization of query string parameters used with <see cref="QueryStringHelper"/>.
/// </summary>
public static class QueryStringHelperJsonExtensions
{
    /// <summary>
    /// Represents query string parameters that can be serialized and deserialized.
    /// This DTO can be used to persist query string data used with QueryStringHelper.
    /// </summary>
    public sealed record QueryStringData
    {
        /// <summary>
        /// Gets the collection of query string parameters.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, string> Parameters { get; init; } = new(StringComparer.Ordinal);
    }

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes query string data to a JSON string.
    /// </summary>
    /// <param name="value">The query string data to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the query string data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this QueryStringData value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="QueryStringData"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="QueryStringData"/> instance if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static QueryStringData? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

        try
        {
            return JsonSerializer.Deserialize<QueryStringData>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="QueryStringData"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="QueryStringData"/> instance if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out QueryStringData? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

        try
        {
            value = JsonSerializer.Deserialize<QueryStringData>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}