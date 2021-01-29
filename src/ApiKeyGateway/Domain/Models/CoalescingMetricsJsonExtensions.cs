// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides JSON serialization helpers for <see cref="CoalescingMetrics"/>.
/// </summary>
public static class CoalescingMetricsJsonExtensions
{
    /// <summary>
    /// Caches JSON serialization options with camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes <see cref="CoalescingMetrics"/> to a JSON string.
    /// </summary>
    /// <param name="value">The metrics to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this CoalescingMetrics value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        
        JsonSerializerOptions options = indented 
            ? new(JsonSerializerOptions) { WriteIndented = true } 
            : JsonSerializerOptions;
        
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to <see cref="CoalescingMetrics"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    public static CoalescingMetrics? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<CoalescingMetrics>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to <see cref="CoalescingMetrics"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized metrics if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    public static bool TryFromJson(string json, out CoalescingMetrics? value)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            value = JsonSerializer.Deserialize<CoalescingMetrics>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
