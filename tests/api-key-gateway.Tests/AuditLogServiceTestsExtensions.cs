// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Extension methods for <see cref="AuditLogServiceTests"/> to provide fluent assertions
/// and helper methods for testing audit log scenarios.
/// </summary>
public static class AuditLogServiceTestsExtensions
{
    /// <summary>
    /// Creates a test audit log with the specified parameters.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="action">The audit action type.</param>
    /// <param name="isSuccess">Whether the action was successful.</param>
    /// <param name="performedBy">Who performed the action.</param>
    /// <param name="resourceType">Type of resource (defaults to "ApiKey").</param>
    /// <returns>A configured audit log instance.</returns>
    public static AuditLog CreateTestAuditLog(
        this string resourceId,
        AuditAction action,
        bool isSuccess = true,
        string? performedBy = null,
        string resourceType = "ApiKey")
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceId);
        ArgumentException.ThrowIfNullOrEmpty(resourceType);

        return new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = resourceId,
            ResourceType = resourceType,
            Action = action,
            IsSuccess = isSuccess,
            PerformedBy = performedBy ?? "test-user",
            PerformedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Verifies that the repository received a call to create the specified log.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="expectedLog">The expected audit log.</param>
    public static void VerifyLogCreated(
        this AuditLogServiceTests test,
        AuditLog expectedLog)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(expectedLog);

        test.GetMockRepository().Verify(
            r => r.CreateAsync(expectedLog),
            Times.Once);
    }

    /// <summary>
    /// Verifies that the logger received an information-level log containing the specified action.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="expectedAction">The expected audit action.</param>
    public static void VerifyInformationLogForAction(
        this AuditLogServiceTests test,
        AuditAction expectedAction)
    {
        ArgumentNullException.ThrowIfNull(test);

        test.GetMockLogger().Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedAction.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once);
    }

    /// <summary>
    /// Sets up the repository to return a specific list of logs for the given resource ID.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="logs">The logs to return.</param>
    /// <param name="limit">Optional limit (defaults to 100).</param>
    public static void SetupGetLogsAsync(
        this AuditLogServiceTests test,
        string resourceId,
        List<AuditLog> logs,
        int limit = 100)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentException.ThrowIfNullOrEmpty(resourceId);
        ArgumentNullException.ThrowIfNull(logs);

        test.GetMockRepository()
            .Setup(r => r.GetByResourceIdAsync(resourceId, limit))
            .ReturnsAsync(logs);
    }

    /// <summary>
    /// Sets up the repository to return a specific list of logs for the given date range.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="startDate">Start date of the range.</param>
    /// <param name="endDate">End date of the range.</param>
    /// <param name="logs">The logs to return.</param>
    public static void SetupGetLogsForPeriodAsync(
        this AuditLogServiceTests test,
        DateTime startDate,
        DateTime endDate,
        List<AuditLog> logs)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(logs);

        test.GetMockRepository()
            .Setup(r => r.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(logs);
    }

    /// <summary>
    /// Sets up the repository to return a specific count when deleting old logs.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="retentionDays">Retention period in days.</param>
    /// <param name="deletedCount">Number of logs that were deleted.</param>
    public static void SetupCleanupOldLogsAsync(
        this AuditLogServiceTests test,
        int retentionDays,
        int deletedCount)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(retentionDays, 0);

        test.GetMockRepository()
            .Setup(r => r.DeleteOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(deletedCount);
    }

    /// <summary>
    /// Gets the mock repository from the test instance.
    /// </summary>
    public static Mock<IAuditLogRepository> GetMockRepository(this AuditLogServiceTests test)
    {
        ArgumentNullException.ThrowIfNull(test);
        return test.GetFieldValue<Mock<IAuditLogRepository>>("_repositoryMock");
    }

    /// <summary>
    /// Gets the mock logger from the test instance.
    /// </summary>
    public static Mock<ILogger<AuditLogService>> GetMockLogger(this AuditLogServiceTests test)
    {
        ArgumentNullException.ThrowIfNull(test);
        return test.GetFieldValue<Mock<ILogger<AuditLogService>>>("_loggerMock");
    }

    /// <summary>
    /// Gets the service under test from the test instance.
    /// </summary>
    public static AuditLogService GetServiceUnderTest(this AuditLogServiceTests test)
    {
        ArgumentNullException.ThrowIfNull(test);
        return test.GetFieldValue<AuditLogService>("_sut");
    }

    /// <summary>
    /// Helper method to assert that a collection of logs contains only the expected actions.
    /// </summary>
    /// <param name="logs">The actual logs.</param>
    /// <param name="expectedActions">The expected action types.</param>
    public static void ContainOnlyActions(
        this List<AuditLog> logs,
        params AuditAction[] expectedActions)
    {
        ArgumentNullException.ThrowIfNull(logs);
        ArgumentNullException.ThrowIfNull(expectedActions);

        logs.Select(l => l.Action)
            .Should()
            .BeEquivalentTo(expectedActions);
    }

    /// <summary>
    /// Helper method to assert that a collection of logs contains only successful operations.
    /// </summary>
    /// <param name="logs">The actual logs.</param>
    public static void ContainOnlySuccessfulOperations(this List<AuditLog> logs)
    {
        ArgumentNullException.ThrowIfNull(logs);

        logs.Should()
            .AllSatisfy(log => log.IsSuccess.Should().BeTrue());
    }

    /// <summary>
    /// Helper method to assert that a collection of logs contains only failed operations.
    /// </summary>
    /// <param name="logs">The actual logs.</param>
    public static void ContainOnlyFailedOperations(this List<AuditLog> logs)
    {
        ArgumentNullException.ThrowIfNull(logs);

        logs.Should()
            .AllSatisfy(log => log.IsSuccess.Should().BeFalse());
    }

    /// <summary>
    /// Helper method to get the private field value from the test instance.
    /// </summary>
    private static T GetFieldValue<T>(this AuditLogServiceTests test, string fieldName)
    {
        var field = typeof(AuditLogServiceTests).GetField(
            fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field == null)
        {
            throw new InvalidOperationException($"Field '{fieldName}' not found in AuditLogServiceTests");
        }

        return (T)field.GetValue(test)!;
    }
}