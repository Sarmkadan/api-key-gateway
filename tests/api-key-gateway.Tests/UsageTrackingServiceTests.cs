// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    private readonly ILogger<UsageTrackingServiceTests> _logger = NullLogger<UsageTrackingServiceTests>.Instance;

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
        _logger.LogInformation("Executing test: {TestName}", "Constructor_NullRepository_ThrowsArgumentNullException");
        var act = () => new UsageTrackingService(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("repository");
        _logger.LogInformation("Test completed: {TestName}", "Constructor_NullRepository_ThrowsArgumentNullException");
    }

    /// <summary>
    /// Tests that constructor throws <see cref="ArgumentNullException"/> when logger is null.
    /// Ensures proper validation of required dependencies.
    /// </summary>
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        _logger.LogInformation("Executing test: {TestName}", "Constructor_NullLogger_ThrowsArgumentNullException");
        var act = () => new UsageTrackingService(_repositoryMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        _logger.LogInformation("Test completed: {TestName}", "Constructor_NullLogger_ThrowsArgumentNullException");
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.RecordUsageAsync(UsageRecord)"/> throws <see cref="ArgumentNullException"/> when record is null.
    /// Validates null input handling for usage record creation.
    /// </summary>
    [Fact]
    public async Task RecordUsageAsync_NullRecord_ThrowsArgumentNullException()
    {
        _logger.LogInformation("Executing test: {TestName}", "RecordUsageAsync_NullRecord_ThrowsArgumentNullException");
        var act = async () => await _sut.RecordUsageAsync(null!);
        _loggerMock.Verify(r => r.LogError(It.IsAny<Exception>(), "Failed to record usage"));
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("record");
        _logger.LogInformation("Test completed: {TestName}", "RecordUsageAsync_NullRecord_ThrowsArgumentNullException");
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.RecordUsageAsync(UsageRecord)"/> successfully records valid usage record in repository.
    /// Verifies that the service creates the record with correct parameters.
    /// </summary>
    [Fact]
    public async Task RecordUsageAsync_ValidRecord_CreatesInRepository()
    {
        _logger.LogInformation("Executing test: {TestName}", "RecordUsageAsync_ValidRecord_CreatesInRepository");
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
        _logger.LogInformation("Test completed: {TestName}", "RecordUsageAsync_ValidRecord_CreatesInRepository");
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.RecordUsageAsync(UsageRecord)"/> wraps repository exceptions in <see cref="DataAccessException"/>.
    /// Validates proper exception handling when repository operations fail.
    /// </summary>
    [Fact]
    public async Task RecordUsageAsync_RepositoryThrows_WrapsInDataAccessException()
    {
        _logger.LogInformation("Executing test: {TestName}", "RecordUsageAsync_RepositoryThrows_WrapsInDataAccessException");
        var record = new UsageRecord { ApiKeyId = "key-123" };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<UsageRecord>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var act = async () => await _sut.RecordUsageAsync(record);
        (await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.DataAccessException>())
            .WithInnerException<InvalidOperationException>()
            .WithMessage("DB error");
        _logger.LogInformation("Test completed: {TestName}", "RecordUsageAsync_RepositoryThrows_WrapsInDataAccessException");
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
        _logger.LogInformation("Executing test: {TestName} with keyId: {KeyId}", "GetUsageStatisticsAsync_EmptyOrNullKeyId_ThrowsValidationException", keyId);
        var act = async () => await _sut.GetUsageStatisticsAsync(keyId!, DateTime.UtcNow, DateTime.UtcNow);
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.ValidationException>();
        _logger.LogInformation("Test completed: {TestName}", "GetUsageStatisticsAsync_EmptyOrNullKeyId_ThrowsValidationException");
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetUsageStatisticsAsync(string, DateTime, DateTime)"/> throws <see cref="ValidationException"/> when end date is before start date.
    /// Validates date range validation logic.
    /// </summary>
    [Fact]
    public async Task GetUsageStatisticsAsync_EndDateBeforeStartDate_ThrowsValidationException()
    {
        _logger.LogInformation("Executing test: {TestName}", "GetUsageStatisticsAsync_EndDateBeforeStartDate_ThrowsValidationException");
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(-1);

        var act = async () => await _sut.GetUsageStatisticsAsync("key-123", startDate, endDate);
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.ValidationException>();
        _logger.LogInformation("Test completed: {TestName}", "GetUsageStatisticsAsync_EndDateBeforeStartDate_ThrowsValidationException");
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetUsageStatisticsAsync(string, DateTime, DateTime)"/> returns correct statistics for valid date range.
    /// Verifies aggregation of usage data including total requests, unique endpoints, success/failure rates, and average response time.
    /// </summary>
    [Fact]
    public async Task GetUsageStatisticsAsync_ValidDateRange_ReturnsStatisticsWithCorrectAggregates()
    {
        _logger.LogInformation("Executing test: {TestName} for ApiKeyId: {ApiKeyId}", "GetUsageStatisticsAsync_ValidDateRange_ReturnsStatisticsWithCorrectAggregates", "key-123");
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
        _logger.LogInformation("Test completed: {TestName}", "GetUsageStatisticsAsync_ValidDateRange_ReturnsStatisticsWithCorrectAggregates");
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetUsageStatisticsAsync(string, DateTime, DateTime)"/> returns zeroed statistics when no records exist.
    /// Validates handling of empty result sets.
    /// </summary>
    [Fact]
    public async Task GetUsageStatisticsAsync_NoRecords_ReturnsZeroedStatistics()
    {
        _logger.LogInformation("Executing test: {TestName} for ApiKeyId: {ApiKeyId}", "GetUsageStatisticsAsync_NoRecords_ReturnsZeroedStatistics", "key-missing");
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
        _logger.LogInformation("Test completed: {TestName}", "GetUsageStatisticsAsync_NoRecords_ReturnsZeroedStatistics");
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
        _logger.LogInformation("Executing test: {TestName} with keyId: {KeyId}", "GetUsageRecordsAsync_EmptyOrNullKeyId_ThrowsValidationException", keyId);
        var act = async () => await _sut.GetUsageRecordsAsync(keyId!, DateTime.UtcNow, DateTime.UtcNow);
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.ValidationException>();
        _logger.LogInformation("Test completed: {TestName}", "GetUsageRecordsAsync_EmptyOrNullKeyId_ThrowsValidationException");
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetUsageRecordsAsync(string, DateTime, DateTime)"/> returns usage records from repository.
    /// Verifies retrieval of raw usage data for a specific API key and date range.
    /// </summary>
    [Fact]
    public async Task GetUsageRecordsAsync_ValidDateRange_ReturnsRecordsFromRepository()
    {
        _logger.LogInformation("Executing test: {TestName} for ApiKeyId: {ApiKeyId}", "GetUsageRecordsAsync_ValidDateRange_ReturnsRecordsFromRepository", "key-456");
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
        _logger.LogInformation("Test completed: {TestName}", "GetUsageRecordsAsync_ValidDateRange_ReturnsRecordsFromRepository");
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
        _logger.LogInformation("Executing test: {TestName} with consumerId: {ConsumerId}", "GetTotalBytesUsedAsync_EmptyOrNullConsumerId_ReturnsZero", consumerId);
        var result = await _sut.GetTotalBytesUsedAsync(consumerId!, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
        result.Should().Be(0);
        _repositoryMock.Verify(r => r.GetByConsumerAndDateRangeAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        _logger.LogInformation("Test completed: {TestName}", "GetTotalBytesUsedAsync_EmptyOrNullConsumerId_ReturnsZero");
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetTotalBytesUsedAsync(string, DateTime, DateTime)"/> returns aggregated bytes transferred for valid consumer.
    /// Verifies calculation of total bytes from multiple usage records.
    /// </summary>
    [Fact]
    public async Task GetTotalBytesUsedAsync_ValidConsumerId_ReturnsAggregatedBytes()
    {
        _logger.LogInformation("Executing test: {TestName} for ConsumerId: {ConsumerId}", "GetTotalBytesUsedAsync_ValidConsumerId_ReturnsAggregatedBytes", "consumer-789");
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
        _logger.LogInformation("Test completed: {TestName}", "GetTotalBytesUsedAsync_ValidConsumerId_ReturnsAggregatedBytes");
    }

    /// <summary>
    /// Tests that <see cref="UsageTrackingService.GetTotalBytesUsedAsync(string, DateTime, DateTime)"/> returns 0 when no records exist.
    /// Validates handling of empty result sets for byte usage calculation.
    /// </summary>
    [Fact]
    public async Task GetTotalBytesUsedAsync_NoRecords_ReturnsZero()
    {
        _logger.LogInformation("Executing test: {TestName} for ConsumerId: {ConsumerId}", "GetTotalBytesUsedAsync_NoRecords_ReturnsZero", "consumer-missing");
        var consumerId = "consumer-missing";
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        _repositoryMock
            .Setup(r => r.GetByConsumerAndDateRangeAsync(consumerId, startDate, endDate))
            .ReturnsAsync(new List<UsageRecord>());

        var result = await _sut.GetTotalBytesUsedAsync(consumerId, startDate, endDate);

        result.Should().Be(0);
        _logger.LogInformation("Test completed: {TestName}", "GetTotalBytesUsedAsync_NoRecords_ReturnsZero");
    }
}
