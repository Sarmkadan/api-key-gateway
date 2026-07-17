// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// System.Text.Json serialization helpers for analytics response types returned by AnalyticsController.
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Provides System.Text.Json serialization extensions for analytics response types
/// returned by <see cref="AnalyticsController"/> actions.
/// </summary>
public static class AnalyticsControllerJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Serializes an analytics summary to a JSON string.
	/// </summary>
	/// <param name="value">The analytics summary to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the analytics summary.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this ApiKeyGateway.Services.AnalyticsSummary value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
			: _jsonSerializerOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Serializes a list of endpoint statistics to a JSON string.
	/// </summary>
	/// <param name="value">The list of endpoint statistics to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the endpoint statistics.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this System.Collections.Generic.List<ApiKeyGateway.Services.EndpointStat> value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
			: _jsonSerializerOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Serializes a list of hourly buckets to a JSON string.
	/// </summary>
	/// <param name="value">The list of hourly buckets to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the hourly buckets.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this System.Collections.Generic.List<ApiKeyGateway.Services.HourlyBucket> value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
			: _jsonSerializerOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Serializes a list of daily buckets to a JSON string.
	/// </summary>
	/// <param name="value">The list of daily buckets to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the daily buckets.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this System.Collections.Generic.List<ApiKeyGateway.Services.DailyBucket> value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
			: _jsonSerializerOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to an analytics summary.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>An analytics summary instance, or null if the JSON is invalid.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
	public static ApiKeyGateway.Services.AnalyticsSummary? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrEmpty(json);

		return JsonSerializer.Deserialize<ApiKeyGateway.Services.AnalyticsSummary>(json, _jsonSerializerOptions);
	}

	/// <summary>
	/// Deserializes a JSON string to a list of endpoint statistics.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A list of endpoint statistics, or null if the JSON is invalid.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
	public static System.Collections.Generic.List<ApiKeyGateway.Services.EndpointStat>? FromJsonToEndpointStats(string json)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrEmpty(json);

		return JsonSerializer.Deserialize<System.Collections.Generic.List<ApiKeyGateway.Services.EndpointStat>>(json, _jsonSerializerOptions);
	}

	/// <summary>
	/// Deserializes a JSON string to a list of hourly buckets.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A list of hourly buckets, or null if the JSON is invalid.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
	public static System.Collections.Generic.List<ApiKeyGateway.Services.HourlyBucket>? FromJsonToHourlyBuckets(string json)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrEmpty(json);

		return JsonSerializer.Deserialize<System.Collections.Generic.List<ApiKeyGateway.Services.HourlyBucket>>(json, _jsonSerializerOptions);
	}

	/// <summary>
	/// Deserializes a JSON string to a list of daily buckets.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A list of daily buckets, or null if the JSON is invalid.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
	public static System.Collections.Generic.List<ApiKeyGateway.Services.DailyBucket>? FromJsonToDailyBuckets(string json)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrEmpty(json);

		return JsonSerializer.Deserialize<System.Collections.Generic.List<ApiKeyGateway.Services.DailyBucket>>(json, _jsonSerializerOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to an analytics summary.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized analytics summary if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
	public static bool TryFromJson(string json, out ApiKeyGateway.Services.AnalyticsSummary? value)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<ApiKeyGateway.Services.AnalyticsSummary>(json, _jsonSerializerOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a list of endpoint statistics.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized list of endpoint statistics if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
	public static bool TryFromJson(string json, out System.Collections.Generic.List<ApiKeyGateway.Services.EndpointStat>? value)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<System.Collections.Generic.List<ApiKeyGateway.Services.EndpointStat>>(json, _jsonSerializerOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a list of hourly buckets.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized list of hourly buckets if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
	public static bool TryFromJson(string json, out System.Collections.Generic.List<ApiKeyGateway.Services.HourlyBucket>? value)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<System.Collections.Generic.List<ApiKeyGateway.Services.HourlyBucket>>(json, _jsonSerializerOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a list of daily buckets.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized list of daily buckets if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
	public static bool TryFromJson(string json, out System.Collections.Generic.List<ApiKeyGateway.Services.DailyBucket>? value)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<System.Collections.Generic.List<ApiKeyGateway.Services.DailyBucket>>(json, _jsonSerializerOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}