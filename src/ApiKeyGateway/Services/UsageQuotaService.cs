// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Services;

/// <summary>
/// Enforces per-key hard usage quotas with calendar-based reset periods.
/// </summary>
public interface IUsageQuotaService
{
    /// <summary>
    /// Checks whether the given API key is within its quota for the current period.
    /// Resets the counter if the period has rolled over.
    /// Returns the quota state so callers can attach response headers.
    /// </summary>
    Task<UsageQuotaResult> CheckAndRecordAsync(string apiKeyId);

    Task<UsageQuota?> GetQuotaAsync(string apiKeyId);
    Task<bool> SetQuotaAsync(string apiKeyId, long quotaLimit, Domain.Enums.QuotaPeriod period);
}

/// <summary>Result from a quota check</summary>
public record UsageQuotaResult(
    bool IsExceeded,
    long Remaining,
    long Limit,
    DateTime PeriodEnd);

public class UsageQuotaService : IUsageQuotaService
{
    private readonly IUsageQuotaRepository _repository;
    private readonly ILogger<UsageQuotaService> _logger;

    public UsageQuotaService(IUsageQuotaRepository repository, ILogger<UsageQuotaService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<UsageQuotaResult> CheckAndRecordAsync(string apiKeyId)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            return new UsageQuotaResult(false, long.MaxValue, long.MaxValue, DateTime.MaxValue);

        var quota = await _repository.GetByApiKeyIdAsync(apiKeyId);
        if (quota == null || !quota.IsEnabled)
            return new UsageQuotaResult(false, long.MaxValue, long.MaxValue, DateTime.MaxValue);

        var now = DateTime.UtcNow;

        // Roll over the period if we have moved into a new calendar window.
        var expectedStart = UsageQuota.GetPeriodStart(now, quota.Period);
        if (quota.PeriodStartAt < expectedStart)
        {
            _logger.LogInformation(
                "Quota period rolled over for API key {ApiKeyId} ({Period}). Resetting counter from {Count}",
                apiKeyId, quota.Period, quota.CurrentUsage);

            quota.ResetPeriod(now);
            await _repository.UpdateAsync(quota);
        }

        if (quota.IsExceeded)
        {
            _logger.LogWarning(
                "Quota exceeded for API key {ApiKeyId}: {Usage}/{Limit} in {Period} period",
                apiKeyId, quota.CurrentUsage, quota.QuotaLimit, quota.Period);

            return new UsageQuotaResult(true, 0, quota.QuotaLimit, quota.GetPeriodEndUtc());
        }

        quota.RecordRequest();
        await _repository.UpdateAsync(quota);

        return new UsageQuotaResult(false, quota.RemainingRequests, quota.QuotaLimit, quota.GetPeriodEndUtc());
    }

    /// <inheritdoc/>
    public async Task<UsageQuota?> GetQuotaAsync(string apiKeyId)
        => await _repository.GetByApiKeyIdAsync(apiKeyId);

    /// <inheritdoc/>
    public async Task<bool> SetQuotaAsync(string apiKeyId, long quotaLimit, Domain.Enums.QuotaPeriod period)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId) || quotaLimit <= 0)
            return false;

        var existing = await _repository.GetByApiKeyIdAsync(apiKeyId);
        var now = DateTime.UtcNow;

        if (existing == null)
        {
            var quota = new UsageQuota
            {
                Id = Guid.NewGuid().ToString(),
                ApiKeyId = apiKeyId,
                QuotaLimit = quotaLimit,
                Period = period,
                PeriodStartAt = UsageQuota.GetPeriodStart(now, period)
            };
            await _repository.CreateAsync(quota);
        }
        else
        {
            var updated = new UsageQuota
            {
                Id = existing.Id,
                ApiKeyId = apiKeyId,
                QuotaLimit = quotaLimit,
                Period = period,
                PeriodStartAt = UsageQuota.GetPeriodStart(now, period),
                CurrentUsage = existing.CurrentUsage,
                IsEnabled = existing.IsEnabled,
                CreatedAt = existing.CreatedAt
            };
            await _repository.UpdateAsync(updated);
        }

        _logger.LogInformation(
            "Quota set for API key {ApiKeyId}: {Limit} requests per {Period}",
            apiKeyId, quotaLimit, period);

        return true;
    }
}

/// <summary>Repository interface for usage quota persistence</summary>
public interface IUsageQuotaRepository
{
    Task<UsageQuota?> GetByApiKeyIdAsync(string apiKeyId);
    Task<UsageQuota> CreateAsync(UsageQuota quota);
    Task UpdateAsync(UsageQuota quota);
    Task DeleteAsync(string id);
}
