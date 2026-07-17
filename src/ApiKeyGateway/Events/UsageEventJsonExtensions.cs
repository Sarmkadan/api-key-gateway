// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace ApiKeyGateway.Events;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="UsageEvent"/> and its derived types.
/// </summary>
public static class UsageEventJsonExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>
    /// Converts a <see cref="UsageEvent"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="UsageEvent"/> to convert.</param>
    /// <param name="indented">Whether to indent the JSON output.</param>
    /// <returns>The JSON string representation of the <see cref="UsageEvent"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this UsageEvent value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? JsonSerializerOptions
            : new JsonSerializerOptions(JsonSerializerOptions) { WriteIndented = false };

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="UsageEvent"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="UsageEvent"/>, or null if the JSON string is invalid.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static UsageEvent? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<UsageEvent>(json, JsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="UsageEvent"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized <see cref="UsageEvent"/> if successful,
    /// or null if the JSON string is invalid.
    /// </param>
    /// <returns>True if the JSON string was successfully deserialized; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out UsageEvent? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<UsageEvent>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
