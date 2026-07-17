using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides JSON serialization helpers for test data.
/// </summary>
public static class UsageTrackingServiceTestsJsonExtensions
{
    /// <summary>
    /// JSON serialization options configured for camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the <see cref="UsageTrackingServiceTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The test instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the test instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this UsageTrackingServiceTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? GetIndentedOptions() : Options);
    }

    /// <summary>
    /// Deserializes a <see cref="UsageTrackingServiceTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized <see cref="UsageTrackingServiceTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown if JSON deserialization fails.</exception>
    public static UsageTrackingServiceTests FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<UsageTrackingServiceTests>(json, Options)
            ?? throw new JsonException("Failed to deserialize UsageTrackingServiceTests.");
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="UsageTrackingServiceTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized test instance, or null if deserialization failed.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out UsageTrackingServiceTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<UsageTrackingServiceTests>(json, Options);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Gets a configured <see cref="JsonSerializerOptions"/> instance with indentation enabled.
    /// </summary>
    /// <returns>A new <see cref="JsonSerializerOptions"/> instance with indentation.</returns>
    private static JsonSerializerOptions GetIndentedOptions() => new(Options)
    {
        WriteIndented = true
    };
}
