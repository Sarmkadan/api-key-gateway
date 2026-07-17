using System;
using System.Text.Json;
using ApiKeyGateway.Services;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides JSON serialization extensions for <see cref="MetricsSnapshot"/> instances.
/// </summary>
public static class MetricsCollectionServiceTestsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	/// <summary>
	/// Serializes a <see cref="MetricsSnapshot"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The value to serialize.</param>
	/// <param name="indented">Whether to indent the JSON for readability.</param>
	/// <returns>The serialized JSON string.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this MetricsSnapshot value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;
		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a <see cref="MetricsSnapshot"/> instance from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized instance, or <see langword="null"/> if the JSON is empty.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or whitespace.</exception>
	public static MetricsSnapshot? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrWhiteSpace(json);

		return string.IsNullOrEmpty(json)
			? null
			: JsonSerializer.Deserialize<MetricsSnapshot>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a <see cref="MetricsSnapshot"/> instance from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized instance if successful.</param>
	/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
	public static bool TryFromJson(string json, out MetricsSnapshot? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		try
		{
			value = JsonSerializer.Deserialize<MetricsSnapshot>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}