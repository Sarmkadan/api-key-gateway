using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiKeyGateway.Repositories;

/// <summary>
/// Extension methods for the <see cref="UsageRepository"/> class.
/// </summary>
public static class UsageRepositoryExtensions
{
    /// <summary>
    /// Retrieves the total number of usage records for an API key within a date range.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="apiKeyId">The ID of the API key to retrieve records for.</param>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>The total number of usage records.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="startDate"/> is after <paramref name="endDate"/>.</exception>
    public static async Task<int> GetUsageRecordCountByApiKeyAndDateRangeAsync(this UsageRepository repository, string apiKeyId, DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);
        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be after end date", nameof(startDate));

        var records = await repository.GetByApiKeyAndDateRangeAsync(apiKeyId, startDate, endDate);
        return records.Count;
    }

    /// <summary>
    /// Retrieves the total number of usage records within a date range, regardless of API key or consumer.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>The total number of usage records.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="startDate"/> is after <paramref name="endDate"/>.</exception>
    public static async Task<int> GetTotalUsageRecordCountAsync(this UsageRepository repository, DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(repository);
        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be after end date", nameof(startDate));

        var records = await repository.GetUsageAsync(startDate, endDate);
        return records.Count;
    }

    /// <summary>
    /// Deletes usage records for an API key older than the specified retention period.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="apiKeyId">The ID of the API key to delete records for.</param>
    /// <param name="retentionDays">The number of days to retain records.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="retentionDays"/> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    public static async Task DeleteOldUsageRecordsByApiKeyAsync(this UsageRepository repository, string apiKeyId, int retentionDays)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);
        if (retentionDays <= 0)
            throw new ArgumentException("Retention days must be positive", nameof(retentionDays));

        await repository.DeleteOldRecordsAsync(retentionDays);
    }

    /// <summary>
    /// Retrieves the average response time for an API key within a date range.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="apiKeyId">The ID of the API key to retrieve records for.</param>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>The average response time in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="startDate"/> is after <paramref name="endDate"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    public static async Task<double> GetAverageResponseTimeByApiKeyAndDateRangeAsync(this UsageRepository repository, string apiKeyId, DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);
        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be after end date", nameof(startDate));

        var records = await repository.GetByApiKeyAndDateRangeAsync(apiKeyId, startDate, endDate);
        if (records.Count == 0)
            return 0;

        var totalResponseTime = records.Sum(r => r.ResponseTimeMs);
        return totalResponseTime / records.Count;
    }
}
