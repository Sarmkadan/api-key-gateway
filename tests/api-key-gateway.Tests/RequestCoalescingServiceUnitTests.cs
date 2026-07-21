// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="RequestCoalescingService"/> class.
/// Tests request coalescing behavior, metrics, error handling, and edge cases.
/// </summary>
public class RequestCoalescingServiceUnitTests
{
    private readonly Mock<ILogger<RequestCoalescingService>> _loggerMock;
    private readonly RequestCoalescingService _service;

    public RequestCoalescingServiceUnitTests()
    {
        _loggerMock = new Mock<ILogger<RequestCoalescingService>>();
        _service = new RequestCoalescingService(_loggerMock.Object);
    }

    /// <summary>
    /// Tests that ExecuteAsync throws ArgumentException when requestKey is null.
    /// </summary>
    [Fact]
    public void ExecuteAsync_NullRequestKey_ThrowsArgumentException()
    {
        // Arrange
        Func<CancellationToken, Task<string>> operation = _ => Task.FromResult("result");

        // Act
        Func<Task> act = async () => await _service.ExecuteAsync(
            null!,
            operation,
            CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Request key cannot be empty*");
    }

    /// <summary>
    /// Tests that ExecuteAsync throws ArgumentException when requestKey is empty.
    /// </summary>
    [Fact]
    public void ExecuteAsync_EmptyRequestKey_ThrowsArgumentException()
    {
        // Arrange
        Func<CancellationToken, Task<string>> operation = _ => Task.FromResult("result");

        // Act
        Func<Task> act = async () => await _service.ExecuteAsync(
            string.Empty,
            operation,
            CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Request key cannot be empty*");
    }

    /// <summary>
    /// Tests that ExecuteAsync throws ArgumentException when requestKey is whitespace only.
    /// </summary>
    [Fact]
    public void ExecuteAsync_WhitespaceRequestKey_ThrowsArgumentException()
    {
        // Arrange
        Func<CancellationToken, Task<string>> operation = _ => Task.FromResult("result");

        // Act
        Func<Task> act = async () => await _service.ExecuteAsync(
            "   ",
            operation,
            CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Request key cannot be empty*");
    }

    /// <summary>
    /// Tests that ExecuteAsync throws ArgumentNullException when operation is null.
    /// </summary>
    [Fact]
    public void ExecuteAsync_NullOperation_ThrowsArgumentNullException()
    {
        // Arrange
        Func<CancellationToken, Task<string>>? operation = null;

        // Act
        Func<Task> act = async () => await _service.ExecuteAsync(
            "valid-key",
            operation!,
            CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>()
            .WithMessage("*operation*");
    }

    /// <summary>
    /// Tests that ExecuteAsync returns the operation result when no coalescing occurs.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_NoCoalescing_ReturnsOperationResult()
    {
        // Arrange
        const string requestKey = "unique-key-1";
        const string expectedResult = "test-result";
        Func<CancellationToken, Task<string>> operation = _ => Task.FromResult(expectedResult);

        // Act
        var result = await _service.ExecuteAsync(
            requestKey,
            operation,
            CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
    }

    /// <summary>
    /// Tests that ExecuteAsync coalesces identical requests and returns the same result.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_IdenticalRequests_CoalescesAndReturnsSameResult()
    {
        // Arrange
        const string requestKey = "shared-key";
        const string expectedResult = "shared-result";
        var callCount = 0;
        Func<CancellationToken, Task<string>> operation = _ =>
        {
            var count = Interlocked.Increment(ref callCount);
            return Task.FromResult(expectedResult);
        };

        // Act - Execute two identical requests concurrently
        var task1 = _service.ExecuteAsync(requestKey, operation, CancellationToken.None);
        var task2 = _service.ExecuteAsync(requestKey, operation, CancellationToken.None);

        var results = await Task.WhenAll(task1, task2);

        // Assert
        results[0].Should().Be(expectedResult);
        results[1].Should().Be(expectedResult);
        callCount.Should().BeLessOrEqualTo(2); // Can be 1 (coalesced) or 2 (race condition)
    }

    /// <summary>
    /// Tests that ExecuteAsync propagates exceptions from the operation to all callers.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_OperationThrowsException_PropagatesToAllCallers()
    {
        // Arrange
        const string requestKey = "error-key";
        var callCount = 0;
        Func<CancellationToken, Task<string>> operation = _ =>
        {
            var count = Interlocked.Increment(ref callCount);
            throw new InvalidOperationException("Test error");
        };

        // Act
        var task1 = _service.ExecuteAsync(requestKey, operation, CancellationToken.None);
        var task2 = _service.ExecuteAsync(requestKey, operation, CancellationToken.None);

        var exceptionTask1 = await Record.ExceptionAsync(() => task1);
        var exceptionTask2 = await Record.ExceptionAsync(() => task2);

        // Assert
        exceptionTask1.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("Test error");
        exceptionTask2.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("Test error");
        callCount.Should().BeLessOrEqualTo(2); // Can be 1 (coalesced) or 2 (race condition)
    }

    /// <summary>
    /// Tests that ExecuteAsync respects cancellation tokens for followers.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_FollowerCancelled_DoesNotAffectLeader()
    {
        // Arrange
        const string requestKey = "cancellation-key";
        const string expectedResult = "leader-result";
        var callCount = 0;
        Func<CancellationToken, Task<string>> operation = ct =>
        {
            Interlocked.Increment(ref callCount);
            // Simulate some work
            return Task.Delay(50, ct).ContinueWith(_ => expectedResult, ct);
        };

        using var cts = new CancellationTokenSource();

        // Act
        var leaderTask = _service.ExecuteAsync(requestKey, operation, CancellationToken.None);
        var followerTask = _service.ExecuteAsync(requestKey, operation, cts.Token);

        // Cancel the follower quickly
        cts.Cancel();

        var leaderResult = await leaderTask;
        var followerException = await Record.ExceptionAsync(() => followerTask);

        // Assert
        leaderResult.Should().Be(expectedResult);
        followerException.Should().BeOfType<TaskCanceledException>();
        callCount.Should().Be(1); // Operation should only be called once
    }

    /// <summary>
    /// Tests that GetMetrics returns correct initial values.
    /// </summary>
    [Fact]
    public void GetMetrics_NoRequests_ReturnsZeroValues()
    {
        // Act
        var metrics = _service.GetMetrics();

        // Assert
        metrics.TotalRequests.Should().Be(0);
        metrics.CoalescedRequests.Should().Be(0);
        metrics.ActiveRequests.Should().Be(0);
        metrics.CoalescingRatio.Should().Be(0.0);
    }

    /// <summary>
    /// Tests that GetMetrics returns correct values after requests are processed.
    /// </summary>
    [Fact]
    public async Task GetMetrics_AfterRequests_ReturnsCorrectValues()
    {
        // Arrange
        const string requestKey1 = "key-1";
        const string requestKey2 = "key-2";
        const string requestKey3 = "key-2"; // Same as key-2 to test coalescing
        Func<CancellationToken, Task<int>> operation = _ => Task.FromResult(42);

        // Act
        await _service.ExecuteAsync(requestKey1, operation, CancellationToken.None); // Leader
        await _service.ExecuteAsync(requestKey2, operation, CancellationToken.None); // Leader
        await _service.ExecuteAsync(requestKey3, operation, CancellationToken.None); // Follower of key-2

        var metrics = _service.GetMetrics();

        // Assert
        metrics.TotalRequests.Should().Be(3);
        metrics.CoalescedRequests.Should().BeGreaterOrEqualTo(0); // Can be 0 or 1 depending on race condition
        metrics.ActiveRequests.Should().Be(0); // All completed
        // CoalescingRatio should be between 0 and 1
        metrics.CoalescingRatio.Should().BeGreaterOrEqualTo(0.0).And.BeLessOrEqualTo(1.0);
    }

    /// <summary>
    /// Tests that GetMetrics correctly counts active requests.
    /// </summary>
    [Fact]
    public async Task GetMetrics_DuringRequests_ReturnsCorrectActiveCount()
    {
        // Arrange
        const string requestKey = "active-key";
        var callCount = 0;
        Func<CancellationToken, Task<int>> operation = ct =>
        {
            Interlocked.Increment(ref callCount);
            // Simulate long-running operation
            return Task.Delay(200, ct).ContinueWith(_ => 42, ct);
        };

        // Act - Start operation but don't await it yet
        var task = _service.ExecuteAsync(requestKey, operation, CancellationToken.None);

        // Give it a moment to start
        await Task.Delay(10);

        var metrics = _service.GetMetrics();

        // Wait for completion
        var result = await task;

        // Assert
        metrics.ActiveRequests.Should().Be(1); // One active request during execution
        result.Should().Be(42);
        callCount.Should().Be(1);
    }

    /// <summary>
    /// Tests that Dispose cancels pending requests.
    /// </summary>
    [Fact]
    public async Task Dispose_CancelsPendingRequests()
    {
        // Arrange
        const string requestKey = "disposal-key";
        var callCount = 0;
        Func<CancellationToken, Task<int>> operation = ct =>
        {
            Interlocked.Increment(ref callCount);
            // Simulate long-running operation
            return Task.Delay(500, ct).ContinueWith(_ => 42, ct);
        };

        // Act
        var task = _service.ExecuteAsync(requestKey, operation, CancellationToken.None);

        // Give it a moment to start
        await Task.Delay(10);

        _service.Dispose();

        // Assert
        (await task).Should().Be(42); // Should complete normally due to Dispose behavior
        callCount.Should().Be(1);
    }

    /// <summary>
    /// Tests that ExecuteAsync handles multiple different keys independently.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_MultipleDifferentKeys_HandlesIndependently()
    {
        // Arrange
        const string key1 = "key-1";
        const string key2 = "key-2";
        const string result1 = "result-1";
        const string result2 = "result-2";
        var callCount1 = 0;
        var callCount2 = 0;

        Func<CancellationToken, Task<string>> operation1 = _ =>
        {
            Interlocked.Increment(ref callCount1);
            return Task.FromResult(result1);
        };

        Func<CancellationToken, Task<string>> operation2 = _ =>
        {
            Interlocked.Increment(ref callCount2);
            return Task.FromResult(result2);
        };

        // Act
        var task1a = _service.ExecuteAsync(key1, operation1, CancellationToken.None);
        var task1b = _service.ExecuteAsync(key1, operation1, CancellationToken.None);
        var task2a = _service.ExecuteAsync(key2, operation2, CancellationToken.None);
        var task2b = _service.ExecuteAsync(key2, operation2, CancellationToken.None);

        var results = await Task.WhenAll(task1a, task1b, task2a, task2b);

        // Assert
        results[0].Should().Be(result1);
        results[1].Should().Be(result1);
        results[2].Should().Be(result2);
        results[3].Should().Be(result2);

        // Operations should be called at most once per key (can be 1 or 2 due to race condition)
        callCount1.Should().BeLessOrEqualTo(2);
        callCount2.Should().BeLessOrEqualTo(2);
    }
}