// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using ApiKeyGateway.Configuration;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Events;
using Microsoft.Extensions.Options;

namespace ApiKeyGateway.Services;

/// <summary>
/// Evaluates a consumer's usage against its quota after a request has been
/// recorded, publishing <see cref="QuotaThresholdReachedEvent"/> when a
/// configured threshold (default 80% / 100%) is crossed.
/// </summary>
public interface IQuotaAlertEvaluator
{
    /// <summary>
    /// Compares the consumer's current-period usage against the API key's quota
    /// limit and publishes threshold events for any newly crossed thresholds.
    /// Each threshold fires at most once per consumer per quota period.
    /// Evaluation failures are logged and swallowed - this method never throws
    /// due to downstream errors, so it can safely run in the request path.
    /// </summary>
    /// <param name="consumerId">Identifier of the consumer that made the request.</param>
    /// <param name="apiKeyId">Identifier of the API key used for the request.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="consumerId"/> or <paramref name="apiKeyId"/> is null or empty.</exception>
    Task EvaluateAsync(string consumerId, string apiKeyId);
}

/// <summary>
/// Tracks the highest threshold already alerted per consumer per quota period
/// so repeated evaluations within the same period do not re-fire events.
/// Registered as a singleton; safe for concurrent use.
/// </summary>
public sealed class QuotaAlertStateCache
{
    private readonly ConcurrentDictionary<string, (DateTime PeriodStart, int Threshold)> _lastAlerted = new();

    /// <summary>
    /// Atomically records that <paramref name="threshold"/> has been alerted for
    /// the consumer in the given period. Returns <see langword="true"/> only for
    /// the caller that advanced the state, guaranteeing at-most-once semantics.
    /// </summary>
    /// <param name="consumerId">Consumer identifier.</param>
    /// <param name="periodStart">UTC start of the current quota period.</param>
    /// <param name="threshold">Threshold percentage being alerted.</param>
    /// <returns><see langword="true"/> if this call won the right to alert; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="consumerId"/> is null or empty.</exception>
    public bool TryMarkAlerted(string consumerId, DateTime periodStart, int threshold)
    {
        ArgumentException.ThrowIfNullOrEmpty(consumerId);

        while (true)
        {
            if (!_lastAlerted.TryGetValue(consumerId, out var existing))
            {
                if (_lastAlerted.TryAdd(consumerId, (periodStart, threshold)))
                    return true;
                continue;
            }

            // New period: any threshold may fire again.
            var stale = existing.PeriodStart != periodStart;
            if (!stale && existing.Threshold >= threshold)
                return false;

            var updated = (periodStart, stale ? threshold : Math.Max(existing.Threshold, threshold));
            if (_lastAlerted.TryUpdate(consumerId, updated, existing))
                return true;
        }
    }

    /// <summary>
    /// Returns the highest threshold already alerted for the consumer in the
    /// given period, or 0 when nothing has been alerted yet.
    /// </summary>
    /// <param name="consumerId">Consumer identifier.</param>
    /// <param name="periodStart">UTC start of the current quota period.</param>
    /// <returns>Highest alerted threshold percentage, or 0.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="consumerId"/> is null or empty.</exception>
    public int GetLastAlertedThreshold(string consumerId, DateTime periodStart)
    {
        ArgumentException.ThrowIfNullOrEmpty(consumerId);

        return _lastAlerted.TryGetValue(consumerId, out var existing) && existing.PeriodStart == periodStart
            ? existing.Threshold
            : 0;
    }
}

/// <summary>
/// Default <see cref="IQuotaAlertEvaluator"/> implementation. Reads the quota
/// for the API key, counts the consumer's usage records in the current quota
/// period, and publishes <see cref="QuotaThresholdReachedEvent"/> for every
/// configured threshold that was crossed and not yet alerted this period.
/// </summary>
public class QuotaAlertEvaluator : IQuotaAlertEvaluator
{
    private readonly IUsageRepository _usageRepository;
    private readonly IUsageQuotaRepository _quotaRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly QuotaAlertStateCache _stateCache;
    private readonly QuotaAlertOptions _options;
    private readonly ILogger<QuotaAlertEvaluator> _logger;

    /// <summary>
    /// Creates a new evaluator.
    /// </summary>
    /// <param name="usageRepository">Repository used to count period usage.</param>
    /// <param name="quotaRepository">Repository providing the quota configuration.</param>
    /// <param name="eventPublisher">Publisher for threshold events.</param>
    /// <param name="stateCache">Singleton cache of already-alerted thresholds.</param>
    /// <param name="options">Alerting options (thresholds, enable flag).</param>
    /// <param name="logger">Logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <see langword="null"/>.</exception>
    public QuotaAlertEvaluator(
        IUsageRepository usageRepository,
        IUsageQuotaRepository quotaRepository,
        IEventPublisher eventPublisher,
        QuotaAlertStateCache stateCache,
        IOptions<QuotaAlertOptions> options,
        ILogger<QuotaAlertEvaluator> logger)
    {
        ArgumentNullException.ThrowIfNull(usageRepository);
        ArgumentNullException.ThrowIfNull(quotaRepository);
        ArgumentNullException.ThrowIfNull(eventPublisher);
        ArgumentNullException.ThrowIfNull(stateCache);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _usageRepository = usageRepository;
        _quotaRepository = quotaRepository;
        _eventPublisher = eventPublisher;
        _stateCache = stateCache;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task EvaluateAsync(string consumerId, string apiKeyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(consumerId);
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);

        if (!_options.Enabled)
            return;

        var thresholds = _options.GetEffectiveThresholds();
        if (thresholds.Count == 0)
            return;

        try
        {
            var quota = await _quotaRepository.GetByApiKeyIdAsync(apiKeyId);
            if (quota is not { IsEnabled: true, QuotaLimit: > 0 })
                return;

            var now = DateTime.UtcNow;
            var periodStart = UsageQuota.GetPeriodStart(now, quota.Period);
            var records = await _usageRepository.GetByConsumerAndDateRangeAsync(consumerId, periodStart, now);
            var currentUsage = (long)records.Count;
            var percentUsed = currentUsage * 100.0 / quota.QuotaLimit;

            foreach (var threshold in thresholds)
            {
                if (percentUsed < threshold)
                    break;

                if (!_stateCache.TryMarkAlerted(consumerId, periodStart, threshold))
                    continue;

                var @event = new QuotaThresholdReachedEvent
                {
                    ConsumerId = consumerId,
                    ApiKeyId = apiKeyId,
                    ThresholdPercentage = threshold,
                    CurrentUsage = currentUsage,
                    QuotaLimit = quota.QuotaLimit,
                    PercentageUsed = percentUsed,
                    PeriodStart = periodStart,
                    PeriodEnd = quota.GetPeriodEndUtc()
                };

                _logger.LogWarning(
                    "Consumer {ConsumerId} reached {Threshold}% of quota on key {ApiKeyId}: {Usage}/{Limit} ({Percent:F1}%)",
                    consumerId, threshold, apiKeyId, currentUsage, quota.QuotaLimit, percentUsed);

                await _eventPublisher.PublishAsync(@event);
            }
        }
        catch (Exception ex)
        {
            // Alerting is best-effort; it must never fail the request pipeline.
            _logger.LogError(ex,
                "Quota alert evaluation failed for consumer {ConsumerId}, key {ApiKeyId}",
                consumerId, apiKeyId);
        }
    }
}
