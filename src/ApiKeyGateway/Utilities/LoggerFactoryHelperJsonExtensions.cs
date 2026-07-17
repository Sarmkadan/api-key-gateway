// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// System.Text.Json serialization helpers for LoggerFactoryHelper configuration state.
// Provides efficient serialization/deserialization with camelCase naming convention.
// =====================================================================

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for logger factory configuration.
/// Enables serialization and deserialization of logger configuration state used with <see cref="LoggerFactoryHelper"/>.
/// </summary>
public static class LoggerFactoryHelperJsonExtensions
{
    /// <summary>
    /// Represents logger factory configuration state that can be serialized and deserialized.
    /// This DTO can be used to persist logger configuration state used with LoggerFactoryHelper.
    /// </summary>
    public sealed record LoggerFactoryConfiguration
    {
        /// <summary>
        /// Gets the default log level.
        /// </summary>
        public string? DefaultLogLevel { get; init; }

        /// <summary>
        /// Gets whether debug logging is enabled.
        /// </summary>
        public bool DebugEnabled { get; init; }

        /// <summary>
        /// Gets whether console logging is enabled.
        /// </summary>
        public bool ConsoleEnabled { get; init; }
    }

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes logger factory configuration to a JSON string.
    /// </summary>
    /// <param name="value">The logger factory configuration to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the logger factory configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this LoggerFactoryConfiguration value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="LoggerFactoryConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="LoggerFactoryConfiguration"/> instance if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static LoggerFactoryConfiguration? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

        try
        {
            return JsonSerializer.Deserialize<LoggerFactoryConfiguration>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="LoggerFactoryConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="LoggerFactoryConfiguration"/> instance if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    public static bool TryFromJson(string json, [NotNullWhen(true)] out LoggerFactoryConfiguration? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentException("JSON string cannot be empty or whitespace.", nameof(json));
        }

        try
        {
            value = JsonSerializer.Deserialize<LoggerFactoryConfiguration>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}