using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="RateLimitingServiceTests"/> instances.
/// </summary>
public static class RateLimitingServiceTestsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
		WriteIndented = false,
	};

	/// <summary>
	/// Serializes a <see cref="RateLimitingServiceTests"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The instance to serialize.</param>
	/// <param name="indented">Whether to indent the JSON for readability.</param>
	/// <returns>A JSON string representation of the instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this RateLimitingServiceTests value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);
		return JsonSerializer.Serialize(value, CreateOptions(indented));
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="RateLimitingServiceTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized instance, or <see langword="null"/> if the JSON is empty.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
	public static RateLimitingServiceTests? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		return JsonSerializer.Deserialize<RateLimitingServiceTests>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="RateLimitingServiceTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized instance if successful.</param>
	/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
	public static bool TryFromJson(string json, out RateLimitingServiceTests? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		try
		{
			value = JsonSerializer.Deserialize<RateLimitingServiceTests>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}

	private static JsonSerializerOptions CreateOptions(bool indented)
		=> new(_jsonOptions)
		{
			WriteIndented = indented,
			PropertyNamingPolicy = _jsonOptions.PropertyNamingPolicy,
			TypeInfoResolver = _jsonOptions.TypeInfoResolver,
		};
}
