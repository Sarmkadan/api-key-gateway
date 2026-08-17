// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="RateLimitFluentExtensions"/> class.
/// </summary>
public class RateLimitFluentExtensionsTests
{
    [Fact]
    public void IsExceededBy_WhenExceeded_ReturnsTrue()
    {
        var rateLimit = new RateLimit { RequestsPerUnit = 100, CurrentRequestCount = 95 };
        Assert.True(rateLimit.IsExceededBy(6));
    }

    [Fact]
    public void IsExceededBy_WhenNotExceeded_ReturnsFalse()
    {
        var rateLimit = new RateLimit { RequestsPerUnit = 100, CurrentRequestCount = 95 };
        Assert.False(rateLimit.IsExceededBy(5));
    }

    [Fact]
    public void RemainingCapacity_ReturnsCorrectValue()
    {
        var rateLimit = new RateLimit { RequestsPerUnit = 100, CurrentRequestCount = 90 };
        Assert.Equal(5, rateLimit.RemainingCapacity(5));
    }

    [Fact]
    public void WindowEnd_ReturnsCorrectTime()
    {
        var start = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var rateLimit = new RateLimit { Unit = RateLimitUnit.Hour };
        var expectedEnd = start.AddHours(1);
        Assert.Equal(expectedEnd, rateLimit.WindowEnd(start));
    }

    [Fact]
    public void ToDisplayString_ReturnsCorrectFormat()
    {
        var rateLimit = new RateLimit { CurrentRequestCount = 10, RequestsPerUnit = 100, Unit = RateLimitUnit.Hour };
        Assert.Equal("10/100 Hour", rateLimit.ToDisplayString());
    }
}
