using System;
using Xunit;
using FluentAssertions;
using ApiKeyGateway.Utilities;

namespace ApiKeyGateway.Tests;

public class DateTimeExtensionsTests
{
    [Fact]
    public void StartOfDay_ShouldReturnMidnightOfSameDay()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15, 14, 30, 45);
        var expected = new DateTime(2024, 6, 15, 0, 0, 0);

        // Act
        var result = date.StartOfDay();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StartOfDay_ShouldReturnSameDateForDateOnly()
    {
        // Arrange
        var date = new DateTime(2024, 12, 31);
        var expected = new DateTime(2024, 12, 31, 0, 0, 0);

        // Act
        var result = date.StartOfDay();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StartOfDay_ShouldReturnSameDateForLeapYear()
    {
        // Arrange
        var date = new DateTime(2024, 2, 29, 10, 15, 20);
        var expected = new DateTime(2024, 2, 29, 0, 0, 0);

        // Act
        var result = date.StartOfDay();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void EndOfDay_ShouldReturnEndOfDay()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15, 14, 30, 45);
        var expected = new DateTime(2024, 6, 15, 23, 59, 59, 999);

        // Act
        var result = date.EndOfDay();

        // Assert - Use millisecond precision for comparison
        result.Should().Be(expected);
    }

    [Fact]
    public void EndOfDay_ShouldReturnCorrectEndForDateOnly()
    {
        // Arrange
        var date = new DateTime(2024, 12, 31);
        var expected = new DateTime(2024, 12, 31, 23, 59, 59, 999);

        // Act
        var result = date.EndOfDay();

        // Assert - Use millisecond precision for comparison
        result.Should().Be(expected);
    }

    [Fact]
    public void EndOfDay_ShouldReturnCorrectEndForLeapYear()
    {
        // Arrange
        var date = new DateTime(2024, 2, 29);
        var expected = new DateTime(2024, 2, 29, 23, 59, 59, 999);

        // Act
        var result = date.EndOfDay();

        // Assert - Use millisecond precision for comparison
        result.Should().Be(expected);
    }

    [Fact]
    public void StartOfWeek_ShouldReturnMondayOfSameWeek()
    {
        // Arrange - Saturday June 15, 2024 (Sunday is first day of week in en-US)
        var date = new DateTime(2024, 6, 15);
        var expected = new DateTime(2024, 6, 9); // Sunday

        // Act
        var result = date.StartOfWeek();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StartOfWeek_ShouldReturnSundayForSunday()
    {
        // Arrange - Sunday June 16, 2024 (Sunday is first day of week in en-US)
        var date = new DateTime(2024, 6, 16);
        var expected = new DateTime(2024, 6, 16); // Sunday

        // Act
        var result = date.StartOfWeek();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StartOfWeek_ShouldReturnSundayForMonday()
    {
        // Arrange - Monday June 10, 2024 (Sunday is first day of week in en-US)
        var date = new DateTime(2024, 6, 10);
        var expected = new DateTime(2024, 6, 9); // Sunday

        // Act
        var result = date.StartOfWeek();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StartOfWeek_ShouldReturnSundayForFriday()
    {
        // Arrange - Friday June 14, 2024 (Sunday is first day of week in en-US)
        var date = new DateTime(2024, 6, 14);
        var expected = new DateTime(2024, 6, 9); // Sunday

        // Act
        var result = date.StartOfWeek();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StartOfMonth_ShouldReturnFirstDayOfMonth()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15);
        var expected = new DateTime(2024, 6, 1);

        // Act
        var result = date.StartOfMonth();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StartOfMonth_ShouldReturnFirstDayForFirstDayOfMonth()
    {
        // Arrange
        var date = new DateTime(2024, 1, 1);
        var expected = new DateTime(2024, 1, 1);

        // Act
        var result = date.StartOfMonth();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StartOfMonth_ShouldReturnFirstDayForLastDayOfMonth()
    {
        // Arrange
        var date = new DateTime(2024, 1, 31);
        var expected = new DateTime(2024, 1, 1);

        // Act
        var result = date.StartOfMonth();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void EndOfMonth_ShouldReturnLastMomentOfMonth()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15);
        var expected = new DateTime(2024, 6, 30, 23, 59, 59, 999);

        // Act
        var result = date.EndOfMonth();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void EndOfMonth_ShouldReturnLastMomentForFebruaryNonLeapYear()
    {
        // Arrange
        var date = new DateTime(2023, 2, 15);
        var expected = new DateTime(2023, 2, 28, 23, 59, 59, 999);

        // Act
        var result = date.EndOfMonth();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void EndOfMonth_ShouldReturnLastMomentForFebruaryLeapYear()
    {
        // Arrange
        var date = new DateTime(2024, 2, 15);
        var expected = new DateTime(2024, 2, 29, 23, 59, 59, 999);

        // Act
        var result = date.EndOfMonth();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void EndOfMonth_ShouldReturnLastMomentForDecember()
    {
        // Arrange
        var date = new DateTime(2024, 12, 15);
        var expected = new DateTime(2024, 12, 31, 23, 59, 59, 999);

        // Act
        var result = date.EndOfMonth();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsInPast_ShouldReturnTrueForPastDate()
    {
        // Arrange - Use a date far in the past
        var pastDate = DateTime.UtcNow.AddDays(-10);

        // Act
        var result = pastDate.IsInPast();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInPast_ShouldReturnFalseForFutureDate()
    {
        // Arrange - Use a date far in the future
        var futureDate = DateTime.UtcNow.AddDays(10);

        // Act
        var result = futureDate.IsInPast();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsInPast_ShouldReturnFalseForCurrentDate()
    {
        // Arrange - Use current date (add 1 second to ensure it's not in the past)
        var currentDate = DateTime.UtcNow.AddSeconds(1);

        // Act
        var result = currentDate.IsInPast();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsInFuture_ShouldReturnTrueForFutureDate()
    {
        // Arrange - Use a date far in the future
        var futureDate = DateTime.UtcNow.AddDays(10);

        // Act
        var result = futureDate.IsInFuture();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInFuture_ShouldReturnFalseForPastDate()
    {
        // Arrange - Use a date far in the past
        var pastDate = DateTime.UtcNow.AddDays(-10);

        // Act
        var result = pastDate.IsInFuture();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsInFuture_ShouldReturnFalseForCurrentDate()
    {
        // Arrange - Use current date (subtract 1 second to ensure it's not in the future)
        var currentDate = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var result = currentDate.IsInFuture();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DaysUntil_ShouldReturnPositiveForFutureDate()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(5);
        var expected = 5;

        // Act
        var result = futureDate.DaysUntil();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void DaysUntil_ShouldReturnNegativeForPastDate()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.Date.AddDays(-5);
        var expected = -5;

        // Act
        var result = pastDate.DaysUntil();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void DaysUntil_ShouldReturnZeroForSameDate()
    {
        // Arrange
        var currentDate = DateTime.UtcNow.Date;
        var expected = 0;

        // Act
        var result = currentDate.DaysUntil();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void DaysUntil_ShouldReturnZeroForTomorrow()
    {
        // Arrange
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var expected = 1;

        // Act
        var result = tomorrow.DaysUntil();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void DaysUntil_ShouldReturnNegativeOneForYesterday()
    {
        // Arrange
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var expected = -1;

        // Act
        var result = yesterday.DaysUntil();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToHumanReadableTime_ShouldReturnJustNowForCurrentTime()
    {
        // Arrange - Use a date very close to now (within 1 second)
        var now = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var result = now.ToHumanReadableTime();

        // Assert - Should return "just now" for very recent times
        result.Should().Be("just now");
    }

    [Fact]
    public void ToHumanReadableTime_ShouldReturnMinutesAgo()
    {
        // Arrange - A date 45 minutes in the past
        var minutesAgo = DateTime.UtcNow.AddMinutes(-45);

        // Act
        var result = minutesAgo.ToHumanReadableTime();

        // Assert - Should return a string ending with "ago"
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith(" ago");
    }

    [Fact]
    public void ToHumanReadableTime_ShouldReturnHoursAgo()
    {
        // Arrange - A date 5 hours in the past
        var hoursAgo = DateTime.UtcNow.AddHours(-5);

        // Act
        var result = hoursAgo.ToHumanReadableTime();

        // Assert - Should return a string ending with "ago"
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith(" ago");
    }

    [Fact]
    public void ToHumanReadableTime_ShouldReturnDaysAgo()
    {
        // Arrange - A date 10 days in the past
        var daysAgo = DateTime.UtcNow.AddDays(-10);

        // Act
        var result = daysAgo.ToHumanReadableTime();

        // Assert - Should return a string ending with "ago"
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith(" ago");
    }

    [Fact]
    public void ToHumanReadableTime_ShouldReturnMonthsAgo()
    {
        // Arrange - A date 45 days in the past
        var monthsAgo = DateTime.UtcNow.AddDays(-45);

        // Act
        var result = monthsAgo.ToHumanReadableTime();

        // Assert - Should return a string ending with "ago"
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith(" ago");
    }

    [Fact]
    public void ToHumanReadableTime_ShouldHandleUtcNowBoundary()
    {
        // Arrange - Less than 60 seconds ago
        var thirtySecondsAgo = DateTime.UtcNow.AddSeconds(-30);

        // Act
        var result = thirtySecondsAgo.ToHumanReadableTime();

        // Assert - Should return "just now" for recent times
        result.Should().Be("just now");
    }

    [Fact]
    public void ToHumanReadableTime_ShouldHandleMultipleHours()
    {
        // Arrange - 23 hours ago
        var hoursAgo = DateTime.UtcNow.AddHours(-23);

        // Act
        var result = hoursAgo.ToHumanReadableTime();

        // Assert - Should return a string ending with "ago"
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith(" ago");
    }

    [Fact]
    public void ToHumanReadableTime_ShouldHandleMultipleDays()
    {
        // Arrange - 29 days ago
        var daysAgo = DateTime.UtcNow.AddDays(-29);

        // Act
        var result = daysAgo.ToHumanReadableTime();

        // Assert - Should return a string ending with "ago"
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith(" ago");
    }

    [Fact]
    public void StartOfDay_ShouldHandleMinValue()
    {
        // Arrange
        var minDate = DateTime.MinValue;
        var expected = DateTime.MinValue;

        // Act
        var result = minDate.StartOfDay();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StartOfDay_ShouldHandleMaxValue()
    {
        // Arrange
        var maxDate = DateTime.MaxValue;
        var expected = new DateTime(DateTime.MaxValue.Year, DateTime.MaxValue.Month, DateTime.MaxValue.Day);

        // Act
        var result = maxDate.StartOfDay();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void EndOfDay_ShouldHandleRegularDate()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15, 14, 30, 45);
        var expected = new DateTime(2024, 6, 15, 23, 59, 59, 999);

        // Act
        var result = date.EndOfDay();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StartOfWeek_ShouldHandleMinValue()
    {
        // Arrange
        var minDate = DateTime.MinValue;

        // Act
        var result = minDate.StartOfWeek();

        // Assert - Should return a valid date
        result.Should().BeOnOrBefore(minDate);
        result.DayOfWeek.Should().Be(DayOfWeek.Sunday);
    }

    [Fact]
    public void StartOfWeek_ShouldHandleMaxValue()
    {
        // Arrange
        var maxDate = DateTime.MaxValue;

        // Act
        var result = maxDate.StartOfWeek();

        // Assert - Should return a valid date
        result.Should().BeOnOrBefore(maxDate);
    }

    [Fact]
    public void StartOfMonth_ShouldHandleMinValue()
    {
        // Arrange
        var minDate = DateTime.MinValue;
        var expected = new DateTime(1, 1, 1);

        // Act
        var result = minDate.StartOfMonth();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StartOfMonth_ShouldHandleMaxValue()
    {
        // Arrange
        var maxDate = DateTime.MaxValue;
        var expected = new DateTime(DateTime.MaxValue.Year, DateTime.MaxValue.Month, 1);

        // Act
        var result = maxDate.StartOfMonth();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void EndOfMonth_ShouldHandleRegularDates()
    {
        // Arrange - test with a regular date
        var date = new DateTime(2024, 6, 15);
        var expected = new DateTime(2024, 6, 30, 23, 59, 59, 999);

        // Act
        var result = date.EndOfMonth();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsInPast_ShouldReturnTrueForDateTimeMinValue()
    {
        // Arrange
        var minDate = DateTime.MinValue;

        // Act
        var result = minDate.IsInPast();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInPast_ShouldReturnFalseForDateTimeMaxValue()
    {
        // Arrange
        var maxDate = DateTime.MaxValue;

        // Act
        var result = maxDate.IsInPast();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsInFuture_ShouldReturnFalseForDateTimeMinValue()
    {
        // Arrange
        var minDate = DateTime.MinValue;

        // Act
        var result = minDate.IsInFuture();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsInFuture_ShouldReturnTrueForDateTimeMaxValue()
    {
        // Arrange
        var maxDate = DateTime.MaxValue;

        // Act
        var result = maxDate.IsInFuture();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DaysUntil_ShouldReturnLargePositiveForFarFutureDate()
    {
        // Arrange
        var farFutureDate = DateTime.UtcNow.Date.AddYears(100);

        // Act
        var result = farFutureDate.DaysUntil();

        // Assert
        result.Should().BeGreaterThan(36000);
    }

    [Fact]
    public void DaysUntil_ShouldReturnLargeNegativeForFarPastDate()
    {
        // Arrange
        var farPastDate = DateTime.UtcNow.Date.AddYears(-100);

        // Act
        var result = farPastDate.DaysUntil();

        // Assert
        result.Should().BeLessThan(-36000);
    }

    [Fact]
    public void ToHumanReadableTime_ShouldReturnJustNowForExactBoundary()
    {
        // Arrange - Exactly 59 seconds ago (boundary for "just now")
        var fiftyNineSecondsAgo = DateTime.UtcNow.AddSeconds(-59);

        // Act
        var result = fiftyNineSecondsAgo.ToHumanReadableTime();

        // Assert
        result.Should().Be("just now");
    }

    [Fact]
    public void ToHumanReadableTime_ShouldReturnMinutesForSixtySecondsAgo()
    {
        // Arrange - Exactly 60 seconds ago (boundary for minutes)
        var sixtySecondsAgo = DateTime.UtcNow.AddSeconds(-60);

        // Act
        var result = sixtySecondsAgo.ToHumanReadableTime();

        // Assert
        result.Should().NotBe("just now");
        result.Should().Contain("m ago");
    }

    [Fact]
    public void ToHumanReadableTime_ShouldReturnHoursForHours()
    {
        // Arrange - 5 hours ago
        var fiveHoursAgo = DateTime.UtcNow.AddHours(-5);

        // Act
        var result = fiveHoursAgo.ToHumanReadableTime();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("h ago");
    }

    [Fact]
    public void ToHumanReadableTime_ShouldReturnDaysForDays()
    {
        // Arrange - 10 days ago
        var tenDaysAgo = DateTime.UtcNow.AddDays(-10);

        // Act
        var result = tenDaysAgo.ToHumanReadableTime();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("d ago");
    }

    [Fact]
    public void ToHumanReadableTime_ShouldReturnMonthsForLargeMonthValue()
    {
        // Arrange - 45 days ago (boundary for months)
        var fortyFiveDaysAgo = DateTime.UtcNow.AddDays(-45);

        // Act
        var result = fortyFiveDaysAgo.ToHumanReadableTime();

        // Assert
        result.Should().Contain("mo ago");
    }
}
