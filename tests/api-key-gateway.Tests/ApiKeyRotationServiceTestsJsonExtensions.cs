using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ApiKeyRotationServiceTests"/>.
/// </summary>
public static class ApiKeyRotationServiceTestsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	};

	/// <summary>
	/// Serializes a <see cref="ApiKeyRotationServiceTests"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The instance to serialize.</param>
	/// <param name="indented">Whether to indent the JSON for readability.</param>
	/// <returns>A JSON string representation of the instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this ApiKeyRotationServiceTests value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);
		return JsonSerializer.Serialize(value, CreateOptions(indented));
	}

	private static JsonSerializerOptions CreateOptions(bool indented) =>
		new JsonSerializerOptions(_jsonOptions)
		{
			PropertyNamingPolicy = _jsonOptions.PropertyNamingPolicy,
			TypeInfoResolver = _jsonOptions.TypeInfoResolver,
			WriteIndented = indented,
		};

	/// <summary>
	/// Deserializes a JSON string to an <see cref="ApiKeyRotationServiceTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized instance, or <see langword="null"/> if deserialization fails.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or whitespace.</exception>
	/// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
	public static ApiKeyRotationServiceTests? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrWhiteSpace(json);

		return JsonSerializer.Deserialize<ApiKeyRotationServiceTests>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to an <see cref="ApiKeyRotationServiceTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized instance if successful.</param>
	/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or whitespace.</exception>
	public static bool TryFromJson(string json, out ApiKeyRotationServiceTests? value)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrWhiteSpace(json);

		try
		{
			value = JsonSerializer.Deserialize<ApiKeyRotationServiceTests>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}
