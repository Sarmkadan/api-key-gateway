using System.Text.Json;
using Xunit;
using ApiKeyGateway.Services;

namespace api_key_gateway.Tests
{
    public class AnalyticsSummaryJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var analyticsSummary = new AnalyticsSummary(); // Replace with actual initialization

            // Act
            var json = AnalyticsSummaryJsonExtensions.ToJson(analyticsSummary);

            // Assert
            Assert.NotEmpty(json);
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsAnalyticsSummary()
        {
            // Arrange
            var json = "{\"key\":\"value\"}"; // Replace with actual JSON string
            var expectedAnalyticsSummary = new AnalyticsSummary(); // Replace with actual initialization

            // Act
            var analyticsSummary = AnalyticsSummaryJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(analyticsSummary);
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => AnalyticsSummaryJsonExtensions.FromJson(null));
        }

        [Fact]
        public void FromJson_EmptyString_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => AnalyticsSummaryJsonExtensions.FromJson(string.Empty));
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndAnalyticsSummary()
        {
            // Arrange
            var json = "{\"key\":\"value\"}"; // Replace with actual JSON string
            var expectedAnalyticsSummary = new AnalyticsSummary(); // Replace with actual initialization

            // Act
            var success = AnalyticsSummaryJsonExtensions.TryFromJson(json, out var analyticsSummary);

            // Assert
            Assert.True(success);
            Assert.NotNull(analyticsSummary);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsFalseAndNullAnalyticsSummary()
        {
            // Act
            var success = AnalyticsSummaryJsonExtensions.TryFromJson(null, out var analyticsSummary);

            // Assert
            Assert.False(success);
            Assert.Null(analyticsSummary);
        }
    }
}
