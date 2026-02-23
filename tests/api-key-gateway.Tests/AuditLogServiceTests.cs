// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

public class AuditLogServiceTests
{
    private readonly Mock<IAuditLogRepository> _repositoryMock;
    private readonly Mock<ILogger<AuditLogService>> _loggerMock;
    private readonly AuditLogService _sut;

    public AuditLogServiceTests()
    {
        _repositoryMock = new Mock<IAuditLogRepository>();
        _loggerMock = new Mock<ILogger<AuditLogService>>();
        _sut = new AuditLogService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new AuditLogService(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("repository");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new AuditLogService(_repositoryMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task LogAsync_NullLog_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.LogAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("log");
    }

    [Fact]
    public async Task LogAsync_ValidLog_CreatesInRepository()
    {
        var log = new AuditLog
        {
            Id = "log-001",
            ResourceId = "key-123",
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            IsSuccess = true,
            PerformedBy = "admin-user"
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        await _sut.LogAsync(log);

        _repositoryMock.Verify(r => r.CreateAsync(log), Times.Once);
    }

    [Fact]
    public async Task LogAsync_RepositoryThrows_FailsSilently()
    {
        var log = new AuditLog
        {
            ResourceId = "key-fail",
            ResourceType = "ApiKey",
            Action = AuditAction.KeyDeleted,
            IsSuccess = false
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var act = async () => await _sut.LogAsync(log);

        await act.Should().NotThrowAsync("service should handle repository errors gracefully");
    }

    [Fact]
    public async Task LogAsync_SuccessfulLog_LogsInformationMessage()
    {
        var log = new AuditLog
        {
            ResourceId = "key-456",
            ResourceType = "ApiKey",
            Action = AuditAction.KeyUsed,
            IsSuccess = true,
            PerformedBy = "consumer-abc"
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        await _sut.LogAsync(log);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("KeyUsed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task GetLogsAsync_EmptyOrNullResourceId_ReturnsEmptyList(string? resourceId)
    {
        var result = await _sut.GetLogsAsync(resourceId!);

        result.Should().BeEmpty();
        _repositoryMock.Verify(r => r.GetByResourceIdAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetLogsAsync_ValidResourceId_ReturnsLogsFromRepository()
    {
        var logs = new List<AuditLog>
        {
            new() { ResourceId = "key-789", ResourceType = "ApiKey", Action = AuditAction.KeyCreated },
            new() { ResourceId = "key-789", ResourceType = "ApiKey", Action = AuditAction.KeyUsed },
            new() { ResourceId = "key-789", ResourceType = "ApiKey", Action = AuditAction.KeyDisabled }
        };

        _repositoryMock
            .Setup(r => r.GetByResourceIdAsync("key-789", 100))
            .ReturnsAsync(logs);

        var result = await _sut.GetLogsAsync("key-789");

        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(logs);
    }

    [Fact]
    public async Task GetLogsAsync_WithCustomLimit_PassesLimitToRepository()
    {
        var logs = new List<AuditLog> { new() { ResourceId = "key-999", ResourceType = "ApiKey" } };

        _repositoryMock
            .Setup(r => r.GetByResourceIdAsync("key-999", 50))
            .ReturnsAsync(logs);

        var result = await _sut.GetLogsAsync("key-999", 50);

        result.Should().HaveCount(1);
        _repositoryMock.Verify(r => r.GetByResourceIdAsync("key-999", 50), Times.Once);
    }

    [Fact]
    public async Task GetLogsAsync_NoLogsFound_ReturnsEmptyList()
    {
        _repositoryMock
            .Setup(r => r.GetByResourceIdAsync("key-notfound", 100))
            .ReturnsAsync(new List<AuditLog>());

        var result = await _sut.GetLogsAsync("key-notfound");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLogsForPeriodAsync_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(-1);

        var act = async () => await _sut.GetLogsForPeriodAsync(startDate, endDate);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*End date must be after start date*");
    }

    [Fact]
    public async Task GetLogsForPeriodAsync_ValidDateRange_ReturnsLogsFromRepository()
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var logs = new List<AuditLog>
        {
            new() { ResourceId = "key-aaa", Action = AuditAction.KeyCreated, CreatedAt = startDate.AddHours(1) },
            new() { ResourceId = "key-bbb", Action = AuditAction.KeyUsed, CreatedAt = startDate.AddDays(2) },
            new() { ResourceId = "key-ccc", Action = AuditAction.KeyRevoked, CreatedAt = endDate.AddHours(-1) }
        };

        _repositoryMock
            .Setup(r => r.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(logs);

        var result = await _sut.GetLogsForPeriodAsync(startDate, endDate);

        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(logs);
    }

    [Fact]
    public async Task GetLogsForPeriodAsync_NoLogsInRange_ReturnsEmptyList()
    {
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow.AddDays(-25);

        _repositoryMock
            .Setup(r => r.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(new List<AuditLog>());

        var result = await _sut.GetLogsForPeriodAsync(startDate, endDate);

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CleanupOldLogsAsync_InvalidRetentionDays_ThrowsArgumentException(int retentionDays)
    {
        var act = async () => await _sut.CleanupOldLogsAsync(retentionDays);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Retention days must be positive*");
    }

    [Fact]
    public async Task CleanupOldLogsAsync_ValidRetentionDays_DeletesOldLogsAndLogsSuccess()
    {
        _repositoryMock
            .Setup(r => r.DeleteOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(42);

        await _sut.CleanupOldLogsAsync(30);

        _repositoryMock.Verify(r => r.DeleteOlderThanAsync(It.Is<DateTime>(
            dt => dt < DateTime.UtcNow && dt > DateTime.UtcNow.AddDays(-31)
        )), Times.Once);
    }

    [Fact]
    public async Task CleanupOldLogsAsync_NoLogsDeleted_LogsZeroCount()
    {
        _repositoryMock
            .Setup(r => r.DeleteOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(0);

        await _sut.CleanupOldLogsAsync(90);

        _repositoryMock.Verify(r => r.DeleteOlderThanAsync(It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task CleanupOldLogsAsync_RepositoryThrows_FailsSilently()
    {
        _repositoryMock
            .Setup(r => r.DeleteOlderThanAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var act = async () => await _sut.CleanupOldLogsAsync(30);

        await act.Should().NotThrowAsync();
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task LogAsync_ConcurrentLogging_AllSucceed()
    {
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var logs = Enumerable.Range(0, 50).Select(i => new AuditLog
        {
            Id = $"log-{i}",
            ResourceId = $"key-{i}",
            ResourceType = "ApiKey",
            Action = AuditAction.KeyUsed
        }).ToList();

        var tasks = logs.Select(log => _sut.LogAsync(log));

        var act = async () => await Task.WhenAll(tasks);

        await act.Should().NotThrowAsync();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<AuditLog>()), Times.Exactly(50));
    }

    [Fact]
    public async Task GetLogsAsync_DefaultLimit_Uses100()
    {
        var logs = Enumerable.Range(0, 50).Select(i => new AuditLog
        {
            ResourceId = "key-batch",
            Action = AuditAction.KeyUsed
        }).ToList();

        _repositoryMock
            .Setup(r => r.GetByResourceIdAsync("key-batch", 100))
            .ReturnsAsync(logs);

        var result = await _sut.GetLogsAsync("key-batch");

        result.Should().HaveCount(50);
        _repositoryMock.Verify(r => r.GetByResourceIdAsync("key-batch", 100), Times.Once);
    }
}
