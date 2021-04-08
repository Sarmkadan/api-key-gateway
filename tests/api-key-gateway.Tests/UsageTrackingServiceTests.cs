// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Unit tests for <see cref="UsageTrackingService"/> which tracks and analyzes API key usage metrics.
/// Tests verify recording usage records, retrieving statistics, and handling edge cases.
/// </summary>
public class UsageTrackingServiceTests
{
    private readonly Mock<IUsageRepository> _repositoryMock;
    private readonly Mock<ILogger<UsageTrackingService>> _loggerMock;
    private readonly UsageTrackingService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsageTrackingServiceTests"/> class.
    /// Sets up mock dependencies for repository and logger.
    /// </summary>
    public UsageTrackingServiceTests()
    {
        _repositoryMock = new Mock<IUsageRepository>();
        _loggerMock = new Mock<ILogger<UsageTrackingService>>();
        _sut = new UsageTrackingService(_repositoryMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Tests that constructor throws <see cref="ArgumentNullException"/> when repository is null.
    /// Ensures proper validation of required dependencies.
    /// </summary>
    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new UsageTrackingService(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("repository");
    }

    /// <summary>
    /// Tests that constructor throws <see cref="ArgumentNullException"/> when logger is null.
    /// Ensures proper validation of required dependencies.
    /// </summary>
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new UsageTrackingService(_repositoryMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.RecordUsageAsync(UsageRecord)"/> throws <see cref="ArgumentNullException"/> when record is null.
    /// Validates null input handling for usage record creation.
    /// </summary>
    [Fact]
    public async Task RecordUsageAsync_NullRecord_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.RecordUsageAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("record");
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.RecordUsageAsync(UsageRecord)"/> successfully records valid usage record in repository.
    /// Verifies that the service creates the record with correct parameters.
    /// </summary>
    [Fact]
    public async Task RecordUsageAsync_ValidRecord_CreatesInRepository()
    {
        var record = new UsageRecord
        {
            Id = "usage-001",
            ApiKeyId = "key-123",
            Endpoint = "/api/data",
            Method = "GET",
            ResponseStatusCode = 200,
            RequestTimestampUtc = DateTime.UtcNow,
            ResponseTimeMs = 45
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<UsageRecord>()))
            .Returns(Task.CompletedTask);

        await _sut.RecordUsageAsync(record);

        _repositoryMock.Verify(r => r.CreateAsync(record), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.RecordUsageAsync(UsageRecord)"/> wraps repository exceptions in <see cref="DataAccessException"/>.
    /// Validates proper exception handling when repository operations fail.
    /// </summary>
    [Fact]
    public async Task RecordUsageAsync_RepositoryThrows_WrapsInDataAccessException()
    {
        var record = new UsageRecord { ApiKeyId = "key-123" };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<UsageRecord>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var act = async () => await _sut.RecordUsageAsync(record);
        (await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.DataAccessException>())
            .WithInnerException<InvalidOperationException>()
            .WithMessage("DB error");
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetUsageStatisticsAsync(string, DateTime, DateTime)"/> throws <see cref="ValidationException"/> when keyId is empty, null, or whitespace.
    /// Validates input validation for API key identifier.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task GetUsageStatisticsAsync_EmptyOrNullKeyId_ThrowsValidationException(string? keyId)
    {
        var act = async () => await _sut.GetUsageStatisticsAsync(keyId!, DateTime.UtcNow, DateTime.UtcNow);
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetUsageStatisticsAsync(string, DateTime, DateTime)"/> throws <see cref="ValidationException"/> when end date is before start date.
    /// Validates date range validation logic.
    /// </summary>
    [Fact]
    public async Task GetUsageStatisticsAsync_EndDateBeforeStartDate_ThrowsValidationException()
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(-1);

        var act = async () => await _sut.GetUsageStatisticsAsync("key-123", startDate, endDate);
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetUsageStatisticsAsync(string, DateTime, DateTime)"/> returns correct statistics for valid date range.
    /// Verifies aggregation of usage data including total requests, unique endpoints, success/failure rates, and average response time.
    /// </summary>
    [Fact]
    public async Task GetUsageStatisticsAsync_ValidDateRange_ReturnsStatisticsWithCorrectAggregates()
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var records = new List<UsageRecord>
        {
            new() { ApiKeyId = "key-123", Endpoint = "/api/users", Method = "GET", ResponseStatusCode = 200, ResponseTimeMs = 50 },
            new() { ApiKeyId = "key-123", Endpoint = "/api/users", Method = "GET", ResponseStatusCode = 200, ResponseTimeMs = 60 },
            new() { ApiKeyId = "key-123", Endpoint = "/api/data", Method = "POST", ResponseStatusCode = 201, ResponseTimeMs = 100 },
            new() { ApiKeyId = "key-123", Endpoint = "/api/data", Method = "POST", ResponseStatusCode = 500, ResponseTimeMs = 30 }
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyAndDateRangeAsync("key-123", startDate, endDate))
            .ReturnsAsync(records);

        var result = await _sut.GetUsageStatisticsAsync("key-123", startDate, endDate);

        result.Should().NotBeNull();
        result.ApiKeyId.Should().Be("key-123");
        result.StartDate.Should().Be(startDate);
        result.EndDate.Should().Be(endDate);
        result.TotalRequests.Should().Be(4);
        result.UniqueEndpoints.Should().Be(2);
        result.SuccessfulRequests.Should().Be(3);
        result.FailedRequests.Should().Be(1);
        result.AverageResponseTimeMs.Should().Be(60);
        result.SuccessRate.Should().Be(75);
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetUsageStatisticsAsync(string, DateTime, DateTime)"/> returns zeroed statistics when no records exist.
    /// Validates handling of empty result sets.
    /// </summary>
    [Fact]
    public async Task GetUsageStatisticsAsync_NoRecords_ReturnsZeroedStatistics()
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        _repositoryMock
            .Setup(r => r.GetByApiKeyAndDateRangeAsync("key-missing", startDate, endDate))
            .ReturnsAsync(new List<UsageRecord>());

        var result = await _sut.GetUsageStatisticsAsync("key-missing", startDate, endDate);

        result.TotalRequests.Should().Be(0);
        result.SuccessfulRequests.Should().Be(0);
        result.FailedRequests.Should().Be(0);
        result.SuccessRate.Should().Be(0);
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetUsageRecordsAsync(string, DateTime, DateTime)"/> throws <see cref="ValidationException"/> when keyId is empty, null, or whitespace.
    /// Validates input validation for API key identifier.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task GetUsageRecordsAsync_EmptyOrNullKeyId_ThrowsValidationException(string? keyId)
    {
        var act = async () => await _sut.GetUsageRecordsAsync(keyId!, DateTime.UtcNow, DateTime.UtcNow);
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetUsageRecordsAsync(string, DateTime, DateTime)"/> returns usage records from repository.
    /// Verifies retrieval of raw usage data for a specific API key and date range.
    /// </summary>
    [Fact]
    public async Task GetUsageRecordsAsync_ValidDateRange_ReturnsRecordsFromRepository()
    {
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;
        var records = new List<UsageRecord>
        {
            new() { ApiKeyId = "key-456", Endpoint = "/api/test", Method = "GET", ResponseStatusCode = 200 },
            new() { ApiKeyId = "key-456", Endpoint = "/api/test", Method = "POST", ResponseStatusCode = 201 }
        };

        _repositoryMock
            .Setup(r => r.GetByApiKeyAndDateRangeAsync("key-456", startDate, endDate))
            .ReturnsAsync(records);

        var result = await _sut.GetUsageRecordsAsync("key-456", startDate, endDate);

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(records);
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetTotalBytesUsedAsync(string, DateTime, DateTime)"/> returns 0 when consumerId is empty, null, or whitespace.
    /// Validates input validation and ensures repository is not called for invalid input.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetTotalBytesUsedAsync_EmptyOrNullConsumerId_ReturnsZero(string? consumerId)
    {
        var result = await _sut.GetTotalBytesUsedAsync(consumerId!, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
        result.Should().Be(0);
        _repositoryMock.Verify(r => r.GetByConsumerAndDateRangeAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetTotalBytesUsedAsync(string, DateTime, DateTime)"/> returns aggregated bytes transferred for valid consumer.
    /// Verifies calculation of total bytes from multiple usage records.
    /// </summary>
    [Fact]
    public async Task GetTotalBytesUsedAsync_ValidConsumerId_ReturnsAggregatedBytes()
    {
        var consumerId = "consumer-789";
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var records = new List<UsageRecord>
        {
            new() { ApiKeyId = "key-a", BytesTransferred = 1024 },
            new() { ApiKeyId = "key-a", BytesTransferred = 2048 },
            new() { ApiKeyId = "key-b", BytesTransferred = 512 }
        };

        _repositoryMock
            .Setup(r => r.GetByConsumerAndDateRangeAsync(consumerId, startDate, endDate))
            .ReturnsAsync(records);

        var result = await _sut.GetTotalBytesUsedAsync(consumerId, startDate, endDate);

        result.Should().Be(3584);
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetTotalBytesUsedAsync(string, DateTime, DateTime)"/> returns 0 when no records exist.
    /// Validates handling of empty result sets for byte usage calculation.
    /// </summary>
    [Fact]
    public async Task GetTotalBytesUsedAsync_NoRecords_ReturnsZero()
    {
        var consumerId = "consumer-missing";
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        _repositoryMock
            .Setup(r => r.GetByConsumerAndDateRangeAsync(consumerId, startDate, endDate))
            .ReturnsAsync(new List<UsageRecord>());

        var result = await _sut.GetTotalBytesUsedAsync(consumerId, startDate, endDate);

        result.Should().Be(0);
    }
}
