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
    /// <summary>Unique identifier for the usage record</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>ID of the API key used for this request</summary>
    public string ApiKeyId { get; init; } = string.Empty;

    /// <summary>ID of the consumer who made the request</summary>
    public string ConsumerId { get; init; } = string.Empty;

    /// <summary>When the usage was recorded (UTC)</summary>
    public DateTime RecordedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Alias for <see cref="RecordedAt"/> using request-timestamp naming</summary>
    public DateTime RequestTimestampUtc
    {
        get => RecordedAt;
        init => RecordedAt = value;
    }

    /// <summary>API endpoint that was accessed</summary>
    public string Endpoint { get; init; } = string.Empty;

    /// <summary>HTTP method used (GET, POST, etc.)</summary>
    public string Method { get; init; } = "GET";

    /// <summary>HTTP response status code</summary>
    public int ResponseStatusCode { get; init; } = 200;

    /// <summary>Size of the request payload in bytes</summary>
    public long RequestBytes { get; init; }

    /// <summary>Size of the response payload in bytes</summary>
    public long ResponseBytes { get; init; }

    /// <summary>Convenience alias for the payload size of the response</summary>
    public long BytesTransferred
    {
        get => ResponseBytes;
        init => ResponseBytes = value;
    }

    /// <summary>Time taken to process the request in milliseconds</summary>
    public int ResponseTimeMs { get; init; }

    /// <summary>Error code if the request resulted in an error</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Source IP address of the request</summary>
    public string? SourceIp { get; set; }

    /// <summary>User agent string from the request</summary>
    public string? UserAgent { get; set; }

    /// <summary>Custom tags for categorizing and filtering usage records</summary>
    public Dictionary<string, string> Tags { get; set; } = [];

    /// <summary>Total bytes transferred (request + response)</summary>
    public long TotalBytes => RequestBytes + ResponseBytes;

    /// <summary>Indicates if the request resulted in an error</summary>
    public bool IsError => ResponseStatusCode >= 400;

    /// <summary>
    /// Gets the total bytes transferred across all usage records
    /// </summary>
    /// <param name="records">Collection of usage records</param>
    /// <returns>Sum of all bytes transferred</returns>
    public static long CalculateTotalBytes(IEnumerable<UsageRecord> records)
    {
        return records.Sum(r => r.TotalBytes);
    }

    /// <summary>
    /// Calculates average response time across usage records
    /// </summary>
    /// <param name="records">Collection of usage records</param>
    /// <returns>Average response time in milliseconds, or 0 if no records</returns>
    public static double CalculateAverageResponseTime(IEnumerable<UsageRecord> records)
    {
        var recordList = records.ToList();
        return recordList.Count > 0 ? recordList.Average(r => r.ResponseTimeMs) : 0;
    }

    /// <summary>
    /// Counts successful requests in a collection of usage records
    /// </summary>
    /// <param name="records">Collection of usage records</param>
    /// <returns>Number of successful requests (status code < 400)</returns>
    public static int CountSuccessfulRequests(IEnumerable<UsageRecord> records)
    {
        return records.Count(r => !r.IsError);
    }

    /// <summary>
    /// Counts error requests in a collection of usage records
    /// </summary>
    /// <param name="records">Collection of usage records</param>
    /// <returns>Number of error requests (status code >= 400)</returns>
    public static int CountErrorRequests(IEnumerable<UsageRecord> records)
    {
        return records.Count(r => r.IsError);
    }
}
