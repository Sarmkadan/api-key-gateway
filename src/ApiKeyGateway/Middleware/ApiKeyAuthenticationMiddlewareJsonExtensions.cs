using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ApiKeyGateway.Middleware;

public static class ApiKeyAuthenticationMiddlewareJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an <see cref="ApiKeyAuthenticationMiddleware"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The middleware instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the middleware.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this ApiKeyAuthenticationMiddleware value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="ApiKeyAuthenticationMiddleware"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized middleware instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    public static ApiKeyAuthenticationMiddleware? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ApiKeyAuthenticationMiddleware>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="ApiKeyAuthenticationMiddleware"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized middleware instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ApiKeyAuthenticationMiddleware? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ApiKeyAuthenticationMiddleware>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}