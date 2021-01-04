using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Extension methods for <see cref="UsageRecord"/> that provide common calculations
/// and queries used throughout the gateway.
/// </summary>
public static class UsageRecordExtensions
{
    /// <summary>
    /// Calculates the total number of bytes transferred for a single usage record
    /// by adding the request and response payload sizes.
    /// </summary>
    /// <param name="record">The usage record to evaluate.</param>
    /// <returns>The sum of <see cref="UsageRecord.RequestBytes"/> and <see cref="UsageRecord.ResponseBytes"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="record"/> is <c>null</c>.</exception>
    public static long GetTotalBytes(this UsageRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        return record.RequestBytes + record.ResponseBytes;
    }

    /// <summary>
    /// Determines whether the request represented by the usage record was successful.
    /// A request is considered successful when the HTTP status code is in the range 200‑299.
    /// </summary>
    /// <param name="record">The usage record to evaluate.</param>
    /// <returns><c>true</c> if the status code is between 200 and 299; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="record"/> is <c>null</c>.</exception>
    public static bool IsSuccessful(this UsageRecord record) =>
        (record?.ResponseStatusCode is >= 200 and < 300);

    /// <summary>
    /// Retrieves a tag value from the <see cref="UsageRecord.Tags"/> dictionary.
    /// </summary>
    /// <param name="record">The usage record containing the tags.</param>
    /// <param name="key">The tag key to look up.</param>
    /// <returns>The associated tag value, or <c>null</c> if the key does not exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="record"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <c>null</c> or empty.</exception>
    public static string? GetTag(this UsageRecord record, string key)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return record.Tags.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Filters a sequence of <see cref="UsageRecord"/> instances to those that match the specified endpoint.
    /// The comparison is case‑insensitive and uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    /// <param name="records">The source sequence of usage records.</param>
    /// <param name="endpoint">The endpoint to filter by.</param>
    /// <returns>An <see cref="IReadOnlyList{UsageRecord}"/> containing the matching records.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="records"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="endpoint"/> is <c>null</c> or empty.</exception>
    public static IReadOnlyList<UsageRecord> FilterByEndpoint(this IEnumerable<UsageRecord> records, string endpoint)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentException.ThrowIfNullOrEmpty(endpoint);

        var filtered = records
            .Where(r => string.Equals(r.Endpoint, endpoint, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return filtered;
    }
}
