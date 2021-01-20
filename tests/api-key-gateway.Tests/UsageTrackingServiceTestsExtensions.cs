using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;

namespace ApiKeyGateway.Tests
{
    /// <summary>
    /// Extension methods for <see cref="UsageTrackingServiceTests"/> to simplify common test scenarios
    /// and improve test readability.
    /// </summary>
    public static class UsageTrackingServiceTestsExtensions
    {
        /// <summary>
        /// Creates a valid usage record for testing purposes.
        /// </summary>
        /// <param name="keyId">The API key identifier</param>
        /// <param name="bytesTransferred">Number of bytes transferred</param>
        /// <param name="timestamp">Optional timestamp (defaults to UtcNow)</param>
        /// <param name="responseStatusCode">HTTP status code (defaults to 200)</param>
        /// <returns>A new UsageRecord instance</returns>
        public static UsageRecord CreateTestUsageRecord(
            this UsageTrackingServiceTests _,
            string keyId = "key-123",
            long bytesTransferred = 1024,
            DateTime? timestamp = null,
            int responseStatusCode = 200)
        {
            ArgumentException.ThrowIfNullOrEmpty(keyId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bytesTransferred);

            return new UsageRecord
            {
                ApiKeyId = keyId,
                BytesTransferred = bytesTransferred,
                RequestTimestampUtc = timestamp ?? DateTime.UtcNow,
                ResponseStatusCode = responseStatusCode
            };
        }

        /// <summary>
        /// Creates a collection of usage records spanning a date range for testing aggregation.
        /// </summary>
        /// <param name="keyId">The API key identifier</param>
        /// <param name="startDate">Start date of the range</param>
        /// <param name="endDate">End date of the range</param>
        /// <param name="dailyRecords">Number of daily records to generate</param>
        /// <param name="bytesPerDay">Bytes transferred per day</param>
        /// <returns>Collection of usage records</returns>
        public static IReadOnlyList<UsageRecord> CreateUsageRecordsForDateRange(
            this UsageTrackingServiceTests _,
            string keyId,
            DateTime startDate,
            DateTime endDate,
            int dailyRecords = 7,
            long bytesPerDay = 1024)
        {
            ArgumentException.ThrowIfNullOrEmpty(keyId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dailyRecords);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bytesPerDay);

            if (endDate < startDate)
            {
                throw new ArgumentException("End date must be greater than or equal to start date", nameof(endDate));
            }

            var records = new List<UsageRecord>(dailyRecords);
            var currentDate = startDate.Date;

            for (int i = 0; i < dailyRecords && currentDate <= endDate.Date; i++, currentDate = currentDate.AddDays(1))
            {
                records.Add(new UsageRecord
                {
                    ApiKeyId = keyId,
                    BytesTransferred = bytesPerDay,
                    RequestTimestampUtc = currentDate.AddHours(12), // Mid-day timestamp
                    ResponseStatusCode = 200
                });
            }

            return records.AsReadOnly();
        }

        /// <summary>
        /// Asserts that two usage statistics objects are equivalent within a tolerance.
        /// </summary>
        /// <param name="expected">Expected statistics</param>
        /// <param name="actual">Actual statistics</param>
        /// <param name="tolerancePercent">Allowed percentage difference (default: 0.01%)</param>
        public static void ShouldBeEquivalentTo(
            this UsageTrackingServiceTests _,
            UsageStatistics expected,
            UsageStatistics actual,
            double tolerancePercent = 0.01)
        {
            Assert.NotNull(expected);
            Assert.NotNull(actual);

            Assert.Equal(expected.ApiKeyId, actual.ApiKeyId, StringComparer.Ordinal);
            Assert.Equal(expected.StartDate, actual.StartDate);
            Assert.Equal(expected.EndDate, actual.EndDate);

            var totalTolerance = expected.TotalRequests * (tolerancePercent / 100.0);
            Assert.InRange(actual.TotalRequests, expected.TotalRequests - (long)totalTolerance, expected.TotalRequests + (long)totalTolerance);

            var successTolerance = expected.SuccessfulRequests * (tolerancePercent / 100.0);
            Assert.InRange(actual.SuccessfulRequests, expected.SuccessfulRequests - (long)successTolerance, expected.SuccessfulRequests + (long)successTolerance);

            var failedTolerance = expected.FailedRequests * (tolerancePercent / 100.0);
            Assert.InRange(actual.FailedRequests, expected.FailedRequests - (long)failedTolerance, expected.FailedRequests + (long)failedTolerance);

            if (expected.TotalRequests > 0)
            {
                var avgTimeTolerance = expected.AverageResponseTimeMs * (tolerancePercent / 100.0);
                Assert.InRange(actual.AverageResponseTimeMs, expected.AverageResponseTimeMs - avgTimeTolerance, expected.AverageResponseTimeMs + avgTimeTolerance);

                var successRateTolerance = expected.SuccessRate * (tolerancePercent / 100.0);
                Assert.InRange(actual.SuccessRate, expected.SuccessRate - successRateTolerance, expected.SuccessRate + successRateTolerance);
            }
            else
            {
                Assert.Equal(0, actual.AverageResponseTimeMs);
                Assert.Equal(0, actual.SuccessRate);
            }
        }

        /// <summary>
        /// Creates a test statistics object with pre-calculated aggregates.
        /// </summary>
        /// <param name="keyId">The API key identifier</param>
        /// <param name="totalRequests">Total number of requests</param>
        /// <param name="successfulRequests">Number of successful requests</param>
        /// <param name="failedRequests">Number of failed requests</param>
        /// <param name="startDate">Start of period</param>
        /// <param name="endDate">End of period</param>
        /// <param name="averageResponseTimeMs">Average response time in milliseconds</param>
        /// <returns>UsageStatistics instance</returns>
        public static UsageStatistics CreateTestStatistics(
            this UsageTrackingServiceTests _,
            string keyId,
            int totalRequests,
            int successfulRequests,
            int failedRequests,
            DateTime startDate,
            DateTime endDate,
            double averageResponseTimeMs = 0)
        {
            ArgumentException.ThrowIfNullOrEmpty(keyId);
            ArgumentOutOfRangeException.ThrowIfNegative(totalRequests);
            ArgumentOutOfRangeException.ThrowIfNegative(successfulRequests);
            ArgumentOutOfRangeException.ThrowIfNegative(failedRequests);
            ArgumentOutOfRangeException.ThrowIfNegative(averageResponseTimeMs);

            if (endDate < startDate)
            {
                throw new ArgumentException("Period end must be greater than or equal to period start", nameof(endDate));
            }

            return new UsageStatistics
            {
                ApiKeyId = keyId,
                StartDate = startDate,
                EndDate = endDate,
                TotalRequests = totalRequests,
                SuccessfulRequests = successfulRequests,
                FailedRequests = failedRequests,
                AverageResponseTimeMs = averageResponseTimeMs
            };
        }
    }
}