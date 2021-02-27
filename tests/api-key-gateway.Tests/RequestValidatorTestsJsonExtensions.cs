using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Tests;

public static class RequestValidatorTestsJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>
    /// Converts a <see cref="RequestValidatorTests"/> object to its JSON string representation.
    /// </summary>
    /// <param name="value">The <see cref="RequestValidatorTests"/> object to convert.</param>
    /// <param name="indented">A value indicating whether the JSON should be formatted with indentation.</param>
    /// <returns>The JSON string representation of the <see cref="RequestValidatorTests"/> object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this RequestValidatorTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented ? JsonOptions : JsonOptions with { WriteIndented = false };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="RequestValidatorTests"/> object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="RequestValidatorTests"/> object, or <c>null</c> if deserialization fails.</returns>
    public static RequestValidatorTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<RequestValidatorTests>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="RequestValidatorTests"/> object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="RequestValidatorTests"/> object, or <c>null</c> if deserialization fails.</param>
    /// <returns><c>true</c> if deserialization succeeds; otherwise, <c>false</c>.</returns>
    public static bool TryFromJson(string json, out RequestValidatorTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<RequestValidatorTests>(json, JsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
