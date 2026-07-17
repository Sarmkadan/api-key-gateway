// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using ApiKeyGateway.Domain.Enums;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Provides JSON serialization extensions for <see cref="RateLimitCalculationHelper"/> type metadata.
/// Since <see cref="RateLimitCalculationHelper"/> is a static class with no state,
/// this class serializes metadata about the type rather than instances of the class itself.
/// </summary>
public static class RateLimitCalculationHelperJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes <see cref="RateLimitCalculationHelper"/> type metadata to a JSON string.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing <see cref="RateLimitCalculationHelper"/> type metadata.</returns>
    public static string ToJson(bool indented = false)
        => JsonSerializer.Serialize(
            new RateLimitCalculationHelperMetadata
            {
                TypeName = "RateLimitCalculationHelper",
                Methods = GetPublicMethodNames()
            },
            indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions);

    /// <summary>
    /// Deserializes a JSON string to <see cref="RateLimitCalculationHelper"/> type metadata.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="RateLimitCalculationHelperMetadata"/> object, or null if the JSON is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static RateLimitCalculationHelperMetadata? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<RateLimitCalculationHelperMetadata>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to <see cref="RateLimitCalculationHelper"/> type metadata.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized metadata if successful.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out RateLimitCalculationHelperMetadata? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        value = null;

        try
        {
            value = JsonSerializer.Deserialize<RateLimitCalculationHelperMetadata>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Metadata model for <see cref="RateLimitCalculationHelper"/> type serialization.
    /// </summary>
    public sealed class RateLimitCalculationHelperMetadata
    {
        /// <summary>Gets or sets the type name.</summary>
        public string? TypeName { get; set; }

        /// <summary>Gets or sets the list of public method names.</summary>
        public IReadOnlyList<string>? Methods { get; set; }
    }

    /// <summary>
    /// Gets the names of public methods on <see cref="RateLimitCalculationHelper"/> for serialization.
    /// </summary>
    private static IReadOnlyList<string> GetPublicMethodNames()
        => [
            nameof(RateLimitCalculationHelper.GetWindowEnd),
            nameof(RateLimitCalculationHelper.GetWindowStart),
            nameof(RateLimitCalculationHelper.GetSecondsUntilAllowed),
            nameof(RateLimitCalculationHelper.CalculateQuotagePercentage),
            nameof(RateLimitCalculationHelper.ShouldWarnAboutLimit),
            nameof(RateLimitCalculationHelper.GetReadableResetTime)
        ];
}