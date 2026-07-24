using System;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class UsageQuotaTests
{
    [Fact]
    public void GetPeriodEndUtc_Daily_ReturnsNextMidnightUtc()
    {
        // Arrange
        var start = new DateTime(2023, 5, 15, 12, 30, 0, DateTimeKind.Utc);
        var quota = new UsageQuota
        {
            Period = QuotaPeriod.Daily,
            PeriodStartAt = start
        };

        // Act
        var end = quota.GetPeriodEndUtc();

        // Assert
        var expected = start.Date.AddDays(1);
        end.Should().Be(expected);
        end.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void GetPeriodEndUtc_Monthly_ReturnsFirstOfNextMonthUtc()
    {
        // Arrange
        var start = new DateTime(2023, 1, 31, 5, 0, 0, DateTimeKind.Utc);
        var quota = new UsageQuota
        {
            Period = QuotaPeriod.Monthly,
            PeriodStartAt = start
        };

        // Act
        var end = quota.GetPeriodEndUtc();

        // Assert
        var expected = new DateTime(2023, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        end.Should().Be(expected);
    }

    [Fact]
    public void GetPeriodEndUtc_Hour_ReturnsOneHourLater()
    {
        // Arrange
        var start = new DateTime(2023, 5, 15, 10, 45, 0, DateTimeKind.Utc);
        var quota = new UsageQuota
        {
            Period = QuotaPeriod.Hour,
            PeriodStartAt = start
        };

        // Act
        var end = quota.GetPeriodEndUtc();

        // Assert
        end.Should().Be(start.AddHours(1));
    }

    [Fact]
    public void GetPeriodEndUtc_Week_ReturnsSevenDaysLater()
    {
        // Arrange
        var start = new DateTime(2023, 5, 15, 0, 0, 0, DateTimeKind.Utc);
        var quota = new UsageQuota
        {
            Period = QuotaPeriod.Week,
            PeriodStartAt = start
        };

        // Act
        var end = quota.GetPeriodEndUtc();

        // Assert
        end.Should().Be(start.Date.AddDays(7));
    }

    [Fact]
    public void ResetPeriod_UpdatesPeriodStartAndZeroesCurrentUsage()
    {
        // Arrange
        var now = new DateTime(2023, 5, 20, 13, 0, 0, DateTimeKind.Utc);
        var quota = new UsageQuota
        {
            Period = QuotaPeriod.Daily,
            PeriodStartAt = now.AddDays(-1),
            CurrentUsage = 123
        };

        // Act
        quota.ResetPeriod(now);

        // Assert
        quota.PeriodStartAt.Should().Be(UsageQuota.GetPeriodStart(now, QuotaPeriod.Daily));
        quota.CurrentUsage.Should().Be(0);
    }

    [Fact]
    public void RecordRequest_IncrementsWhenEnabledAndDoesNotWhenDisabled()
    {
        // Enabled case
        var enabledQuota = new UsageQuota { IsEnabled = true, CurrentUsage = 0 };
        enabledQuota.RecordRequest();
        enabledQuota.CurrentUsage.Should().Be(1);

        // Disabled case
        var disabledQuota = new UsageQuota { IsEnabled = false, CurrentUsage = 0 };
        disabledQuota.RecordRequest();
        disabledQuota.CurrentUsage.Should().Be(0);
    }

    [Fact]
    public void RemainingRequests_And_IsExceeded_BehaveCorrectly()
    {
        // Arrange
        var quota = new UsageQuota
        {
            QuotaLimit = 100,
            CurrentUsage = 30,
            IsEnabled = true
        };

        // Act / Assert
        quota.RemainingRequests.Should().Be(70);
        quota.IsExceeded.Should().BeFalse();

        // Exceed limit
        quota.CurrentUsage = 100;
        quota.IsExceeded.Should().BeTrue();

        // Disabled quota should never be exceeded
        quota.IsEnabled = false;
        quota.CurrentUsage = 200;
        quota.IsExceeded.Should().BeFalse();
    }

    [Fact]
    public void GetPeriodStart_ReturnsCorrectStart_ForAllSupportedPeriods()
    {
        // Daily
        var utcNow = new DateTime(2023, 5, 15, 12, 34, 56, DateTimeKind.Utc);
        UsageQuota.GetPeriodStart(utcNow, QuotaPeriod.Daily)
            .Should().Be(utcNow.Date);

        // Hour
        UsageQuota.GetPeriodStart(utcNow, QuotaPeriod.Hour)
            .Should().Be(new DateTime(2023, 5, 15, 12, 0, 0, DateTimeKind.Utc));

        // Week (assuming Sunday = 0)
        var weekStart = utcNow.Date.AddDays(-(int)utcNow.DayOfWeek);
        UsageQuota.GetPeriodStart(utcNow, QuotaPeriod.Week)
            .Should().Be(weekStart);

        // Monthly
        UsageQuota.GetPeriodStart(utcNow, QuotaPeriod.Monthly)
            .Should().Be(new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}
