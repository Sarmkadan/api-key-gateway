// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Xunit;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="RateLimitExtensions"/> class.
/// Tests the extension methods for rate limiting functionality including violation detection,
/// remaining request calculation, and request allowance checks.
/// </summary>
public class RateLimitExtensionsTests
{
    /// <summary>
    /// Tests that IsViolated returns false when rate limit is not violated.
    /// Verifies that <see cref="RateLimitExtensions.IsViolated(RateLimit)"/> returns false when CurrentRequestCount is below RequestsPerUnit.
    /// </summary>
    [Fact]
    public void IsViolated_NotViolated_ReturnsFalse()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            CurrentRequestCount = 99,
            IsEnabled = true
        };

        // Act
        var result = rateLimit.IsViolated();

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that IsViolated returns true when rate limit is violated.
    /// Verifies that <see cref="RateLimitExtensions.IsViolated(RateLimit)"/> returns true when CurrentRequestCount equals RequestsPerUnit.
    /// </summary>
    [Fact]
    public void IsViolated_AtLimit_ReturnsTrue()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Minute,
            CurrentRequestCount = 100,
            IsEnabled = true
        };

        // Act
        var result = rateLimit.IsViolated();

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that IsViolated returns false when rate limit is disabled.
    /// Verifies that <see cref="RateLimitExtensions.IsViolated(RateLimit)"/> returns false when IsEnabled is false.
    /// </summary>
    [Fact]
    public void IsViolated_DisabledRateLimit_ReturnsFalse()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            CurrentRequestCount = 100,
            IsEnabled = false
        };

        // Act
        var result = rateLimit.IsViolated();

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that IsViolated returns false when requests per unit is unlimited.
    /// Verifies that <see cref="RateLimitExtensions.IsViolated(RateLimit)"/> returns false when RequestsPerUnit is unlimited (-1).
    /// </summary>
    [Fact]
    public void IsViolated_UnlimitedRequests_ReturnsFalse()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = QuotaLimit.Unlimited,
            Unit = RateLimitUnit.Hour,
            CurrentRequestCount = int.MaxValue,
            IsEnabled = true
        };

        // Act
        var result = rateLimit.IsViolated();

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that GetRemainingRequests returns correct remaining count.
    /// Verifies that <see cref="RateLimitExtensions.GetRemainingRequests(RateLimit)"/> returns the correct number of remaining requests.
    /// </summary>
    [Fact]
    public void GetRemainingRequests_HasRemaining_ReturnsCorrectCount()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            CurrentRequestCount = 75
        };

        // Act
        var result = rateLimit.GetRemainingRequests();

        // Assert
        Assert.Equal(25, result);
    }

    /// <summary>
    /// Tests that GetRemainingRequests returns 0 when at limit.
    /// Verifies that <see cref="RateLimitExtensions.GetRemainingRequests(RateLimit)"/> returns 0 when CurrentRequestCount equals RequestsPerUnit.
    /// </summary>
    [Fact]
    public void GetRemainingRequests_AtLimit_ReturnsZero()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = 50,
            Unit = RateLimitUnit.Minute,
            CurrentRequestCount = 50
        };

        // Act
        var result = rateLimit.GetRemainingRequests();

        // Assert
        Assert.Equal(0, result);
    }

    /// <summary>
    /// Tests that GetRemainingRequests returns int.MaxValue for unlimited requests.
    /// Verifies that <see cref="RateLimitExtensions.GetRemainingRequests(RateLimit)"/> returns int.MaxValue when RequestsPerUnit is unlimited.
    /// </summary>
    [Fact]
    public void GetRemainingRequests_Unlimited_ReturnsMaxValue()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = QuotaLimit.Unlimited,
            Unit = RateLimitUnit.Hour,
            CurrentRequestCount = 0
        };

        // Act
        var result = rateLimit.GetRemainingRequests();

        // Assert
        Assert.Equal(int.MaxValue, result);
    }

    /// <summary>
    /// Tests that ShouldAllowRequest returns true when request should be allowed.
    /// Verifies that <see cref="RateLimitExtensions.ShouldAllowRequest(RateLimit)"/> returns true when CanProcessRequest returns true.
    /// </summary>
    [Fact]
    public void ShouldAllowRequest_ShouldAllow_ReturnsTrue()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            CurrentRequestCount = 99,
            IsEnabled = true
        };

        // Act
        var result = rateLimit.ShouldAllowRequest();

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that ShouldAllowRequest returns false when request should not be allowed.
    /// Verifies that <see cref="RateLimitExtensions.ShouldAllowRequest(RateLimit)"/> returns false when CanProcessRequest returns false.
    /// </summary>
    [Fact]
    public void ShouldAllowRequest_ShouldNotAllow_ReturnsFalse()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Minute,
            CurrentRequestCount = 100,
            IsEnabled = true
        };

        // Act
        var result = rateLimit.ShouldAllowRequest();

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that ShouldAllowRequest returns true for unlimited requests.
    /// Verifies that <see cref="RateLimitExtensions.ShouldAllowRequest(RateLimit)"/> returns true when Unit is Unlimited.
    /// </summary>
    [Fact]
    public void ShouldAllowRequest_UnlimitedUnit_ReturnsTrue()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = 0,
            Unit = RateLimitUnit.Unlimited,
            CurrentRequestCount = int.MaxValue,
            IsEnabled = false
        };

        // Act
        var result = rateLimit.ShouldAllowRequest();

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that IsViolated throws ArgumentNullException when rateLimit is null.
    /// </summary>
    [Fact]
    public void IsViolated_WithNullRateLimit_ThrowsArgumentNullException()
    {
        // Arrange
        RateLimit? nullRateLimit = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullRateLimit.IsViolated());
    }

    /// <summary>
    /// Tests that GetRemainingRequests throws ArgumentNullException when rateLimit is null.
    /// </summary>
    [Fact]
    public void GetRemainingRequests_WithNullRateLimit_ThrowsArgumentNullException()
    {
        // Arrange
        RateLimit? nullRateLimit = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullRateLimit.GetRemainingRequests());
    }

    /// <summary>
    /// Tests that ShouldAllowRequest throws ArgumentNullException when rateLimit is null.
    /// </summary>
    [Fact]
    public void ShouldAllowRequest_WithNullRateLimit_ThrowsArgumentNullException()
    {
        // Arrange
        RateLimit? nullRateLimit = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullRateLimit.ShouldAllowRequest());
    }

}