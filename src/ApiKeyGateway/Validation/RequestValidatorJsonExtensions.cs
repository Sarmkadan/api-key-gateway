// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// System.Text.Json serialization helpers for RequestValidator validation results.
// Provides extension-style API for serializing and deserializing ValidationResult
// instances returned by RequestValidator methods.
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Validation;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="ValidationResult"/> instances
/// returned by RequestValidator methods.
/// </summary>
public static class RequestValidatorJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		WriteIndented = false
	};

	/// <summary>
	/// Serializes a <see cref="ValidationResult"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The validation result to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the validation result.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	/// <exception cref="JsonException">Thrown when serialization fails.</exception>
	public static string ToJson(this ValidationResult value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = new JsonSerializerOptions(_jsonOptions)
		{
			WriteIndented = indented
		};

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string into a <see cref="ValidationResult"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A <see cref="ValidationResult"/> instance if successful; otherwise, null.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
	/// <exception cref="JsonException">Thrown when deserialization fails.</exception>
	public static ValidationResult? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			return JsonSerializer.Deserialize<ValidationResult>(json, _jsonOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string into a <see cref="ValidationResult"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized <see cref="ValidationResult"/> instance if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static bool TryFromJson(string json, out ValidationResult? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<ValidationResult>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}