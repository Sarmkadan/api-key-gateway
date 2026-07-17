// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="AuditLogServiceTests"/> instances.
/// </summary>
public static class AuditLogServiceTestsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	/// <summary>
	/// Serializes an <see cref="AuditLogServiceTests"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static string ToJson(this AuditLogServiceTests value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = new JsonSerializerOptions(_jsonSerializerOptions)
		{
			WriteIndented = indented
		};

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to an <see cref="AuditLogServiceTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized instance, or null if the JSON is empty or whitespace.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
	/// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
	public static AuditLogServiceTests? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		return string.IsNullOrWhiteSpace(json)
			? null
			: JsonSerializer.Deserialize<AuditLogServiceTests>(json, _jsonSerializerOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to an <see cref="AuditLogServiceTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized instance if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static bool TryFromJson(string json, out AuditLogServiceTests? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		value = null;

		if (string.IsNullOrWhiteSpace(json))
		{
			return true;
		}

		try
		{
			value = JsonSerializer.Deserialize<AuditLogServiceTests>(json, _jsonSerializerOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}
