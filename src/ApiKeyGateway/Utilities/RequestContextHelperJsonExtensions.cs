// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// System.Text.Json serialization helpers for RequestContext.
// Provides efficient serialization/deserialization with camelCase naming convention.
// =====================================================================

using System.Text.Json;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="RequestContext"/>.
/// </summary>
public static class RequestContextHelperJsonExtensions
{
	/// <summary>
	/// JSON serialization options with camelCase naming policy.
	/// </summary>
	private static readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	/// <summary>
	/// Converts a <see cref="RequestContext"/> to its JSON representation.
	/// </summary>
	/// <param name="value">The request context to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representing the request context.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this RequestContext value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Parses a <see cref="RequestContext"/> from its JSON representation.
	/// </summary>
	/// <param name="json">The JSON string to parse.</param>
	/// <returns>The deserialized request context, or null if the JSON represents a null value.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
	public static RequestContext? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrEmpty(json);

		return JsonSerializer.Deserialize<RequestContext>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to parse a <see cref="RequestContext"/> from its JSON representation.
	/// </summary>
	/// <param name="json">The JSON string to parse.</param>
	/// <param name="value">Receives the deserialized request context if successful.</param>
	/// <returns>True if parsing succeeded; otherwise, false.</returns>
	public static bool TryFromJson(string json, out RequestContext? value)
	{
		if (string.IsNullOrEmpty(json))
		{
			value = null;
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<RequestContext>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}