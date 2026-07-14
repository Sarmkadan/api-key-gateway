// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Provides JSON serialization helpers for <see cref="RateLimitRepository"/>.
/// </summary>
public static class RateLimitRepositoryJsonExtensions
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
    /// Serializes <see cref="RateLimitRepository"/> to a JSON string.
    /// </summary>
    /// <param name="value">The rate limit repository to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the rate limit repository.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this RateLimitRepository value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializerOptions options = indented
            ? new(JsonSerializerOptions) { WriteIndented = true }
            : JsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to <see cref="RateLimit"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized rate limit.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    public static RateLimit? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<RateLimit>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to <see cref="RateLimit"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized rate limit if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    public static bool TryFromJson(string json, out RateLimit? value)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentException.ThrowIfNullOrEmpty(json);
            value = JsonSerializer.Deserialize<RateLimit>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}