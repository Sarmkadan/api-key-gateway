// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Comprehensive tests for UsageQuotaService covering quota exceeded, exactly-at-limit,
// and reset behavior scenarios
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
/// Comprehensive unit tests for <see cref="UsageQuotaService"/> covering quota exceeded,
/// exactly-at-limit, and reset behavior scenarios.
/// </summary>
public class UsageQuotaServiceTestsComprehensive
{
    private readonly Mock<IUsageQuotaRepository> _repositoryMock;
    private readonly Mock<ILogger<UsageQuotaService>> _loggerMock;
    private readonly UsageQuotaService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsageQuotaServiceTestsComprehensive"/> class.
    /// </summary>
    public UsageQuotaServiceTestsComprehensive()
    {
        _repositoryMock = new Mock<IUsageQuotaRepository>();
        _loggerMock = new Mock<ILogger<UsageQuotaService>>();
        _sut = new UsageQuotaService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    /// <summary>
    /// Tests quota exceeded scenario: usage exactly at limit returns IsExceeded=true with zero remaining.
    /// </summary>
    public async Task CheckAndRecordAsync_UsageExactlyAtLimit_ReturnsExceededWithZeroRemaining()
    {
        // Arrange
        var quota = new UsageQuota
        {
            ApiKeyId = "key-exactly-at-limit",
            QuotaLimit = 100,
            Period = QuotaPeriod.Daily,
            CurrentUsage = 100,
            IsEnabled = true,
            PeriodStartAt = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Daily)
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-exactly-at-limit"))
            .ReturnsAsync(quota);

        // Act
        var result = await _sut.CheckAndRecordAsync("key-exactly-at-limit");

        // Assert
        result.IsExceeded.Should().BeTrue("because usage equals quota limit");
        result.Remaining.Should().Be(0, "because no requests remain when exactly at limit");
        result.Limit.Should().Be(100);
        result.PeriodEnd.Should().BeAfter(DateTime.UtcNow, "because period end should be in the future");

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UsageQuota>()), Times.Never,
            "because exceeded quota should not update repository");
    }

    [Fact]
    /// <summary>
    /// Tests quota exceeded scenario: usage exceeds limit returns IsExceeded=true with zero remaining.
    /// </summary>
    public async Task CheckAndRecordAsync_UsageExceedsLimit_ReturnsExceededWithZeroRemaining()
    {
        // Arrange
        var quota = new UsageQuota
        {
            ApiKeyId = "key-exceeds-limit",
            QuotaLimit = 100,
            Period = QuotaPeriod.Hour,
            CurrentUsage = 150,
            IsEnabled = true,
            PeriodStartAt = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Hour)
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-exceeds-limit"))
            .ReturnsAsync(quota);

        // Act
        var result = await _sut.CheckAndRecordAsync("key-exceeds-limit");

        // Assert
        result.IsExceeded.Should().BeTrue("because usage exceeds quota limit");
        result.Remaining.Should().Be(0, "because no requests remain when quota is exceeded");
        result.Limit.Should().Be(100);

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UsageQuota>()), Times.Never,
            "because exceeded quota should not update repository");
    }

    [Fact]
    /// <summary>
    /// Tests quota exceeded scenario: disabled quota with high usage returns unlimited quota.
    /// </summary>
    public async Task CheckAndRecordAsync_DisabledQuotaWithHighUsage_ReturnsUnlimited()
    {
        // Arrange
        var quota = new UsageQuota
        {
            ApiKeyId = "key-disabled-high-usage",
            QuotaLimit = 100,
            Period = QuotaPeriod.Day,
            CurrentUsage = 1000,
            IsEnabled = false, // Disabled
            PeriodStartAt = DateTime.UtcNow.Date
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-disabled-high-usage"))
            .ReturnsAsync(quota);

        // Act
        var result = await _sut.CheckAndRecordAsync("key-disabled-high-usage");

        // Assert
        result.IsExceeded.Should().BeFalse("because disabled quotas are unlimited");
        result.Remaining.Should().Be(long.MaxValue, "because disabled quotas have unlimited remaining");
        result.Limit.Should().Be(long.MaxValue);
    }

    [Fact]
    /// <summary>
    /// Tests exactly-at-limit scenario for daily period.
    /// </summary>
    public async Task CheckAndRecordAsync_ExactlyAtLimitDaily_ReturnsCorrectState()
    {
        // Arrange
        var quota = new UsageQuota
        {
            ApiKeyId = "key-daily-limit",
            QuotaLimit = 1000,
            Period = QuotaPeriod.Daily,
            CurrentUsage = 1000,
            IsEnabled = true,
            PeriodStartAt = DateTime.UtcNow.Date
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-daily-limit"))
            .ReturnsAsync(quota);

        // Act
        var result = await _sut.CheckAndRecordAsync("key-daily-limit");

        // Assert
        result.IsExceeded.Should().BeTrue();
        result.Remaining.Should().Be(0);
        result.Limit.Should().Be(1000);
        quota.CurrentUsage.Should().Be(1000, "because usage should not be incremented when exceeded");
    }

    [Fact]
    /// <summary>
    /// Tests exactly-at-limit scenario for monthly period.
    /// </summary>
    public async Task CheckAndRecordAsync_ExactlyAtLimitMonthly_ReturnsCorrectState()
    {
        // Arrange
        var quota = new UsageQuota
        {
            ApiKeyId = "key-monthly-limit",
            QuotaLimit = 5000,
            Period = QuotaPeriod.Monthly,
            CurrentUsage = 5000,
            IsEnabled = true,
            PeriodStartAt = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-monthly-limit"))
            .ReturnsAsync(quota);

        // Act
        var result = await _sut.CheckAndRecordAsync("key-monthly-limit");

        // Assert
        result.IsExceeded.Should().BeTrue();
        result.Remaining.Should().Be(0);
        result.Limit.Should().Be(5000);
    }

    [Fact]
    /// <summary>
    /// Tests exactly-at-limit scenario for hourly period.
    /// </summary>
    public async Task CheckAndRecordAsync_ExactlyAtLimitHourly_ReturnsCorrectState()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var periodStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);

        var quota = new UsageQuota
        {
            ApiKeyId = "key-hourly-limit",
            QuotaLimit = 100,
            Period = QuotaPeriod.Hour,
            CurrentUsage = 100,
            IsEnabled = true,
            PeriodStartAt = periodStart
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-hourly-limit"))
            .ReturnsAsync(quota);

        // Act
        var result = await _sut.CheckAndRecordAsync("key-hourly-limit");

        // Assert
        result.IsExceeded.Should().BeTrue();
        result.Remaining.Should().Be(0);
        result.Limit.Should().Be(100);
    }

    [Fact]
    /// <summary>
    /// Tests reset behavior: period rollover resets counter and allows new requests.
    /// </summary>
    public async Task CheckAndRecordAsync_PeriodRollover_ResetsCounterAndAllowsNewRequests()
    {
        // Arrange
        var oldPeriodStart = DateTime.UtcNow.AddDays(-2); // Rolled over 2 days ago
        var quota = new UsageQuota
        {
            ApiKeyId = "key-rollover-reset",
            QuotaLimit = 100,
            Period = QuotaPeriod.Daily,
            CurrentUsage = 95, // High usage in old period
            IsEnabled = true,
            PeriodStartAt = oldPeriodStart
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-rollover-reset"))
            .ReturnsAsync(quota);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CheckAndRecordAsync("key-rollover-reset");

        // Assert
        result.IsExceeded.Should().BeFalse("because period was rolled over and counter reset");
        result.Remaining.Should().Be(99, "because counter was reset to 0, then incremented to 1");
        result.Limit.Should().Be(100);
        quota.CurrentUsage.Should().Be(1, "because counter should be reset to 0 then incremented to 1");
        quota.PeriodStartAt.Should().BeAfter(oldPeriodStart, "because period start should be updated to current period");

        _repositoryMock.Verify(r => r.UpdateAsync(quota), Times.Once,
            "because rollover should persist the reset");
    }

    [Fact]
    /// <summary>
    /// Tests reset behavior: multiple requests after period rollover work correctly.
    /// </summary>
    public async Task CheckAndRecordAsync_MultipleRequestsAfterRollover_WorksCorrectly()
    {
        // Arrange
        var oldPeriodStart = DateTime.UtcNow.AddDays(-1);
        var quota = new UsageQuota
        {
            ApiKeyId = "key-multiple-after-rollover",
            QuotaLimit = 50,
            Period = QuotaPeriod.Daily,
            CurrentUsage = 45,
            IsEnabled = true,
            PeriodStartAt = oldPeriodStart
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-multiple-after-rollover"))
            .ReturnsAsync(quota);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
            .Returns(Task.CompletedTask);

        // Act - make 10 requests
        var results = new List<UsageQuotaResult>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(await _sut.CheckAndRecordAsync("key-multiple-after-rollover"));
        }

        // Assert
        results.Should().AllSatisfy(r => r.IsExceeded.Should().BeFalse());
        results[^1].Remaining.Should().Be(40, "because 10 requests were recorded after reset (45-5+10=50)");
        quota.CurrentUsage.Should().Be(10, "because counter should be reset then incremented 10 times");

        _repositoryMock.Verify(r => r.UpdateAsync(quota), Times.Exactly(10),
            "because each request should update the counter");
    }

    [Fact]
    /// <summary>
    /// Tests reset behavior: period rollover with zero limit quota.
    /// </summary>
    public async Task CheckAndRecordAsync_PeriodRolloverWithZeroLimit_ResetsAndAllowsNoRequests()
    {
        // Arrange
        var oldPeriodStart = DateTime.UtcNow.AddDays(-1);
        var quota = new UsageQuota
        {
            ApiKeyId = "key-zero-limit-rollover",
            QuotaLimit = 0,
            Period = QuotaPeriod.Daily,
            CurrentUsage = 0,
            IsEnabled = true,
            PeriodStartAt = oldPeriodStart
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-zero-limit-rollover"))
            .ReturnsAsync(quota);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CheckAndRecordAsync("key-zero-limit-rollover");

        // Assert
        result.IsExceeded.Should().BeTrue("because zero limit quota is always exceeded");
        result.Remaining.Should().Be(0);
        result.Limit.Should().Be(0);
        quota.CurrentUsage.Should().Be(0, "because usage should not increment for zero limit");
        quota.PeriodStartAt.Should().BeAfter(oldPeriodStart, "because period should be reset");

        _repositoryMock.Verify(r => r.UpdateAsync(quota), Times.Once,
            "because rollover should persist the reset even when exceeded");
    }

    [Fact]
    /// <summary>
    /// Tests reset behavior: weekly period rollover.
    /// </summary>
    public async Task CheckAndRecordAsync_WeeklyPeriodRollover_ResetsCounter()
    {
        // Arrange - create quota that started last week
        var oldPeriodStart = DateTime.UtcNow.AddDays(-8); // 8 days ago = 1 week + 1 day
        var quota = new UsageQuota
        {
            ApiKeyId = "key-weekly-rollover",
            QuotaLimit = 200,
            Period = QuotaPeriod.Week,
            CurrentUsage = 180,
            IsEnabled = true,
            PeriodStartAt = oldPeriodStart
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-weekly-rollover"))
            .ReturnsAsync(quota);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CheckAndRecordAsync("key-weekly-rollover");

        // Assert
        result.IsExceeded.Should().BeFalse();
        result.Remaining.Should().Be(199);
        quota.CurrentUsage.Should().Be(1);

        // Verify new period start is correct (Sunday midnight)
        var expectedWeekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        quota.PeriodStartAt.Date.Should().Be(expectedWeekStart.Date);
    }

    [Fact]
    /// <summary>
    /// Tests reset behavior: monthly period rollover at month boundary.
    /// </summary>
    public async Task CheckAndRecordAsync_MonthlyPeriodRollover_ResetsCounter()
    {
        // Arrange - create quota that started in previous month
        var oldPeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month - 1, 1);
        var quota = new UsageQuota
        {
            ApiKeyId = "key-monthly-rollover",
            QuotaLimit = 1000,
            Period = QuotaPeriod.Monthly,
            CurrentUsage = 950,
            IsEnabled = true,
            PeriodStartAt = oldPeriodStart
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-monthly-rollover"))
            .ReturnsAsync(quota);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CheckAndRecordAsync("key-monthly-rollover");

        // Assert
        result.IsExceeded.Should().BeFalse();
        result.Remaining.Should().Be(999);
        quota.CurrentUsage.Should().Be(1);

        // Verify new period start is first day of current month
        var expectedMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        quota.PeriodStartAt.Should().Be(expectedMonthStart);
    }

    [Fact]
    /// <summary>
    /// Tests exactly-at-limit scenario: request is rejected but quota state is correct.
    /// </summary>
    public async Task CheckAndRecordAsync_ExactlyAtLimit_RequestRejectedWithCorrectState()
    {
        // Arrange
        var quota = new UsageQuota
        {
            ApiKeyId = "key-exact-limit-reject",
            QuotaLimit = 1,
            Period = QuotaPeriod.Daily,
            CurrentUsage = 1,
            IsEnabled = true,
            PeriodStartAt = DateTime.UtcNow.Date
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-exact-limit-reject"))
            .ReturnsAsync(quota);

        // Act - try to make a request when exactly at limit
        var result = await _sut.CheckAndRecordAsync("key-exact-limit-reject");

        // Assert
        result.IsExceeded.Should().BeTrue("because request should be rejected when at limit");
        result.Remaining.Should().Be(0);

        // Verify usage was not incremented
        quota.CurrentUsage.Should().Be(1, "because usage should not increase when quota is exceeded");

        // Verify repository was not updated (no new request recorded)
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UsageQuota>()), Times.Never);
    }

    [Fact]
    /// <summary>
    /// Tests quota exceeded scenario with different periods to ensure behavior is consistent.
    /// </summary>
    public async Task CheckAndRecordAsync_QuotaExceeded_BehaviorConsistentAcrossPeriods()
    {
        // Test with Hour period
        var hourQuota = new UsageQuota
        {
            ApiKeyId = "key-hour-exceeded",
            QuotaLimit = 50,
            Period = QuotaPeriod.Hour,
            CurrentUsage = 50,
            IsEnabled = true,
            PeriodStartAt = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Hour)
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-hour-exceeded"))
            .ReturnsAsync(hourQuota);

        var hourResult = await _sut.CheckAndRecordAsync("key-hour-exceeded");
        hourResult.IsExceeded.Should().BeTrue();
        hourResult.Remaining.Should().Be(0);

        // Test with Daily period
        var dailyQuota = new UsageQuota
        {
            ApiKeyId = "key-daily-exceeded",
            QuotaLimit = 1000,
            Period = QuotaPeriod.Daily,
            CurrentUsage = 1000,
            IsEnabled = true,
            PeriodStartAt = DateTime.UtcNow.Date
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-daily-exceeded"))
            .ReturnsAsync(dailyQuota);

        var dailyResult = await _sut.CheckAndRecordAsync("key-daily-exceeded");
        dailyResult.IsExceeded.Should().BeTrue();
        dailyResult.Remaining.Should().Be(0);

        // Test with Monthly period
        var monthlyQuota = new UsageQuota
        {
            ApiKeyId = "key-monthly-exceeded",
            QuotaLimit = 5000,
            Period = QuotaPeriod.Monthly,
            CurrentUsage = 5000,
            IsEnabled = true,
            PeriodStartAt = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-monthly-exceeded"))
            .ReturnsAsync(monthlyQuota);

        var monthlyResult = await _sut.CheckAndRecordAsync("key-monthly-exceeded");
        monthlyResult.IsExceeded.Should().BeTrue();
        monthlyResult.Remaining.Should().Be(0);
    }

}
