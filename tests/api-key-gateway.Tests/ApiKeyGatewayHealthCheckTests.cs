// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Configuration;
using ApiKeyGateway.Data;
using ApiKeyGateway.Diagnostics;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for <see cref="ApiKeyGatewayHealthCheck"/> and the <c>AddApiKeyGatewayHealthCheck</c> registration.
/// </summary>
public class ApiKeyGatewayHealthCheckTests
{
    private readonly Mock<IDbConnection> _connectionMock;
    private readonly ServiceCollection _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyGatewayHealthCheckTests"/> class with a
    /// connection mock that opens and closes successfully.
    /// </summary>
    public ApiKeyGatewayHealthCheckTests()
    {
        _connectionMock = new Mock<IDbConnection>();
        _services = new ServiceCollection();
        _services.AddScoped(_ => _connectionMock.Object);
    }

    /// <summary>
    /// Verifies that the check is Healthy when the store is reachable and no usage tracker is registered.
    /// </summary>
    [Fact]
    public async Task CheckHealthAsync_StoreReachableWithoutTracker_ReturnsHealthy()
    {
        // Arrange
        var sut = CreateSut(threshold: 10);

        // Act
        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data["store"].Should().Be("reachable");
        result.Data["usageTracker"].Should().Be("not_registered");
        result.Data["rateLimiterBackend"].Should().Be("not_registered");
    }

    /// <summary>
    /// Verifies that a store connection failure makes the check Unhealthy and does not leak the provider message.
    /// </summary>
    [Fact]
    public async Task CheckHealthAsync_StoreOpenFails_ReturnsUnhealthyWithoutProviderMessage()
    {
        // Arrange
        _connectionMock.Setup(c => c.OpenAsync()).ThrowsAsync(new InvalidOperationException("Server=prod-sql-01;Password=hunter2"));
        var sut = CreateSut(threshold: 10);

        // Act
        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("API key store is unreachable");
        result.Data["store"].Should().Be("unreachable");
        result.Description.Should().NotContain("prod-sql-01");
        result.Data.Values.Should().NotContain(v => v.ToString()!.Contains("hunter2"));
    }

    /// <summary>
    /// Verifies that a pending flush queue above the threshold makes the check Degraded and reports the count.
    /// </summary>
    [Fact]
    public async Task CheckHealthAsync_PendingAboveThreshold_ReturnsDegraded()
    {
        // Arrange
        var trackerMock = new Mock<IUsageTracker>();
        trackerMock.Setup(t => t.GetPendingRequestCount()).Returns(11);
        _services.AddSingleton(trackerMock.Object);
        var sut = CreateSut(threshold: 10);

        // Act
        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data["usageTracker"].Should().Be("backlogged");
        result.Data["usageFlushPendingRequests"].Should().Be(11L);
        result.Data["usageFlushQueueThreshold"].Should().Be(10L);
    }

    /// <summary>
    /// Verifies that a pending count exactly at the threshold is still Healthy.
    /// </summary>
    [Fact]
    public async Task CheckHealthAsync_PendingAtThreshold_ReturnsHealthy()
    {
        // Arrange
        var trackerMock = new Mock<IUsageTracker>();
        trackerMock.Setup(t => t.GetPendingRequestCount()).Returns(10);
        _services.AddSingleton(trackerMock.Object);
        var sut = CreateSut(threshold: 10);

        // Act
        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data["usageTracker"].Should().Be("ok");
    }

    /// <summary>
    /// Verifies that a store failure takes precedence over a backlogged flush queue.
    /// </summary>
    [Fact]
    public async Task CheckHealthAsync_StoreDownAndBacklogged_ReturnsUnhealthy()
    {
        // Arrange
        _connectionMock.Setup(c => c.OpenAsync()).ThrowsAsync(new InvalidOperationException("down"));
        var trackerMock = new Mock<IUsageTracker>();
        trackerMock.Setup(t => t.GetPendingRequestCount()).Returns(500);
        _services.AddSingleton(trackerMock.Object);
        var sut = CreateSut(threshold: 10);

        // Act
        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    /// <summary>
    /// Verifies that a registered rate limiter is reported with its in-memory backend.
    /// </summary>
    [Fact]
    public async Task CheckHealthAsync_RateLimiterRegistered_ReportsInMemoryBackend()
    {
        // Arrange
        _services.AddSingleton(new Mock<IRateLimiter>().Object);
        var sut = CreateSut(threshold: 10);

        // Act
        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Data["rateLimiterBackend"].Should().Be("in_memory");
    }

    /// <summary>
    /// Verifies that <c>AddApiKeyGatewayHealthCheck</c> registers the check under its name with the ready tag.
    /// </summary>
    [Fact]
    public void AddApiKeyGatewayHealthCheck_Registration_IsTaggedReady()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHealthChecks().AddApiKeyGatewayHealthCheck(o => o.UsageFlushQueueDegradedThreshold = 5);
        var provider = services.BuildServiceProvider();

        // Act
        var registration = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations.Single();
        var thresholdOptions = provider.GetRequiredService<IOptions<ApiKeyGatewayHealthCheckOptions>>().Value;

        // Assert
        registration.Name.Should().Be("api-key-gateway");
        registration.Tags.Should().Contain("ready");
        thresholdOptions.UsageFlushQueueDegradedThreshold.Should().Be(5);
    }

    private ApiKeyGatewayHealthCheck CreateSut(long threshold)
    {
        var provider = _services.BuildServiceProvider();
        var options = Options.Create(new ApiKeyGatewayHealthCheckOptions { UsageFlushQueueDegradedThreshold = threshold });
        return new ApiKeyGatewayHealthCheck(
            provider.GetRequiredService<IServiceScopeFactory>(),
            options,
            NullLogger<ApiKeyGatewayHealthCheck>.Instance);
    }
}
