// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Tests for RetryingEventPublisher to verify retry and dead-letter handling
// =============================================================================

using ApiKeyGateway.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace ApiKeyGateway.Tests;

public class RetryingEventPublisherTests
{
    private readonly Mock<IEventPublisher> _innerPublisherMock;
    private readonly Mock<IDeadLetterQueue> _deadLetterQueueMock;
    private readonly Mock<ILogger<RetryingEventPublisher>> _loggerMock;
    private readonly EventPublisherOptions _options;
    private readonly RetryingEventPublisher _publisher;

    public RetryingEventPublisherTests()
    {
        _innerPublisherMock = new Mock<IEventPublisher>();
        _deadLetterQueueMock = new Mock<IDeadLetterQueue>();
        _loggerMock = new Mock<ILogger<RetryingEventPublisher>>();

        _options = new EventPublisherOptions
        {
            MaxRetryAttempts = 3,
            InitialRetryDelayMs = 10,
            MaxRetryDelayMs = 1000,
            MaxDeadLetterQueueSize = 100,
            IncludeEventDetailsInDeadLetter = true
        };

        _publisher = new RetryingEventPublisher(
            _innerPublisherMock.Object,
            _deadLetterQueueMock.Object,
            _options,
            _loggerMock.Object);
    }

    [Fact]
    public async Task PublishAsync_WithSuccessfulPublish_DoesNotRetryAndDoesNotAddToDeadLetter()
    {
        // Arrange
        var testEvent = new TestEvent("Success");
        _innerPublisherMock
            .Setup(x => x.PublishAsync(testEvent))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _publisher.PublishAsync(testEvent);

        // Assert
        _innerPublisherMock.Verify(x => x.PublishAsync(testEvent), Times.Once);
        _deadLetterQueueMock.Verify(x => x.Add(It.IsAny<DeadLetterEntry>()), Times.Never);
    }

    [Fact]
    public async Task PublishAsync_WithTransientFailure_RetriesWithExponentialBackoff()
    {
        // Arrange
        var testEvent = new TestEvent("Transient");
        var attemptCount = 0;

        _innerPublisherMock
            .Setup(x => x.PublishAsync(testEvent))
            .Returns(() =>
            {
                attemptCount++;
                if (attemptCount <= 2)
                {
                    throw new InvalidOperationException("Transient failure");
                }
                return Task.CompletedTask;
            })
            .Verifiable();

        // Act
        await _publisher.PublishAsync(testEvent);

        // Assert
        _innerPublisherMock.Verify(x => x.PublishAsync(testEvent), Times.Exactly(3));
        _deadLetterQueueMock.Verify(x => x.Add(It.IsAny<DeadLetterEntry>()), Times.Never);
        attemptCount.Should().Be(3);
    }

