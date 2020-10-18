// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

public class ApiKeyModelTests
{
    [Fact]
    public void CanBeUsed_ActiveNonExpiredKey_ReturnsTrue()
    {
        // Arrange
        var key = new ApiKey { Status = ApiKeyStatus.Active };

        // Act & Assert
        key.CanBeUsed().Should().BeTrue();
    }

    [Theory]
    [InlineData(ApiKeyStatus.Disabled)]
    [InlineData(ApiKeyStatus.Revoked)]
    [InlineData(ApiKeyStatus.Suspended)]
    public void CanBeUsed_InactiveStatus_ReturnsFalse(ApiKeyStatus status)
    {
        // Arrange
        var key = new ApiKey { Status = status };

        // Act & Assert
        key.CanBeUsed().Should().BeFalse();
    }

    [Fact]
    public void CanBeUsed_ExpiredKey_ReturnsFalse()
    {
        // Arrange
        var key = new ApiKey
        {
            Status = ApiKeyStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        key.CanBeUsed().Should().BeFalse();
    }

    [Fact]
    public void RecordUsage_Called_IncrementsRequestCountAndUpdatesLastUsed()
    {
        // Arrange
        var key = new ApiKey { Status = ApiKeyStatus.Active };
        var beforeUsage = DateTime.UtcNow;

        // Act
        key.RecordUsage(bytes: 1024);

        // Assert
        key.RequestCount.Should().Be(1);
        key.BytesTransferred.Should().Be(1024);
        key.LastUsedAt.Should().BeOnOrAfter(beforeUsage);
    }

    [Fact]
    public void Disable_ActiveKey_SetsDisabledStatusAndTimestamp()
    {
        // Arrange
        var key = new ApiKey { Status = ApiKeyStatus.Active };
        var before = DateTime.UtcNow;

        // Act
        key.Disable();

        // Assert
        key.Status.Should().Be(ApiKeyStatus.Disabled);
        key.DisabledAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Enable_DisabledKey_RestoresActiveStatusAndClearsTimestamp()
    {
        // Arrange
        var key = new ApiKey { Status = ApiKeyStatus.Disabled, DisabledAt = DateTime.UtcNow.AddHours(-1) };

        // Act
        key.Enable();

        // Assert
        key.Status.Should().Be(ApiKeyStatus.Active);
        key.DisabledAt.Should().BeNull();
    }

    [Fact]
    public void IsIpAllowed_NullWhitelist_AllowsAnyIp()
    {
        // Arrange
        var key = new ApiKey { IpWhitelist = null };

        // Act & Assert
        key.IsIpAllowed("10.0.0.1").Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_IpInCommaDelimitedWhitelist_ReturnsTrue()
    {
        // Arrange
        var key = new ApiKey { IpWhitelist = "10.0.0.1, 192.168.1.50, 172.16.0.1" };

        // Act & Assert
        key.IsIpAllowed("192.168.1.50").Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_IpNotInWhitelist_ReturnsFalse()
    {
        // Arrange
        var key = new ApiKey { IpWhitelist = "10.0.0.1,10.0.0.2" };

        // Act & Assert
        key.IsIpAllowed("10.0.0.3").Should().BeFalse();
    }
}

public class RateLimitModelTests
{
    [Fact]
    public void CanProcessRequest_CountBelowLimit_ReturnsTrue()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            CurrentRequestCount = 99
        };

        // Act & Assert
        rateLimit.CanProcessRequest().Should().BeTrue();
    }

    [Fact]
    public void CanProcessRequest_CountAtLimit_ReturnsFalse()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Minute,
            CurrentRequestCount = 100
        };

        // Act & Assert
        rateLimit.CanProcessRequest().Should().BeFalse();
    }

    [Fact]
    public void CanProcessRequest_UnlimitedUnit_AlwaysReturnsTrue()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = 0,
            Unit = RateLimitUnit.Unlimited,
            CurrentRequestCount = int.MaxValue
        };

        // Act & Assert
        rateLimit.CanProcessRequest().Should().BeTrue();
    }

    [Fact]
    public void RecordRequest_Called_IncrementsCurrentRequestCount()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = 10,
            Unit = RateLimitUnit.Second,
            CurrentRequestCount = 3
        };

        // Act
        rateLimit.RecordRequest();

        // Assert
        rateLimit.CurrentRequestCount.Should().Be(4);
    }

    [Fact]
    public void ResetWindow_Called_ZeroesCounterAndUpdatesLastResetAt()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            CurrentRequestCount = 75
        };
        var before = DateTime.UtcNow;

        // Act
        rateLimit.ResetWindow();

        // Assert
        rateLimit.CurrentRequestCount.Should().Be(0);
        rateLimit.LastResetAt.Should().BeOnOrAfter(before);
    }

    [Theory]
    [InlineData(RateLimitUnit.Second, 1)]
    [InlineData(RateLimitUnit.Minute, 60)]
    [InlineData(RateLimitUnit.Hour, 3600)]
    [InlineData(RateLimitUnit.Day, 86400)]
    public void GetWindowInSeconds_KnownUnits_ReturnsCorrectDuration(RateLimitUnit unit, int expectedSeconds)
    {
        // Arrange
        var rateLimit = new RateLimit { Unit = unit };

        // Act
        var result = rateLimit.GetWindowInSeconds();

        // Assert
        result.Should().Be(expectedSeconds);
    }
}
