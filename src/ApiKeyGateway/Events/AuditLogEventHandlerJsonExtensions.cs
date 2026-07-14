// SPDX-License-Identifier: MIT
// © 2024 RedRocket

using System;
using System.Text.Json;
using ApiKeyGateway.Events;

/// <summary>
/// JSON (de)serialization helpers for <see cref="AuditLogEventHandler"/>.
/// </summary>
public static class AuditLogEventHandlerJsonExtensions
{
    /// <summary>
    /// Cached serializer options that use camel‑case property names.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the <see cref="AuditLogEventHandler"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The handler instance to serialize.</param>
    /// <param name="indented">If <c>true</c>, the output JSON will be formatted with indentation.</param>
    /// <returns>A JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this AuditLogEventHandler value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an <see cref="AuditLogEventHandler"/> instance.
    /// </summary>
    /// <param name="json">The JSON representation of an <see cref="AuditLogEventHandler"/>.</param>
    /// <returns>The deserialized <see cref="AuditLogEventHandler"/>, or <c>null</c> if the JSON is <c>null</c> or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON cannot be deserialized into an <see cref="AuditLogEventHandler"/>.</exception>
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
    /// or <c>null</c> otherwise.
    /// </param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
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
