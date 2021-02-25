// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ApiKeyGateway.Tests;

public static class UsageAnalyticsServiceTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the <see cref="UsageAnalyticsServiceTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this UsageAnalyticsServiceTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? GetIndentedOptions() : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="UsageAnalyticsServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or null if the JSON is null or empty.</returns>
    public static UsageAnalyticsServiceTests? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<UsageAnalyticsServiceTests>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="UsageAnalyticsServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out UsageAnalyticsServiceTests? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<UsageAnalyticsServiceTests>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static JsonSerializerOptions GetIndentedOptions()
    {
        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = true
        };
        return options;
    }
}