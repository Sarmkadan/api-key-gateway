using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides JSON serialization helpers for <see cref="UsageQuotaServiceTests"/>.
/// </summary>
public static class UsageQuotaServiceTestsJsonExtensions
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
    /// Serializes the <see cref="UsageQuotaServiceTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The test instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the test instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this UsageQuotaServiceTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(Options)
        {
            WriteIndented = indented
        };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="UsageQuotaServiceTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized <see cref="UsageQuotaServiceTests"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown if JSON deserialization fails.</exception>
    public static UsageQuotaServiceTests FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<UsageQuotaServiceTests>(json, Options)
            ?? throw new JsonException("Failed to deserialize UsageQuotaServiceTests.");
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="UsageQuotaServiceTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized test instance if deserialization succeeded, or <see langword="null"/> if deserialization failed.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out UsageQuotaServiceTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<UsageQuotaServiceTests>(json, Options);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
