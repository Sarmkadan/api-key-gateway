// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Provides JSON serialization helpers for <see cref="TransformationRule"/>.
/// </summary>
public static class DatabaseTransformationRuleRepositoryJsonExtensions
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
    /// Serializes <see cref="TransformationRule"/> to a JSON string.
    /// </summary>
    /// <param name="value">The transformation rule to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the transformation rule.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this TransformationRule value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializerOptions options = indented
            ? new(JsonSerializerOptions) { WriteIndented = true }
            : JsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to <see cref="TransformationRule"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized transformation rule.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    public static TransformationRule? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<TransformationRule>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to <see cref="TransformationRule"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized transformation rule if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    public static bool TryFromJson(string json, out TransformationRule? value)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentException.ThrowIfNullOrEmpty(json);
            value = JsonSerializer.Deserialize<TransformationRule>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}