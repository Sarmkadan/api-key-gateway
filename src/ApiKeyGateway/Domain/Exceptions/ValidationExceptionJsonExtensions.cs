// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// System.Text.Json serialization helpers for ValidationException.
// Provides efficient serialization/deserialization with camelCase naming convention.
// =====================================================================

using System.Text.Json;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ValidationException"/>.
/// </summary>
public static class ValidationExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Converts a <see cref="ValidationException"/> to its JSON representation.
    /// </summary>
    /// <param name="value">The validation exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the exception.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this ValidationException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses a <see cref="ValidationException"/> from its JSON representation.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>The deserialized validation exception, or null if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
    /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized into a <see cref="ValidationException"/>.</exception>
    public static ValidationException? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ValidationException>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to parse a <see cref="ValidationException"/> from its JSON representation.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <param name="value">Receives the deserialized validation exception if successful.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
    public static bool TryFromJson(string json, out ValidationException? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ValidationException>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}