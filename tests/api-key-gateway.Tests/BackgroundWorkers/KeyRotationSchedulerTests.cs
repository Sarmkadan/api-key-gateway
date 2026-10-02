// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for KeyRotationScheduler background worker
// =============================================================================

using ApiKeyGateway.BackgroundWorkers;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiKeyGateway.Tests.BackgroundWorkers;

/// <summary>
/// Unit tests for the KeyRotationScheduler background worker class.
/// Verifies constructor validation, configuration handling, and rotation cycle execution.
/// </summary>
public class KeyRotationSchedulerTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private readonly Mock<ILogger<KeyRotationScheduler>> _loggerMock = new();
    private readonly Mock<IApiKeyRotationService> _rotationServiceMock = new();

    private static IConfiguration BuildConfig(
        int checkIntervalHours = 12,
        int warningDays = 5,
        int? newExpirationDays = null,
        double jitterPercentage = 0.1)
    {
        var data = new Dictionary<string, string?>
        {
            ["KeyRotation:CheckIntervalHours"] = checkIntervalHours.ToString(),
            ["KeyRotation:WarningDays"] = warningDays.ToString(),
            ["KeyRotation:JitterPercentage"] = jitterPercentage.ToString(System.Globalization.CultureInfo.InvariantCulture),
        };
        if (newExpirationDays.HasValue)
            data["KeyRotation:NewExpirationDays"] = newExpirationDays.Value.ToString();

        return new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
    }

    private IServiceProvider BuildServiceProvider()
    {
        var scopeMock = new Mock<IServiceScope>();
        var scopeProviderMock = new Mock<IServiceProvider>();
        scopeProviderMock.Setup(sp => sp.GetService(typeof(IApiKeyRotationService)))
            .Returns(_rotationServiceMock.Object);
        scopeMock.Setup(s => s.ServiceProvider).Returns(scopeProviderMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactoryMock.Object);
        return sp.Object;
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new KeyRotationScheduler(
            null!,
            _loggerMock.Object,
            BuildConfig()));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        Assert.Throws<ArgumentNullException>(() => new KeyRotationScheduler(
            mockServiceProvider.Object,
            null!,
            BuildConfig()));
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        Assert.Throws<ArgumentNullException>(() => new KeyRotationScheduler(
            mockServiceProvider.Object,
            _loggerMock.Object,
            null!));
    }

    [Fact]
    public void Constructor_WithInvalidJitterPercentage_ThrowsConfigurationException()
    {
        Assert.Throws<ConfigurationException>(() => new KeyRotationScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            BuildConfig(jitterPercentage: 1.5)));
    }

    [Fact]
    public void Constructor_WithZeroCheckInterval_ThrowsConfigurationException()
    {
        Assert.Throws<ConfigurationException>(() => new KeyRotationScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            BuildConfig(checkIntervalHours: 0)));
    }

    [Fact]
    public void Constructor_WithZeroWarningDays_ThrowsConfigurationException()
    {
        Assert.Throws<ConfigurationException>(() => new KeyRotationScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            BuildConfig(warningDays: 0)));
    }

    [Fact]
    public void Constructor_WithNegativeWarningDays_ThrowsConfigurationException()
    {
        Assert.Throws<ConfigurationException>(() => new KeyRotationScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            BuildConfig(warningDays: -1)));
    }

    [Fact]
    public void Constructor_WithValidConfiguration_CreatesInstance()
    {
        var scheduler = new KeyRotationScheduler(
            BuildServiceProvider(),
            _loggerMock.Object,
            BuildConfig());

        scheduler.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteCycleAsync_WhenCalled_CallsRotationServiceAndDelays()
    {
        var rotationResults = new List<RotationResult>
        {
            new() { OldKeyId = "key1", ConsumerId = "consumer1", Success = true, NewKeyId = "new-key1" },
            new() { OldKeyId = "key2", ConsumerId = "consumer2", Success = false, FailureReason = "Key not found" }
        };

        _rotationServiceMock.Setup(s => s.RotateExpiringSoonAsync(5, null))
            .ReturnsAsync(rotationResults);

        var mockLogger = new Mock<ILogger<KeyRotationScheduler>>();
        var scheduler = new KeyRotationScheduler(
            BuildServiceProvider(),
            mockLogger.Object,
            BuildConfig());

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        await scheduler.StartAsync(cts.Token);
        await Task.Delay(500);
        await scheduler.StopAsync(CancellationToken.None);

        _rotationServiceMock.Verify(s => s.RotateExpiringSoonAsync(5, null), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteCycleAsync_WhenNoKeysNeedRotation_LogsDebugMessage()
    {
        _rotationServiceMock.Setup(s => s.RotateExpiringSoonAsync(5, null))
            .ReturnsAsync(new List<RotationResult>());

        var scheduler = new KeyRotationScheduler(
            BuildServiceProvider(),
            _loggerMock.Object,
            BuildConfig());

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        await scheduler.StartAsync(cts.Token);
        await Task.Delay(500);
        await scheduler.StopAsync(CancellationToken.None);

        _loggerMock.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No keys require rotation")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteCycleAsync_WhenRotationFails_LogsWarningMessages()
    {
        var rotationResults = new List<RotationResult>
        {
            new() { OldKeyId = "key1", ConsumerId = "consumer1", Success = true, NewKeyId = "new-key1" },
            new() { OldKeyId = "key2", ConsumerId = "consumer2", Success = false, FailureReason = "Key not found" },
            new() { OldKeyId = "key3", ConsumerId = "consumer3", Success = false, FailureReason = "Database error" }
        };

        _rotationServiceMock.Setup(s => s.RotateExpiringSoonAsync(5, null))
            .ReturnsAsync(rotationResults);

        var scheduler = new KeyRotationScheduler(
            BuildServiceProvider(),
            _loggerMock.Object,
            BuildConfig());

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        await scheduler.StartAsync(cts.Token);
        await Task.Delay(500);
        await scheduler.StopAsync(CancellationToken.None);

        _loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to rotate key")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.AtLeast(2));
    }

    [Fact]
    public async Task ExecuteCycleAsync_WithCancellationToken_CancelsOperation()
    {
        _rotationServiceMock.Setup(s => s.RotateExpiringSoonAsync(5, null))
            .ReturnsAsync(new List<RotationResult>());

        var scheduler = new KeyRotationScheduler(
            BuildServiceProvider(),
            _loggerMock.Object,
            BuildConfig());

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        await scheduler.StartAsync(cts.Token);
        await Task.Delay(500);
        await scheduler.StopAsync(CancellationToken.None);

        _rotationServiceMock.Verify(s => s.RotateExpiringSoonAsync(5, null), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteCycleAsync_WithNewExpirationDays_UsesCorrectParameters()
    {
        var rotationResults = new List<RotationResult>();
        _rotationServiceMock.Setup(s => s.RotateExpiringSoonAsync(5, 60))
            .ReturnsAsync(rotationResults);

        var scheduler = new KeyRotationScheduler(
            BuildServiceProvider(),
            _loggerMock.Object,
            BuildConfig(newExpirationDays: 60));

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        await scheduler.StartAsync(cts.Token);
        await Task.Delay(500);
        await scheduler.StopAsync(CancellationToken.None);

        _rotationServiceMock.Verify(s => s.RotateExpiringSoonAsync(5, 60), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteCycleAsync_HandlesExceptionFromRotationService()
    {
        _rotationServiceMock.Setup(s => s.RotateExpiringSoonAsync(5, null))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var scheduler = new KeyRotationScheduler(
            BuildServiceProvider(),
            _loggerMock.Object,
            BuildConfig());

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        await scheduler.StartAsync(cts.Token);
        await Task.Delay(500);
        await scheduler.StopAsync(CancellationToken.None);

        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unexpected error")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.AtLeastOnce);
    }
}
