// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Services;

/// <summary>
/// Tracks API usage metrics for analytics and billing
/// </summary>
public interface IUsageTrackingService
{
    /// <summary>
    /// Records a new usage entry for an API key.
    /// </summary>
    /// <param name="record">The usage record to record.</param>
    Task RecordUsageAsync(UsageRecord record);

    /// <summary>
    /// Retrieves aggregated usage statistics for an API key within a date range.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <param name="startDate">Start of the date range.</param>
    /// <param name="endDate">End of the date range.</param>
    /// <returns>The aggregated usage statistics.</returns>
    Task<UsageStatistics> GetUsageStatisticsAsync(string apiKeyId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Retrieves detailed usage records for an API key within a date range.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <param name="startDate">Start of the date range.</param>
    /// <param name="endDate">End of the date range.</param>
    /// <returns>The matching usage records.</returns>
    Task<List<UsageRecord>> GetUsageRecordsAsync(string apiKeyId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Calculates the total bytes transferred by a consumer within a date range.
    /// </summary>
    /// <param name="consumerId">The ID of the consumer.</param>
    /// <param name="startDate">Start of the date range.</param>
    /// <param name="endDate">End of the date range.</param>
    /// <returns>The total number of bytes transferred.</returns>
    Task<long> GetTotalBytesUsedAsync(string consumerId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Retrieves all usage records across all API keys within a date range.
    /// </summary>
    /// <param name="startDate">Start of the date range.</param>
    /// <param name="endDate">End of the date range.</param>
    /// <returns>The matching usage records.</returns>
    Task<List<UsageRecord>> GetUsageAsync(DateTime startDate, DateTime endDate);
}

/// <summary>
/// Default implementation of <see cref="IUsageTrackingService"/> backed by <see cref="IUsageRepository"/>.
/// </summary>
public class UsageTrackingService : IUsageTrackingService
{
    private readonly IUsageRepository _repository;
    private readonly ILogger<UsageTrackingService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsageTrackingService"/> class.
    /// </summary>
    /// <param name="repository">The usage data repository.</param>
    /// <param name="logger">The logger.</param>
    public UsageTrackingService(IUsageRepository repository, ILogger<UsageTrackingService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Records a new usage entry for an API key
    /// </summary>
    /// <param name="record">The usage record to record.</param>
    public async Task RecordUsageAsync(UsageRecord record)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        try
        {
            await _repository.CreateAsync(record);
            _logger.LogDebug("Usage recorded for API key {ApiKeyId}: {Endpoint} {Method} -> {StatusCode}",
                record.ApiKeyId, record.Endpoint, record.Method, record.ResponseStatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record usage for API key {ApiKeyId}", record.ApiKeyId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.DataAccessFailed, nameof(RecordUsageAsync), nameof(UsageRecord), ex);
        }
    }

    /// <summary>
    /// Retrieves usage statistics for an API key within a date range
    /// </summary>
    public async Task<UsageStatistics> GetUsageStatisticsAsync(string apiKeyId, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
        {
            throw new ValidationException("API Key ID cannot be empty", nameof(apiKeyId), apiKeyId);
        }

        if (endDate < startDate)
        {
            throw new ValidationException("End date must be after start date", nameof(endDate), endDate);
        }

        try
        {
            var records = await _repository.GetByApiKeyAndDateRangeAsync(apiKeyId, startDate, endDate);

            var stats = new UsageStatistics
            {
                ApiKeyId = apiKeyId,
                StartDate = startDate,
                EndDate = endDate,
                TotalRequests = records.Count,
                SuccessfulRequests = UsageRecord.CountSuccessfulRequests(records),
                FailedRequests = UsageRecord.CountErrorRequests(records),
                TotalBytesTransferred = UsageRecord.CalculateTotalBytes(records),
                AverageResponseTimeMs = UsageRecord.CalculateAverageResponseTime(records),
                UniqueEndpoints = records.Select(r => r.Endpoint).Distinct().Count()
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get usage statistics for API key {ApiKeyId}", apiKeyId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.DataAccessFailed, nameof(GetUsageStatisticsAsync), nameof(UsageRecord), ex);
        }
    }

    /// <summary>
    /// Retrieves detailed usage records for an API key
    /// </summary>
    public async Task<List<UsageRecord>> GetUsageRecordsAsync(string apiKeyId, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
        {
            throw new ValidationException("API Key ID cannot be empty", nameof(apiKeyId), apiKeyId);
        }

        try
        {
            return await _repository.GetByApiKeyAndDateRangeAsync(apiKeyId, startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get usage records for API key {ApiKeyId}", apiKeyId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.DataAccessFailed, nameof(GetUsageRecordsAsync), nameof(UsageRecord), ex);
        }
    }

    /// <summary>
    /// Calculates total bytes transferred by a consumer
    /// </summary>
    public async Task<long> GetTotalBytesUsedAsync(string consumerId, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
        {
            return 0;
        }

        try
        {
            var records = await _repository.GetByConsumerAndDateRangeAsync(consumerId, startDate, endDate);
            return UsageRecord.CalculateTotalBytes(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate total bytes used for consumer {ConsumerId}", consumerId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.DataAccessFailed, nameof(GetTotalBytesUsedAsync), nameof(UsageRecord), ex);
        }
    }

    /// <summary>
    /// Retrieves all usage records across all API keys within a date range
    /// </summary>
    public async Task<List<UsageRecord>> GetUsageAsync(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
        {
            throw new ValidationException("End date must be after start date", nameof(endDate), endDate);
        }

        try
        {
            return await _repository.GetUsageAsync(startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all usage records");
            throw new DataAccessException(Domain.Constants.ErrorMessages.DataAccessFailed, nameof(GetUsageAsync), nameof(UsageRecord), ex);
        }
    }
}

/// <summary>
/// Statistics about API usage for a key
/// </summary>
public class UsageStatistics
{
    /// <summary>The ID of the API key these statistics belong to</summary>
    public string ApiKeyId { get; set; } = string.Empty;

    /// <summary>Start of the reporting period</summary>
    public DateTime StartDate { get; set; }

    /// <summary>End of the reporting period</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Total number of requests in the period</summary>
    public int TotalRequests { get; set; }

    /// <summary>Number of successful requests in the period</summary>
    public int SuccessfulRequests { get; set; }

    /// <summary>Number of failed requests in the period</summary>
    public int FailedRequests { get; set; }

    /// <summary>Total bytes transferred in the period</summary>
    public long TotalBytesTransferred { get; set; }

    /// <summary>Average response time in milliseconds</summary>
    public double AverageResponseTimeMs { get; set; }

    /// <summary>Number of distinct endpoints accessed in the period</summary>
    public int UniqueEndpoints { get; set; }

    /// <summary>Percentage of requests that succeeded (0 to 100)</summary>
    public double SuccessRate => TotalRequests > 0 ? (SuccessfulRequests * 100.0) / TotalRequests : 0;
}

/// <summary>
/// Repository interface for usage data access
/// </summary>
public interface IUsageRepository
{
    /// <summary>
    /// Persists a single usage record.
    /// </summary>
    /// <param name="record">The usage record to persist.</param>
    Task CreateAsync(UsageRecord record);

    /// <summary>
    /// Persists a batch of usage records in a single operation.
    /// </summary>
    /// <param name="records">The usage records to persist.</param>
    Task WriteBatchAsync(IReadOnlyList<UsageRecord> records);

    /// <summary>
    /// Retrieves usage records for an API key within a date range.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <param name="startDate">Start of the date range.</param>
    /// <param name="endDate">End of the date range.</param>
    /// <returns>The matching usage records.</returns>
    Task<List<UsageRecord>> GetByApiKeyAndDateRangeAsync(string apiKeyId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Retrieves usage records for a consumer within a date range.
    /// </summary>
    /// <param name="consumerId">The ID of the consumer.</param>
    /// <param name="startDate">Start of the date range.</param>
    /// <param name="endDate">End of the date range.</param>
    /// <returns>The matching usage records.</returns>
    Task<List<UsageRecord>> GetByConsumerAndDateRangeAsync(string consumerId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Retrieves all usage records within a date range.
    /// </summary>
    /// <param name="startDate">Start of the date range.</param>
    /// <param name="endDate">End of the date range.</param>
    /// <returns>The matching usage records.</returns>
    Task<List<UsageRecord>> GetUsageAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Deletes usage records older than the given retention period.
    /// </summary>
    /// <param name="retentionDays">Number of days of records to keep.</param>
    Task DeleteOldRecordsAsync(int retentionDays);
}
