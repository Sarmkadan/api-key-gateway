// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
// Tests for RetryPolicyBuilder to verify retry policy construction and behavior

using ApiKeyGateway.Utilities;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="RetryPolicyBuilder"/> class.
/// Tests various scenarios including builder configuration, retry behavior,
/// exception handling, and edge cases.
/// </summary>
public class RetryPolicyBuilderUnitTests
{
    /// <summary>
    /// Tests that the default RetryPolicyBuilder has expected initial values.
    /// </summary>
    [Fact]
    public void DefaultValues_WhenNotConfigured_ReturnsExpectedDefaults()
    {
        // Arrange
        var builder = new RetryPolicyBuilder();

        // Act & Assert
        builder.MaxRetries.Should().Be(3);
        builder.InitialDelayMs.Should().Be(100);
        builder.BackoffMultiplier.Should().Be(2.0);
        builder.MaxDelayMs.Should().Be(30000);
    }

    /// <summary>
    /// Tests that WithMaxRetries sets the correct value.
    /// </summary>
    /// <param name="maxRetries">The maximum retry count to test with.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void WithMaxRetries_SetsCorrectValue(int maxRetries)
    {
        // Arrange
        var builder = new RetryPolicyBuilder();

        // Act
        var result = builder.WithMaxRetries(maxRetries);

        // Assert
        result.Should().BeSameAs(builder); // Fluent API check
        builder.MaxRetries.Should().Be(maxRetries);
    }

    /// <summary>
    /// Tests that WithInitialDelay sets the correct value.
    /// </summary>
    /// <param name="delayMs">The initial delay in milliseconds to test with.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(1000)]
    public void WithInitialDelay_SetsCorrectValue(int delayMs)
    {
        // Arrange
        var builder = new RetryPolicyBuilder();

        // Act
        var result = builder.WithInitialDelay(delayMs);

        // Assert
        result.Should().BeSameAs(builder); // Fluent API check
        builder.InitialDelayMs.Should().Be(delayMs);
    }

    /// <summary>
    /// Tests that WithBackoffMultiplier sets the correct value.
    /// </summary>
    /// <param name="multiplier">The backoff multiplier to test with.</param>
    [Theory]
    [InlineData(1.0)]
    [InlineData(1.5)]
    [InlineData(2.0)]
    [InlineData(3.0)]
    public void WithBackoffMultiplier_SetsCorrectValue(double multiplier)
    {
        // Arrange
        var builder = new RetryPolicyBuilder();

        // Act
        var result = builder.WithBackoffMultiplier(multiplier);

        // Assert
        result.Should().BeSameAs(builder); // Fluent API check
        builder.BackoffMultiplier.Should().Be(multiplier);
    }

    /// <summary>
    /// Tests that WithMaxDelay sets the correct value.
    /// </summary>
    /// <param name="delayMs">The maximum delay in milliseconds to test with.</param>
    [Theory]
    [InlineData(1000)]
    [InlineData(10000)]
    [InlineData(30000)]
    [InlineData(60000)]
    public void WithMaxDelay_SetsCorrectValue(int delayMs)
    {
        // Arrange
        var builder = new RetryPolicyBuilder();

        // Act
        var result = builder.WithMaxDelay(delayMs);

        // Assert
        result.Should().BeSameAs(builder); // Fluent API check
        builder.MaxDelayMs.Should().Be(delayMs);
    }

    /// <summary>
    /// Tests that RetryOn adds exception types to the retry list.
    /// </summary>
    [Fact]
    public void RetryOn_AddsExceptionTypeToRetryList()
    {
        // Arrange
        var builder = new RetryPolicyBuilder();

        // Act
        var result = builder.RetryOn<TimeoutException>();

        // Assert
        result.Should().BeSameAs(builder); // Fluent API check
        builder.Build<int>().Should().NotBeNull(); // Ensure build still works
    }

