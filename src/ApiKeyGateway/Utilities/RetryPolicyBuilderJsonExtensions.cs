// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Provides JSON serialization helpers for <see cref="RetryPolicyBuilder"/>.
/// </summary>
public static class RetryPolicyBuilderJsonExtensions
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
    /// Serializes <see cref="RetryPolicyBuilder"/> to a JSON string.
    /// </summary>
    /// <param name="value">The retry policy builder to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the retry policy builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this RetryPolicyBuilder value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializerOptions options = indented
            ? new(JsonSerializerOptions) { WriteIndented = true }
            : JsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to <see cref="RetryPolicyBuilder"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized retry policy builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    public static RetryPolicyBuilder? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<RetryPolicyBuilder>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to <see cref="RetryPolicyBuilder"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized retry policy builder if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    public static bool TryFromJson(string json, out RetryPolicyBuilder? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<RetryPolicyBuilder>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}