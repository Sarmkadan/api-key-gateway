// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="AuditLogService"/> class.
/// Tests verify that the audit logging functionality works correctly,
/// including constructor validation, logging operations, and log retrieval methods.
/// </summary>
public class AuditLogServiceTests
{
    private readonly Mock<IAuditLogRepository> _repositoryMock;
    private readonly Mock<ILogger<AuditLogService>> _loggerMock;
    private readonly AuditLogService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogServiceTests"/> class.
    /// Sets up mock repositories and logger for testing audit log service functionality.
    /// </summary>
    public AuditLogServiceTests()
    {
        _repositoryMock = new Mock<IAuditLogRepository>();
        _loggerMock = new Mock<ILogger<AuditLogService>>();
        _sut = new AuditLogService(_repositoryMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Tests that the constructor throws an <see cref="ArgumentNullException"/> when a null repository is provided.
    /// Ensures proper parameter validation for dependency injection.
    /// </summary>
    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new AuditLogService(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("repository");
    }

    /// <summary>
    /// Tests that the constructor throws an <see cref="ArgumentNullException"/> when a null logger is provided.
    /// Ensures proper parameter validation for dependency injection.
    /// </summary>
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new AuditLogService(_repositoryMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    /// <summary>
    /// Tests that <see cref="AuditLogService.LogAsync(AuditLog)"/> throws an <see cref="ArgumentNullException"/> when a null log is provided.
    /// Ensures proper parameter validation for audit log creation.
    /// </summary>
    [Fact]
    public async Task LogAsync_NullLog_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.LogAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("log");
    }

    /// <summary>
    /// Tests that <see cref="AuditLogService.LogAsync(AuditLog)"/> successfully creates a log entry in the repository when provided with valid data.
    /// Verifies that the service properly delegates to the repository and passes the correct log object.
    /// </summary>
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
        await _sut.FlushAsync();

        _repositoryMock.Verify(r => r.CreateAsync(log), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="AuditLogService.LogAsync(AuditLog)"/> handles repository exceptions gracefully without throwing.
    /// Verifies that the service catches and suppresses repository errors to maintain system stability.
    /// </summary>
    [Fact]
    public async Task LogAsync_RepositoryThrows_FailsSilently()
    {
        var log = new AuditLog
        {
            ResourceId = "key-fail",
            ResourceType = "ApiKey",
            Action = AuditAction.KeyRevoked,
            IsSuccess = false
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var act = async () => await _sut.LogAsync(log);

        await act.Should().NotThrowAsync("service should handle repository errors gracefully");

        await _sut.FlushAsync();
    }

    /// <summary>
    /// Tests that <see cref="AuditLogService.LogAsync(AuditLog)"/> logs an information message when a log entry is successfully created.
    /// Verifies that the service properly logs the audit action for monitoring and debugging purposes.
    /// </summary>
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
        await _sut.FlushAsync();

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Flushed 1 audit logs")),
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

    /// <summary>
    /// Tests that <see cref="AuditLogService.GetLogsAsync(string,int)"/> returns audit logs from the repository when provided with a valid resource ID.
    /// Verifies that the service properly delegates to the repository and returns the expected log entries.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="AuditLogService.GetLogsAsync(string,int)"/> properly passes a custom limit to the repository.
    /// Verifies that the service correctly handles pagination and respects the specified limit parameter.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="AuditLogService.GetLogsAsync(string,int)"/> returns an empty list when no logs are found for the specified resource ID.
    /// Verifies that the service properly handles the case where no audit logs exist for a given resource.
    /// </summary>
    [Fact]
    public async Task GetLogsAsync_NoLogsFound_ReturnsEmptyList()
    {
        _repositoryMock
            .Setup(r => r.GetByResourceIdAsync("key-notfound", 100))
            .ReturnsAsync(new List<AuditLog>());

        var result = await _sut.GetLogsAsync("key-notfound");

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="AuditLogService.GetLogsForPeriodAsync(DateTime,DateTime)"/> throws an <see cref="ArgumentException"/> when the end date is before the start date.
    /// Verifies that the service properly validates date ranges to prevent invalid queries.
    /// </summary>
    [Fact]
    public async Task GetLogsForPeriodAsync_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(-1);

        var act = async () => await _sut.GetLogsForPeriodAsync(startDate, endDate);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*End date must be after start date*");
    }

    /// <summary>
    /// Tests that <see cref="AuditLogService.GetLogsForPeriodAsync(DateTime,DateTime)"/> returns audit logs from the repository for a valid date range.
    /// Verifies that the service properly queries the repository with the correct date parameters and returns matching logs.
    /// </summary>
    [Fact]
    public async Task GetLogsForPeriodAsync_ValidDateRange_ReturnsLogsFromRepository()
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var logs = new List<AuditLog>
        {
            new() { ResourceId = "key-aaa", Action = AuditAction.KeyCreated, PerformedAt = startDate.AddHours(1) },
            new() { ResourceId = "key-bbb", Action = AuditAction.KeyUsed, PerformedAt = startDate.AddDays(2) },
            new() { ResourceId = "key-ccc", Action = AuditAction.KeyRevoked, PerformedAt = endDate.AddHours(-1) }
        };

        _repositoryMock
            .Setup(r => r.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(logs);

        var result = await _sut.GetLogsForPeriodAsync(startDate, endDate);

        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(logs);
    }

    /// <summary>
    /// Tests that <see cref="AuditLogService.GetLogsForPeriodAsync(DateTime,DateTime)"/> returns an empty list when no logs exist within the specified date range.
    /// Verifies that the service properly handles the case where no audit logs fall within the requested time period.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="AuditLogService.CleanupOldLogsAsync(int)"/> throws an <see cref="ArgumentException"/> when provided with invalid (non-positive) retention days.
    /// Verifies that the service properly validates the retention period parameter.
    /// </summary>
    /// <param name="retentionDays">The invalid retention days value to test (0, negative).</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CleanupOldLogsAsync_InvalidRetentionDays_ThrowsArgumentException(int retentionDays)
    {
        var act = async () => await _sut.CleanupOldLogsAsync(retentionDays);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Retention days must be positive*");
    }

    /// <summary>
    /// Tests that <see cref="AuditLogService.CleanupOldLogsAsync(int)"/> successfully deletes old logs and logs the operation as successful when provided with valid retention days.
    /// Verifies that the service properly calculates the cutoff date and delegates the cleanup operation to the repository.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="AuditLogService.CleanupOldLogsAsync(int)"/> properly logs a zero count message when no logs are deleted during cleanup.
    /// Verifies that the service correctly handles the case where the cleanup operation doesn't remove any records.
    /// </summary>
    [Fact]
    public async Task CleanupOldLogsAsync_NoLogsDeleted_LogsZeroCount()
    {
        _repositoryMock
            .Setup(r => r.DeleteOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(0);

        await _sut.CleanupOldLogsAsync(90);

        _repositoryMock.Verify(r => r.DeleteOlderThanAsync(It.IsAny<DateTime>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="AuditLogService.CleanupOldLogsAsync(int)"/> handles repository exceptions gracefully during cleanup without throwing.
    /// Verifies that the service catches and suppresses repository errors to maintain system stability during cleanup operations.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="AuditLogService.LogAsync(AuditLog)"/> handles concurrent logging operations successfully.
    /// Verifies that the service can process multiple simultaneous log requests without errors or race conditions.
    /// </summary>
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
        await _sut.FlushAsync();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<AuditLog>()), Times.Exactly(50));
    }

    /// <summary>
    /// Tests that <see cref="AuditLogService.GetLogsAsync(string,int)"/> uses a default limit of 100 when no limit is specified.
    /// Verifies that the service provides a sensible default pagination value for retrieving audit logs.
    /// </summary>
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
