// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Tests for InMemoryEventPublisher subscriber isolation and thread-safety
// =====================================================================

using ApiKeyGateway.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiKeyGateway.Tests;

public class InMemoryEventPublisherTests
{
    private readonly Mock<ILogger<InMemoryEventPublisher>> _loggerMock;
    private readonly InMemoryEventPublisher _publisher;

    public InMemoryEventPublisherTests()
    {
        _loggerMock = new Mock<ILogger<InMemoryEventPublisher>>();
        _publisher = new InMemoryEventPublisher(_loggerMock.Object);
    }

    [Fact]
    public async Task PublishAsync_ExceptionInOneSubscriber_DoesNotPreventOtherSubscribersFromReceivingEvent()
    {
        // Arrange
        var testEvent = new TestEvent("ExceptionPropagation");
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
        throwingSubscriberCalled.Should().BeTrue("Throwing subscriber should be called");
        successfulSubscriberCalled.Should().BeTrue("Successful subscriber should still be called despite error in first subscriber");

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
    public async Task PublishAsync_WithZeroSubscribers_DoesNotThrowAndLogsDebug()
    {
        // Arrange
        var testEvent = new TestEvent("NoSubscribers");

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
    public async Task PublishAsync_SubscribingUnsubscribingDuringPublish_ThreadSafetyMaintained()
    {
        // Arrange
        var testEvent = new TestEvent("ConcurrentModifications");
        var handler2Called = false;

        // Create handlers
        Task Handler1(TestEvent e) { return Task.CompletedTask; }
        Task Handler2(TestEvent e) { handler2Called = true; return Task.CompletedTask; }
        Task Handler3(TestEvent e) { return Task.CompletedTask; }

        _publisher.Subscribe<TestEvent>(Handler1);
        _publisher.Subscribe<TestEvent>(Handler2);

        // Act - publish while modifying subscribers
        var publishTask = _publisher.PublishAsync(testEvent);

        // Unsubscribe and subscribe concurrently with publish
        _publisher.Unsubscribe<TestEvent>(Handler1);
        _publisher.Subscribe<TestEvent>(Handler3);

        // Wait for publish to complete
        await publishTask;

        // Assert - all handlers that were subscribed at the time of publish should be called
        handler2Called.Should().BeTrue("Handler2 should be called");

        // Verify thread-safe logging occurred
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Publishing TestEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_MultipleConcurrentPublishes_ThreadSafetyMaintained()
    {
        // Arrange
        var event1 = new TestEvent("ConcurrentPublish1");
        var event2 = new TestEvent("ConcurrentPublish2");
        var handler1Called = new TaskCompletionSource<bool>();
        var handler2Called = new TaskCompletionSource<bool>();
        var invocationCount = 0;

        Task SlowHandler(TestEvent e)
        {
            invocationCount++;
            if (invocationCount == 1)
            {
                // Simulate slow handler to force concurrent execution
                Thread.Sleep(100);
            }
            handler1Called.SetResult(true);
            return Task.CompletedTask;
        }

        Task FastHandler(TestEvent e)
        {
            handler2Called.SetResult(true);
            return Task.CompletedTask;
        }

        _publisher.Subscribe<TestEvent>(SlowHandler);
        _publisher.Subscribe<TestEvent>(FastHandler);

        // Act - publish concurrently
        var publishTask1 = _publisher.PublishAsync(event1);
        var publishTask2 = _publisher.PublishAsync(event2);

        await Task.WhenAll(publishTask1, publishTask2);

        // Wait for handlers to complete
        await Task.WhenAll(handler1Called.Task, handler2Called.Task);

        // Assert - both handlers should be called for each event
        invocationCount.Should().Be(2, "SlowHandler should be called twice (once per event)");
    }

    [Fact]
    public async Task Subscribe_SameHandlerRegisteredMultipleTimes_InvokedMultipleTimes()
    {
        // Arrange
        var testEvent = new TestEvent("MultipleRegistrations");
        var invocationCount = 0;

        Task Handler(TestEvent e)
        {
            invocationCount++;
            return Task.CompletedTask;
        }

        // Subscribe the same handler multiple times
        _publisher.Subscribe<TestEvent>(Handler);
        _publisher.Subscribe<TestEvent>(Handler);
        _publisher.Subscribe<TestEvent>(Handler);

        // Act
        await _publisher.PublishAsync(testEvent);

        // Assert - handler should be invoked 3 times
        invocationCount.Should().Be(3);
    }

    [Fact]
    public void Unsubscribe_RegisteredHandler_RemovesHandler()
    {
        // Arrange
        var handlerCalled = false;
        Task Handler(TestEvent e)
        {
            handlerCalled = true;
            return Task.CompletedTask;
        }

        _publisher.Subscribe<TestEvent>(Handler);

        // Act - unsubscribe
        _publisher.Unsubscribe<TestEvent>(Handler);

        // Publish should not call the handler
        _publisher.PublishAsync(new TestEvent("AfterUnsubscribe"));

        // Assert
        handlerCalled.Should().BeFalse("Handler should not be called after unsubscribe");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Event subscriber unregistered for TestEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public void Unsubscribe_UnregisteringNonExistentHandler_DoesNotThrow()
    {
        // Arrange
        Task Handler1(TestEvent e) => Task.CompletedTask;
        Task Handler2(TestEvent e) => Task.CompletedTask;

        _publisher.Subscribe<TestEvent>(Handler1);

        // Act - try to unsubscribe a different handler
        var act = () => _publisher.Unsubscribe<TestEvent>(Handler2);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Unsubscribe_NullHandler_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _publisher.Unsubscribe<TestEvent>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Subscribe_NullHandler_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _publisher.Subscribe<TestEvent>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task PublishAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _publisher.PublishAsync<TestEvent>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Unsubscribe_CleansUpEmptyEventTypeLists_PreventsMemoryLeaks()
    {
        // Arrange
        Task Handler(TestEvent e) => Task.CompletedTask;

        _publisher.Subscribe<TestEvent>(Handler);

        // Verify event type is registered
        var subscribersField = _publisher.GetType().GetField("_subscribers",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var subscribersDict = subscribersField?.GetValue(_publisher) as Dictionary<Type, List<Delegate>>;
        var hasEventTypeBefore = subscribersDict?.ContainsKey(typeof(TestEvent)) ?? false;
        hasEventTypeBefore.Should().BeTrue("Event type should be registered before unsubscribe");

        // Act - unsubscribe
        _publisher.Unsubscribe<TestEvent>(Handler);

        // Verify event type is removed
        var hasEventTypeAfter = subscribersDict?.ContainsKey(typeof(TestEvent)) ?? false;
        hasEventTypeAfter.Should().BeFalse("Event type should be removed after last handler is unsubscribed");
    }

    [Fact]
    public async Task PublishAsync_WithManySubscribers_AllInvokedInOrder()
    {
        // Arrange
        var testEvent = new TestEvent("ManySubscribers");
        var invocationOrder = new List<int>();

        for (int i = 0; i < 100; i++)
        {
            int index = i;
            _publisher.Subscribe<TestEvent>(e => Task.Run(() => invocationOrder.Add(index)));
        }

        // Act
        await _publisher.PublishAsync(testEvent);

        // Assert - all subscribers should be called in registration order
        invocationOrder.Should().HaveCount(100);
        invocationOrder.Should().BeInAscendingOrder("Subscribers should be called in registration order");
    }

    [Fact]
    public async Task PublishAsync_ConcurrentSubscribeUnsubscribeChurn_NoExceptionsOrMemoryLeaks()
    {
        // Arrange
        var testEvent = new TestEvent("ChurnTest");
        var exceptions = new List<Exception>();
        var iterations = 1000;

        // Act - perform rapid subscribe/unsubscribe operations
        var tasks = new List<Task>();
        for (int i = 0; i < iterations; i++)
        {
            int iteration = i;
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    Task Handler(TestEvent e) => Task.CompletedTask;
                    _publisher.Subscribe<TestEvent>(Handler);
                    if (iteration % 2 == 0)
                    {
                        _publisher.Unsubscribe<TestEvent>(Handler);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - no exceptions should occur
        exceptions.Should().BeEmpty("No exceptions should occur during concurrent subscribe/unsubscribe operations");

        // Publish should still work after churn
        var act = () => _publisher.PublishAsync(testEvent);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_DifferentEventTypes_OnlySubscribersForThatTypeInvoked()
    {
        // Arrange
        var event1 = new TestEvent("Type1");
        var event2 = new DifferentTestEvent("Type2");
        var testEventSubscriberCalled = false;
        var differentEventSubscriberCalled = false;

        _publisher.Subscribe<TestEvent>(e => Task.Run(() => testEventSubscriberCalled = true));
        _publisher.Subscribe<DifferentTestEvent>(e => Task.Run(() => differentEventSubscriberCalled = true));

        // Act
        await _publisher.PublishAsync(event1);
        await _publisher.PublishAsync(event2);

        // Assert
        testEventSubscriberCalled.Should().BeTrue("TestEvent subscriber should be called");
        differentEventSubscriberCalled.Should().BeTrue("DifferentTestEvent subscriber should be called");
    }

    [Fact]
    public void Unsubscribe_AfterMultipleSubscriptions_OnlyRemovesSpecificHandler()
    {
        // Arrange
        var handler1Called = false;
        var handler2Called = false;

        Task Handler1(TestEvent e)
        {
            handler1Called = true;
            return Task.CompletedTask;
        }

        Task Handler2(TestEvent e)
        {
            handler2Called = true;
            return Task.CompletedTask;
        }

        _publisher.Subscribe<TestEvent>(Handler1);
        _publisher.Subscribe<TestEvent>(Handler2);
        _publisher.Subscribe<TestEvent>(Handler1); // Register Handler1 twice

        // Act - unsubscribe Handler1 once
        _publisher.Unsubscribe<TestEvent>(Handler1);

        // Publish should still call Handler1 once and Handler2
        _publisher.PublishAsync(new TestEvent("PartialUnsubscribe"));

        // Assert
        handler1Called.Should().BeTrue("Handler1 should still be called once");
        handler2Called.Should().BeTrue("Handler2 should be called");
    }

    // Test event types
    private record TestEvent(string Name);
    private record DifferentTestEvent(string Name);
}