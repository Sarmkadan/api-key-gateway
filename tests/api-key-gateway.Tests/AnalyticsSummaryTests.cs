using System;
using ApiKeyGateway.Services;
using Xunit;

namespace api_key_gateway.Tests
{
    public class AnalyticsSummaryTests
    {
        [Fact]
        public void DefaultConstructor_ShouldInitializePropertiesWithDefaults()
        {
            var summary = new AnalyticsSummary();

            Assert.Equal(string.Empty, summary.ApiKeyId);
            Assert.Equal(default(DateTime), summary.From);
            Assert.Equal(default(DateTime), summary.To);
            Assert.Equal(0, summary.TotalRequests);
            Assert.Equal(0, summary.SuccessfulRequests);
            Assert.Equal(0, summary.FailedRequests);
            Assert.Equal(0.0, summary.SuccessRatePercent);
            Assert.Equal(0.0, summary.ErrorRatePercent);
            Assert.Equal(0.0, summary.AverageResponseTimeMs);
            Assert.Equal(0L, summary.TotalBytesTransferred);
        }

        [Fact]
        public void PropertyAssignments_ShouldPersistValues()
        {
            var summary = new AnalyticsSummary
            {
                ApiKeyId = "key123",
                From = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                To = new DateTime(2023, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                TotalRequests = 100,
                SuccessfulRequests = 80,
                FailedRequests = 20,
                SuccessRatePercent = 80.0,
                ErrorRatePercent = 20.0,
                AverageResponseTimeMs = 123.45,
                TotalBytesTransferred = 9_876_543_210
            };

            Assert.Equal("key123", summary.ApiKeyId);
            Assert.Equal(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), summary.From);
            Assert.Equal(new DateTime(2023, 1, 2, 0, 0, 0, DateTimeKind.Utc), summary.To);
            Assert.Equal(100, summary.TotalRequests);
            Assert.Equal(80, summary.SuccessfulRequests);
            Assert.Equal(20, summary.FailedRequests);
            Assert.Equal(80.0, summary.SuccessRatePercent);
            Assert.Equal(20.0, summary.ErrorRatePercent);
            Assert.Equal(123.45, summary.AverageResponseTimeMs);
            Assert.Equal(9_876_543_210L, summary.TotalBytesTransferred);
        }

        [Fact]
        public void ObjectInitializer_ShouldSetAllProperties()
        {
            var now = DateTime.UtcNow;
            var summary = new AnalyticsSummary
            {
                ApiKeyId = "abc",
                From = now,
                To = now.AddHours(1),
                TotalRequests = 10,
                SuccessfulRequests = 7,
                FailedRequests = 3,
                SuccessRatePercent = 70.0,
                ErrorRatePercent = 30.0,
                AverageResponseTimeMs = 200.5,
                TotalBytesTransferred = 5_000
            };

            Assert.Equal("abc", summary.ApiKeyId);
            Assert.Equal(now, summary.From);
            Assert.Equal(now.AddHours(1), summary.To);
            Assert.Equal(10, summary.TotalRequests);
            Assert.Equal(7, summary.SuccessfulRequests);
            Assert.Equal(3, summary.FailedRequests);
            Assert.Equal(70.0, summary.SuccessRatePercent);
            Assert.Equal(30.0, summary.ErrorRatePercent);
            Assert.Equal(200.5, summary.AverageResponseTimeMs);
            Assert.Equal(5_000L, summary.TotalBytesTransferred);
        }

        [Fact]
        public void SettingNullApiKeyId_ShouldAllowNull()
        {
            var summary = new AnalyticsSummary { ApiKeyId = null };
            Assert.Null(summary.ApiKeyId);
        }

        [Fact]
        public void NegativeValues_ShouldBeStored()
        {
            var summary = new AnalyticsSummary
            {
                TotalRequests = -1,
                SuccessfulRequests = -5,
                FailedRequests = -2,
                SuccessRatePercent = -10.0,
                ErrorRatePercent = -20.0,
                AverageResponseTimeMs = -100.0,
                TotalBytesTransferred = -123
            };

            Assert.Equal(-1, summary.TotalRequests);
            Assert.Equal(-5, summary.SuccessfulRequests);
            Assert.Equal(-2, summary.FailedRequests);
            Assert.Equal(-10.0, summary.SuccessRatePercent);
            Assert.Equal(-20.0, summary.ErrorRatePercent);
            Assert.Equal(-100.0, summary.AverageResponseTimeMs);
            Assert.Equal(-123L, summary.TotalBytesTransferred);
        }
    }
}
