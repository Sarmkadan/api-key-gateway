// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;

namespace ApiKeyGateway.Extensions;

/// <summary>
/// Provides JSON serialization and deserialization for StringExtensions type metadata.
/// Since StringExtensions is a static class, this class serializes metadata about the extension methods
/// rather than instances of StringExtensions itself.
/// </summary>
public static class StringExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes StringExtensions type metadata to a JSON string.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing StringExtensions type metadata.</returns>
    public static string ToJson(bool indented = false)
    {
        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        var metadata = new StringExtensionsMetadata
        {
            TypeName = "StringExtensions",
            Methods = GetPublicMethodNames(),
        };

        return JsonSerializer.Serialize(metadata, options);
    }

    /// <summary>
    /// Deserializes a JSON string to StringExtensions type metadata.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A StringExtensionsMetadata object, or null if the JSON is null or empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static StringExtensionsMetadata? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<StringExtensionsMetadata>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to StringExtensions type metadata.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized metadata if successful.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out StringExtensionsMetadata? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<StringExtensionsMetadata>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Metadata model for StringExtensions type serialization.
    /// </summary>
    public sealed class StringExtensionsMetadata
    {
        /// <summary>Gets or sets the type name.</summary>
        public string? TypeName { get; set; }

        /// <summary>Gets or sets the list of public method names.</summary>
        public IReadOnlyList<string>? Methods { get; set; }
    }

    /// <summary>
    /// Gets the names of public methods on StringExtensions for serialization.
    /// </summary>
    private static IReadOnlyList<string> GetPublicMethodNames()
    {
        return [
            nameof(StringExtensions.Truncate),
            nameof(StringExtensions.TruncateWithEllipsis),
            nameof(StringExtensions.ContainsAny),
            nameof(StringExtensions.StartsWithAny),
            nameof(StringExtensions.ToSlug),
            nameof(StringExtensions.CapitalizeFirst),
            nameof(StringExtensions.ToList),
            nameof(StringExtensions.IsNumeric),
            nameof(StringExtensions.TryParseInt),
            nameof(StringExtensions.TryParseLong)
        ];
    }
}
