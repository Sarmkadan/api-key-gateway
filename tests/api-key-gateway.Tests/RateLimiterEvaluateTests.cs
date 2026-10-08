// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for <see cref="RateLimiter.Evaluate"/> and its relationship to <see cref="RateLimiter.TryAcquire"/>.
/// </summary>
public class RateLimiterEvaluateTests
{
    private readonly RateLimiter _sut = new(NullLogger<RateLimiter>.Instance, defaultMaxRequests: 2, defaultWindow: TimeSpan.FromMinutes(1));

    /// <summary>
    /// Verifies that a request under the limit is allowed and reports the remaining permits.
    /// </summary>
    [Fact]
    public void Evaluate_UnderLimit_ReturnsAllowedWithRemaining()
    {
        // Arrange & Act
        var decision = _sut.Evaluate("key-1");

        // Assert
        decision.Allowed.Should().BeTrue();
        decision.ApiKeyId.Should().Be("key-1");
        decision.Limit.Should().Be(2);
        decision.Remaining.Should().Be(1);
    }

    /// <summary>
    /// Verifies that a request over the limit is denied, with no permits left and a future reset time.
    /// </summary>
    [Fact]
    public void Evaluate_OverLimit_ReturnsDeniedWithFutureReset()
    {
        // Arrange
        _sut.Evaluate("key-1");
        _sut.Evaluate("key-1");

        // Act
        var decision = _sut.Evaluate("key-1");

        // Assert
        decision.Allowed.Should().BeFalse();
        decision.Remaining.Should().Be(0);
        decision.ResetAtUtc.Should().BeAfter(DateTime.UtcNow);
    }

    /// <summary>
    /// Verifies that a denied decision does not consume a permit.
    /// </summary>
    [Fact]
    public void Evaluate_Denied_DoesNotConsumePermit()
    {
        // Arrange
        _sut.Evaluate("key-1");
        _sut.Evaluate("key-1");
        _sut.Evaluate("key-1");

        // Act
        var remaining = _sut.GetRemainingPermits("key-1");

        // Assert
        remaining.Should().Be(0);
    }

    /// <summary>
    /// Verifies that <see cref="RateLimiter.TryAcquire"/> still denies once the window is full.
    /// </summary>
    [Fact]
    public void TryAcquire_AfterWindowFull_ReturnsFalse()
    {
        // Arrange
        Assert.True(_sut.TryAcquire("key-1"));
        Assert.True(_sut.TryAcquire("key-1"));

        // Act
        var allowed = _sut.TryAcquire("key-1");

        // Assert
        allowed.Should().BeFalse();
    }
}
