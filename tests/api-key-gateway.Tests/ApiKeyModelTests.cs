// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ApiKeyGateway.Domain.Models.ApiKey"/> model class.
/// Tests the functionality of key status checks, usage tracking, and IP whitelist validation.
/// </summary>
public class ApiKeyModelTests
{
    /// <summary>
    /// Tests that an active API key without expiration can be used.
    /// Verifies that <see cref="ApiKeyGateway.Domain.Models.ApiKey"/> with <see cref="ApiKeyGateway.Domain.Enums.ApiKeyStatus.Active"/> status
    /// and no expiration date returns true from <see cref="ApiKeyGateway.Domain.Models.ApiKey.CanBeUsed"/> method.
    /// </summary>
    [Fact]
    public void CanBeUsed_ActiveNonExpiredKey_ReturnsTrue()
    {
        // Arrange
        var key = new ApiKey { Status = ApiKeyStatus.Active };

        // Act & Assert
        key.CanBeUsed().Should().BeTrue();
    }

    /// <summary>
    /// Tests that API keys with inactive statuses cannot be used.
    /// Verifies that <see cref="ApiKeyGateway.Domain.Models.ApiKey"/> with statuses <see cref="ApiKeyGateway.Domain.Enums.ApiKeyStatus.Disabled"/>,
    /// <see cref="ApiKeyGateway.Domain.Enums.ApiKeyStatus.Revoked"/>, or <see cref="ApiKeyGateway.Domain.Enums.ApiKeyStatus.Suspended"/>
    /// returns false from <see cref="ApiKeyGateway.Domain.Models.ApiKey.CanBeUsed"/> method.
    /// </summary>
    /// <param name="status">The inactive status to test.</param>
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

    /// <summary>
    /// Tests that an expired API key cannot be used even if active.
    /// Verifies that <see cref="ApiKeyGateway.Domain.Models.ApiKey"/> with <see cref="ApiKeyGateway.Domain.Enums.ApiKeyStatus.Active"/> status
    /// but an expiration date in the past returns false from <see cref="ApiKeyGateway.Domain.Models.ApiKey.CanBeUsed"/> method.
    /// </summary>
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

    /// <summary>
    /// Tests that recording usage increments request count and updates last used timestamp.
    /// Verifies that calling <see cref="ApiKeyGateway.Domain.Models.ApiKey.RecordUsage"/> on an <see cref="ApiKeyGateway.Domain.Models.ApiKey"/>
    /// increments the <see cref="ApiKeyGateway.Domain.Models.ApiKey.RequestCount"/> property, sets the
    /// <see cref="ApiKeyGateway.Domain.Models.ApiKey.BytesTransferred"/> property to the provided value,
    /// and updates the <see cref="ApiKeyGateway.Domain.Models.ApiKey.LastUsedAt"/> timestamp.
    /// </summary>
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

    /// <summary>
    /// Tests that disabling an active key sets the disabled status and timestamp.
    /// Verifies that calling <see cref="ApiKeyGateway.Domain.Models.ApiKey.Disable"/> on an <see cref="ApiKeyGateway.Domain.Models.ApiKey"/>
    /// with <see cref="ApiKeyGateway.Domain.Enums.ApiKeyStatus.Active"/> status changes the status to
    /// <see cref="ApiKeyGateway.Domain.Enums.ApiKeyStatus.Disabled"/> and sets the
    /// <see cref="ApiKeyGateway.Domain.Models.ApiKey.DisabledAt"/> timestamp.
    /// </summary>
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

    /// <summary>
    /// Tests that enabling a disabled key restores active status and clears the disabled timestamp.
    /// Verifies that calling <see cref="ApiKeyGateway.Domain.Models.ApiKey.Enable"/> on an <see cref="ApiKeyGateway.Domain.Models.ApiKey"/>
    /// with <see cref="ApiKeyGateway.Domain.Enums.ApiKeyStatus.Disabled"/> status changes the status back to
    /// <see cref="ApiKeyGateway.Domain.Enums.ApiKeyStatus.Active"/> and sets the
    /// <see cref="ApiKeyGateway.Domain.Models.ApiKey.DisabledAt"/> property to null.
    /// </summary>
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

    /// <summary>
    /// Tests that a key with null IP whitelist allows any IP address.
    /// Verifies that when <see cref="ApiKeyGateway.Domain.Models.ApiKey.IpWhitelist"/> is null,
    /// the <see cref="ApiKeyGateway.Domain.Models.ApiKey.IsIpAllowed"/> method returns true for any IP address.
    /// </summary>
    [Fact]
    public void IsIpAllowed_NullWhitelist_AllowsAnyIp()
    {
        // Arrange
        var key = new ApiKey { IpWhitelist = null };

        // Act & Assert
        key.IsIpAllowed("10.0.0.1").Should().BeTrue();
    }

