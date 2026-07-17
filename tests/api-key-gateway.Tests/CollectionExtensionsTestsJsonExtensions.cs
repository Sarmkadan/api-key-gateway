using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides JSON serialization helpers for serializing and deserializing <see cref="CollectionExtensionsTests"/> instances.
/// </summary>
public static class CollectionExtensionsTestsJsonExtensions
{
    /// <summary>
    /// Gets the JSON serialization options configured for camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="CollectionExtensionsTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The test instance to serialize. Must not be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the test instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToJson(this CollectionExtensionsTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(Options)
        {
            WriteIndented = indented
        };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="CollectionExtensionsTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
    /// <returns>A deserialized <see cref="CollectionExtensionsTests"/> instance.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">JSON deserialization failed or returned null.</exception>
    public static CollectionExtensionsTests FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<CollectionExtensionsTests>(json, Options)
            ?? throw new JsonException("Failed to deserialize CollectionExtensionsTests.");
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="CollectionExtensionsTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
    /// <param name="value">The deserialized test instance, or null if deserialization failed.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out CollectionExtensionsTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<CollectionExtensionsTests>(json, Options);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}