// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="ApiResponseBuilder{T}"/> instances
/// </summary>
public static class ApiResponseBuilderJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an ApiResponseBuilder instance to a JSON string
    /// </summary>
    /// <typeparam name="T">The type of data contained in the response</typeparam>
    /// <param name="value">The ApiResponseBuilder instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the ApiResponseBuilder</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static string ToJson<T>(this ApiResponseBuilder<T> value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        var response = value.Build();
        return JsonSerializer.Serialize(response, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an ApiResponseBuilder instance
    /// </summary>
    /// <typeparam name="T">The type of data contained in the response</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>An ApiResponseBuilder instance, or null if the JSON is null or whitespace</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty</exception>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized</exception>
    public static ApiResponseBuilder<T>? FromJson<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var response = JsonSerializer.Deserialize<ApiResponse>(json, _jsonSerializerOptions);
        if (response is null)
        {
            return null;
        }

        var builder = new ApiResponseBuilder<T>();

        if (response.Success)
        {
            builder.Success(response.Message);
        }
        else
        {
            builder.Error(response.StatusCode ?? 500, response.Message ?? "Error", response.ErrorCode);
        }

        if (response.Data is not null)
        {
            builder.WithData((T?)response.Data);
        }

        if (response.Errors is not null)
        {
            foreach (var error in response.Errors)
            {
                builder.AddError(error);
            }
        }

        if (response.Metadata is not null)
        {
            foreach (var kvp in response.Metadata)
            {
                builder.WithMetadata(kvp.Key, kvp.Value);
            }
        }

        return builder;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an ApiResponseBuilder instance
    /// </summary>
    /// <typeparam name="T">The type of data contained in the response</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized ApiResponseBuilder if successful</param>
    /// <returns>True if deserialization succeeds; otherwise, false</returns>
    public static bool TryFromJson<T>(string json, out ApiResponseBuilder<T>? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            var response = JsonSerializer.Deserialize<ApiResponse>(json, _jsonSerializerOptions);
            if (response is null)
            {
                return true;
            }

            var builder = new ApiResponseBuilder<T>();

            if (response.Success)
            {
                builder.Success(response.Message);
            }
            else
            {
                builder.Error(response.StatusCode ?? 500, response.Message ?? "Error", response.ErrorCode);
            }

            if (response.Data is not null)
            {
                builder.WithData((T?)response.Data);
            }

            if (response.Errors is not null)
            {
                foreach (var error in response.Errors)
                {
                    builder.AddError(error);
                }
            }

            if (response.Metadata is not null)
            {
                foreach (var kvp in response.Metadata)
                {
                    builder.WithMetadata(kvp.Key, kvp.Value);
                }
            }

            value = builder;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Internal model representing the structure of an API response for serialization/deserialization
    /// </summary>
    private sealed class ApiResponse
    {
        public bool Success { get; set; }
        public int? StatusCode { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
        public object? Data { get; set; }
        public List<string>? Errors { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}