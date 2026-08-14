using System;
using System.Collections.Generic;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using Xunit;

namespace api_key_gateway.Tests;

public class AnalyticsSummaryValidationTests
{
    private static AnalyticsSummary CreateValidSummary()
    {
        var now = DateTime.UtcNow;
        return new AnalyticsSummary
        {
            ApiKeyId = "test-key",
            From = now.AddHours(-2),
            To = now,
            TotalRequests = 100,
            SuccessfulRequests = 80,
            FailedRequests = 20,
            SuccessRatePercent = 80.0,
            ErrorRatePercent = 20.0,
            AverageResponseTimeMs = 150.5,
            TotalBytesTransferred = 1_024_000,
            UniqueEndpoints = 5,
            UniqueSourceIps = 3
        };
    }

    [Fact]
    public void Validate_ReturnsEmpty_ForValidSummary()
    {
        // Arrange
        var summary = CreateValidSummary();

        // Act
        var errors = summary.Validate();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForValidSummary()
    {
        // Arrange
        var summary = CreateValidSummary();

        // Act
        var result = summary.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_ForValidSummary()
    {
        // Arrange
        var summary = CreateValidSummary();

        // Act & Assert
        var exception = Record.Exception(() => summary.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenSummaryIsNull()
    {
        // Arrange
        AnalyticsSummary? summary = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => summary!.Validate());
    }

    [Fact]
    public void IsValid_ThrowsArgumentNullException_WhenSummaryIsNull()
    {
        // Arrange
        AnalyticsSummary? summary = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => summary!.IsValid());
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_WithAllErrors()
    {
        // Arrange: create a summary with multiple validation failures
        var summary = new AnalyticsSummary
        {
            ApiKeyId = "",                     // empty
            From = DateTime.UtcNow.AddHours(1), // future
            To = DateTime.UtcNow.AddHours(2),   // future and after From
            TotalRequests = -5,                // negative
            SuccessfulRequests = 10,
            FailedRequests = 0,
            SuccessRatePercent = 150,          // out of range
            ErrorRatePercent = -10,            // out of range
            AverageResponseTimeMs = -1,
            TotalBytesTransferred = -100,
            UniqueEndpoints = -1,
            UniqueSourceIps = -1
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() => summary.EnsureValid());

        // Assert
        var message = ex.Message;
        Assert.Contains("ApiKeyId must not be empty.", message);
        Assert.Contains("From date cannot be in the future.", message);
        Assert.Contains("To date cannot be in the future.", message);
        Assert.Contains("TotalRequests must be non-negative.", message);
        Assert.Contains("SuccessRatePercent must be between 0 and 100 inclusive.", message);
        Assert.Contains("ErrorRatePercent must be between 0 and 100 inclusive.", message);
        Assert.Contains("AverageResponseTimeMs must be non-negative.", message);
        Assert.Contains("TotalBytesTransferred must be non-negative.", message);
        Assert.Contains("UniqueEndpoints must be non-negative.", message);
        Assert.Contains("UniqueSourceIps must be non-negative.", message);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0, 0)] // all zero, valid
    [InlineData(100, 100, 0, 100, 0, 0)] // 100% success
    [InlineData(100, 0, 100, 0, 100, 0)] // 100% error
    public void Validate_BoundaryValues_AreAccepted(
        int totalRequests,
        int successful,
        int failed,
        double successRate,
        double errorRate,
        double toleranceIgnored) // placeholder to keep signature consistent
    {
        // Arrange
        var now = DateTime.UtcNow;
        var summary = new AnalyticsSummary
        {
            ApiKeyId = "boundary",
            From = now.AddHours(-1),
            To = now,
            TotalRequests = totalRequests,
            SuccessfulRequests = successful,
            FailedRequests = failed,
            SuccessRatePercent = successRate,
            ErrorRatePercent = errorRate,
            AverageResponseTimeMs = 0,
            TotalBytesTransferred = 0,
            UniqueEndpoints = 0,
            UniqueSourceIps = 0
        };

        // Act
        var errors = summary.Validate();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ReturnsError_WhenSuccessfulPlusFailedDoesNotMatchTotal()
    {
        // Arrange
        var summary = CreateValidSummary();
        summary.SuccessfulRequests = 70; // mismatch (70+20 != 100)

        // Act
        var errors = summary.Validate();

        // Assert
        Assert.Contains("SuccessfulRequests + FailedRequests must equal TotalRequests.", errors);
    }

    [Fact]
    public void Validate_ReturnsError_WhenSuccessAndErrorRatesDoNotSumTo100()
    {
        // Arrange
        var summary = CreateValidSummary();
        summary.SuccessRatePercent = 70.0;
        summary.ErrorRatePercent = 20.0; // sum = 90, outside tolerance

        // Act
        var errors = summary.Validate();

        // Assert
        Assert.Contains("SuccessRatePercent + ErrorRatePercent must equal 100 within tolerance.", errors);
    }
}
