// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for RateLimitCalculationHelper class
// =============================================================================

using System;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Utilities;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="RateLimitCalculationHelper"/> class.
/// Tests rate limit calculations, window management, and quota tracking functionality.
/// </summary>
public class RateLimitCalculationHelperUnitTests
{
    /// <summary>
    /// Tests GetWindowEnd with Second unit - should add 1 second to current time.
    /// </summary>
    [Fact]
    public void GetWindowEnd_SecondUnit_AddsOneSecond()
    {
        // Arrange
        var currentTime = new DateTime(2026, 7, 21, 14, 30, 45, 123);
        var expected = new DateTime(2026, 7, 21, 14, 30, 46, 123);

        // Act
        var result = RateLimitCalculationHelper.GetWindowEnd(currentTime, RateLimitUnit.Second);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests GetWindowEnd with Minute unit - should round up to next minute.
    /// </summary>
    [Fact]
    public void GetWindowEnd_MinuteUnit_RoundsUpToNextMinute()
    {
        // Arrange
        var currentTime = new DateTime(2026, 7, 21, 14, 30, 45);
        var expected = new DateTime(2026, 7, 21, 14, 31, 0);

        // Act
        var result = RateLimitCalculationHelper.GetWindowEnd(currentTime, RateLimitUnit.Minute);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests GetWindowEnd with Hour unit - should round up to next hour.
    /// </summary>
    [Fact]
    public void GetWindowEnd_HourUnit_RoundsUpToNextHour()
    {
        // Arrange
        var currentTime = new DateTime(2026, 7, 21, 14, 30, 45);
        var expected = new DateTime(2026, 7, 21, 15, 0, 0);

        // Act
        var result = RateLimitCalculationHelper.GetWindowEnd(currentTime, RateLimitUnit.Hour);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests GetWindowEnd with Day unit - should round up to next day at midnight.
    /// </summary>
    [Fact]
    public void GetWindowEnd_DayUnit_RoundsUpToNextDay()
    {
        // Arrange
        var currentTime = new DateTime(2026, 7, 21, 14, 30, 45);
        var expected = new DateTime(2026, 7, 22, 0, 0, 0);

        // Act
        var result = RateLimitCalculationHelper.GetWindowEnd(currentTime, RateLimitUnit.Day);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests GetWindowEnd with Month unit - should round up to next month.
    /// </summary>
    [Fact]
    public void GetWindowEnd_MonthUnit_RoundsUpToNextMonth()
    {
        // Arrange
        var currentTime = new DateTime(2026, 7, 21, 14, 30, 45);
        var expected = new DateTime(2026, 8, 1, 0, 0, 0);

        // Act
        var result = RateLimitCalculationHelper.GetWindowEnd(currentTime, RateLimitUnit.Month);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests GetWindowStart with Second unit - should subtract 1 second.
    /// </summary>
    [Fact]
    public void GetWindowStart_SecondUnit_SubtractsOneSecond()
    {
        // Arrange
        var currentTime = new DateTime(2026, 7, 21, 14, 30, 45, 123);
        var expected = new DateTime(2026, 7, 21, 14, 30, 44, 123);

        // Act
        var result = RateLimitCalculationHelper.GetWindowStart(currentTime, RateLimitUnit.Second);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests GetWindowStart with Minute unit - should round down to start of minute.
    /// </summary>
    [Fact]
    public void GetWindowStart_MinuteUnit_RoundsDownToStartOfMinute()
    {
        // Arrange
        var currentTime = new DateTime(2026, 7, 21, 14, 30, 45);
        var expected = new DateTime(2026, 7, 21, 14, 30, 0);

        // Act
        var result = RateLimitCalculationHelper.GetWindowStart(currentTime, RateLimitUnit.Minute);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests GetWindowStart with Hour unit - should round down to start of hour.
    /// </summary>
    [Fact]
    public void GetWindowStart_HourUnit_RoundsDownToStartOfHour()
    {
        // Arrange
        var currentTime = new DateTime(2026, 7, 21, 14, 30, 45);
        var expected = new DateTime(2026, 7, 21, 14, 0, 0);

        // Act
        var result = RateLimitCalculationHelper.GetWindowStart(currentTime, RateLimitUnit.Hour);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests GetWindowStart with Day unit - should round down to start of day.
    /// </summary>
    [Fact]
    public void GetWindowStart_DayUnit_RoundsDownToStartOfDay()
    {
        // Arrange
        var currentTime = new DateTime(2026, 7, 21, 14, 30, 45);
        var expected = new DateTime(2026, 7, 21, 0, 0, 0);

        // Act
        var result = RateLimitCalculationHelper.GetWindowStart(currentTime, RateLimitUnit.Day);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests GetWindowStart with Month unit - should round down to start of month.
    /// </summary>
    [Fact]
    public void GetWindowStart_MonthUnit_RoundsDownToStartOfMonth()
    {
        // Arrange
        var currentTime = new DateTime(2026, 7, 21, 14, 30, 45);
        var expected = new DateTime(2026, 7, 1, 0, 0, 0);

        // Act
        var result = RateLimitCalculationHelper.GetWindowStart(currentTime, RateLimitUnit.Month);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests GetSecondsUntilAllowed returns 0 when under limit.
    /// </summary>
    [Fact]
    public void GetSecondsUntilAllowed_UnderLimit_ReturnsZero()
    {
        // Arrange
        var windowStart = DateTime.UtcNow.AddSeconds(-1); // 1 second ago
        var limit = 10;
        var unit = RateLimitUnit.Minute;

        // Act
        var result = RateLimitCalculationHelper.GetSecondsUntilAllowed(5, limit, windowStart, unit);

        // Assert
        result.Should().Be(0, "under limit should be allowed immediately");
    }

    /// <summary>
    /// Tests GetSecondsUntilAllowed returns positive value when at limit (needs to wait for window reset).
    /// </summary>
    [Fact]
    public void GetSecondsUntilAllowed_AtLimit_ReturnsPositiveValue()
    {
        // Arrange
        var windowStart = DateTime.UtcNow.AddSeconds(-1); // 1 second ago
        var currentUsage = 10;
        var limit = 10;
        var unit = RateLimitUnit.Minute;

        // Act
        var result = RateLimitCalculationHelper.GetSecondsUntilAllowed(currentUsage, limit, windowStart, unit);

        // Assert - at limit means you need to wait for window to reset
        result.Should().BeGreaterThan(0, "at limit needs to wait for window reset");
    }

    /// <summary>
    /// Tests GetSecondsUntilAllowed returns positive value when over limit and window hasn't expired.
    /// </summary>
    [Fact]
    public void GetSecondsUntilAllowed_OverLimit_ReturnsPositiveValue()
    {
        // Arrange - use a very recent window start so window hasn't expired
        // For Minute unit, window is 60 seconds. Set window start to 1 second ago,
        // so window ends in ~59 seconds
        var windowStart = DateTime.UtcNow.AddSeconds(-1);
        var currentUsage = 15;
        var limit = 10;
        var unit = RateLimitUnit.Minute;

        // Act
        var result = RateLimitCalculationHelper.GetSecondsUntilAllowed(currentUsage, limit, windowStart, unit);

        // Assert - should be positive (around 59 seconds)
        result.Should().BeGreaterThan(0, "over limit with active window should return positive seconds until reset");
    }

    /// <summary>
    /// Tests CalculateQuotagePercentage with normal values.
    /// </summary>
    [Fact]
    public void CalculateQuotagePercentage_NormalValues_ReturnsCorrectPercentage()
    {
        // Arrange
        var currentUsage = 75;
        var limit = 100;

        // Act
        var result = RateLimitCalculationHelper.CalculateQuotagePercentage(currentUsage, limit);

        // Assert
        result.Should().Be(75);
    }

    /// <summary>
    /// Tests CalculateQuotagePercentage when usage equals limit - should return 100.
    /// </summary>
    [Fact]
    public void CalculateQuotagePercentage_UsageEqualsLimit_Returns100()
    {
        // Arrange
        var currentUsage = 100;
        var limit = 100;

        // Act
        var result = RateLimitCalculationHelper.CalculateQuotagePercentage(currentUsage, limit);

        // Assert
        result.Should().Be(100);
    }

    /// <summary>
    /// Tests CalculateQuotagePercentage when usage exceeds limit - should cap at 100.
    /// </summary>
    [Fact]
    public void CalculateQuotagePercentage_UsageExceedsLimit_CapsAt100()
    {
        // Arrange
        var currentUsage = 150;
        var limit = 100;

        // Act
        var result = RateLimitCalculationHelper.CalculateQuotagePercentage(currentUsage, limit);

        // Assert
        result.Should().Be(100);
    }

    /// <summary>
    /// Tests CalculateQuotagePercentage with zero limit - should return 0.
    /// </summary>
    [Fact]
    public void CalculateQuotagePercentage_ZeroLimit_ReturnsZero()
    {
        // Arrange
        var currentUsage = 50;
        var limit = 0;

        // Act
        var result = RateLimitCalculationHelper.CalculateQuotagePercentage(currentUsage, limit);

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Tests CalculateQuotagePercentage with negative limit - should return 0.
    /// </summary>
    [Fact]
    public void CalculateQuotagePercentage_NegativeLimit_ReturnsZero()
    {
        // Arrange
        var currentUsage = 50;
        var limit = -10;

        // Act
        var result = RateLimitCalculationHelper.CalculateQuotagePercentage(currentUsage, limit);

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Tests ShouldWarnAboutLimit with percentage below 80 - should return false.
    /// </summary>
    [Fact]
    public void ShouldWarnAboutLimit_Below80_ReturnsFalse()
    {
        // Arrange
        var percentage = 79;

        // Act
        var result = RateLimitCalculationHelper.ShouldWarnAboutLimit(percentage);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests ShouldWarnAboutLimit at 80% - should return true.
    /// </summary>
    [Fact]
    public void ShouldWarnAboutLimit_At80_ReturnsTrue()
    {
        // Arrange
        var percentage = 80;

        // Act
        var result = RateLimitCalculationHelper.ShouldWarnAboutLimit(percentage);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests ShouldWarnAboutLimit at 89% - should return true.
    /// </summary>
    [Fact]
    public void ShouldWarnAboutLimit_At89_ReturnsTrue()
    {
        // Arrange
        var percentage = 89;

        // Act
        var result = RateLimitCalculationHelper.ShouldWarnAboutLimit(percentage);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests ShouldWarnAboutLimit at 90% - should return true.
    /// </summary>
    [Fact]
    public void ShouldWarnAboutLimit_At90_ReturnsTrue()
    {
        // Arrange
        var percentage = 90;

        // Act
        var result = RateLimitCalculationHelper.ShouldWarnAboutLimit(percentage);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests ShouldWarnAboutLimit at 100% - should return true.
    /// </summary>
    [Fact]
    public void ShouldWarnAboutLimit_At100_ReturnsTrue()
    {
        // Arrange
        var percentage = 100;

        // Act
        var result = RateLimitCalculationHelper.ShouldWarnAboutLimit(percentage);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests GetReadableResetTime with negative timespan - should return "immediately".
    /// </summary>
    [Fact]
    public void GetReadableResetTime_NegativeTimespan_ReturnsImmediately()
    {
        // Arrange
        var windowEnd = DateTime.UtcNow.AddSeconds(-1);
        var now = DateTime.UtcNow;

        // Act
        var result = RateLimitCalculationHelper.GetReadableResetTime(windowEnd, now);

        // Assert
        result.Should().Be("immediately");
    }

    /// <summary>
    /// Tests GetReadableResetTime with hours - should return hours format.
    /// </summary>
    [Fact]
    public void GetReadableResetTime_Hours_ReturnsHoursFormat()
    {
        // Arrange
        var windowEnd = DateTime.UtcNow.AddHours(3.5);
        var now = DateTime.UtcNow;

        // Act
        var result = RateLimitCalculationHelper.GetReadableResetTime(windowEnd, now);

        // Assert
        result.Should().Be("4 hours");
    }

    /// <summary>
    /// Tests GetReadableResetTime with minutes - should return minutes format.
    /// </summary>
    [Fact]
    public void GetReadableResetTime_Minutes_ReturnsMinutesFormat()
    {
        // Arrange
        var windowEnd = DateTime.UtcNow.AddMinutes(45.7);
        var now = DateTime.UtcNow;

        // Act
        var result = RateLimitCalculationHelper.GetReadableResetTime(windowEnd, now);

        // Assert
        result.Should().Be("46 minutes");
    }

    /// <summary>
    /// Tests GetReadableResetTime with seconds - should return seconds format.
    /// </summary>
    [Fact]
    public void GetReadableResetTime_Seconds_ReturnsSecondsFormat()
    {
        // Arrange
        var windowEnd = DateTime.UtcNow.AddSeconds(35.2);
        var now = DateTime.UtcNow;

        // Act
        var result = RateLimitCalculationHelper.GetReadableResetTime(windowEnd, now);

        // Assert
        result.Should().Be("36 seconds");
    }

    /// <summary>
    /// Tests GetReadableResetTime with null now parameter - should use UtcNow.
    /// </summary>
    [Fact]
    public void GetReadableResetTime_NullNow_UsesUtcNow()
    {
        // Arrange
        var windowEnd = DateTime.UtcNow.AddSeconds(10);

        // Act
        var result = RateLimitCalculationHelper.GetReadableResetTime(windowEnd, null);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain("seconds");
    }

    /// <summary>
    /// Tests GetWindowEnd with unknown RateLimitUnit - should throw ArgumentException.
    /// </summary>
    [Fact]
    public void GetWindowEnd_UnknownUnit_ThrowsArgumentException()
    {
        // Arrange
        var currentTime = DateTime.UtcNow;
        var unknownUnit = (RateLimitUnit)999;

        // Act
        Action act = () => RateLimitCalculationHelper.GetWindowEnd(currentTime, unknownUnit);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests GetWindowStart with unknown RateLimitUnit - should throw ArgumentException.
    /// </summary>
    [Fact]
    public void GetWindowStart_UnknownUnit_ThrowsArgumentException()
    {
        // Arrange
        var currentTime = DateTime.UtcNow;
        var unknownUnit = (RateLimitUnit)999;

        // Act
        Action act = () => RateLimitCalculationHelper.GetWindowStart(currentTime, unknownUnit);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests GetSecondsUntilAllowed with negative secondsUntilReset - should return 0.
    /// </summary>
    [Fact]
    public void GetSecondsUntilAllowed_NegativeSecondsUntilReset_ReturnsZero()
    {
        // This test verifies the Math.Max(0, secondsUntilReset) behavior
        // Arrange
        var currentUsage = 15;
        var limit = 10;
        var windowStart = DateTime.UtcNow.AddMinutes(-10); // Window already expired
        var unit = RateLimitUnit.Minute;

        // Act
        var result = RateLimitCalculationHelper.GetSecondsUntilAllowed(currentUsage, limit, windowStart, unit);

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Tests CalculateQuotagePercentage with large numbers.
    /// </summary>
    [Fact]
    public void CalculateQuotagePercentage_LargeNumbers_ReturnsCorrectValue()
    {
        // Arrange
        var currentUsage = 7500;
        var limit = 10000;

        // Act
        var result = RateLimitCalculationHelper.CalculateQuotagePercentage(currentUsage, limit);

        // Assert
        result.Should().Be(75);
    }
}
