// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using ApiKeyGateway.Domain.Enums;

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="UsageQuota"/>.
/// </summary>
public static class UsageQuotaJsonExtensions
{
    /// <summary>
    /// Gets the default JSON serialization options for <see cref="UsageQuota"/>.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes a <see cref="UsageQuota"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The usage quota to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the usage quota.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
public static string ToJson(this UsageQuota value, bool indented = false)
{
	ArgumentNullException.ThrowIfNull(value);

	var options = indented
		? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

	return JsonSerializer.Serialize(value, options);
}

    /// <summary>
    /// Deserializes a JSON string to a <see cref="UsageQuota"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized usage quota, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static UsageQuota? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<UsageQuota>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="UsageQuota"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized usage quota if successful, otherwise null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out UsageQuota? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<UsageQuota>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}