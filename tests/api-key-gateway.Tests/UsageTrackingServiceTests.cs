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

public class UsageTrackingServiceTests
{
    private readonly Mock<IUsageRepository> _repositoryMock;
    private readonly Mock<ILogger<UsageTrackingService>> _loggerMock;
    private readonly UsageTrackingService _sut;

    public UsageTrackingServiceTests()
    {
        _repositoryMock = new Mock<IUsageRepository>();
        _loggerMock = new Mock<ILogger<UsageTrackingService>>();
        _sut = new UsageTrackingService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new UsageTrackingService(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("repository");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new UsageTrackingService(_repositoryMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task RecordUsageAsync_NullRecord_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.RecordUsageAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("record");
    }

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

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task GetUsageStatisticsAsync_EmptyOrNullKeyId_ThrowsValidationException(string? keyId)
    {
        var act = async () => await _sut.GetUsageStatisticsAsync(keyId!, DateTime.UtcNow, DateTime.UtcNow);
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task GetUsageStatisticsAsync_EndDateBeforeStartDate_ThrowsValidationException()
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(-1);

        var act = async () => await _sut.GetUsageStatisticsAsync("key-123", startDate, endDate);
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.ValidationException>();
    }

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

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task GetUsageRecordsAsync_EmptyOrNullKeyId_ThrowsValidationException(string? keyId)
    {
        var act = async () => await _sut.GetUsageRecordsAsync(keyId!, DateTime.UtcNow, DateTime.UtcNow);
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.ValidationException>();
    }

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

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetTotalBytesUsedAsync_EmptyOrNullConsumerId_ReturnsZero(string? consumerId)
    {
        var result = await _sut.GetTotalBytesUsedAsync(consumerId!, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
        result.Should().Be(0);
        _repositoryMock.Verify(r => r.GetByConsumerAndDateRangeAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

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
