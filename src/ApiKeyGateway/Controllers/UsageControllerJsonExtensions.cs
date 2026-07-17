// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="UsageController"/>
/// </summary>
public static class UsageControllerJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver()
		{
			Modifiers = { JsonContextModifier.ApplyCamelCaseNaming }
		}
	};

	/// <summary>
	/// Serializes the <see cref="UsageController"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The controller instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the controller.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this UsageController value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a new <see cref="UsageController"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A new <see cref="UsageController"/> instance populated from the JSON.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
	public static UsageController? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		return JsonSerializer.Deserialize<UsageController>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a new <see cref="UsageController"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized controller instance, or null on failure.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	public static bool TryFromJson(string json, out UsageController? value)
	{
		value = null;

		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<UsageController>(json, _jsonOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			return false;
		}
	}

	/// <summary>
	/// JSON type info modifier that applies camelCase naming policy to property names.
	/// </summary>
	private static class JsonContextModifier
	{
		public static void ApplyCamelCaseNaming(JsonTypeInfo jsonTypeInfo)
		{
			if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object)
			{
				foreach (var property in jsonTypeInfo.Properties)
				{
					property.Name = char.ToLowerInvariant(property.Name[0]) + property.Name[1..];
				}
			}
		}
	}
}