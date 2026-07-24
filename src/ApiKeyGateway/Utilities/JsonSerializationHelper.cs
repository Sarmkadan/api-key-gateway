// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Centralized JSON serialization helper that enforces consistent
/// serialization behavior across the application. We use PascalCase
/// for internal C# code but convert to camelCase for API responses
/// to follow REST API conventions.
/// </summary>
public static class JsonSerializationHelper
{
    /// <summary>
    /// Gets the hardened JSON serializer options for deserializing attacker-controlled input.
    /// This configuration disables polymorphic type resolution and sets a maximum depth
    /// to prevent stack overflow attacks via deeply nested JSON.
    /// </summary>
    public static JsonSerializerOptions HardenedDeserializeOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        // Security hardening: prevent polymorphic type confusion attacks
        // TypeNameHandling = null (default, no TypeNameHandling equivalent)
        // $type binder is not used (default behavior)
        // Prevent stack overflow via deeply nested JSON
        MaxDepth = 16,
        // Prevent DoS via large payloads - enforce at HTTP layer before parsing
        // This is the maximum allowed JSON payload size in bytes
        // Actual enforcement should happen at HTTP middleware level
    };

    private static readonly JsonSerializerOptions ApiOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    private static readonly JsonSerializerOptions FormattedApiOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    /// <summary>
    /// Serializes an object to JSON using API conventions (camelCase, compact).
    /// </summary>
    public static string SerializeCompact<T>(T obj) =>
        JsonSerializer.Serialize(obj, ApiOptions);

    /// <summary>
    /// Serializes an object to JSON with pretty printing for human readability.
    /// Used for admin endpoints and debugging responses.
    /// </summary>
    public static string SerializeFormatted<T>(T obj) =>
        JsonSerializer.Serialize(obj, FormattedApiOptions);

    /// <summary>
    /// Deserializes JSON to object using API conventions.
    /// </summary>
    public static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, ApiOptions);

    /// <summary>
    /// Deserializes JSON from attacker-controlled input using hardened security settings.
    /// This should be used when deserializing any untrusted JSON input (request bodies, headers, etc.).
    /// </summary>
    /// <typeparam name="T">The type to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized object, or null if deserialization fails.</returns>
    public static T? DeserializeHardened<T>(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<T>(json, HardenedDeserializeOptions);
    }

    /// <summary>
    /// Safely attempts deserialization with error handling.
    /// Returns null on parse failure instead of throwing.
    /// </summary>
    public static T? SafeDeserialize<T>(string json, ILogger? logger = null)
    {
        try
        {
            return Deserialize<T>(json);
        }
        catch (JsonException ex)
        {
            logger?.LogWarning(ex, "Failed to deserialize JSON");
            return default;
        }
    }

    /// <summary>
    /// Validates that the input string is valid JSON without full deserialization.
    /// Useful for quick validation before processing.
    /// </summary>
    public static bool IsValidJson(string input)
    {
        try
        {
            using var doc = JsonDocument.Parse(input);
            return doc.RootElement.ValueKind != JsonValueKind.Undefined;
        }
        catch
        {
            return false;
        }
    }
}
