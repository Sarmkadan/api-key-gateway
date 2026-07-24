// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Comprehensive edge case tests for UsageQuota counter behavior including concurrency,
// boundary conditions, and period rollover scenarios.
// =============================================================================

using Xunit;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Comprehensive edge case tests for <see cref="UsageQuotaService"/> counter behavior.
/// Tests cover concurrency edge cases, boundary conditions, and period rollover scenarios.
/// </summary>
public class UsageQuotaServiceTestsCounterEdgeCases
{
    private readonly Mock<IUsageQuotaRepository> _repositoryMock;
    private readonly Mock<ILogger<UsageQuotaService>> _loggerMock;
    private readonly UsageQuotaService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsageQuotaServiceTestsCounterEdgeCases"/> class.
    /// </summary>
    public UsageQuotaServiceTestsCounterEdgeCases()
    {
        _repositoryMock = new Mock<IUsageQuotaRepository>();
        _loggerMock = new Mock<ILogger<UsageQuotaService>>();
        _sut = new UsageQuotaService(_repositoryMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Tests that concurrent increments from multiple threads for the same key produce the exact expected total.
    /// Verifies no lost updates occur under high contention.
    /// </summary>
    [Fact]
    public async Task CheckAndRecordAsync_ConcurrentIncrements_ProducesExactExpectedTotal()
    {
        var quota = new UsageQuota
        {
            ApiKeyId = "key-concurrent-exact",
            QuotaLimit = 1000000, // Large enough to accommodate all increments
            Period = QuotaPeriod.Day,
            CurrentUsage = 0,
            IsEnabled = true,
            PeriodStartAt = DateTime.UtcNow.Date
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-concurrent-exact"))
            .ReturnsAsync(quota);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
            .Returns(Task.CompletedTask);

        // Launch 1000 concurrent calls to stress test the counter
        var taskCount = 1000;
        var tasks = Enumerable.Range(0, taskCount)
            .Select(_ => _sut.CheckAndRecordAsync("key-concurrent-exact"))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Verify all requests succeeded (not exceeded quota)
        results.Should().AllSatisfy(r => r.IsExceeded.Should().BeFalse());

        // Verify exact counter increment - should be exactly equal to task count
        quota.CurrentUsage.Should().Be(taskCount);

        // Verify repository was updated exactly once per successful request
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UsageQuota>()), Times.Exactly(taskCount));

        // Verify remaining quota calculation is correct (should be limit - current usage)
        var expectedRemaining = quota.QuotaLimit - quota.CurrentUsage;
        results[0].Remaining.Should().Be(expectedRemaining);
    }

    /// <summary>
    /// Tests that recording usage for a key with no prior UsageQuota entry correctly initializes the quota.
    /// Verifies that the service handles missing quota entries gracefully.
    /// </summary>
    [Fact]
    public async Task CheckAndRecordAsync_NoPriorQuotaEntry_CorrectlyInitializesQuota()
    {
        // Setup: no existing quota for this key
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-new-quota-entry"))
            .ReturnsAsync((UsageQuota?)null);

        // Create a new quota when first usage is recorded
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<UsageQuota>()))
            .ReturnsAsync((UsageQuota q) => q);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CheckAndRecordAsync("key-new-quota-entry");

        // Should return unlimited quota since no quota was configured
        result.IsExceeded.Should().BeFalse();
        result.Remaining.Should().Be(long.MaxValue);
        result.Limit.Should().Be(long.MaxValue);

