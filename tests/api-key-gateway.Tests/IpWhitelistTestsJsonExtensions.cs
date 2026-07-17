// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides JSON serialization helpers for <see cref="IpWhitelistTests"/> test fixture instances.
/// </summary>
public static class IpWhitelistTestsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	/// <summary>
	/// Serializes the <see cref="IpWhitelistTests"/> test fixture instance to a JSON string.
	/// </summary>
	/// <param name="value">The test fixture instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A JSON representation of the test fixture instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this IpWhitelistTests value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		return JsonSerializer.Serialize(value, indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions);
	}

	/// <summary>
	/// Deserializes a JSON string to an <see cref="IpWhitelistTests"/> test fixture instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize. Represents a JSON <c>null</c> value.</param>
	/// <returns>The deserialized test fixture instance, or <see langword="null"/> if the JSON represents a <c>null</c> value.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized into a <see cref="IpWhitelistTests"/> instance.</exception>
	public static IpWhitelistTests? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		return JsonSerializer.Deserialize<IpWhitelistTests>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to an <see cref="IpWhitelistTests"/> test fixture instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize. Represents a JSON <c>null</c> value.</param>
	/// <param name="value">Receives the deserialized test fixture instance if deserialization succeeds.</param>
	/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
	public static bool TryFromJson(string json, out IpWhitelistTests? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		try
		{
			value = JsonSerializer.Deserialize<IpWhitelistTests>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = default;
			return false;
		}
	}
}
