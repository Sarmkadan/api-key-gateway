using ApiKeyGateway.Services;
using Xunit;

namespace api_key_gateway.Tests
{
    public class AnalyticsSummaryExtensionsTests
    {
        [Fact]
        public void ToSummaryString_HappyPath_ReturnsExpectedString()
        {
            // Arrange
            var summary = new AnalyticsSummary
            {
                ApiKeyId = "test-api-key",
                From = new DateTime(2022, 1, 1),
                To = new DateTime(2022, 1, 31),
                TotalRequests = 100,
                SuccessfulRequests = 90,
                FailedRequests = 10,
                SuccessRatePercent = 90.0,
                ErrorRatePercent = 10.0,
                AverageResponseTimeMs = 100.0
            };

            // Act
            var result = summary.ToSummaryString();

            // Assert
            Assert.NotNull(result);
            Assert.Contains("API Key: test-api-key", result);
            Assert.Contains("Period: 2022-01-01 to 2022-01-31", result);
            Assert.Contains("Requests: 100 (Success: 90, Failed: 10)", result);
            Assert.Contains("Success Rate: 90.00%", result);
            Assert.Contains("Error Rate: 10.00%", result);
            Assert.Contains("Avg Response Time: 100.00ms", result);
        }

        [Fact]
        public void ToSummaryString_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => AnalyticsSummaryExtensions.ToSummaryString(null));
        }

        [Fact]
        public void FormatAverageResponseTime_HappyPath_ReturnsExpectedString()
        {
            // Arrange
            var summary = new AnalyticsSummary
            {
                AverageResponseTimeMs = 100.0
            };

            // Act
            var result = summary.FormatAverageResponseTime();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("100.00", result);
        }

        [Fact]
        public void FormatAverageResponseTime_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => AnalyticsSummaryExtensions.FormatAverageResponseTime(null));
        }

        [Fact]
        public void FormatSuccessRatePercent_HappyPath_ReturnsExpectedString()
        {
            // Arrange
            var summary = new AnalyticsSummary
            {
                SuccessRatePercent = 90.0
            };

            // Act
            var result = summary.FormatSuccessRatePercent();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("90.00", result);
        }

        [Fact]
        public void FormatSuccessRatePercent_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => AnalyticsSummaryExtensions.FormatSuccessRatePercent(null));
        }

        [Fact]
        public void FormatErrorRatePercent_HappyPath_ReturnsExpectedString()
        {
            // Arrange
            var summary = new AnalyticsSummary
            {
                ErrorRatePercent = 10.0
            };

            // Act
            var result = summary.FormatErrorRatePercent();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("10.00", result);
        }

        [Fact]
        public void FormatErrorRatePercent_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => AnalyticsSummaryExtensions.FormatErrorRatePercent(null));
        }
    }
}