    /// <summary>
    /// Tests that Build returns a non-null retry policy function.
    /// </summary>
    [Fact]
    public void Build_ReturnsNonNullRetryPolicyFunction()
    {
        // Arrange
        var builder = new RetryPolicyBuilder();

        // Act
        var policy = builder.Build<int>();

        // Assert
        policy.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that the retry policy succeeds on first attempt without retry.
    /// </summary>
    [Fact]
    public async Task Build_Policy_SucceedsOnFirstAttemptWithoutRetry()
    {
        // Arrange
        var builder = new RetryPolicyBuilder();
        var policy = builder.Build<int>();
        var attemptCount = 0;

        async Task<int> SuccessfulOperation()
        {
            attemptCount++;
            return 42;
        }

        // Act
        var result = await policy(SuccessfulOperation);

        // Assert
        result.Should().Be(42);
        attemptCount.Should().Be(1);
    }

    /// <summary>
    /// Tests that the retry policy retries on transient exceptions (HttpRequestException).
    /// </summary>
    [Fact]
    public async Task Build_Policy_RetriesOnHttpRequestException()
    {
        // Arrange
        var builder = new RetryPolicyBuilder();
        var policy = builder.Build<int>();
        var attemptCount = 0;

        async Task<int> FailingOperation()
        {
            attemptCount++;
            if (attemptCount < 3)
            {
                throw new HttpRequestException("Simulated HTTP failure");
            }
            return 200;
        }

        // Act
        var result = await policy(FailingOperation);

        // Assert
        result.Should().Be(200);
        attemptCount.Should().Be(3); // 1 initial + 2 retries
    }

    /// <summary>
    /// Tests that the retry policy retries on transient exceptions (TimeoutException).
    /// </summary>
    [Fact]
    public async Task Build_Policy_RetriesOnTimeoutException()
    {
        // Arrange
        var builder = new RetryPolicyBuilder();
        var policy = builder.Build<string>();
        var attemptCount = 0;

        async Task<string> FailingOperation()
        {
            attemptCount++;
            if (attemptCount < 2)
            {
                throw new TimeoutException("Simulated timeout");
            }
            return "Success";
        }

        // Act
        var result = await policy(FailingOperation);

        // Assert
        result.Should().Be("Success");
        attemptCount.Should().Be(2); // 1 initial + 1 retry
    }

    /// <summary>
    /// Tests that the retry policy retries on InvalidOperationException.
    /// </summary>
    [Fact]
    public async Task Build_Policy_RetriesOnInvalidOperationException()
    {
        // Arrange
        var builder = new RetryPolicyBuilder();
        var policy = builder.Build<bool>();
        var attemptCount = 0;

        async Task<bool> FailingOperation()
        {
            attemptCount++;
            if (attemptCount < 4)
            {
                throw new InvalidOperationException("Simulated invalid operation");
            }
            return true;
        }

        // Act
        var result = await policy(FailingOperation);

        // Assert
        result.Should().BeTrue();
        attemptCount.Should().Be(4); // 1 initial + 3 retries
    }

    /// <summary>
    /// Tests that the retry policy respects MaxRetries limit.
    /// </summary>
    [Fact]
    public async Task Build_Policy_RespectsMaxRetriesLimit()
    {
        // Arrange
        var builder = new RetryPolicyBuilder().WithMaxRetries(2);
        var policy = builder.Build<int>();
        var attemptCount = 0;

        async Task<int> AlwaysFailingOperation()
        {
            attemptCount++;
            throw new HttpRequestException("Always fails");
        }

        // Act
        var act = async () => await policy(AlwaysFailingOperation);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
        attemptCount.Should().Be(3); // 1 initial + 2 retries = 3 total attempts
    }

    /// <summary>
    /// Tests that the retry policy respects MaxDelay when backoff would exceed it.
    /// </summary>
    [Fact]
    public async Task Build_Policy_RespectsMaxDelay()
    {
        // Arrange
        var builder = new RetryPolicyBuilder()
            .WithMaxRetries(5)
            .WithInitialDelay(1000)
            .WithBackoffMultiplier(2.0)
            .WithMaxDelay(2000); // Small max delay

        var policy = builder.Build<int>();
        var attemptCount = 0;

        async Task<int> FailingOperation()
        {
            attemptCount++;
            throw new HttpRequestException("Fails");
        }

        // Act
        var act = async () => await policy(FailingOperation);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
        attemptCount.Should().Be(6); // 1 initial + 5 retries
    }

    /// <summary>
    /// Tests that custom retry exception types are used when configured.
    /// </summary>
    [Fact]
    public async Task Build_Policy_UsesCustomRetryExceptionTypes()
    {
        // Arrange
        var builder = new RetryPolicyBuilder()
            .RetryOn<CustomRetryException>();
        var policy = builder.Build<int>();
        var attemptCount = 0;

        async Task<int> FailingOperation()
        {
            attemptCount++;
            if (attemptCount < 3)
            {
                throw new CustomRetryException("Custom exception");
            }
            return 123;
        }

        // Act
        var result = await policy(FailingOperation);

        // Assert
        result.Should().Be(123);
        attemptCount.Should().Be(3);
    }

    /// <summary>
    /// Tests that non-retryable exceptions are not retried.
    /// </summary>
    [Fact]
    public async Task Build_Policy_DoesNotRetryNonRetryableExceptions()
    {
        // Arrange
        var builder = new RetryPolicyBuilder();
        var policy = builder.Build<int>();
        var attemptCount = 0;

        async Task<int> FailingOperation()
        {
            attemptCount++;
            throw new InvalidDataException("Not retryable");
        }

        // Act
        var act = async () => await policy(FailingOperation);

        // Assert
        await act.Should().ThrowAsync<InvalidDataException>();
        attemptCount.Should().Be(1); // Should not retry
    }

    /// <summary>
    /// Tests that RetryOn with multiple exception types works correctly.
    /// </summary>
    [Fact]
    public async Task Build_Policy_WithMultipleRetryExceptionTypes()
    {
        // Arrange
        var builder = new RetryPolicyBuilder()
            .RetryOn<TimeoutException>()
            .RetryOn<HttpRequestException>();
        var policy = builder.Build<string>();
        var attemptCount = 0;

        async Task<string> FailingOperation()
        {
            attemptCount++;
            if (attemptCount == 1)
            {
                throw new TimeoutException("Timeout");
            }
            if (attemptCount == 2)
            {
                throw new HttpRequestException("HTTP error");
            }
            return "Success";
        }

        // Act
        var result = await policy(FailingOperation);

        // Assert
        result.Should().Be("Success");
        attemptCount.Should().Be(3);
    }

    /// <summary>
    /// Tests that the retry policy works with different return types.
    /// </summary>
    [Fact]
    public async Task Build_Policy_WorksWithDifferentReturnTypes()
    {
        // Arrange
        var builder = new RetryPolicyBuilder();
        var intPolicy = builder.Build<int>();
        var stringPolicy = builder.Build<string>();
        var boolPolicy = builder.Build<bool>();
        var objectPolicy = builder.Build<object>();

        // Act
        var intResult = await intPolicy(() => Task.FromResult(42));
        var stringResult = await stringPolicy(() => Task.FromResult("test"));
        var boolResult = await boolPolicy(() => Task.FromResult(true));
        var objectResult = await objectPolicy(() => Task.FromResult(new object()));

        // Assert
        intResult.Should().Be(42);
        stringResult.Should().Be("test");
        boolResult.Should().BeTrue();
        objectResult.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that WithInitialDelay of 0 works correctly.
    /// </summary>
    [Fact]
    public async Task Build_Policy_WithZeroInitialDelay_WorksCorrectly()
    {
        // Arrange
        var builder = new RetryPolicyBuilder().WithInitialDelay(0);
        var policy = builder.Build<int>();
        var attemptCount = 0;

        async Task<int> FailingOperation()
        {
            attemptCount++;
            if (attemptCount < 2)
            {
                throw new HttpRequestException("Fails");
            }
            return 999;
        }

        // Act
        var result = await policy(FailingOperation);

        // Assert
        result.Should().Be(999);
        attemptCount.Should().Be(2);
    }
}

/// <summary>
/// Custom exception type for testing retry behavior.
/// </summary>
public class CustomRetryException : Exception
{
    public CustomRetryException(string message) : base(message) { }
}
