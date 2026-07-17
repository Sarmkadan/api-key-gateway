// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace ApiKeyGateway.Utilities;

using System.Text.Json;

/// <summary>
/// Provides System.Text.Json serialization extensions for DateTime values
/// </summary>
public static class DateTimeExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a DateTime to a JSON string
    /// </summary>
    /// <param name="dateTime">The DateTime value to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the DateTime</returns>
    public static string ToJson(this DateTime dateTime, bool indented = false)
    {
        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(dateTime, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a DateTime
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized DateTime, or null if the JSON is null or whitespace</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be converted to DateTime</exception>
    public static DateTime? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<DateTime>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a DateTime
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">The deserialized DateTime, or null if parsing failed</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJson(string? json, out DateTime? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<DateTime>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
