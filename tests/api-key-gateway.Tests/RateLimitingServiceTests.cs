// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

public class RateLimitingServiceTests
{
    private readonly Mock<IRateLimitRepository> _repositoryMock;
    private readonly Mock<ILogger<RateLimitingService>> _loggerMock;
    private readonly RateLimitingService _sut;

    public RateLimitingServiceTests()
    {
        _repositoryMock = new Mock<IRateLimitRepository>();
        _loggerMock = new Mock<ILogger<RateLimitingService>>();
        _sut = new RateLimitingService(_repositoryMock.Object, _loggerMock.Object);
    }

    // -------------------------------------------------------------------------
    // Constructor guards
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new RateLimitingService(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("repository");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new RateLimitingService(_repositoryMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // -------------------------------------------------------------------------
    // CheckLimitAsync
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CheckLimitAsync_EmptyOrNullKeyId_ThrowsArgumentException(string? apiKeyId)
    {
        var act = async () => await _sut.CheckLimitAsync(apiKeyId!);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CheckLimitAsync_NoRateLimitConfigured_ReturnsTrue()
    {
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-001"))
            .ReturnsAsync((RateLimit?)null);

        var result = await _sut.CheckLimitAsync("key-001");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckLimitAsync_BelowLimit_ReturnsTrue()
    {
        var rateLimit = new RateLimit
        {
            ApiKeyId = "key-001",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            CurrentRequestCount = 50,
            LastResetAt = DateTime.UtcNow
        };
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-001"))
            .ReturnsAsync(rateLimit);

        var result = await _sut.CheckLimitAsync("key-001");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckLimitAsync_AtLimit_ThrowsRateLimitExceededException()
    {
        var rateLimit = new RateLimit
        {
            ApiKeyId = "key-001",
            RequestsPerUnit = 10,
            Unit = RateLimitUnit.Minute,
            CurrentRequestCount = 10,
            LastResetAt = DateTime.UtcNow
        };
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-001"))
            .ReturnsAsync(rateLimit);

        var act = async () => await _sut.CheckLimitAsync("key-001");
        await act.Should().ThrowAsync<RateLimitExceededException>();
    }

    [Fact]
    public async Task CheckLimitAsync_ExpiredWindow_ResetsCounterAndAllows()
    {
        var rateLimit = new RateLimit
        {
            ApiKeyId = "key-001",
            RequestsPerUnit = 10,
            Unit = RateLimitUnit.Minute,
            CurrentRequestCount = 10,
            LastResetAt = DateTime.UtcNow.AddMinutes(-5) // expired window
        };
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-001"))
            .ReturnsAsync(rateLimit);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<RateLimit>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CheckLimitAsync("key-001");
        result.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // RecordRequestAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RecordRequestAsync_EmptyKeyId_DoesNotQueryRepository()
    {
        await _sut.RecordRequestAsync("");
        _repositoryMock.Verify(r => r.GetByApiKeyIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RecordRequestAsync_NoRateLimitConfigured_DoesNothing()
    {
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-001"))
            .ReturnsAsync((RateLimit?)null);

        await _sut.RecordRequestAsync("key-001");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<RateLimit>()), Times.Never);
    }

    [Fact]
    public async Task RecordRequestAsync_ValidKey_IncrementsCountAndPersists()
    {
        var rateLimit = new RateLimit
        {
            ApiKeyId = "key-001",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            CurrentRequestCount = 5,
            LastResetAt = DateTime.UtcNow
        };
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-001"))
            .ReturnsAsync(rateLimit);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<RateLimit>()))
            .Returns(Task.CompletedTask);

        await _sut.RecordRequestAsync("key-001");

        rateLimit.CurrentRequestCount.Should().Be(6);
        _repositoryMock.Verify(r => r.UpdateAsync(rateLimit), Times.Once);
    }

    // -------------------------------------------------------------------------
    // UpdateLimitAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateLimitAsync_NonExistentKey_ReturnsFalse()
    {
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("missing"))
            .ReturnsAsync((RateLimit?)null);

        var result = await _sut.UpdateLimitAsync("missing", 100, RateLimitUnit.Hour);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateLimitAsync_ExistingKey_UpdatesAndReturnsTrue()
    {
        var rateLimit = new RateLimit
        {
            Id = "rl-001",
            ApiKeyId = "key-001",
            RequestsPerUnit = 10,
            Unit = RateLimitUnit.Minute,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-001"))
            .ReturnsAsync(rateLimit);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<RateLimit>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateLimitAsync("key-001", 500, RateLimitUnit.Hour);

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<RateLimit>(
            rl => rl.RequestsPerUnit == 500 && rl.Unit == RateLimitUnit.Hour
        )), Times.Once);
    }

    // -------------------------------------------------------------------------
    // ResetWindowAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ResetWindowAsync_NonExistentKey_DoesNotThrow()
    {
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("missing"))
            .ReturnsAsync((RateLimit?)null);

        var act = async () => await _sut.ResetWindowAsync("missing");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ResetWindowAsync_ExistingKey_ResetsCounterAndPersists()
    {
        var rateLimit = new RateLimit
        {
            ApiKeyId = "key-001",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            CurrentRequestCount = 75
        };
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-001"))
            .ReturnsAsync(rateLimit);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<RateLimit>()))
            .Returns(Task.CompletedTask);

        await _sut.ResetWindowAsync("key-001");

        rateLimit.CurrentRequestCount.Should().Be(0);
        _repositoryMock.Verify(r => r.UpdateAsync(rateLimit), Times.Once);
    }

    // -------------------------------------------------------------------------
    // Concurrency / thread-safety
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CheckLimitAsync_ConcurrentCallsAtLimit_AllThrowRateLimitExceededException()
    {
        var rateLimit = new RateLimit
        {
            ApiKeyId = "key-concurrent",
            RequestsPerUnit = 5,
            Unit = RateLimitUnit.Minute,
            CurrentRequestCount = 5,
            LastResetAt = DateTime.UtcNow
        };
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-concurrent"))
            .ReturnsAsync(rateLimit);

        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
        var tasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            try { await _sut.CheckLimitAsync("key-concurrent"); }
            catch (Exception ex) { exceptions.Add(ex); }
        });

