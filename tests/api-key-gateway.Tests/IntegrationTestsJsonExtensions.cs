// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides JSON serialization extensions for <see cref="IntegrationTests"/> instances.
/// </summary>
public static class IntegrationTestsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	/// <summary>
	/// Serializes the <see cref="IntegrationTests"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The instance to serialize. Must not be null.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A JSON string representation of the instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this IntegrationTests value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
			: _jsonSerializerOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to an <see cref="IntegrationTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
	/// <returns>The deserialized instance, or <see langword="null"/> if the JSON is invalid.</returns>
	/// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or consists only of white-space.</exception>
	public static IntegrationTests? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		return JsonSerializer.Deserialize<IntegrationTests>(json, _jsonSerializerOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to an <see cref="IntegrationTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
	/// <param name="value">Receives the deserialized instance if successful; otherwise, <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or consists only of white-space.</exception>
	public static bool TryFromJson(string json, out IntegrationTests? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<IntegrationTests>(json, _jsonSerializerOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}