    /// <summary>
    /// Tests that an IP address in the comma-delimited whitelist is allowed.
    /// Verifies that when <see cref="ApiKeyGateway.Domain.Models.ApiKey.IpWhitelist"/> contains a comma-separated list of IP addresses,
    /// the <see cref="ApiKeyGateway.Domain.Models.ApiKey.IsIpAllowed"/> method returns true
    /// for an IP address that exists in the whitelist.
    /// </summary>
    [Fact]
    public void IsIpAllowed_IpInCommaDelimitedWhitelist_ReturnsTrue()
    {
        // Arrange
        var key = new ApiKey { IpWhitelist = "10.0.0.1, 192.168.1.50, 172.16.0.1" };

        // Act & Assert
        key.IsIpAllowed("192.168.1.50").Should().BeTrue();
    }

    /// <summary>
    /// Tests that an IP address not in the whitelist is rejected.
    /// Verifies that when <see cref="ApiKeyGateway.Domain.Models.ApiKey.IpWhitelist"/> contains a comma-separated list of IP addresses,
    /// the <see cref="ApiKeyGateway.Domain.Models.ApiKey.IsIpAllowed"/> method returns false
    /// for an IP address that does not exist in the whitelist.
    /// </summary>
    [Fact]
    public void IsIpAllowed_IpNotInWhitelist_ReturnsFalse()
    {
        // Arrange
        var key = new ApiKey { IpWhitelist = "10.0.0.1,10.0.0.2" };

        // Act & Assert
        key.IsIpAllowed("10.0.0.3").Should().BeFalse();
    }
}

/// <summary>
/// Contains unit tests for the <see cref="ApiKeyGateway.Domain.Models.RateLimit"/> model class.
/// Tests the functionality of rate limiting including request counting, window management,
/// and request processing validation.
/// </summary>
public class RateLimitModelTests
{
    /// <summary>
    /// Tests that a rate limit allows processing when request count is below the limit.
    /// Verifies that <see cref="ApiKeyGateway.Domain.Models.RateLimit.CanProcessRequest"/> returns true
    /// when <see cref="ApiKeyGateway.Domain.Models.RateLimit.CurrentRequestCount"/> is less than
    /// <see cref="ApiKeyGateway.Domain.Models.RateLimit.RequestsPerUnit"/>.
    /// </summary>
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

    /// <summary>
    /// Tests that a rate limit rejects processing when request count equals the limit.
    /// Verifies that <see cref="ApiKeyGateway.Domain.Models.RateLimit.CanProcessRequest"/> returns false
    /// when <see cref="ApiKeyGateway.Domain.Models.RateLimit.CurrentRequestCount"/> equals
    /// <see cref="ApiKeyGateway.Domain.Models.RateLimit.RequestsPerUnit"/>.
    /// </summary>
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

    /// <summary>
    /// Tests that a rate limit with unlimited unit always allows processing.
    /// Verifies that <see cref="ApiKeyGateway.Domain.Models.RateLimit.CanProcessRequest"/> returns true
    /// when <see cref="ApiKeyGateway.Domain.Models.RateLimit.Unit"/> is <see cref="ApiKeyGateway.Domain.Enums.RateLimitUnit.Unlimited"/>
    /// regardless of the current request count.
    /// </summary>
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

    /// <summary>
    /// Tests that recording a request increments the current request count.
    /// Verifies that calling <see cref="ApiKeyGateway.Domain.Models.RateLimit.RecordRequest"/> on a <see cref="ApiKeyGateway.Domain.Models.RateLimit"/>
    /// increments the <see cref="ApiKeyGateway.Domain.Models.RateLimit.CurrentRequestCount"/> property by 1.
    /// </summary>
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

    /// <summary>
    /// Tests that resetting the window zeroes the counter and updates the last reset timestamp.
    /// Verifies that calling <see cref="ApiKeyGateway.Domain.Models.RateLimit.ResetWindow"/> on a <see cref="ApiKeyGateway.Domain.Models.RateLimit"/>
    /// sets the <see cref="ApiKeyGateway.Domain.Models.RateLimit.CurrentRequestCount"/> property to 0
    /// and updates the <see cref="ApiKeyGateway.Domain.Models.RateLimit.LastResetAt"/> timestamp.
    /// </summary>
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

    /// <summary>
    /// Tests that known rate limit units return correct window durations in seconds.
    /// Verifies that <see cref="ApiKeyGateway.Domain.Models.RateLimit.GetWindowInSeconds"/> returns the correct number of seconds
    /// for each <see cref="ApiKeyGateway.Domain.Enums.RateLimitUnit"/> value:
    /// <list type="bullet">
    /// <item><see cref="ApiKeyGateway.Domain.Enums.RateLimitUnit.Second"/> returns 1 second</item>
    /// <item><see cref="ApiKeyGateway.Domain.Enums.RateLimitUnit.Minute"/> returns 60 seconds</item>
    /// <item><see cref="ApiKeyGateway.Domain.Enums.RateLimitUnit.Hour"/> returns 3600 seconds</item>
    /// <item><see cref="ApiKeyGateway.Domain.Enums.RateLimitUnit.Day"/> returns 86400 seconds</item>
    /// </list>
    /// </summary>
    /// <param name="unit">The rate limit unit to test.</param>
    /// <param name="expectedSeconds">The expected duration in seconds for the given unit.</param>
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