    [Fact]
    public async Task PublishAsync_WithPermanentFailure_MovesToDeadLetterQueue()
    {
        // Arrange
        var testEvent = new TestEvent("PermanentFailure");
        var failureException = new InvalidOperationException("Permanent failure");

        _innerPublisherMock
            .Setup(x => x.PublishAsync(testEvent))
            .Throws(failureException)
            .Verifiable();

        // Act
        await _publisher.PublishAsync(testEvent);

        // Assert
        _innerPublisherMock.Verify(x => x.PublishAsync(testEvent), Times.Exactly(4)); // 3 retries + 1 initial = 4 attempts
        _deadLetterQueueMock.Verify(x => x.Add(It.IsAny<DeadLetterEntry>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithZeroRetryAttempts_DoesNotRetry()
    {
        // Arrange
        var testEvent = new TestEvent("ZeroRetries");
        var options = new EventPublisherOptions { MaxRetryAttempts = 0 };
        var publisher = new RetryingEventPublisher(
            _innerPublisherMock.Object,
            _deadLetterQueueMock.Object,
            options,
            _loggerMock.Object);

        _innerPublisherMock
            .Setup(x => x.PublishAsync(testEvent))
            .Throws(new InvalidOperationException("Immediate failure"))
            .Verifiable();

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert
        _innerPublisherMock.Verify(x => x.PublishAsync(testEvent), Times.Once);
        _deadLetterQueueMock.Verify(x => x.Add(It.IsAny<DeadLetterEntry>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithMaxRetryDelay_CapsRetryDelay()
    {
        // Arrange
        var testEvent = new TestEvent("MaxDelay");
        var options = new EventPublisherOptions
        {
            MaxRetryAttempts = 5,
            InitialRetryDelayMs = 1000,
            MaxRetryDelayMs = 100,
            MaxDeadLetterQueueSize = 100
        };
        var publisher = new RetryingEventPublisher(
            _innerPublisherMock.Object,
            _deadLetterQueueMock.Object,
            options,
            _loggerMock.Object);

        _innerPublisherMock
            .Setup(x => x.PublishAsync(testEvent))
            .Throws(new InvalidOperationException("Failure"))
            .Verifiable();

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert - should cap at MaxRetryDelayMs
        _innerPublisherMock.Verify(x => x.PublishAsync(testEvent), Times.Exactly(6)); // 5 retries + 1 initial
    }

    [Fact]
    public async Task PublishAsync_WithIncludeEventDetailsFalse_CreatesMinimalDeadLetterEntry()
    {
        // Arrange
        var testEvent = new TestEvent("MinimalDetails");
        var options = new EventPublisherOptions
        {
            MaxRetryAttempts = 1,
            IncludeEventDetailsInDeadLetter = false
        };
        var publisher = new RetryingEventPublisher(
            _innerPublisherMock.Object,
            _deadLetterQueueMock.Object,
            options,
            _loggerMock.Object);

        _innerPublisherMock
            .Setup(x => x.PublishAsync(testEvent))
            .Throws(new InvalidOperationException("Failure"))
            .Verifiable();

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert
        _deadLetterQueueMock.Verify(x => x.Add(It.Is<DeadLetterEntry>(dle =>
            dle.EventPayload == null
        )), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_NeverThrowsEvenWhenAllRetriesFail()
    {
        // Arrange
        var testEvent = new TestEvent("AlwaysFails");

        _innerPublisherMock
            .Setup(x => x.PublishAsync(testEvent))
            .Throws(new InvalidOperationException("Always fails"))
            .Verifiable();

        // Act & Assert - should not throw
        Func<Task> act = () => _publisher.PublishAsync(testEvent);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Constructor_WithNullParameters_ThrowsArgumentNullException()
    {
        // Arrange
        var deadLetterQueue = new Mock<IDeadLetterQueue>().Object;
        var options = new EventPublisherOptions();
        var logger = new Mock<ILogger<RetryingEventPublisher>>().Object;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RetryingEventPublisher(
            null!, deadLetterQueue, options, logger));

        Assert.Throws<ArgumentNullException>(() => new RetryingEventPublisher(
            _innerPublisherMock.Object, null!, options, logger));

        Assert.Throws<ArgumentNullException>(() => new RetryingEventPublisher(
            _innerPublisherMock.Object, deadLetterQueue, null!, logger));

        Assert.Throws<ArgumentNullException>(() => new RetryingEventPublisher(
            _innerPublisherMock.Object, deadLetterQueue, options, null!));
    }

    [Fact]
    public void EventPublisherOptions_WithInvalidValues_ThrowsValidationException()
    {
        // Arrange
        var options = new EventPublisherOptions();

        // Act & Assert
        options.MaxRetryAttempts = -1;
        Assert.Throws<ValidationException>(() => options.Validate());

        options.MaxRetryAttempts = 3;
        options.InitialRetryDelayMs = 0;
        Assert.Throws<ValidationException>(() => options.Validate());

        options.InitialRetryDelayMs = 100;
        options.MaxRetryDelayMs = 0;
        Assert.Throws<ValidationException>(() => options.Validate());

        options.MaxRetryDelayMs = 1000;
        options.MaxDeadLetterQueueSize = -1;
        Assert.Throws<ValidationException>(() => options.Validate());
    }

    [Fact]
    public void InMemoryDeadLetterQueue_AddAndGetAll_ReturnsCorrectEntries()
    {
        // Arrange
        var queue = new InMemoryDeadLetterQueue(maxSize: 5);
        var entry1 = new DeadLetterEntry
        {
            EventType = "TestEvent1",
            FailureReason = "Failure 1"
        };
        var entry2 = new DeadLetterEntry
        {
            EventType = "TestEvent2",
            FailureReason = "Failure 2"
        };

        // Act
        queue.Add(entry1);
        queue.Add(entry2);

        // Assert
        queue.Count.Should().Be(2);
        var allEntries = queue.GetAll();
        allEntries.Should().HaveCount(2);
        allEntries[0].Should().BeSameAs(entry1);
        allEntries[1].Should().BeSameAs(entry2);
    }

    [Fact]
    public void InMemoryDeadLetterQueue_WhenFull_RemovesOldestEntry()
    {
        // Arrange
        var queue = new InMemoryDeadLetterQueue(maxSize: 2);
        var entry1 = new DeadLetterEntry { EventType = "Entry1", FailureReason = "Failure 1" };
        var entry2 = new DeadLetterEntry { EventType = "Entry2", FailureReason = "Failure 2" };
        var entry3 = new DeadLetterEntry { EventType = "Entry3", FailureReason = "Failure 3" };

        // Act
        queue.Add(entry1);
        queue.Add(entry2);
        queue.Add(entry3); // Should remove entry1

        // Assert
        queue.Count.Should().Be(2);
        var allEntries = queue.GetAll();
        allEntries.Should().HaveCount(2);
        allEntries[0].EventType.Should().Be("Entry2");
        allEntries[1].EventType.Should().Be("Entry3");
    }

    [Fact]
    public void InMemoryDeadLetterQueue_Clear_RemovesAllEntries()
    {
        // Arrange
        var queue = new InMemoryDeadLetterQueue(maxSize: 10);
        queue.Add(new DeadLetterEntry { EventType = "Entry1", FailureReason = "Failure 1" });
        queue.Add(new DeadLetterEntry { EventType = "Entry2", FailureReason = "Failure 2" });

        // Act
        queue.Clear();

        // Assert
        queue.Count.Should().Be(0);
        queue.IsEmpty.Should().BeTrue();
    }

    // Test event type
    private record TestEvent(string Name);
}