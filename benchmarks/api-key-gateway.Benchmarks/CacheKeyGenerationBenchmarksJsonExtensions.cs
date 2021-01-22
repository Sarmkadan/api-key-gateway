// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect

// System.Text.Json serialization helpers for CacheKeyGenerationBenchmarks benchmarks

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Benchmarks;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="CacheKeyGenerationBenchmarks"/>.
/// </summary>
public static class CacheKeyGenerationBenchmarksJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };


    /// <summary>
    /// Serializes the <see cref="CacheKeyGenerationBenchmarks"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The benchmarks instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the benchmarks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this CacheKeyGenerationBenchmarks value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="CacheKeyGenerationBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized benchmarks instance, or null if JSON is empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static CacheKeyGenerationBenchmarks? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<CacheKeyGenerationBenchmarks>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="CacheKeyGenerationBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized benchmarks instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out CacheKeyGenerationBenchmarks? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<CacheKeyGenerationBenchmarks>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}