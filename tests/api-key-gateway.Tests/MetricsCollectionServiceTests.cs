using Xunit;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for <see cref="MetricsCollectionService"/>.
/// Verifies that request and error recording correctly updates the service's metrics snapshot.
/// </summary>
public class MetricsCollectionServiceTests
{
    private readonly Mock<ILogger<MetricsCollectionService>> _loggerMock;
    private readonly MetricsCollectionService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsCollectionServiceTests"/> class.
    /// Sets up a mocked <see cref="ILogger{MetricsCollectionService}"/> and creates the service under test.
    /// </summary>
    public MetricsCollectionServiceTests()
    {
        _loggerMock = new Mock<ILogger<MetricsCollectionService>>();
        _sut = new MetricsCollectionService(_loggerMock.Object);
    }

    /// <summary>
    /// Verifies that calling <see cref="MetricsCollectionService.RecordRequest"/>
    /// with valid data increments the total request count and updates the average latency.
    /// </summary>
    [Fact]
    public void RecordRequest_ValidData_UpdatesMetrics()
    {
        // Act
        _sut.RecordRequest("api-1", "GET /test", 200, 100);
        var snapshot = _sut.GetSnapshot();

        // Assert
        snapshot.TotalRequests.Should().Be(1);
        snapshot.AverageLatencyMs.Should().Be(100);
    }

    /// <summary>
    /// Verifies that calling <see cref="MetricsCollectionService.RecordError"/>
    /// with valid data increments the total error count and records the error code.
    /// </summary>
    [Fact]
    public void RecordError_ValidData_UpdatesMetrics()
    {
        // Act
        _sut.RecordError("api-1", "ERR_01");
        var snapshot = _sut.GetSnapshot();

        // Assert
        snapshot.TotalErrors.Should().Be(1);
        snapshot.ErrorsByCode.Should().ContainKey("ERR_01");
    }
}
