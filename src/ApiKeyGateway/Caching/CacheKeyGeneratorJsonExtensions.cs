// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// System.Text.Json serialization extensions for CacheKeyGenerator
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Caching;

/// <summary>
/// Provides System.Text.Json serialization extensions for cache key generation patterns.
/// Enables serialization and deserialization of cache key generator configuration.
/// </summary>
public static class CacheKeyGeneratorJsonExtensions
{
    /// <summary>
    /// Represents the configuration for cache key generation patterns.
    /// This DTO can be serialized and deserialized to persist cache key generation settings.
    /// </summary>
    public sealed record CacheKeyGeneratorConfiguration
    {
        /// <summary>
        /// Gets the cache key prefix used for all generated keys.
        /// </summary>
        public string Prefix { get; init; } = "apigw";

        /// <summary>
        /// Gets the separator character used between key components.
        /// </summary>
        public char Separator { get; init; } = ':';

        /// <summary>
        /// Creates a <see cref="CacheKeyGeneratorConfiguration"/> with the default cache key generator settings.
        /// </summary>
        /// <returns>A configuration instance representing the default cache key generator settings.</returns>
        public static CacheKeyGeneratorConfiguration FromCacheKeyGenerator() =>
            new() { Prefix = "apigw", Separator = ':' };
    }

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the cache key generator configuration to a JSON string.
    /// </summary>
    /// <param name="value">The cache key generator configuration to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the cache key generator configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this CacheKeyGeneratorConfiguration value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="CacheKeyGeneratorConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="CacheKeyGeneratorConfiguration"/> instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static CacheKeyGeneratorConfiguration? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<CacheKeyGeneratorConfiguration>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="CacheKeyGeneratorConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance, or null on failure.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out CacheKeyGeneratorConfiguration? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<CacheKeyGeneratorConfiguration>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
