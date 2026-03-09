// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

public class UsageAnalyticsServiceTests
{
    private readonly Mock<IUsageTrackingService> _trackingMock;
    private readonly Mock<ILogger<UsageAnalyticsService>> _loggerMock;
    private readonly UsageAnalyticsService _sut;

    private static readonly DateTime From = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = new(2025, 1, 8, 0, 0, 0, DateTimeKind.Utc);

    public UsageAnalyticsServiceTests()
    {
        _trackingMock = new Mock<IUsageTrackingService>();
        _loggerMock = new Mock<ILogger<UsageAnalyticsService>>();
        _sut = new UsageAnalyticsService(_trackingMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetSummaryAsync_EmptyKeyId_ThrowsArgumentException()
    {
        var act = async () => await _sut.GetSummaryAsync(string.Empty, From, To);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetSummaryAsync_EndBeforeStart_ThrowsArgumentException()
    {
        var act = async () => await _sut.GetSummaryAsync("key-1", To, From);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetSummaryAsync_NoRecords_ReturnsZeroMetrics()
    {
        // Arrange
        _trackingMock
            .Setup(t => t.GetUsageRecordsAsync("key-1", From, To))
            .ReturnsAsync([]);

        // Act
        var summary = await _sut.GetSummaryAsync("key-1", From, To);

        // Assert
        summary.TotalRequests.Should().Be(0);
        summary.SuccessfulRequests.Should().Be(0);
        summary.FailedRequests.Should().Be(0);
        summary.SuccessRatePercent.Should().Be(0);
        summary.ErrorRatePercent.Should().Be(0);
        summary.AverageResponseTimeMs.Should().Be(0);
    }

    [Fact]
    public async Task GetSummaryAsync_MixedRecords_CalculatesMetricsCorrectly()
    {
        // Arrange
        var records = BuildRecords(new[]
        {
            (200, 100, "/api/a", "GET", "10.0.0.1"),
            (200, 200, "/api/b", "POST", "10.0.0.2"),
            (500, 50,  "/api/a", "GET", "10.0.0.1"),
            (404, 75,  "/api/c", "GET", "10.0.0.3")
        });

        _trackingMock
            .Setup(t => t.GetUsageRecordsAsync("key-1", From, To))
            .ReturnsAsync(records);

        // Act
        var summary = await _sut.GetSummaryAsync("key-1", From, To);

        // Assert
        summary.TotalRequests.Should().Be(4);
        summary.SuccessfulRequests.Should().Be(2);
        summary.FailedRequests.Should().Be(2);
        summary.SuccessRatePercent.Should().Be(50);
        summary.ErrorRatePercent.Should().Be(50);
        summary.AverageResponseTimeMs.Should().Be(106.25); // (100+200+50+75)/4
        summary.UniqueEndpoints.Should().Be(3);
        summary.UniqueSourceIps.Should().Be(3);
    }

    [Fact]
    public async Task GetTopEndpointsAsync_ReturnsEndpointsOrderedByCount()
    {
        // Arrange
        var records = BuildRecords(new[]
        {
            (200, 100, "/api/users", "GET", "1.1.1.1"),
            (200, 100, "/api/users", "GET", "1.1.1.1"),
            (200, 100, "/api/users", "GET", "1.1.1.1"),
            (200, 100, "/api/orders", "GET", "1.1.1.1"),
            (200, 100, "/api/orders", "GET", "1.1.1.1"),
            (500, 100, "/api/items",  "GET", "1.1.1.1")
        });

        _trackingMock
            .Setup(t => t.GetUsageRecordsAsync("key-1", From, To))
            .ReturnsAsync(records);

        // Act
        var endpoints = await _sut.GetTopEndpointsAsync("key-1", From, To, limit: 10);

        // Assert
        endpoints.Should().HaveCount(3);
        endpoints[0].Endpoint.Should().Be("/api/users");
        endpoints[0].RequestCount.Should().Be(3);
        endpoints[1].Endpoint.Should().Be("/api/orders");
        endpoints[1].RequestCount.Should().Be(2);
        endpoints[2].Endpoint.Should().Be("/api/items");
        endpoints[2].ErrorCount.Should().Be(1);
        endpoints[2].ErrorRatePercent.Should().Be(100);
    }

    [Fact]
    public async Task GetTopEndpointsAsync_LimitRespected()
    {
        // Arrange
        var records = BuildRecords(new[]
        {
            (200, 10, "/api/a", "GET", "1.1.1.1"),
            (200, 10, "/api/b", "GET", "1.1.1.1"),
            (200, 10, "/api/c", "GET", "1.1.1.1")
        });

        _trackingMock
            .Setup(t => t.GetUsageRecordsAsync("key-1", From, To))
            .ReturnsAsync(records);

        // Act
        var endpoints = await _sut.GetTopEndpointsAsync("key-1", From, To, limit: 2);

        // Assert
        endpoints.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetHourlyTrendAsync_GroupsByHour()
    {
        // Arrange
        var hour1 = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var hour2 = new DateTime(2025, 1, 1, 11, 0, 0, DateTimeKind.Utc);
        var records = new List<UsageRecord>
        {
            MakeRecord(200, 100, "/a", "GET", hour1.AddMinutes(10)),
            MakeRecord(200, 200, "/a", "GET", hour1.AddMinutes(30)),
            MakeRecord(500, 50,  "/b", "GET", hour2.AddMinutes(5))
        };

        _trackingMock
            .Setup(t => t.GetUsageRecordsAsync("key-1", From, To))
            .ReturnsAsync(records);

        // Act
        var buckets = await _sut.GetHourlyTrendAsync("key-1", From, To);

        // Assert
        buckets.Should().HaveCount(2);
        buckets[0].Hour.Should().Be(hour1);
        buckets[0].RequestCount.Should().Be(2);
        buckets[0].ErrorCount.Should().Be(0);
        buckets[1].Hour.Should().Be(hour2);
        buckets[1].RequestCount.Should().Be(1);
        buckets[1].ErrorCount.Should().Be(1);
    }

    [Fact]
    public async Task GetDailyTrendAsync_GroupsByDay()
    {
        // Arrange
        var day1 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var day2 = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var records = new List<UsageRecord>
        {
            MakeRecord(200, 100, "/a", "GET", day1.AddHours(8),  requestBytes: 100, responseBytes: 200),
            MakeRecord(200, 150, "/b", "GET", day1.AddHours(10), requestBytes: 50,  responseBytes: 100),
            MakeRecord(404, 80,  "/c", "GET", day2.AddHours(6))
        };

        _trackingMock
            .Setup(t => t.GetUsageRecordsAsync("key-1", From, To))
            .ReturnsAsync(records);

        // Act
        var buckets = await _sut.GetDailyTrendAsync("key-1", From, To);

        // Assert
        buckets.Should().HaveCount(2);
        buckets[0].Date.Should().Be(day1);
        buckets[0].RequestCount.Should().Be(2);
        buckets[0].ErrorCount.Should().Be(0);
        buckets[0].TotalBytes.Should().Be(450); // 100+200+50+100
        buckets[1].Date.Should().Be(day2);
        buckets[1].ErrorCount.Should().Be(1);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static List<UsageRecord> BuildRecords(
        (int statusCode, int responseTimeMs, string endpoint, string method, string ip)[] tuples)
    {
        var baseTime = new DateTime(2025, 1, 3, 12, 0, 0, DateTimeKind.Utc);
        return tuples
            .Select((t, i) => MakeRecord(t.statusCode, t.responseTimeMs, t.endpoint, t.method,
                baseTime.AddMinutes(i), sourceIp: t.ip))
            .ToList();
    }

    private static UsageRecord MakeRecord(
        int statusCode, int responseTimeMs, string endpoint, string method,
        DateTime recordedAt, string? sourceIp = null,
        long requestBytes = 0, long responseBytes = 0) => new()
    {
        Id = Guid.NewGuid().ToString(),
        ApiKeyId = "key-1",
        ConsumerId = "consumer-1",
        RecordedAt = recordedAt,
        Endpoint = endpoint,
        Method = method,
        ResponseStatusCode = statusCode,
        ResponseTimeMs = responseTimeMs,
        SourceIp = sourceIp,
        RequestBytes = requestBytes,
        ResponseBytes = responseBytes
    };
}
