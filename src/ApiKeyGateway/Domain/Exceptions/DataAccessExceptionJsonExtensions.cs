// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect

// System.Text.Json serialization helpers for DataAccessException.
// Provides efficient serialization/deserialization with camelCase naming convention.
// =====================================================================

using System.Text.Json;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="DataAccessException"/>.
/// </summary>
public static class DataAccessExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Converts a <see cref="DataAccessException"/> to its JSON representation.
    /// </summary>
    /// <param name="value">The data access exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this DataAccessException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses a <see cref="DataAccessException"/> from its JSON representation.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>The deserialized data access exception, or null if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static DataAccessException? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<DataAccessException>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to parse a <see cref="DataAccessException"/> from its JSON representation.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <param name="value">Receives the deserialized data access exception if successful.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out DataAccessException? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<DataAccessException>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}