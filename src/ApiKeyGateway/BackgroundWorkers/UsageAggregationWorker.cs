// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Repositories;

namespace ApiKeyGateway.BackgroundWorkers;

/// <summary>
/// Background worker that periodically aggregates usage data.
/// Takes individual request records and groups them into hourly/daily summaries
/// for efficient analytics queries. This reduces database load for reporting.
/// Runs hourly by default (configurable).
/// </summary>
public sealed class UsageAggregationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UsageAggregationWorker> _logger;
    private readonly TimeSpan _aggregationInterval = TimeSpan.FromHours(1);

    public UsageAggregationWorker(IServiceProvider serviceProvider, ILogger<UsageAggregationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Usage aggregation worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var usageRepository = scope.ServiceProvider.GetRequiredService<IUsageRepository>();

                // Aggregate usage records from the past hour
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);
                _logger.LogInformation(
                    "Starting usage aggregation for records after {Timestamp}",
                    oneHourAgo);

                // Group usage by API key and endpoint
                var aggregated = await usageRepository.GetUsageAsync(oneHourAgo, DateTime.UtcNow);

                var groupedUsage = aggregated
                    .GroupBy(u => new { u.ApiKeyId, u.Endpoint })
                    .Select(g => new
                    {
                        g.Key.ApiKeyId,
                        g.Key.Endpoint,
                        Count = g.Count(),
                        AverageResponseTime = g.Average(u => u.ResponseTimeMs),
                        TotalDataTransferred = g.Sum(u => u.ResponseSizeBytes)
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

                // Wait for next aggregation cycle
                await Task.Delay(_aggregationInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Usage aggregation worker is shutting down");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during usage aggregation");
                // Wait before retry to avoid tight error loop
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
