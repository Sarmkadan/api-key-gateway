// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="TransformationRule"/>.
/// All serialization uses invariant culture and camelCase property naming for consistency.
/// </summary>
public static class TransformationRuleJsonExtensions
{
    /// <summary>
    /// Cached JSON serialization options configured for invariant culture and camelCase property naming.
    /// Uses Web defaults for enum handling and reference cycle ignoring.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Converts a <see cref="TransformationRule"/> instance to its JSON representation.
    /// </summary>
    /// <param name="value">The transformation rule to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the transformation rule.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="JsonException">Thrown when serialization fails due to invalid data or circular references.</exception>
    public static string ToJson(this TransformationRule value, bool indented = false)
        => JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);

    /// <summary>
    /// Parses a JSON string into a <see cref="TransformationRule"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to parse. Must not be null, empty, or whitespace.</param>
    /// <returns>The deserialized transformation rule, or <see langword="null"/> if parsing fails.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized to a <see cref="TransformationRule"/>.</exception>
    public static TransformationRule? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize<TransformationRule>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to parse a JSON string into a <see cref="TransformationRule"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to parse. Must not be null, empty, or whitespace.</param>
    /// <param name="value">Receives the deserialized transformation rule if successful.</param>
    /// <returns><see langword="true"/> if parsing succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static bool TryFromJson(string json, out TransformationRule? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            value = JsonSerializer.Deserialize<TransformationRule>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}