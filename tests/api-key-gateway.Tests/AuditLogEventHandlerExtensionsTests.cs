// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for AuditLogEventHandlerExtensions covering extension methods that
// handle bulk operations and event delegation for audit logging.
// =====================================================================

using Xunit;
using ApiKeyGateway.Events;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Unit tests for <see cref="AuditLogEventHandlerExtensions"/> extension methods.
/// Tests bulk handling of events and event delegation functionality.
/// </summary>
public class AuditLogEventHandlerExtensionsTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<ILogger<AuditLogEventHandler>> _loggerMock;
    private readonly AuditLogEventHandler _handler;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IAuditLogRepository> _repositoryMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogEventHandlerExtensionsTests"/> class.
    /// </summary>
    public AuditLogEventHandlerExtensionsTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _loggerMock = new Mock<ILogger<AuditLogEventHandler>>();
        _handler = new AuditLogEventHandler(_scopeFactoryMock.Object, _loggerMock.Object);

        _scopeMock = new Mock<IServiceScope>();
        _repositoryMock = new Mock<IAuditLogRepository>();

        _scopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(_scopeMock.Object);

        _scopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(() => new ServiceCollection()
                .AddSingleton(_repositoryMock.Object)
                .BuildServiceProvider());
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyCreatedAsync properly handles a single event.
    /// </summary>
    public async Task HandleApiKeyCreatedAsync_SingleEvent_DelegatesToHandler()
    {
        // Arrange
        var @event = new ApiKeyCreatedEvent
        {
            ApiKeyId = Guid.NewGuid().ToString(),
            Name = "Test Key",
            CreatedBy = "test-user"
        };

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleApiKeyCreatedAsync(new[] { @event });

        // Assert
        _repositoryMock.Verify(
            x => x.CreateAsync(It.Is<AuditLog>(log =>
                log.ResourceId == @event.ApiKeyId &&
                log.Action == AuditAction.KeyCreated &&
                log.PerformedBy == @event.CreatedBy &&
                log.Reason == $"API key '{@event.Name}' created")),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyCreatedAsync handles multiple events in sequence.
    /// </summary>
    public async Task HandleApiKeyCreatedAsync_MultipleEvents_ProcessesAll()
    {
        // Arrange
        var events = new[]
        {
            new ApiKeyCreatedEvent { ApiKeyId = Guid.NewGuid().ToString(), Name = "Key1", CreatedBy = "user1" },
            new ApiKeyCreatedEvent { ApiKeyId = Guid.NewGuid().ToString(), Name = "Key2", CreatedBy = "user2" },
            new ApiKeyCreatedEvent { ApiKeyId = Guid.NewGuid().ToString(), Name = "Key3", CreatedBy = "user3" }
        };

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleApiKeyCreatedAsync(events);

        // Assert
        _repositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<AuditLog>()),
            Times.Exactly(3));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyCreatedAsync throws ArgumentNullException when handler is null.
    /// </summary>
    public async Task HandleApiKeyCreatedAsync_NullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLogEventHandler? nullHandler = null;
        var events = new[] { new ApiKeyCreatedEvent() };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => nullHandler!.HandleApiKeyCreatedAsync(events));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyCreatedAsync throws ArgumentNullException when events collection is null.
    /// </summary>
    public async Task HandleApiKeyCreatedAsync_NullEvents_ThrowsArgumentNullException()
    {
        // Arrange
        var @event = new ApiKeyCreatedEvent();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _handler.HandleApiKeyCreatedAsync(null!));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyCreatedAsync throws ArgumentNullException when events collection is empty.
    /// </summary>
    public async Task HandleApiKeyCreatedAsync_EmptyEventsCollection_NoException()
    {
        // Arrange & Act
        var act = () => _handler.HandleApiKeyCreatedAsync(Array.Empty<ApiKeyCreatedEvent>());

        // Assert
        await act.Should().NotThrowAsync<ArgumentNullException>();
        _repositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<AuditLog>()),
            Times.Never);
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyRotatedAsync properly handles a single event.
    /// </summary>
    public async Task HandleApiKeyRotatedAsync_SingleEvent_DelegatesToHandler()
    {
        // Arrange
        var @event = new ApiKeyRotatedEvent
        {
            ApiKeyId = Guid.NewGuid().ToString(),
            RotatedBy = "admin-user"
        };

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleApiKeyRotatedAsync(new[] { @event });

        // Assert
        _repositoryMock.Verify(
            x => x.CreateAsync(It.Is<AuditLog>(log =>
                log.ResourceId == @event.ApiKeyId &&
                log.Action == AuditAction.KeyRevoked &&
                log.PerformedBy == @event.RotatedBy &&
                log.Reason == "API key rotated: previous secret revoked and replaced")),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyRotatedAsync handles multiple events in sequence.
    /// </summary>
    public async Task HandleApiKeyRotatedAsync_MultipleEvents_ProcessesAll()
    {
        // Arrange
        var events = new[]
        {
            new ApiKeyRotatedEvent { ApiKeyId = Guid.NewGuid().ToString(), RotatedBy = "user1" },
            new ApiKeyRotatedEvent { ApiKeyId = Guid.NewGuid().ToString(), RotatedBy = "user2" }
        };

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleApiKeyRotatedAsync(events);

        // Assert
        _repositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<AuditLog>()),
            Times.Exactly(2));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyRotatedAsync throws ArgumentNullException when handler is null.
    /// </summary>
    public async Task HandleApiKeyRotatedAsync_NullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLogEventHandler? nullHandler = null;
        var events = new[] { new ApiKeyRotatedEvent() };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => nullHandler!.HandleApiKeyRotatedAsync(events));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyRotatedAsync throws ArgumentNullException when events collection is null.
    /// </summary>
    public async Task HandleApiKeyRotatedAsync_NullEvents_ThrowsArgumentNullException()
    {
        // Arrange
        var @event = new ApiKeyRotatedEvent();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _handler.HandleApiKeyRotatedAsync(null!));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyDisabledAsync properly handles a single event.
    /// </summary>
    public async Task HandleApiKeyDisabledAsync_SingleEvent_DelegatesToHandler()
    {
        // Arrange
        var @event = new ApiKeyDisabledEvent
        {
            ApiKeyId = Guid.NewGuid().ToString(),
            DisabledBy = "security-admin",
            Reason = "Compromised key"
        };

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleApiKeyDisabledAsync(new[] { @event });

        // Assert
        _repositoryMock.Verify(
            x => x.CreateAsync(It.Is<AuditLog>(log =>
                log.ResourceId == @event.ApiKeyId &&
                log.Action == AuditAction.KeyDisabled &&
                log.PerformedBy == @event.DisabledBy &&
                log.Reason == @event.Reason)),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyDisabledAsync handles multiple events in sequence.
    /// </summary>
    public async Task HandleApiKeyDisabledAsync_MultipleEvents_ProcessesAll()
    {
        // Arrange
        var events = new[]
        {
            new ApiKeyDisabledEvent { ApiKeyId = Guid.NewGuid().ToString(), DisabledBy = "admin1", Reason = "Compromised" },
            new ApiKeyDisabledEvent { ApiKeyId = Guid.NewGuid().ToString(), DisabledBy = "admin2", Reason = "Policy violation" }
        };

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleApiKeyDisabledAsync(events);

        // Assert
        _repositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<AuditLog>()),
            Times.Exactly(2));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyDisabledAsync throws ArgumentNullException when handler is null.
    /// </summary>
    public async Task HandleApiKeyDisabledAsync_NullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLogEventHandler? nullHandler = null;
        var events = new[] { new ApiKeyDisabledEvent() };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => nullHandler!.HandleApiKeyDisabledAsync(events));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyDisabledAsync throws ArgumentNullException when events collection is null.
    /// </summary>
    public async Task HandleApiKeyDisabledAsync_NullEvents_ThrowsArgumentNullException()
    {
        // Arrange
        var @event = new ApiKeyDisabledEvent();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _handler.HandleApiKeyDisabledAsync(null!));
    }

    [Fact]
    /// <summary>
    /// Tests that CreateEventDelegate returns a non-null delegate.
    /// </summary>
    public void CreateEventDelegate_ReturnsNonNullDelegate()
    {
        // Arrange & Act
        var @delegate = _handler.CreateEventDelegate();

        // Assert
        @delegate.Should().NotBeNull();
    }

    [Fact]
    /// <summary>
    /// Tests that CreateEventDelegate throws ArgumentNullException when handler is null.
    /// </summary>
    public void CreateEventDelegate_NullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLogEventHandler? nullHandler = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullHandler!.CreateEventDelegate());
    }

    [Fact]
    /// <summary>
    /// Tests that CreateEventDelegate properly handles ApiKeyCreatedEvent.
    /// </summary>
    public async Task CreateEventDelegate_HandlesApiKeyCreatedEvent()
    {
        // Arrange
        var @event = new ApiKeyCreatedEvent
        {
            ApiKeyId = Guid.NewGuid().ToString(),
            Name = "Test Key",
            CreatedBy = "test-user"
        };

        var @delegate = _handler.CreateEventDelegate();

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await @delegate(@event);

        // Assert
        _repositoryMock.Verify(
            x => x.CreateAsync(It.Is<AuditLog>(log =>
                log.ResourceId == @event.ApiKeyId &&
                log.Action == AuditAction.KeyCreated &&
                log.PerformedBy == @event.CreatedBy)),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that CreateEventDelegate properly handles ApiKeyRotatedEvent.
    /// </summary>
    public async Task CreateEventDelegate_HandlesApiKeyRotatedEvent()
    {
        // Arrange
        var @event = new ApiKeyRotatedEvent
        {
            ApiKeyId = Guid.NewGuid().ToString(),
            RotatedBy = "admin-user"
        };

        var @delegate = _handler.CreateEventDelegate();

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await @delegate(@event);

        // Assert
        _repositoryMock.Verify(
            x => x.CreateAsync(It.Is<AuditLog>(log =>
                log.ResourceId == @event.ApiKeyId &&
                log.Action == AuditAction.KeyRevoked &&
                log.PerformedBy == @event.RotatedBy)),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that CreateEventDelegate properly handles ApiKeyDisabledEvent.
    /// </summary>
    public async Task CreateEventDelegate_HandlesApiKeyDisabledEvent()
    {
        // Arrange
        var @event = new ApiKeyDisabledEvent
        {
            ApiKeyId = Guid.NewGuid().ToString(),
            DisabledBy = "security-admin",
            Reason = "Compromised key"
        };

        var @delegate = _handler.CreateEventDelegate();

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await @delegate(@event);

        // Assert
        _repositoryMock.Verify(
            x => x.CreateAsync(It.Is<AuditLog>(log =>
                log.ResourceId == @event.ApiKeyId &&
                log.Action == AuditAction.KeyDisabled &&
                log.PerformedBy == @event.DisabledBy)),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that CreateEventDelegate ignores unsupported event types.
    /// </summary>
    public async Task CreateEventDelegate_IgnoresUnsupportedEventType()
    {
        // Arrange
        var unsupportedEvent = new ApiKeyUpdatedEvent
        {
            ApiKeyId = Guid.NewGuid().ToString(),
            UpdatedBy = "user1"
        };

        var @delegate = _handler.CreateEventDelegate();

        // Act
        await @delegate(unsupportedEvent);

        // Assert - should not throw and should not call repository
        _repositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<AuditLog>()),
            Times.Never);
    }

    [Fact]
    /// <summary>
    /// Tests that all three event types can be handled by the same delegate.
    /// </summary>
    public async Task CreateEventDelegate_HandlesAllEventTypes()
    {
        // Arrange
        var createdEvent = new ApiKeyCreatedEvent { ApiKeyId = Guid.NewGuid().ToString(), Name = "Key1", CreatedBy = "user1" };
        var rotatedEvent = new ApiKeyRotatedEvent { ApiKeyId = Guid.NewGuid().ToString(), RotatedBy = "user2" };
        var disabledEvent = new ApiKeyDisabledEvent { ApiKeyId = Guid.NewGuid().ToString(), DisabledBy = "user3", Reason = "Test" };

        var @delegate = _handler.CreateEventDelegate();

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await @delegate(createdEvent);
        await @delegate(rotatedEvent);
        await @delegate(disabledEvent);

        // Assert
        _repositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<AuditLog>()),
            Times.Exactly(3));
    }
}
