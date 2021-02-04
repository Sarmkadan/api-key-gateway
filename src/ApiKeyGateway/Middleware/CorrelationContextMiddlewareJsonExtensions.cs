// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Middleware;

/// <summary>
/// Provides JSON serialization extensions for <see cref="CorrelationContextMiddleware"/>.
/// </summary>
public static class CorrelationContextMiddlewareJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the <see cref="CorrelationContextMiddleware"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The middleware instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the middleware.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this CorrelationContextMiddleware value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(new
        {
            Type = nameof(CorrelationContextMiddleware),
            Assembly = typeof(CorrelationContextMiddleware).Assembly.GetName().Name
        }, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="CorrelationContextMiddleware"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized <see cref="CorrelationContextMiddleware"/> instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static CorrelationContextMiddleware? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return TryFromJson(json, out var value) ? value : null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="CorrelationContextMiddleware"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out CorrelationContextMiddleware? value)
    {
        value = null;

        try
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("Type", out var typeElement) &&
                typeElement.GetString() == nameof(CorrelationContextMiddleware))
            {
                // Middleware instances are stateless and context-dependent,
                // so we return a new instance rather than attempting to reconstruct state
                value = new CorrelationContextMiddleware(
                    static context => Task.CompletedTask,
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<CorrelationContextMiddleware>.Instance);
                return true;
            }

            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}