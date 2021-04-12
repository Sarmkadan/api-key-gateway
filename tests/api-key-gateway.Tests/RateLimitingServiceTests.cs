// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="RateLimitingService"/> class.
/// </summary>
public class RateLimitingServiceTests
{
    private readonly Mock<IRateLimitRepository> _repositoryMock;
    private readonly Mock<ILogger<RateLimitingService>> _loggerMock;
    private readonly RateLimitingService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitingServiceTests"/> class,
    /// creating mocks for <see cref="IRateLimitRepository"/> and <see cref="ILogger{RateLimitingService}"/>
    /// and the system under test.
    /// </summary>
    public RateLimitingServiceTests()
    {
        _repositoryMock = new Mock<IRateLimitRepository>();
        _loggerMock = new Mock<ILogger<RateLimitingService>>();
        _sut = new RateLimitingService(_repositoryMock.Object, _loggerMock.Object);
    }

    // -------------------------------------------------------------------------
    // Constructor guards
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that constructing <see cref="RateLimitingService"/> with a null repository
    /// throws an <see cref="ArgumentNullException"/> with parameter name "repository".
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new RateLimitingService(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("repository");
    }

    /// <summary>
    /// Verifies that constructing <see cref="RateLimitingService"/> with a null logger
    /// throws an <see cref="ArgumentNullException"/> with parameter name "logger".
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new RateLimitingService(_repositoryMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // -------------------------------------------------------------------------
    // CheckLimitAsync
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that calling <see cref="RateLimitingService.CheckLimitAsync"/> with an empty,
    /// whitespace, or null API key identifier throws an <see cref="ArgumentException"/>.
    /// </summary>
    /// <param name="apiKeyId">The API key identifier to test; may be empty, whitespace, or null.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CheckLimitAsync_EmptyOrNullKeyId_ThrowsArgumentException(string? apiKeyId)
    {
        var act = async () => await _sut.CheckLimitAsync(apiKeyId!);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Verifies that when no rate‑limit configuration exists for a given key,
    /// <see cref="RateLimitingService.CheckLimitAsync"/> returns <c>true</c>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task CheckLimitAsync_NoRateLimitConfigured_ReturnsTrue()
    {
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-001"))
            .ReturnsAsync((RateLimit?)null);

        var result = await _sut.CheckLimitAsync("key-001");
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that when the current request count is below the configured limit,
    /// <see cref="RateLimitingService.CheckLimitAsync"/> returns <c>true</c>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that when the request count equals the allowed limit,
    /// <see cref="RateLimitingService.CheckLimitAsync"/> throws a <see cref="RateLimitExceededException"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that when the rate‑limit window has expired,
    /// <see cref="RateLimitingService.CheckLimitAsync"/> resets the counter and returns <c>true</c>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that calling <see cref="RateLimitingService.RecordRequestAsync"/> with an empty
    /// API key identifier does not query the repository.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task RecordRequestAsync_EmptyKeyId_DoesNotQueryRepository()
    {
        await _sut.RecordRequestAsync("");
        _repositoryMock.Verify(r => r.GetByApiKeyIdAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Verifies that when no rate‑limit configuration exists for a given key,
    /// <see cref="RateLimitingService.RecordRequestAsync"/> performs no update.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task RecordRequestAsync_NoRateLimitConfigured_DoesNothing()
    {
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-001"))
            .ReturnsAsync((RateLimit?)null);

        await _sut.RecordRequestAsync("key-001");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<RateLimit>()), Times.Never);
    }

    /// <summary>
    /// Verifies that a valid API key causes the request count to be incremented
    /// and the updated <see cref="RateLimit"/> to be persisted.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that attempting to update the limit for a non‑existent key returns <c>false</c>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateLimitAsync_NonExistentKey_ReturnsFalse()
    {
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("missing"))
            .ReturnsAsync((RateLimit?)null);

        var result = await _sut.UpdateLimitAsync("missing", 100, RateLimitUnit.Hour);
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that updating an existing key modifies the request limit and unit,
    /// persists the change, and returns <c>true</c>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that resetting the window for a non‑existent key does not throw an exception.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task ResetWindowAsync_NonExistentKey_DoesNotThrow()
    {
        _repositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("missing"))
            .ReturnsAsync((RateLimit?)null);

        var act = async () => await _sut.ResetWindowAsync("missing");
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Verifies that resetting the window for an existing key sets the request count to zero
    /// and persists the change.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that concurrent calls to <see cref="RateLimitingService.CheckLimitAsync"/>
    /// when the limit is already reached cause all calls to throw <see cref="RateLimitExceededException"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that concurrent calls to <see cref="RateLimitingService.CheckLimitAsync"/>
    /// on a key whose window has expired allow all requests.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that concurrent calls to <see cref="RateLimitingService.ResetWindowAsync"/>
    /// do not throw and correctly reset the request count.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that concurrent calls to <see cref="RateLimitingService.CheckLimitAsync"/>
    /// when the current count is well below the limit all return <c>true</c>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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
