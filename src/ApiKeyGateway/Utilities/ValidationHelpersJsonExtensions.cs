// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Provides JSON serialization and deserialization for <see cref="ValidationHelpers"/> type metadata.
/// Since <see cref="ValidationHelpers"/> is a static class with no state, this class serializes
/// metadata about the validation methods it provides.
/// </summary>
public static class ValidationHelpersJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	/// <summary>
	/// Serializes <see cref="ValidationHelpers"/> type metadata to a JSON string.
	/// </summary>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representing ValidationHelpers type metadata.</returns>
	public static string ToJson(bool indented = false)
	{
		var options = new JsonSerializerOptions(indented ? _jsonSerializerOptions : _jsonSerializerOptions)
		{
			WriteIndented = indented
		};

		var metadata = new ValidationHelpersMetadata
		{
			TypeName = nameof(ValidationHelpers),
			Methods = GetPublicMethodNames()
		};

		return JsonSerializer.Serialize(metadata, options);
	}

	/// <summary>
	/// Deserializes a JSON string to <see cref="ValidationHelpers"/> type metadata.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A <see cref="ValidationHelpersMetadata"/> object, or null if the JSON is null or empty.</returns>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
	public static ValidationHelpersMetadata? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(json);

		return JsonSerializer.Deserialize<ValidationHelpersMetadata>(json, _jsonSerializerOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to <see cref="ValidationHelpers"/> type metadata.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized metadata if successful.</param>
	/// <returns>True if deserialization succeeds; otherwise, false.</returns>
	public static bool TryFromJson(string json, out ValidationHelpersMetadata? value)
	{
		value = null;

		if (string.IsNullOrWhiteSpace(json))
		{
			return true;
		}

		try
		{
			value = JsonSerializer.Deserialize<ValidationHelpersMetadata>(json, _jsonSerializerOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}

	/// <summary>
	/// Gets the names of public methods on ValidationHelpers for serialization.
	/// </summary>
	private static IReadOnlyList<string> GetPublicMethodNames()
	{
		return [
			nameof(ValidationHelpers.IsValidEmail),
			nameof(ValidationHelpers.IsValidApiKeyFormat),
			nameof(ValidationHelpers.IsValidIpAddress),
			nameof(ValidationHelpers.IsValidGuid),
			nameof(ValidationHelpers.IsValidUrl),
			nameof(ValidationHelpers.SanitizeInput)
		];
	}

	/// <summary>
	/// Metadata model for ValidationHelpers type serialization.
	/// </summary>
	public sealed class ValidationHelpersMetadata
	{
		/// <summary>Gets or sets the type name.</summary>
		public string? TypeName { get; set; }

		/// <summary>Gets or sets the list of public method names.</summary>
		public IReadOnlyList<string>? Methods { get; set; }
	}
}
