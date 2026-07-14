// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// System.Text.Json serialization helpers for KeyStoreUnavailableException
// =====================================================================

using System.Text.Json;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="KeyStoreUnavailableException"/>.
/// </summary>
public static class KeyStoreUnavailableExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Converts a <see cref="KeyStoreUnavailableException"/> to its JSON representation.
    /// </summary>
    /// <param name="value">The key store unavailable exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this KeyStoreUnavailableException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses a <see cref="KeyStoreUnavailableException"/> from its JSON representation.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>The deserialized key store unavailable exception, or null if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static KeyStoreUnavailableException? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<KeyStoreUnavailableException>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to parse a <see cref="KeyStoreUnavailableException"/> from its JSON representation.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <param name="value">Receives the deserialized key store unavailable exception if successful.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out KeyStoreUnavailableException? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<KeyStoreUnavailableException>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}