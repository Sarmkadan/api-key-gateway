// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Tests for InMemoryEventPublisher to verify event publishing behavior
// =============================================================================

using ApiKeyGateway.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiKeyGateway.Tests;

public class EventPublisherTests
{
    private readonly Mock<ILogger<InMemoryEventPublisher>> _loggerMock;
    private readonly InMemoryEventPublisher _publisher;

    public EventPublisherTests()
    {
        _loggerMock = new Mock<ILogger<InMemoryEventPublisher>>();
        _publisher = new InMemoryEventPublisher(_loggerMock.Object);
    }

    [Fact]
    public async Task PublishAsync_WithZeroSubscribers_DoesNotThrowAndLogsDebug()
    {
        // Arrange
        var testEvent = new TestEvent("Test");

        // Act
        var act = () => _publisher.PublishAsync(testEvent);

        // Assert
        await act.Should().NotThrowAsync();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No subscribers for event type TestEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithOneSubscriber_InvokesSubscriberOnce()
    {
        // Arrange
        var testEvent = new TestEvent("Single");
        var subscriberCalled = false;
        var subscriberEvent = (TestEvent?)null;

        void Subscriber(TestEvent e)
        {
            subscriberCalled = true;
            subscriberEvent = e;
        }

        _publisher.Subscribe<TestEvent>(e => Task.Run(() => Subscriber(e)));

        // Act
        await _publisher.PublishAsync(testEvent);

        // Assert
        subscriberCalled.Should().BeTrue();
        subscriberEvent.Should().BeSameAs(testEvent);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Publishing TestEvent to 1 subscribers")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleSubscribers_AllSubscribersInvoked()
    {
        // Arrange
        var testEvent = new TestEvent("Multiple");
        var invocationOrder = new List<int>();
        var subscriber1Called = false;
        var subscriber2Called = false;
        var subscriber3Called = false;

        _publisher.Subscribe<TestEvent>(e => Task.Run(() =>
        {
            invocationOrder.Add(1);
            subscriber1Called = true;
        }));

        _publisher.Subscribe<TestEvent>(e => Task.Run(() =>
        {
            invocationOrder.Add(2);
            subscriber2Called = true;
        }));

        _publisher.Subscribe<TestEvent>(e => Task.Run(() =>
        {
            invocationOrder.Add(3);
            subscriber3Called = true;
        }));

        // Act
        await _publisher.PublishAsync(testEvent);

        // Assert
        subscriber1Called.Should().BeTrue();
        subscriber2Called.Should().BeTrue();
        subscriber3Called.Should().BeTrue();
        invocationOrder.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Publishing TestEvent to 3 subscribers")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_SubscriberThrows_ContinuesToNextSubscriber()
    {
        // Arrange
        var testEvent = new TestEvent("Throwing");
        var successfulSubscriberCalled = false;
        var throwingSubscriberCalled = false;

        _publisher.Subscribe<TestEvent>(e => Task.Run(() =>
        {
            throwingSubscriberCalled = true;
            throw new InvalidOperationException("Test exception");
        }));

        _publisher.Subscribe<TestEvent>(e => Task.Run(() =>
        {
            successfulSubscriberCalled = true;
        }));

        // Act
        var act = () => _publisher.PublishAsync(testEvent);

        // Assert
        await act.Should().NotThrowAsync();
        throwingSubscriberCalled.Should().BeTrue();
        successfulSubscriberCalled.Should().BeTrue();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error in event handler for TestEvent")),
                It.Is<InvalidOperationException>(ex => ex.Message == "Test exception"),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_DifferentEventTypes_OnlySubscribersForThatTypeInvoked()
    {
        // Arrange
        var event1 = new TestEvent("Type1");
        var event2 = new TestEvent("Type2");
        var event3 = new DifferentTestEvent("Type3");
        var testEventSubscriberCalled = false;
        var differentEventSubscriberCalled = false;

        _publisher.Subscribe<TestEvent>(e => Task.Run(() => testEventSubscriberCalled = true));
        _publisher.Subscribe<DifferentTestEvent>(e => Task.Run(() => differentEventSubscriberCalled = true));

        // Act
        await _publisher.PublishAsync(event1);
        await _publisher.PublishAsync(event2);
        await _publisher.PublishAsync(event3);

        // Assert
        testEventSubscriberCalled.Should().BeTrue();
        differentEventSubscriberCalled.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_SameSubscriberRegisteredMultipleTimes_InvokedMultipleTimes()
    {
        // Arrange
        var testEvent = new TestEvent("MultipleRegistrations");
        var invocationCount = 0;

        Task Handler(TestEvent e)
        {
            invocationCount++;
            return Task.CompletedTask;
        }

        _publisher.Subscribe<TestEvent>(Handler);
        _publisher.Subscribe<TestEvent>(Handler);
        _publisher.Subscribe<TestEvent>(Handler);

        // Act
        await _publisher.PublishAsync(testEvent);

        // Assert
        invocationCount.Should().Be(3);
    }

    [Fact]
    public async Task Subscribe_RegistersHandlerForEventType()
    {
        // Arrange
        var handlerCalled = false;

        // Act
        _publisher.Subscribe<TestEvent>(e => Task.Run(() => handlerCalled = true));

        // Assert
        handlerCalled.Should().BeFalse(); // Not called yet
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Event subscriber registered for TestEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    // Test event types
    private record TestEvent(string Name);
    private record DifferentTestEvent(string Name);
}