// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="TransformationPipelineOptions"/>
/// using camelCase property naming convention.
/// </summary>
public static class TransformationPipelineOptionsJsonExtensions
{
    /// <summary>
    /// Gets the cached JSON serializer options configured with camelCase property naming.
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this TransformationPipelineOptions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializerOptions options = indented
            ? new JsonSerializerOptions(JsonSerializerOptions) { WriteIndented = true }
            : JsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to <see cref="TransformationPipelineOptions"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized transformation pipeline options, or <see langword="null"/> if JSON is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or consists only of whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized into <see cref="TransformationPipelineOptions"/>.</exception>
    public static TransformationPipelineOptions? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<TransformationPipelineOptions>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to <see cref="TransformationPipelineOptions"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized transformation pipeline options
    /// if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out TransformationPipelineOptions? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
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