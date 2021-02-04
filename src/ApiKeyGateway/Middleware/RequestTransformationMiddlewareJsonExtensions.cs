// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Middleware;

/// <summary>
/// Provides JSON serialization helpers for <see cref="RequestTransformationMiddleware"/>.
/// </summary>
public static class RequestTransformationMiddlewareJsonExtensions
{
    /// <summary>
    /// Caches JSON serialization options with camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes <see cref="RequestTransformationMiddleware"/> to a JSON string.
    /// </summary>
    /// <param name="value">The middleware instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the middleware.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this RequestTransformationMiddleware value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializerOptions options = indented
            ? new(JsonSerializerOptions) { WriteIndented = true }
            : JsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to <see cref="RequestTransformationMiddleware"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized middleware instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    public static RequestTransformationMiddleware? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<RequestTransformationMiddleware>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to <see cref="RequestTransformationMiddleware"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized middleware instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    public static bool TryFromJson(string json, out RequestTransformationMiddleware? value)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentException.ThrowIfNullOrEmpty(json);

            value = JsonSerializer.Deserialize<RequestTransformationMiddleware>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
