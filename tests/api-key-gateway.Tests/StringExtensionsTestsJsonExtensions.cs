// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using ApiKeyGateway.Extensions;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides unit tests for <see cref="StringExtensionsJsonExtensions"/> JSON serialization methods.
/// </summary>
public static class StringExtensionsTestsJsonExtensions
{
    /// <summary>
    /// Metadata model for StringExtensionsJsonExtensions type serialization.
    /// </summary>
    public sealed class StringExtensionsMetadata
    {
        /// <summary>Gets or sets the type name.</summary>
        public string? TypeName { get; set; }

        /// <summary>Gets or sets the list of public method names.</summary>
        public IReadOnlyList<string>? Methods { get; set; }
    }
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions _jsonOptionsIndented = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes StringExtensionsJsonExtensions metadata to a JSON string.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representing StringExtensions metadata.</returns>
    public static string ToJson(bool indented = false)
    {
        return StringExtensionsJsonExtensions.ToJson(indented);
    }

    /// <summary>
    /// Deserializes a JSON string to StringExtensionsJsonExtensions metadata.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A StringExtensionsMetadata object, or null if the JSON is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or consists only of whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static StringExtensionsMetadata? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<StringExtensionsMetadata>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to StringExtensionsJsonExtensions metadata.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized metadata if successful.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out StringExtensionsMetadata? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<StringExtensionsMetadata>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}