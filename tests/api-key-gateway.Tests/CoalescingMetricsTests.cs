// -----------------------------------------------------------------------------
// Unit tests for CoalescingMetrics
// -----------------------------------------------------------------------------
using System;
using ApiKeyGateway.Domain.Models;
using Xunit;

namespace api_key_gateway.Tests
{
    public class CoalescingMetricsTests
    {
        [Fact]
        public void Properties_Should_Store_Provided_Values()
        {
            // Arrange
            var metrics = new CoalescingMetrics
            {
                TotalRequests = 123,
                CoalescedRequests = 45,
                ActiveRequests = 7
            };

            // Assert
            Assert.Equal(123, metrics.TotalRequests);
            Assert.Equal(45, metrics.CoalescedRequests);
            Assert.Equal(7, metrics.ActiveRequests);
        }

        [Fact]
        public void CoalescingRatio_Should_Return_Zero_When_TotalRequests_Is_Zero()
        {
            // Arrange
            var metrics = new CoalescingMetrics
            {
                TotalRequests = 0,
                CoalescedRequests = 0,
                ActiveRequests = 0
            };

            // Act
            var ratio = metrics.CoalescingRatio;

            // Assert
            Assert.Equal(0.0, ratio);
        }

        [Fact]
        public void CoalescingRatio_Should_Calculate_Correct_Ratio_For_Normal_Values()
        {
            // Arrange
            var metrics = new CoalescingMetrics
            {
                TotalRequests = 200,
                CoalescedRequests = 50,
                ActiveRequests = 3
            };

            // Act
            var ratio = metrics.CoalescingRatio;

            // Assert
            Assert.Equal(0.25, ratio, precision: 10);
        }

        [Fact]
        public void CoalescingRatio_Should_Be_One_When_All_Requests_Are_Coalesced()
        {
            // Arrange
            var metrics = new CoalescingMetrics
            {
                TotalRequests = 10,
                CoalescedRequests = 10,
                ActiveRequests = 0
            };

            // Act
            var ratio = metrics.CoalescingRatio;

            // Assert
            Assert.Equal(1.0, ratio, precision: 10);
        }

        [Fact]
        public void CoalescingRatio_Can_Exceed_One_If_CoalescedRequests_Greater_Than_TotalRequests()
        {
            // Arrange
            var metrics = new CoalescingMetrics
            {
                TotalRequests = 5,
                CoalescedRequests = 8,
                ActiveRequests = 1
            };

            // Act
            var ratio = metrics.CoalescingRatio;

            // Assert
            Assert.True(ratio > 1.0);
            Assert.Equal(8.0 / 5.0, ratio, precision: 10);
        }

        [Fact]
        public void ActiveRequests_Can_Be_Negative_Without_Throwing()
        {
            // This test documents the current behaviour – the model does not enforce
            // non‑negative values, so a negative value should be stored as‑is.
            var metrics = new CoalescingMetrics
            {
                TotalRequests = 1,
                CoalescedRequests = 0,
                ActiveRequests = -3
            };

            Assert.Equal(-3, metrics.ActiveRequests);
        }
    }
}
