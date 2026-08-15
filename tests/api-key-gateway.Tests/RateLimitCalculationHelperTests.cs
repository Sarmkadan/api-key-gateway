using Xunit;
using ApiKeyGateway.Utilities;
using ApiKeyGateway.Domain.Enums;

namespace api_key_gateway.Tests
{
    public class RateLimitCalculationHelperTests
    {
        [Fact]
        public void GetWindowEnd_ReturnsCorrectEndForMinute()
        {
            var time = new DateTime(2023, 10, 27, 14, 23, 45);
            var result = RateLimitCalculationHelper.GetWindowEnd(time, RateLimitUnit.Minute);
            Assert.Equal(new DateTime(2023, 10, 27, 14, 24, 0), result);
        }

        [Fact]
        public void GetWindowStart_ReturnsCorrectStartForHour()
        {
            var time = new DateTime(2023, 10, 27, 14, 23, 45);
            var result = RateLimitCalculationHelper.GetWindowStart(time, RateLimitUnit.Hour);
            Assert.Equal(new DateTime(2023, 10, 27, 14, 0, 0), result);
        }

        [Fact]
        public void GetSecondsUntilAllowed_ReturnsMaxValueWhenLimitExceeded()
        {
            var windowStart = DateTime.UtcNow;
            var result = RateLimitCalculationHelper.GetSecondsUntilAllowed(10, 10, windowStart, RateLimitUnit.Minute);
            Assert.Equal(int.MaxValue, result);
        }

        [Fact]
        public void GetSecondsUntilAllowed_ReturnsPositiveValueWhenAllowed()
        {
            var windowStart = DateTime.UtcNow;
            var result = RateLimitCalculationHelper.GetSecondsUntilAllowed(5, 10, windowStart, RateLimitUnit.Minute);
            Assert.True(result > 0);
        }

        [Fact]
        public void CalculateQuotagePercentage_ReturnsZeroIfLimitIsZero()
        {
            var result = RateLimitCalculationHelper.CalculateQuotagePercentage(10, 0);
            Assert.Equal(0, result);
        }

        [Theory]
        [InlineData(79, false)]
        [InlineData(80, true)]
        [InlineData(100, true)]
        public void ShouldWarnAboutLimit_ReturnsExpected(int percentage, bool expected)
        {
            var result = RateLimitCalculationHelper.ShouldWarnAboutLimit(percentage);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetReadableResetTime_ReturnsImmediatelyIfPast()
        {
            var past = DateTime.UtcNow.AddMinutes(-1);
            var result = RateLimitCalculationHelper.GetReadableResetTime(past);
            Assert.Equal("immediately", result);
        }
    }
}