        await Task.WhenAll(tasks);

        exceptions.Should().HaveCount(10);
        exceptions.Should().AllSatisfy(ex => ex.Should().BeOfType<RateLimitExceededException>());
    }

    [Fact]
    public async Task CheckLimitAsync_ConcurrentCallsOnExpiredWindow_AllRequestsAllowed()
    {
        var rateLimit = new RateLimit
        {
            ApiKeyId = "key-expired-window",
            RequestsPerUnit = 10,
            Unit = RateLimitUnit.Minute,
            CurrentRequestCount = 10,
            LastResetAt = DateTime.UtcNow.AddMinutes(-5)
        };
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-expired-window"))
            .ReturnsAsync(rateLimit);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<RateLimit>()))
            .Returns(Task.CompletedTask);

        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
        var tasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            try { await _sut.CheckLimitAsync("key-expired-window"); }
            catch (Exception ex) { exceptions.Add(ex); }
        });

        await Task.WhenAll(tasks);

        exceptions.Should().BeEmpty("all requests should be allowed after window expires");
    }

    [Fact]
    public async Task ResetWindowAsync_ConcurrentCalls_DoesNotThrowAndUpdatesCache()
    {
        var rateLimit = new RateLimit
        {
            ApiKeyId = "key-reset",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            CurrentRequestCount = 50
        };
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-reset"))
            .ReturnsAsync(rateLimit);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<RateLimit>()))
            .Returns(Task.CompletedTask);

        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
        var tasks = Enumerable.Range(0, 20).Select(async _ =>
        {
            try { await _sut.ResetWindowAsync("key-reset"); }
            catch (Exception ex) { exceptions.Add(ex); }
        });

        await Task.WhenAll(tasks);

        exceptions.Should().BeEmpty("concurrent resets must not throw");
        rateLimit.CurrentRequestCount.Should().Be(0);
    }

    [Fact]
    public async Task CheckLimitAsync_ConcurrentCallsBelowLimit_AllReturnTrue()
    {
        var rateLimit = new RateLimit
        {
            ApiKeyId = "key-below-limit",
            RequestsPerUnit = 1000,
            Unit = RateLimitUnit.Hour,
            CurrentRequestCount = 1,
            LastResetAt = DateTime.UtcNow
        };
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-below-limit"))
            .ReturnsAsync(rateLimit);

        var results = new System.Collections.Concurrent.ConcurrentBag<bool>();
        var tasks = Enumerable.Range(0, 50).Select(async _ =>
        {
            var result = await _sut.CheckLimitAsync("key-below-limit");
            results.Add(result);
        });

        await Task.WhenAll(tasks);

        results.Should().HaveCount(50);
        results.Should().OnlyContain(r => r, "all requests should pass when well below limit");
    }
}
