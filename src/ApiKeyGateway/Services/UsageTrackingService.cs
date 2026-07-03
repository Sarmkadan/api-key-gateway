// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Services;

/// <summary>
/// Tracks API usage metrics for analytics and billing
/// </summary>
public interface IUsageTrackingService
{
    Task RecordUsageAsync(UsageRecord record);
    Task<UsageStatistics> GetUsageStatisticsAsync(string apiKeyId, DateTime startDate, DateTime endDate);
    Task<List<UsageRecord>> GetUsageRecordsAsync(string apiKeyId, DateTime startDate, DateTime endDate);
    Task<long> GetTotalBytesUsedAsync(string consumerId, DateTime startDate, DateTime endDate);
}

public class UsageTrackingService : IUsageTrackingService
{
    private readonly IUsageRepository _repository;
    private readonly ILogger<UsageTrackingService> _logger;

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
            throw new ArgumentNullException(nameof(record));

        try
        {
            await _repository.CreateAsync(record);
            _logger.LogDebug("Usage recorded for API key {ApiKeyId}: {Endpoint} {Method} -> {StatusCode}",
                record.ApiKeyId, record.Endpoint, record.Method, record.ResponseStatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record usage for API key {ApiKeyId}", record.ApiKeyId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.DataAccessFailed, ex, nameof(RecordUsageAsync), nameof(UsageRecord));
        }
    }

    /// <summary>
    /// Retrieves usage statistics for an API key within a date range
    /// </summary>
    public async Task<UsageStatistics> GetUsageStatisticsAsync(string apiKeyId, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            throw new ValidationException("API Key ID cannot be empty", nameof(apiKeyId), apiKeyId);

        if (endDate < startDate)
            throw new ValidationException("End date must be after start date", nameof(endDate), endDate);

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
            throw new DataAccessException(Domain.Constants.ErrorMessages.DataAccessFailed, ex, nameof(GetUsageStatisticsAsync), nameof(UsageRecord));
        }
    }

    /// <summary>
    /// Retrieves detailed usage records for an API key
    /// </summary>
    public async Task<List<UsageRecord>> GetUsageRecordsAsync(string apiKeyId, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            throw new ValidationException("API Key ID cannot be empty", nameof(apiKeyId), apiKeyId);

        try
        {
            return await _repository.GetByApiKeyAndDateRangeAsync(apiKeyId, startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get usage records for API key {ApiKeyId}", apiKeyId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.DataAccessFailed, ex, nameof(GetUsageRecordsAsync), nameof(UsageRecord));
        }
    }

    /// <summary>
    /// Calculates total bytes transferred by a consumer
    /// </summary>
    public async Task<long> GetTotalBytesUsedAsync(string consumerId, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
            return 0;

        try
        {
            var records = await _repository.GetByConsumerAndDateRangeAsync(consumerId, startDate, endDate);
            return UsageRecord.CalculateTotalBytes(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate total bytes used for consumer {ConsumerId}", consumerId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.DataAccessFailed, ex, nameof(GetTotalBytesUsedAsync), nameof(UsageRecord));
        }
    }
}

/// <summary>
/// Statistics about API usage for a key
/// </summary>
public class UsageStatistics
{
    public string ApiKeyId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public long TotalBytesTransferred { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public int UniqueEndpoints { get; set; }
    public double SuccessRate => TotalRequests > 0 ? (SuccessfulRequests * 100.0) / TotalRequests : 0;
}

/// <summary>
/// Repository interface for usage data access
/// </summary>
public interface IUsageRepository
{
    Task CreateAsync(UsageRecord record);
    Task<List<UsageRecord>> GetByApiKeyAndDateRangeAsync(string apiKeyId, DateTime startDate, DateTime endDate);
    Task<List<UsageRecord>> GetByConsumerAndDateRangeAsync(string consumerId, DateTime startDate, DateTime endDate);
    Task DeleteOldRecordsAsync(int retentionDays);
}
