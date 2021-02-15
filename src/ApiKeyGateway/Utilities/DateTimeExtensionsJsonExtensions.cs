// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for DateTime values
/// </summary>
public static class DateTimeExtensionsJsonExtensions
{
    private static readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a DateTime to a JSON string
    /// </summary>
    /// <param name="dateTime">The DateTime value to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the DateTime</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dateTime"/> is null</exception>
    public static string ToJson(this DateTime dateTime, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(dateTime);

        var options = indented
            ? new System.Text.Json.JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return System.Text.Json.JsonSerializer.Serialize(dateTime, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a DateTime
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized DateTime, or null if the JSON is null or whitespace</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be converted to DateTime</exception>
    public static DateTime? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return System.Text.Json.JsonSerializer.Deserialize<DateTime>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a DateTime
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">The deserialized DateTime, or null if parsing failed</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJson(string json, out DateTime? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            value = System.Text.Json.JsonSerializer.Deserialize<DateTime>(json, _jsonOptions);
            return true;
        }
        catch (System.Text.Json.JsonException)
        {
            return false;
        }
    }
}