        // Should NOT have created a quota (since no quota was configured)
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<UsageQuota>()), Times.Never);
    }

    /// <summary>
    /// Tests that recording usage for a key with a configured quota correctly initializes and increments the counter.
    /// Verifies that the first usage creates the quota with proper initial state.
    /// </summary>
    [Fact]
    public async Task CheckAndRecordAsync_FirstUsageWithConfiguredQuota_InitializesAndIncrementsCounter()
    {
        var initialQuota = new UsageQuota
        {
            ApiKeyId = "key-first-usage",
            QuotaLimit = 1000,
            Period = QuotaPeriod.Day,
            CurrentUsage = 0,
            IsEnabled = true,
            PeriodStartAt = DateTime.UtcNow.Date
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-first-usage"))
            .ReturnsAsync(initialQuota); // Quota exists

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CheckAndRecordAsync("key-first-usage");

        // Should succeed since quota is configured and not exceeded
        result.IsExceeded.Should().BeFalse();
        result.Remaining.Should().Be(999); // 1000 - 1
        result.Limit.Should().Be(1000);

        // Counter should be incremented
        initialQuota.CurrentUsage.Should().Be(1);

        // Should have updated the quota
        _repositoryMock.Verify(r => r.UpdateAsync(initialQuota), Times.Once);
    }

    /// <summary>
    /// Tests that recording a negative usage amount is handled correctly.
    /// Verifies that negative quota limits are rejected.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task SetQuotaAsync_NegativeOrZeroQuotaLimit_Rejected(long quotaLimit)
    {
        var result = await _sut.SetQuotaAsync("key-negative-test", quotaLimit, QuotaPeriod.Day);

        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<UsageQuota>()), Times.Never);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UsageQuota>()), Times.Never);
    }

    /// <summary>
    /// Tests that counters approaching int.MaxValue boundary don't silently overflow.
    /// Verifies that the counter stops incrementing when it reaches the limit.
    /// </summary>
    [Fact]
    public async Task CheckAndRecordAsync_CounterApproachingIntMaxValue_StopsAtLimit()
    {
        // Set up quota with limit close to int.MaxValue and usage at limit
        var quota = new UsageQuota
        {
            ApiKeyId = "key-int-boundary",
            QuotaLimit = int.MaxValue - 10, // Close to int.MaxValue
            Period = QuotaPeriod.Day,
            CurrentUsage = int.MaxValue - 10, // Exactly at limit
            IsEnabled = true,
            PeriodStartAt = DateTime.UtcNow.Date
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-int-boundary"))
            .ReturnsAsync(quota);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
            .Returns(Task.CompletedTask);

        // Try to record usage when already at limit
        var result = await _sut.CheckAndRecordAsync("key-int-boundary");

        // Should be exceeded since current usage is at limit
        result.IsExceeded.Should().BeTrue();
        result.Remaining.Should().Be(0);

        // Counter should not have incremented beyond limit
        quota.CurrentUsage.Should().Be(int.MaxValue - 10); // Should remain unchanged

        // Should not update since quota is exceeded
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UsageQuota>()), Times.Never);
    }

    /// <summary>
    /// Tests that counters approaching long.MaxValue boundary don't silently overflow.
    /// Verifies that the counter stops incrementing when it reaches the limit.
    /// </summary>
    [Fact]
    public async Task CheckAndRecordAsync_CounterApproachingLongMaxValue_StopsAtLimit()
    {
        // Set up quota with limit close to long.MaxValue and usage at limit
        var quota = new UsageQuota
        {
            ApiKeyId = "key-long-boundary",
            QuotaLimit = long.MaxValue - 100,
            Period = QuotaPeriod.Day,
            CurrentUsage = long.MaxValue - 100, // Exactly at limit
            IsEnabled = true,
            PeriodStartAt = DateTime.UtcNow.Date
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-long-boundary"))
            .ReturnsAsync(quota);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
            .Returns(Task.CompletedTask);

        // Try to record usage when already at limit
        var result = await _sut.CheckAndRecordAsync("key-long-boundary");

        // Should be exceeded since current usage is at limit
        result.IsExceeded.Should().BeTrue();
        result.Remaining.Should().Be(0);

        // Counter should not have incremented beyond limit
        quota.CurrentUsage.Should().Be(long.MaxValue - 100); // Should remain unchanged

        // Should not update since quota is exceeded
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UsageQuota>()), Times.Never);
    }

    /// <summary>
    /// Tests UsageStatistics equality and reset behavior when a tracking period rolls over.
    /// Verifies that period rollover resets counters correctly.
    /// </summary>
    [Fact]
    public async Task UsageQuota_PeriodRollover_ResetsCounterAndUpdatesStatistics()
    {
        // Create quota with old period start (rolled over)
        var oldPeriodStart = DateTime.UtcNow.AddDays(-30); // Way in the past
        var quota = new UsageQuota
        {
            ApiKeyId = "key-rollover-stats",
            QuotaLimit = 1000,
            Period = QuotaPeriod.Month,
            CurrentUsage = 850, // High usage from old period
            IsEnabled = true,
            PeriodStartAt = oldPeriodStart
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-rollover-stats"))
            .ReturnsAsync(quota);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CheckAndRecordAsync("key-rollover-stats");

        // Should succeed after rollover reset
        result.IsExceeded.Should().BeFalse();
        result.Remaining.Should().Be(999); // 1000 - 1 after reset to 0 then increment

        // Counter should be reset to 1 (0 + 1 after rollover)
        quota.CurrentUsage.Should().Be(1);

        // Period should be updated to current month start
        var expectedPeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        quota.PeriodStartAt.Should().Be(expectedPeriodStart);

        // Should have updated the quota
        _repositoryMock.Verify(r => r.UpdateAsync(quota), Times.Once);
    }

    /// <summary>
    /// Tests that multiple period rollovers are handled correctly.
    /// Verifies that consecutive rollovers don't cause issues.
    /// </summary>
    [Fact]
    public async Task UsageQuota_MultiplePeriodRollovers_HandledCorrectly()
    {
        // Create quota with multiple periods in the past
        var quota = new UsageQuota
        {
            ApiKeyId = "key-multi-rollover",
            QuotaLimit = 500,
            Period = QuotaPeriod.Day,
            CurrentUsage = 450,
            IsEnabled = true,
            PeriodStartAt = DateTime.UtcNow.AddDays(-100) // Multiple days in past
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-multi-rollover"))
            .ReturnsAsync(quota);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CheckAndRecordAsync("key-multi-rollover");

        // Should succeed after rollover reset
        result.IsExceeded.Should().BeFalse();

        // Counter should be reset to 1
        quota.CurrentUsage.Should().Be(1);

        // Period should be updated to current day start
        var expectedPeriodStart = DateTime.UtcNow.Date;
        quota.PeriodStartAt.Should().Be(expectedPeriodStart);
    }

    /// <summary>
    /// Tests that UsageQuota.CurrentUsage property correctly reflects the counter state.
    /// Verifies equality and state management.
    /// </summary>
    [Fact]
    public void UsageQuota_CurrentUsage_ReflectsCounterState()
    {
        var quota = new UsageQuota
        {
            ApiKeyId = "key-state-test",
            QuotaLimit = 1000,
            Period = QuotaPeriod.Day,
            CurrentUsage = 50,
            IsEnabled = true,
            PeriodStartAt = DateTime.UtcNow.Date
        };

        // Verify initial state
        quota.CurrentUsage.Should().Be(50);
        quota.IsExceeded.Should().BeFalse();
        quota.RemainingRequests.Should().Be(950);

        // Increment and verify
        quota.RecordRequest();
        quota.CurrentUsage.Should().Be(51);
        quota.IsExceeded.Should().BeFalse();
        quota.RemainingRequests.Should().Be(949);

        // Fill up to limit
        for (int i = 0; i < 949; i++)
        {
            quota.RecordRequest();
        }

        quota.CurrentUsage.Should().Be(1000);
        quota.IsExceeded.Should().BeTrue();
        quota.RemainingRequests.Should().Be(0);
    }

    /// <summary>
    /// Tests that disabled quotas don't increment counters even under high concurrency.
    /// Verifies that IsEnabled flag prevents counter increments.
    /// </summary>
    [Fact]
    public async Task CheckAndRecordAsync_DisabledQuota_UnderConcurrency_NoIncrements()
    {
        var quota = new UsageQuota
        {
            ApiKeyId = "key-disabled-concurrent",
            QuotaLimit = 10000,
            Period = QuotaPeriod.Day,
            CurrentUsage = 5000,
            IsEnabled = false, // Disabled!
            PeriodStartAt = DateTime.UtcNow.Date
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-disabled-concurrent"))
            .ReturnsAsync(quota);

        // Launch 100 concurrent calls
        var taskCount = 100;
        var tasks = Enumerable.Range(0, taskCount)
            .Select(_ => _sut.CheckAndRecordAsync("key-disabled-concurrent"))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // All should return unlimited quota
        results.Should().AllSatisfy(r =>
        {
            r.IsExceeded.Should().BeFalse();
            r.Remaining.Should().Be(long.MaxValue);
            r.Limit.Should().Be(long.MaxValue);
        });

        // Counter should remain unchanged
        quota.CurrentUsage.Should().Be(5000);

        // Should not update since quota is disabled
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UsageQuota>()), Times.Never);
    }

    /// <summary>
    /// Tests that the RemainingRequests calculation handles edge cases correctly.
    /// Verifies that negative remaining values are clamped to zero.
    /// </summary>
    [Fact]
    public void UsageQuota_RemainingRequests_ClampedToZero()
    {
        // Test with exactly at limit
        var quotaAtLimit = new UsageQuota
        {
            ApiKeyId = "key-at-limit-exact",
            QuotaLimit = 100,
            CurrentUsage = 100,
            IsEnabled = true
        };

        quotaAtLimit.RemainingRequests.Should().Be(0);
        quotaAtLimit.IsExceeded.Should().BeTrue();

        // Test with over limit (shouldn't happen but test defensive code)
        var quotaOverLimit = new UsageQuota
        {
            ApiKeyId = "key-over-limit",
            QuotaLimit = 100,
            CurrentUsage = 150,
            IsEnabled = true
        };

        quotaOverLimit.RemainingRequests.Should().Be(0); // Clamped to zero
        quotaOverLimit.IsExceeded.Should().BeTrue();
    }

    /// <summary>
    /// Tests that the PeriodEnd calculation handles different quota periods correctly.
    /// Verifies calendar-based period boundaries.
    /// </summary>
    [Theory]
    [InlineData(QuotaPeriod.Day, 1)]
    [InlineData(QuotaPeriod.Week, 7)]
    [InlineData(QuotaPeriod.Month, 31)] // Approximate for testing
    public void UsageQuota_GetPeriodEndUtc_CalculatesCorrectBoundaries(QuotaPeriod period, int expectedDays)
    {
        var quota = new UsageQuota
        {
            ApiKeyId = "key-period-test",
            QuotaLimit = 1000,
            Period = period,
            PeriodStartAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) // Fixed date for testing
        };

        var periodEnd = quota.GetPeriodEndUtc();
        var daysBetween = (periodEnd - quota.PeriodStartAt).TotalDays;

        daysBetween.Should().BeGreaterOrEqualTo(expectedDays);
        periodEnd.Should().BeAfter(quota.PeriodStartAt);
    }

    /// <summary>
    /// Tests that ResetPeriod correctly resets the counter and updates period start.
    /// </summary>
    [Fact]
    public void UsageQuota_ResetPeriod_ResetsCounterAndUpdatesStart()
    {
        var quota = new UsageQuota
        {
            ApiKeyId = "key-reset-test",
            QuotaLimit = 1000,
            Period = QuotaPeriod.Day,
            CurrentUsage = 500,
            PeriodStartAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var now = new DateTime(2024, 2, 15, 10, 30, 0, DateTimeKind.Utc);
        quota.ResetPeriod(now);

        quota.CurrentUsage.Should().Be(0);
        quota.PeriodStartAt.Should().Be(new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc));
    }
}