// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Provides JSON serialization helpers for <see cref="TransformationPipelineOptions"/>.
/// </summary>
public static class TransformationPipelineOptionsJsonExtensions
{
    /// <summary>
    /// Caches JSON serialization options with camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes <see cref="TransformationPipelineOptions"/> to a JSON string.
    /// </summary>
    /// <param name="value">The transformation pipeline options to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the transformation pipeline options.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this TransformationPipelineOptions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializerOptions options = indented
            ? new(JsonSerializerOptions) { WriteIndented = true }
            : JsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to <see cref="TransformationPipelineOptions"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized transformation pipeline options.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    public static TransformationPipelineOptions? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<TransformationPipelineOptions>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to <see cref="TransformationPipelineOptions"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized transformation pipeline options if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    public static bool TryFromJson(string json, out TransformationPipelineOptions? value)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentException.ThrowIfNullOrEmpty(json);
            value = JsonSerializer.Deserialize<TransformationPipelineOptions>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}