using System;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using Xunit;

namespace api_key_gateway.Tests
{
    public class RateLimitTests
    {
        private RateLimit CreateDefaultRateLimit(
            int requestsPerUnit = 5,
            RateLimitUnit unit = RateLimitUnit.Minute,
            bool isEnabled = true)
        {
            return new RateLimit
            {
                Id = Guid.NewGuid().ToString(),
                ApiKeyId = Guid.NewGuid().ToString(),
                RequestsPerUnit = requestsPerUnit,
                Unit = unit,
                IsEnabled = isEnabled,
                CreatedAt = DateTime.UtcNow,
                CurrentRequestCount = 0,
                LastResetAt = null
            };
        }

        [Theory]
        [InlineData(RateLimitUnit.Second, 1)]
        [InlineData(RateLimitUnit.Minute, 60)]
        [InlineData(RateLimitUnit.Hour, 3600)]
        [InlineData(RateLimitUnit.Day, 86400)]
        [InlineData(RateLimitUnit.Unlimited, int.MaxValue)]
        public void GetWindowInSeconds_ReturnsExpectedSeconds(RateLimitUnit unit, int expectedSeconds)
        {
            // Arrange
            var rateLimit = CreateDefaultRateLimit(unit: unit);

            // Act
            var seconds = rateLimit.GetWindowInSeconds();

            // Assert
            Assert.Equal(expectedSeconds, seconds);
        }

        [Fact]
        public void CanProcessRequest_ReturnsTrue_WhenBelowLimitAndEnabled()
        {
            // Arrange
            var rateLimit = CreateDefaultRateLimit(requestsPerUnit: 3);
            rateLimit.RecordRequest(); // count = 1
            rateLimit.RecordRequest(); // count = 2

            // Act
            var canProcess = rateLimit.CanProcessRequest();

            // Assert
            Assert.True(canProcess);
        }

        [Fact]
        public void CanProcessRequest_ReturnsFalse_WhenAtOrAboveLimit()
        {
            // Arrange
            var rateLimit = CreateDefaultRateLimit(requestsPerUnit: 2);
            rateLimit.RecordRequest(); // count = 1
            rateLimit.RecordRequest(); // count = 2 (reached limit)

            // Act
            var canProcess = rateLimit.CanProcessRequest();

            // Assert
            Assert.False(canProcess);
        }

        [Fact]
        public void CanProcessRequest_AlwaysTrue_WhenDisabledOrUnlimited()
        {
            // Disabled case
            var disabled = CreateDefaultRateLimit(isEnabled: false);
            disabled.RecordRequest(); // count increments but should be ignored
            Assert.True(disabled.CanProcessRequest());

            // Unlimited case
            var unlimited = CreateDefaultRateLimit(unit: RateLimitUnit.Unlimited);
            unlimited.RecordRequest(); // count increments but should be ignored
            Assert.True(unlimited.CanProcessRequest());
        }

        [Fact]
        public void RecordRequest_IncrementsCurrentRequestCount_OnlyWhenEnabledAndLimited()
        {
            // Enabled & limited
            var limited = CreateDefaultRateLimit();
            limited.RecordRequest();
            Assert.Equal(1, limited.CurrentRequestCount);

            // Disabled
            var disabled = CreateDefaultRateLimit(isEnabled: false);
            disabled.RecordRequest();
            Assert.Equal(0, disabled.CurrentRequestCount);

            // Unlimited unit
            var unlimited = CreateDefaultRateLimit(unit: RateLimitUnit.Unlimited);
            unlimited.RecordRequest();
            Assert.Equal(0, unlimited.CurrentRequestCount);
        }

        [Fact]
        public void ResetWindow_ClearsCounter_AndUpdatesLastResetAt()
        {
            // Arrange
            var rateLimit = CreateDefaultRateLimit();
            rateLimit.RecordRequest();
            rateLimit.RecordRequest();
            Assert.Equal(2, rateLimit.CurrentRequestCount);
            Assert.Null(rateLimit.LastResetAt);

            // Act
            rateLimit.ResetWindow();

            // Assert
            Assert.Equal(0, rateLimit.CurrentRequestCount);
            Assert.NotNull(rateLimit.LastResetAt);
            // The reset timestamp should be recent (within the last second)
            Assert.InRange(rateLimit.LastResetAt.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow);
        }

        [Fact]
        public void EdgeCase_ZeroRequestsPerUnit_AlwaysBlocksWhenEnabled()
        {
            // Arrange
            var rateLimit = CreateDefaultRateLimit(requestsPerUnit: 0);

            // Act
            var canProcess = rateLimit.CanProcessRequest();

            // Assert
            Assert.False(canProcess);
        }
    }
}
