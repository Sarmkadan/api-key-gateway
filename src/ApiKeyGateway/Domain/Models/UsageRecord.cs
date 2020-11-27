// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Tracks API usage metrics for billing and analytics
/// </summary>
public class UsageRecord
{
    public string Id { get; init; } = string.Empty;
    public string ApiKeyId { get; init; } = string.Empty;
    public string ConsumerId { get; init; } = string.Empty;
    public DateTime RecordedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Alias for <see cref="RecordedAt"/> using request-timestamp naming</summary>
    public DateTime RequestTimestampUtc
    {
        get => RecordedAt;
        init => RecordedAt = value;
    }

    public string Endpoint { get; init; } = string.Empty;
    public string Method { get; init; } = "GET";
    public int ResponseStatusCode { get; init; } = 200;
    public long RequestBytes { get; init; }
    public long ResponseBytes { get; init; }

    /// <summary>Convenience alias for the payload size of the response</summary>
    public long BytesTransferred
    {
        get => ResponseBytes;
        init => ResponseBytes = value;
    }

    public int ResponseTimeMs { get; init; }
    public string? ErrorCode { get; set; }
    public string? SourceIp { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, string> Tags { get; set; } = [];
    public long TotalBytes => RequestBytes + ResponseBytes;
    public bool IsError => ResponseStatusCode >= 400;

    /// <summary>
    /// Gets the usage for a specific time period
    /// </summary>
    public static long CalculateTotalBytes(IEnumerable<UsageRecord> records)
    {
        return records.Sum(r => r.TotalBytes);
    }

    /// <summary>
    /// Calculates average response time
    /// </summary>
    public static double CalculateAverageResponseTime(IEnumerable<UsageRecord> records)
    {
        var recordList = records.ToList();
        return recordList.Count > 0 ? recordList.Average(r => r.ResponseTimeMs) : 0;
    }

    /// <summary>
    /// Counts successful requests
    /// </summary>
    public static int CountSuccessfulRequests(IEnumerable<UsageRecord> records)
    {
        return records.Count(r => !r.IsError);
    }

    /// <summary>
    /// Counts error requests
    /// </summary>
    public static int CountErrorRequests(IEnumerable<UsageRecord> records)
    {
        return records.Count(r => r.IsError);
    }
}
