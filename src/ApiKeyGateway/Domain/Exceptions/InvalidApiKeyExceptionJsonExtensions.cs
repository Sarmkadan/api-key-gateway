// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect

// System.Text.Json serialization helpers for InvalidApiKeyException.
// Provides efficient serialization/deserialization with camelCase naming convention.
// =====================================================================

using System.Text.Json;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="InvalidApiKeyException"/>.
/// </summary>
public static class InvalidApiKeyExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Converts an <see cref="InvalidApiKeyException"/> to its JSON representation.
    /// </summary>
    /// <param name="value">The invalid API key exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this InvalidApiKeyException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses an <see cref="InvalidApiKeyException"/> from its JSON representation.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>The deserialized invalid API key exception, or null if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static InvalidApiKeyException? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<InvalidApiKeyException>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to parse an <see cref="InvalidApiKeyException"/> from its JSON representation.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <param name="value">Receives the deserialized invalid API key exception if successful.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out InvalidApiKeyException? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<InvalidApiKeyException>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}