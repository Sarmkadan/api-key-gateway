// SPDX-License-Identifier: MIT
// © 2024 RedRocket

using System;
using System.Text.Json;
using ApiKeyGateway.Events;

/// <summary>
/// Provides JSON (de)serialization extensions for <see cref="AuditLogEventHandler"/>.
/// </summary>
public static class AuditLogEventHandlerJsonExtensions
{
    /// <summary>
    /// Cached serializer options configured for camelCase property names and web defaults.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the <see cref="AuditLogEventHandler"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The handler instance to serialize.</param>
    /// <param name="indented">If <see langword="true"/>, the output JSON will be formatted with indentation.</param>
    /// <returns>A JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this AuditLogEventHandler value, bool indented = false) =>
        JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options);

    /// <summary>
    /// Deserializes a JSON string into an <see cref="AuditLogEventHandler"/> instance.
    /// </summary>
    /// <param name="json">The JSON representation of an <see cref="AuditLogEventHandler"/>.</param>
    /// <returns>
    /// The deserialized <see cref="AuditLogEventHandler"/>, or <see langword="null"/> if the JSON is <see langword="null"/> or empty.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="JsonException">The JSON cannot be deserialized into an <see cref="AuditLogEventHandler"/>.</exception>
    public static AuditLogEventHandler? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<AuditLogEventHandler>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into an <see cref="AuditLogEventHandler"/> instance.
    /// </summary>
    /// <param name="json">The JSON representation of an <see cref="AuditLogEventHandler"/>.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized <see cref="AuditLogEventHandler"/> if the operation succeeded,
    /// or <see langword="null"/> otherwise.
    /// </param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out AuditLogEventHandler? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<AuditLogEventHandler>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
