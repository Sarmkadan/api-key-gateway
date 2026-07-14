using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides JSON serialization helpers for <see cref="ApiKeyModelTests"/>.
/// </summary>
public static class ApiKeyModelTestsJsonExtensions
{
    /// <summary>
    /// JSON serialization options configured for camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="ApiKeyModelTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The test instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the test instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this ApiKeyModelTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(Options);
        options.WriteIndented = indented;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="ApiKeyModelTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized <see cref="ApiKeyModelTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown if JSON deserialization fails.</exception>
    public static ApiKeyModelTests FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<ApiKeyModelTests>(json, Options)
            ?? throw new JsonException("Failed to deserialize ApiKeyModelTests.");
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="ApiKeyModelTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized test instance, or null if deserialization failed.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ApiKeyModelTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<ApiKeyModelTests>(json, Options);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
