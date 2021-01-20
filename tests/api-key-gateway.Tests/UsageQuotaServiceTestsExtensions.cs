using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using ApiKeyGateway.Tests;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests
{
    /// <summary>
    /// Extension methods for <see cref="UsageQuotaServiceTests"/> to simplify common test scenarios.
    /// </summary>
    public static class UsageQuotaServiceTestsExtensions
    {
        /// <summary>
        /// Creates a mock <see cref="IUsageQuotaRepository"/> with the specified quota values.
        /// </summary>
        /// <param name="quotaLimit">The quota limit to set, or null for unlimited.</param>
        /// <param name="currentUsage">The current usage count.</param>
        /// <param name="periodStart">The period start date.</param>
        /// <returns>A configured mock repository.</returns>
        public static Mock<IUsageQuotaRepository> SetupQuotaRepository(
            this UsageQuotaServiceTests _,
            long? quotaLimit = null,
            long currentUsage = 0,
            DateTime? periodStart = null)
        {
            ArgumentNullException.ThrowIfNull(_);

            var mockRepo = new Mock<IUsageQuotaRepository>();

            if (quotaLimit.HasValue)
            {
                var quota = new UsageQuota
                {
                    ApiKeyId = "test-key",
                    QuotaLimit = quotaLimit.Value,
                    CurrentUsage = currentUsage,
                    PeriodStartAt = periodStart ?? DateTime.UtcNow.Date,
                    IsEnabled = true
                };

                mockRepo
                    .Setup(r => r.GetByApiKeyIdAsync("test-key"))
                    .ReturnsAsync(quota);

                mockRepo
                    .Setup(r => r.CreateAsync(It.IsAny<UsageQuota>()))
                    .ReturnsAsync(quota);

                mockRepo
                    .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
                    .Returns(Task.CompletedTask);
            }
            else
            {
                mockRepo
                    .Setup(r => r.GetByApiKeyIdAsync("test-key"))
                    .ReturnsAsync((UsageQuota)null);
            }

            return mockRepo;
        }

        /// <summary>
        /// Creates a <see cref="UsageQuotaService"/> instance with the specified dependencies.
        /// </summary>
        /// <param name="mockRepo">Mock repository instance.</param>
        /// <param name="mockLogger">Mock logger instance.</param>
        /// <returns>A configured <see cref="UsageQuotaService"/> instance.</returns>
        public static UsageQuotaService CreateQuotaService(
            this UsageQuotaServiceTests _,
            Mock<IUsageQuotaRepository> mockRepo,
            Mock<ILogger<UsageQuotaService>> mockLogger = null)
        {
            ArgumentNullException.ThrowIfNull(_);
            ArgumentNullException.ThrowIfNull(mockRepo);

            var logger = mockLogger?.Object ?? new Mock<ILogger<UsageQuotaService>>().Object;
            return new UsageQuotaService(mockRepo.Object, logger);
        }

        /// <summary>
        /// Verifies that a quota was set with the expected values.
        /// </summary>
        /// <param name="mockRepo">The mock repository to verify.</param>
        /// <param name="expectedApiKeyId">Expected API key identifier.</param>
        /// <param name="expectedLimit">Expected quota limit.</param>
        public static void VerifyQuotaSet(
            this UsageQuotaServiceTests _,
            Mock<IUsageQuotaRepository> mockRepo,
            string expectedApiKeyId,
            long expectedLimit)
        {
            ArgumentNullException.ThrowIfNull(_);
            ArgumentNullException.ThrowIfNull(mockRepo);

            mockRepo.Verify(
                r => r.CreateAsync(It.Is<UsageQuota>(q =>
                    q.ApiKeyId == expectedApiKeyId &&
                    q.QuotaLimit == expectedLimit)),
                Times.Once);
        }

        /// <summary>
        /// Verifies that a quota was retrieved with the expected values.
        /// </summary>
        /// <param name="mockRepo">The mock repository to verify.</param>
        /// <param name="expectedApiKeyId">Expected API key identifier.</param>
        public static void VerifyQuotaGet(
            this UsageQuotaServiceTests _,
            Mock<IUsageQuotaRepository> mockRepo,
            string expectedApiKeyId)
        {
            ArgumentNullException.ThrowIfNull(_);
            ArgumentNullException.ThrowIfNull(mockRepo);

            mockRepo.Verify(
                r => r.GetByApiKeyIdAsync(expectedApiKeyId),
                Times.Once);
        }

        /// <summary>
        /// Creates a collection of quota keys for testing batch operations.
        /// </summary>
        /// <param name="count">Number of keys to generate.</param>
        /// <param name="quotaLimit">Quota limit for each key.</param>
        /// <returns>A dictionary mapping key IDs to quota values.</returns>
        public static IReadOnlyDictionary<string, long> CreateQuotaKeys(
            this UsageQuotaServiceTests _,
            int count,
            long quotaLimit)
        {
            ArgumentNullException.ThrowIfNull(_);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(count, 0);

            return Enumerable.Range(1, count)
                .ToDictionary(
                    i => $"key-{i:D6}",
                    _ => quotaLimit);
        }

        /// <summary>
        /// Parses a quota limit from a string value.
        /// </summary>
        /// <param name="quotaString">String representation of quota limit.</param>
        /// <returns>The parsed quota limit, or null if invalid.</returns>
        public static long? ParseQuotaLimit(this UsageQuotaServiceTests _, string quotaString)
        {
            ArgumentNullException.ThrowIfNull(_);

            if (string.IsNullOrWhiteSpace(quotaString))
            {
                return null;
            }

            if (quotaString.Equals("unlimited", StringComparison.OrdinalIgnoreCase) ||
                quotaString == "-1")
            {
                return -1;
            }

            if (long.TryParse(quotaString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var limit))
            {
                return limit;
            }

            return null;
        }

        /// <summary>
        /// Creates a mock logger that captures log messages.
        /// </summary>
        /// <param name="_">The test instance.</param>
        /// <returns>A mock logger with captured messages.</returns>
        public static (Mock<ILogger<UsageQuotaService>> Mock, List<string> Messages) CreateCapturingLogger(
            this UsageQuotaServiceTests _)
        {
            ArgumentNullException.ThrowIfNull(_);

            var messages = new List<string>();
            var mockLogger = new Mock<ILogger<UsageQuotaService>>();

            mockLogger
                .Setup(l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>(
                    (level, eventId, state, exception, formatter) =>
                    {
                        var message = formatter?.Invoke(state, exception);
                        if (!string.IsNullOrEmpty(message))
                        {
                            messages.Add($"[{level}] {message}");
                        }
                    });

            return (mockLogger, messages);
        }
    }
}
