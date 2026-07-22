// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for AuditLogEventHandler, UsageTrackingEventHandler, and RateLimitEventHandler
// covering audit logging, usage tracking, and rate limit event handling.
// =====================================================================

using Xunit;
using ApiKeyGateway.Events;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Unit tests for <see cref="AuditLogEventHandler"/> and related event handlers.
/// Tests individual event handler methods for audit logging, usage tracking, and rate limiting.
/// </summary>
public class AuditLogEventHandlerTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<ILogger<AuditLogEventHandler>> _loggerMock;
    private readonly AuditLogEventHandler _auditHandler;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IAuditLogRepository> _auditRepositoryMock;

    private readonly Mock<IMetricsCollectionService> _metricsMock;
    private readonly Mock<ILogger<UsageTrackingEventHandler>> _usageLoggerMock;
    private readonly UsageTrackingEventHandler _usageHandler;
    private readonly Mock<IUsageTrackingService> _usageTrackingMock;

    private readonly Mock<ILogger<RateLimitEventHandler>> _rateLimitLoggerMock;
    private readonly RateLimitEventHandler _rateLimitHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogEventHandlerTests"/> class.
    /// </summary>
    public AuditLogEventHandlerTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _loggerMock = new Mock<ILogger<AuditLogEventHandler>>();
        _auditHandler = new AuditLogEventHandler(_scopeFactoryMock.Object, _loggerMock.Object);

        _metricsMock = new Mock<IMetricsCollectionService>();
        _usageLoggerMock = new Mock<ILogger<UsageTrackingEventHandler>>();
        _usageTrackingMock = new Mock<IUsageTrackingService>();
        _usageHandler = new UsageTrackingEventHandler(
            _scopeFactoryMock.Object,
            _metricsMock.Object,
            _usageLoggerMock.Object);

        _rateLimitLoggerMock = new Mock<ILogger<RateLimitEventHandler>>();
        _rateLimitHandler = new RateLimitEventHandler(_metricsMock.Object, _rateLimitLoggerMock.Object);

        _scopeMock = new Mock<IServiceScope>();
        _auditRepositoryMock = new Mock<IAuditLogRepository>();

        _scopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(_scopeMock.Object);

        _scopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(() => new ServiceCollection()
                .AddSingleton(_auditRepositoryMock.Object)
                .AddSingleton(_usageTrackingMock.Object)
                .BuildServiceProvider());
    }

    #region AuditLogEventHandler Tests

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyCreatedAsync properly logs and persists API key creation events.
    /// </summary>
    public async Task HandleApiKeyCreatedAsync_HappyPath_LogsAndPersists()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid().ToString();
        var @event = new ApiKeyCreatedEvent
        {
            ApiKeyId = apiKeyId,
            Name = "Test API Key",
            CreatedBy = "admin-user"
        };

        _auditRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await _auditHandler.HandleApiKeyCreatedAsync(@event);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Recording audit: API key created")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _auditRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<AuditLog>(log =>
                log.ResourceId == apiKeyId &&
                log.ResourceType == "ApiKey" &&
                log.Action == AuditAction.KeyCreated &&
                log.PerformedBy == "admin-user" &&
                log.Reason == "API key 'Test API Key' created")),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyCreatedAsync throws ArgumentNullException when event is null.
    /// </summary>
    public async Task HandleApiKeyCreatedAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        ApiKeyCreatedEvent? nullEvent = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _auditHandler.HandleApiKeyCreatedAsync(nullEvent!));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyCreatedAsync handles events with empty/null optional fields.
    /// </summary>
    public async Task HandleApiKeyCreatedAsync_EmptyOptionalFields_HandlesGracefully()
    {
        // Arrange
        var @event = new ApiKeyCreatedEvent
        {
            ApiKeyId = Guid.NewGuid().ToString(),
            Name = "",
            CreatedBy = "user"
        };

        _auditRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        var act = () => _auditHandler.HandleApiKeyCreatedAsync(@event);

        // Assert
        await act.Should().NotThrowAsync();
        _auditRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<AuditLog>()),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyRotatedAsync properly logs and persists API key rotation events.
    /// </summary>
    public async Task HandleApiKeyRotatedAsync_HappyPath_LogsAndPersists()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid().ToString();
        var @event = new ApiKeyRotatedEvent
        {
            ApiKeyId = apiKeyId,
            RotatedBy = "admin-user"
        };

        _auditRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await _auditHandler.HandleApiKeyRotatedAsync(@event);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Recording audit: API key rotated")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _auditRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<AuditLog>(log =>
                log.ResourceId == apiKeyId &&
                log.ResourceType == "ApiKey" &&
                log.Action == AuditAction.KeyRevoked &&
                log.PerformedBy == "admin-user" &&
                log.Reason == "API key rotated: previous secret revoked and replaced")),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyRotatedAsync throws ArgumentNullException when event is null.
    /// </summary>
    public async Task HandleApiKeyRotatedAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        ApiKeyRotatedEvent? nullEvent = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _auditHandler.HandleApiKeyRotatedAsync(nullEvent!));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyDisabledAsync properly logs and persists API key disable events.
    /// </summary>
    public async Task HandleApiKeyDisabledAsync_HappyPath_LogsAndPersists()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid().ToString();
        var @event = new ApiKeyDisabledEvent
        {
            ApiKeyId = apiKeyId,
            DisabledBy = "security-admin",
            Reason = "Key compromised"
        };

        _auditRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await _auditHandler.HandleApiKeyDisabledAsync(@event);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Recording audit: API key disabled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _auditRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<AuditLog>(log =>
                log.ResourceId == apiKeyId &&
                log.ResourceType == "ApiKey" &&
                log.Action == AuditAction.KeyDisabled &&
                log.PerformedBy == "security-admin" &&
                log.Reason == "Key compromised")),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyDisabledAsync throws ArgumentNullException when event is null.
    /// </summary>
    public async Task HandleApiKeyDisabledAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        ApiKeyDisabledEvent? nullEvent = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _auditHandler.HandleApiKeyDisabledAsync(nullEvent!));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyDisabledAsync handles empty reason strings.
    /// </summary>
    public async Task HandleApiKeyDisabledAsync_EmptyReason_HandlesGracefully()
    {
        // Arrange
        var @event = new ApiKeyDisabledEvent
        {
            ApiKeyId = Guid.NewGuid().ToString(),
            DisabledBy = "admin",
            Reason = ""
        };

        _auditRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        var act = () => _auditHandler.HandleApiKeyDisabledAsync(@event);

        // Assert
        await act.Should().NotThrowAsync();
        _auditRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<AuditLog>()),
            Times.Once);
    }

    #endregion

    #region UsageTrackingEventHandler Tests

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyUsedAsync properly records API usage for billing and analytics.
    /// </summary>
    public async Task HandleApiKeyUsedAsync_HappyPath_RecordsUsage()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid().ToString();
        var @event = new ApiKeyUsedEvent
        {
            ApiKeyId = apiKeyId,
            Endpoint = "/api/v1/users",
            HttpStatusCode = 200,
            ResponseTimeMs = 150,
            ResponseSizeBytes = 2048
        };

        _usageTrackingMock
            .Setup(x => x.RecordUsageAsync(It.IsAny<UsageRecord>()))
            .Returns(Task.CompletedTask);

        // Act
        await _usageHandler.HandleApiKeyUsedAsync(@event);

        // Assert
        _usageLoggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Usage recorded:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _metricsMock.Verify(
            x => x.RecordRequest(
                apiKeyId,
                @event.Endpoint,
                @event.HttpStatusCode,
                @event.ResponseTimeMs),
            Times.Once);

        _usageTrackingMock.Verify(
            x => x.RecordUsageAsync(It.Is<UsageRecord>(record =>
                record.ApiKeyId == apiKeyId &&
                record.Endpoint == @event.Endpoint &&
                record.ResponseStatusCode == 200 &&
                record.ResponseBytes == 2048 &&
                record.ResponseTimeMs == 150)),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyUsedAsync clamps large response times to int.MaxValue.
    /// </summary>
    public async Task HandleApiKeyUsedAsync_LargeResponseTime_ClampsToIntMax()
    {
        // Arrange
        var @event = new ApiKeyUsedEvent
        {
            ApiKeyId = Guid.NewGuid().ToString(),
            Endpoint = "/api/test",
            HttpStatusCode = 200,
            ResponseTimeMs = long.MaxValue,
            ResponseSizeBytes = 1024
        };

        _usageTrackingMock
            .Setup(x => x.RecordUsageAsync(It.IsAny<UsageRecord>()))
            .Returns(Task.CompletedTask);

        // Act
        await _usageHandler.HandleApiKeyUsedAsync(@event);

        // Assert - response time should be clamped
        _usageTrackingMock.Verify(
            x => x.RecordUsageAsync(It.Is<UsageRecord>(record =>
                record.ResponseTimeMs == int.MaxValue)),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that HandleApiKeyUsedAsync throws ArgumentNullException when event is null.
    /// </summary>
    public async Task HandleApiKeyUsedAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        ApiKeyUsedEvent? nullEvent = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _usageHandler.HandleApiKeyUsedAsync(nullEvent!));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleQuotaExhaustedAsync properly logs and records quota exhaustion events.
    /// </summary>
    public async Task HandleQuotaExhaustedAsync_HappyPath_LogsAndRecords()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid().ToString();
        var resetTime = DateTime.UtcNow.AddHours(1);
        var @event = new QuotaExhaustedEvent
        {
            ApiKeyId = apiKeyId,
            Endpoint = "/api/v1/data",
            HttpStatusCode = 429,
            Limit = 1000,
            WindowResetTime = resetTime
        };

        _auditRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act
        await _usageHandler.HandleQuotaExhaustedAsync(@event);

        // Assert
        _usageLoggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CRITICAL: Quota exhausted")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _metricsMock.Verify(
            x => x.RecordError(apiKeyId, "QUOTA_EXHAUSTED"),
            Times.Once);

        _auditRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<AuditLog>(log =>
                log.ResourceId == apiKeyId &&
                log.ResourceType == "ApiKey" &&
                log.Action == AuditAction.RateLimitExceeded &&
                log.IsSuccess == false &&
                log.Reason.Contains("Usage quota of 1000 requests exhausted") &&
                log.Reason.Contains(resetTime.ToString("O")))),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that HandleQuotaExhaustedAsync throws ArgumentNullException when event is null.
    /// </summary>
    public async Task HandleQuotaExhaustedAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        QuotaExhaustedEvent? nullEvent = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _usageHandler.HandleQuotaExhaustedAsync(nullEvent!));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleUsageWarningAsync properly logs usage warning events.
    /// </summary>
    public async Task HandleUsageWarningAsync_HappyPath_LogsWarning()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid().ToString();
        var @event = new UsageWarningEvent
        {
            ApiKeyId = apiKeyId,
            Endpoint = "/api/v1/data",
            HttpStatusCode = 200,
            CurrentUsage = 800,
            Limit = 1000,
            PercentageUsed = 80
        };

        // Act
        await _usageHandler.HandleUsageWarningAsync(@event);

        // Assert
        _usageLoggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Usage warning:") &&
                    v.ToString()!.Contains("80%") &&
                    v.ToString()!.Contains("800/1000")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that HandleUsageWarningAsync throws ArgumentNullException when event is null.
    /// </summary>
    public async Task HandleUsageWarningAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        UsageWarningEvent? nullEvent = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _usageHandler.HandleUsageWarningAsync(nullEvent!));
    }

    #endregion

    #region RateLimitEventHandler Tests

    [Fact]
    /// <summary>
    /// Tests that HandleRateLimitExceededAsync properly records rate limit violations.
    /// </summary>
    public async Task HandleRateLimitExceededAsync_HappyPath_RecordsViolation()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid().ToString();
        var @event = new RateLimitExceededEvent
        {
            ApiKeyId = apiKeyId,
            Endpoint = "/api/v1/heavy",
            HttpStatusCode = 429,
            CurrentUsage = 150,
            Limit = 100,
            SecondsUntilReset = 30
        };

        // Act
        await _rateLimitHandler.HandleRateLimitExceededAsync(@event);

        // Assert
        _rateLimitLoggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rate limit exceeded:") &&
                    v.ToString()!.Contains("150/100") &&
                    v.ToString()!.Contains("30s")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _metricsMock.Verify(
            x => x.RecordRateLimitExceeded(apiKeyId),
            Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that HandleRateLimitExceededAsync throws ArgumentNullException when event is null.
    /// </summary>
    public async Task HandleRateLimitExceededAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        RateLimitExceededEvent? nullEvent = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _rateLimitHandler.HandleRateLimitExceededAsync(nullEvent!));
    }

    [Fact]
    /// <summary>
    /// Tests that HandleRateLimitExceededAsync handles boundary values correctly.
    /// </summary>
    public async Task HandleRateLimitExceededAsync_BoundaryValues_HandlesCorrectly()
    {
        // Arrange - test with minimum values
        var @event = new RateLimitExceededEvent
        {
            ApiKeyId = Guid.NewGuid().ToString(),
            Endpoint = "/api/test",
            HttpStatusCode = 429,
            CurrentUsage = 1,
            Limit = 1,
            SecondsUntilReset = 1
        };

        // Act
        await _rateLimitHandler.HandleRateLimitExceededAsync(@event);

        // Assert
        _metricsMock.Verify(
            x => x.RecordRateLimitExceeded(It.IsAny<string>()),
            Times.Once);
    }

    #endregion
}