// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using JsonException = System.Text.Json.JsonException;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="UsageAnalyticsServiceTests"/>.
/// </summary>
public static class UsageAnalyticsServiceTestsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
		ReferenceHandler = ReferenceHandler.IgnoreCycles
	};

	/// <summary>
	/// Serializes the <see cref="UsageAnalyticsServiceTests"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A JSON string representation of the instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this UsageAnalyticsServiceTests value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		return JsonSerializer.Serialize(value, indented ? GetIndentedOptions() : _jsonOptions);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="UsageAnalyticsServiceTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static UsageAnalyticsServiceTests? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		return JsonSerializer.Deserialize<UsageAnalyticsServiceTests>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="UsageAnalyticsServiceTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">The deserialized instance, or null if deserialization fails.</param>
	/// <returns>True if deserialization succeeds; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static bool TryFromJson(string json, out UsageAnalyticsServiceTests? value)
	{
		value = null;

		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<UsageAnalyticsServiceTests>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}

	/// <summary>
	/// Creates a new <see cref="JsonSerializerOptions"/> instance with indentation enabled.
	/// </summary>
	/// <returns>A new <see cref="JsonSerializerOptions"/> with <see cref="JsonSerializerOptions.WriteIndented"/> set to true.</returns>
	private static JsonSerializerOptions GetIndentedOptions() =>
		new(_jsonOptions) { WriteIndented = true };
}