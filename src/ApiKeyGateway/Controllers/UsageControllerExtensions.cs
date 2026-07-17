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
    /// <param name="controller">The <see cref="UsageController"/> instance. Must not be <see langword="null"/>.</param>
    /// <param name="apiKeyId">The ID of the API key. Must not be <see langword="null"/>, <see langword="string.Empty"/>, or whitespace.</param>
    /// <returns>A task containing the action result with usage statistics.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="controller"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="apiKeyId"/> is <see langword="null"/>, <see langword="string.Empty"/>, or consists only of whitespace.
    /// </exception>
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
    /// <param name="controller">The <see cref="UsageController"/> instance. Must not be <see langword="null"/>.</param>
    /// <param name="apiKeyId">The ID of the API key. Must not be <see langword="null"/>, <see langword="string.Empty"/>, or whitespace.</param>
    /// <param name="limit">The maximum number of records to retrieve.</param>
    /// <returns>A task containing the action result with a list of usage records.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="controller"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="apiKeyId"/> is <see langword="null"/>, <see langword="string.Empty"/>, or consists only of whitespace.
    /// </exception>
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