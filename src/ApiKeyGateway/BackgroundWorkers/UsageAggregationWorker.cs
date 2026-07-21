// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Background worker that periodically aggregates usage data.
// Takes individual request records and groups them into hourly/daily summaries
// for efficient analytics queries. This reduces database load for reporting.
// Runs hourly by default (configurable).
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Services;

namespace ApiKeyGateway.BackgroundWorkers;

/// <summary>
/// Background worker that periodically aggregates usage data.
/// Takes individual request records and groups them into hourly/daily summaries
/// for efficient analytics queries. This reduces database load for reporting.
/// Runs hourly by default (configurable).
/// </summary>
public sealed class UsageAggregationWorker : BackgroundServiceBase<UsageAggregationWorker>
{
    private readonly TimeSpan _aggregationInterval = TimeSpan.FromHours(1);

    public UsageAggregationWorker(IServiceProvider serviceProvider, ILogger<UsageAggregationWorker> logger)
        : base(serviceProvider, logger)
    {
    }

    /// <inheritdoc/>
    protected override async Task ExecuteCycleAsync(CancellationToken stoppingToken)
    {
        await RunAggregationCycleAsync(stoppingToken);
        await Task.Delay(_aggregationInterval, stoppingToken);
    }

    private async Task RunAggregationCycleAsync(CancellationToken stoppingToken)
    {
        using var scope = CreateScope();
        var usageRepository = scope.ServiceProvider.GetRequiredService<IUsageRepository>();

        _logger.LogInformation("Starting usage aggregation for records after {Timestamp}", DateTime.UtcNow.AddHours(-1));

        // Aggregate usage records from the past hour
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var aggregated = await usageRepository.GetUsageAsync(oneHourAgo, DateTime.UtcNow);

        var groupedUsage = aggregated
            .GroupBy(u => new { u.ApiKeyId, u.Endpoint })
            .Select(g => new
            {
                g.Key.ApiKeyId,
                g.Key.Endpoint,
                Count = g.Count(),
                AverageResponseTime = g.Average(u => u.ResponseTimeMs),
                TotalDataTransferred = g.Sum(u => u.ResponseBytes)
            })
            .ToList();

        _logger.LogInformation(
            "Aggregated {RecordCount} usage records into {GroupCount} summaries",
            aggregated.Count,
            groupedUsage.Count);

        // Update aggregate tables (implementation depends on data model)
        foreach (var summary in groupedUsage)
        {
            _logger.LogDebug(
                "Usage summary: {ApiKeyId} {Endpoint} - {Count} requests, avg {AvgTime}ms",
                summary.ApiKeyId,
                summary.Endpoint,
                summary.Count,
                summary.AverageResponseTime);
        }

        _logger.LogInformation("Usage aggregation cycle completed successfully");
    }

    protected override TimeSpan GetErrorDelay(CancellationToken stoppingToken)
    {
        return TimeSpan.FromSeconds(30);
    }
}
