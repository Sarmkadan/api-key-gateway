using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Provides extension methods for <see cref="UsageController"/>.
/// </summary>
public static class UsageControllerExtensions
{
    /// <summary>
    /// Retrieves a summary of usage statistics for a given API key over the last 30 days.
    /// </summary>
    /// <param name="controller">The <see cref="UsageController"/> instance.</param>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <returns>A task containing the action result with usage statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    public static async Task<ActionResult<UsageStatisticsResponse>> GetUsageStatisticsSummaryAsync(
        this UsageController controller,
        string apiKeyId)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);

        return await controller.GetKeyStatistics(
            apiKeyId,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow);
    }

    /// <summary>
    /// Retrieves the most recent usage records for an API key over the last 7 days.
    /// </summary>
    /// <param name="controller">The <see cref="UsageController"/> instance.</param>
    /// <param name="apiKeyId">The ID of the API key.</param>
    /// <param name="limit">The maximum number of records to retrieve.</param>
    /// <returns>A task containing the action result with a list of usage records.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    public static async Task<ActionResult<List<UsageRecordResponse>>> GetRecentUsageRecordsAsync(
        this UsageController controller,
        string apiKeyId,
        int limit = 50)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);

        return await controller.GetKeyRecords(
            apiKeyId,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow,
            limit);
    }
}
