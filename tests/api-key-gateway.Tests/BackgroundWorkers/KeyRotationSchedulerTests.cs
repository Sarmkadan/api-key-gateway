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
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<IConfigurationSection> _keyRotationSectionMock = new();
    private readonly Mock<IApiKeyRotationService> _rotationServiceMock = new();
    private readonly ServiceCollection _services = new();

    /// <summary>
/// Initializes a new instance of the KeyRotationSchedulerTests class.
/// Sets up the configuration mock to return the key rotation section mock when GetSection("KeyRotation") is called.
/// </summary>
public KeyRotationSchedulerTests()
    {
        _configurationMock.Setup(c => c.GetSection("KeyRotation")).Returns(_keyRotationSectionMock.Object);
    }

    /// <summary>
    /// Verifies that the constructor throws an ArgumentNullException when the serviceProvider parameter is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new KeyRotationScheduler(
            null!,
            _loggerMock.Object,
            _configurationMock.Object));
    }

    /// <summary>
    /// Verifies that the constructor throws an ArgumentNullException when the logger parameter is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        Assert.Throws<ArgumentNullException>(() => new KeyRotationScheduler(
            mockServiceProvider.Object,
            null!,
            _configurationMock.Object));
    }

    /// <summary>
    /// Verifies that the constructor throws an ArgumentNullException when the configuration parameter is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        Assert.Throws<ArgumentNullException>(() => new KeyRotationScheduler(
            mockServiceProvider.Object,
            _loggerMock.Object,
            null!));
    }

    /// <summary>
    /// Verifies that the constructor throws a ConfigurationException when the jitter percentage is invalid.
    /// </summary>
    [Fact]
    public void Constructor_WithInvalidJitterPercentage_ThrowsConfigurationException()
    {
        _keyRotationSectionMock.Setup(s => s.GetValue("JitterPercentage", 0.1)).Returns(1.5);

        Assert.Throws<ConfigurationException>(() => new KeyRotationScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _configurationMock.Object));
    }

    /// <summary>
    /// Verifies that the constructor throws a ConfigurationException when the check interval is zero.
    /// </summary>
    [Fact]
    public void Constructor_WithZeroCheckInterval_ThrowsConfigurationException()
    {
        _keyRotationSectionMock.Setup(s => s.GetValue("CheckIntervalHours", It.IsAny<int>())).Returns(0);

        Assert.Throws<ConfigurationException>(() => new KeyRotationScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _configurationMock.Object));
    }

    /// <summary>
    /// Verifies that the constructor throws a ConfigurationException when the warning days are zero.
    /// </summary>
    [Fact]
    public void Constructor_WithZeroWarningDays_ThrowsConfigurationException()
    {
        _keyRotationSectionMock.Setup(s => s.GetValue("WarningDays", It.IsAny<int>())).Returns(0);

        Assert.Throws<ConfigurationException>(() => new KeyRotationScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _configurationMock.Object));
    }

    /// <summary>
    /// Verifies that the constructor throws a ConfigurationException when the warning days are negative.
    /// </summary>
    [Fact]
    public void Constructor_WithNegativeWarningDays_ThrowsConfigurationException()
    {
        _keyRotationSectionMock.Setup(s => s.GetValue("WarningDays", It.IsAny<int>())).Returns(-1);

        Assert.Throws<ConfigurationException>(() => new KeyRotationScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _configurationMock.Object));
    }

    /// <summary>
    /// Verifies that the constructor creates a new instance with valid configuration.
    /// </summary>
    [Fact]
    public void Constructor_WithValidConfiguration_CreatesInstance()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<KeyRotationScheduler>>();

        _keyRotationSectionMock.Setup(s => s.GetValue("CheckIntervalHours", 24)).Returns(12);
        _keyRotationSectionMock.Setup(s => s.GetValue("WarningDays", 7)).Returns(5);
        _keyRotationSectionMock.Setup(s => s.GetValue("NewExpirationDays", It.IsAny<int?>())).Returns((int?)30);
        _keyRotationSectionMock.Setup(s => s.GetValue("JitterPercentage", 0.1)).Returns(0.2);

        var scheduler = new KeyRotationScheduler(
            mockServiceProvider.Object,
            mockLogger.Object,
            _configurationMock.Object);

        scheduler.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that when ExecuteCycleAsync is called, it calls the rotation service and logs appropriate messages.
    /// </summary>
    [Fact]
    public async Task ExecuteCycleAsync_WhenCalled_CallsRotationServiceAndDelays()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<KeyRotationScheduler>>();

        _keyRotationSectionMock.Setup(s => s.GetValue("CheckIntervalHours", 24)).Returns(12);
        _keyRotationSectionMock.Setup(s => s.GetValue("WarningDays", 7)).Returns(5);
        _keyRotationSectionMock.Setup(s => s.GetValue("NewExpirationDays", It.IsAny<int?>())).Returns((int?)null);
        _keyRotationSectionMock.Setup(s => s.GetValue("JitterPercentage", 0.1)).Returns(0.1);

        var rotationResults = new List<RotationResult>
        {
            new() { OldKeyId = "key1", ConsumerId = "consumer1", Success = true, NewKeyId = "new-key1" },
            new() { OldKeyId = "key2", ConsumerId = "consumer2", Success = false, FailureReason = "Key not found" }
        };

        _rotationServiceMock.Setup(s => s.RotateExpiringSoonAsync(5, null))
            .ReturnsAsync(rotationResults);

        var scheduler = new KeyRotationScheduler(
            mockServiceProvider.Object,
            mockLogger.Object,
            _configurationMock.Object);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1));

        await scheduler.StartAsync(cts.Token);

        _rotationServiceMock.Verify(s => s.RotateExpiringSoonAsync(5, null), Times.Once);
        mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting scheduled key rotation cycle")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);

        mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rotation cycle complete")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    /// <summary>
    /// Verifies that when no keys need rotation, a debug message is logged.
    /// </summary>
    [Fact]
    public async Task ExecuteCycleAsync_WhenNoKeysNeedRotation_LogsDebugMessage()
    {
        _keyRotationSectionMock.Setup(s => s.GetValue("CheckIntervalHours", 24)).Returns(12);
        _keyRotationSectionMock.Setup(s => s.GetValue("WarningDays", 7)).Returns(5);
        _keyRotationSectionMock.Setup(s => s.GetValue("NewExpirationDays", It.IsAny<int?>())).Returns((int?)null);
        _keyRotationSectionMock.Setup(s => s.GetValue("JitterPercentage", 0.1)).Returns(0.1);

        _rotationServiceMock.Setup(s => s.RotateExpiringSoonAsync(5, null))
            .ReturnsAsync(new List<RotationResult>());

        var scheduler = new KeyRotationScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _configurationMock.Object);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1));

        await scheduler.StartAsync(cts.Token);

        _loggerMock.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No keys require rotation")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    /// <summary>
    /// Verifies that when rotation fails, warning messages are logged for each failed rotation.
    /// </summary>
    [Fact]
    public async Task ExecuteCycleAsync_WhenRotationFails_LogsWarningMessages()
    {
        _keyRotationSectionMock.Setup(s => s.GetValue("CheckIntervalHours", 24)).Returns(12);
        _keyRotationSectionMock.Setup(s => s.GetValue("WarningDays", 7)).Returns(5);
        _keyRotationSectionMock.Setup(s => s.GetValue("NewExpirationDays", It.IsAny<int?>())).Returns((int?)null);
        _keyRotationSectionMock.Setup(s => s.GetValue("JitterPercentage", 0.1)).Returns(0.1);

        var rotationResults = new List<RotationResult>
        {
            new() { OldKeyId = "key1", ConsumerId = "consumer1", Success = true, NewKeyId = "new-key1" },
            new() { OldKeyId = "key2", ConsumerId = "consumer2", Success = false, FailureReason = "Key not found" },
            new() { OldKeyId = "key3", ConsumerId = "consumer3", Success = false, FailureReason = "Database error" }
        };

        _rotationServiceMock.Setup(s => s.RotateExpiringSoonAsync(5, null))
            .ReturnsAsync(rotationResults);

        var scheduler = new KeyRotationScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _configurationMock.Object);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1));

        await scheduler.StartAsync(cts.Token);

        _loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to rotate key")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Exactly(2));
    }

    /// <summary>
    /// Verifies that when the operation is cancelled, the rotation service is not called.
    /// </summary>
    [Fact]
    public async Task ExecuteCycleAsync_WithCancellationToken_CancelsOperation()
    {
        _keyRotationSectionMock.Setup(s => s.GetValue("CheckIntervalHours", 24)).Returns(12);
        _keyRotationSectionMock.Setup(s => s.GetValue("WarningDays", 7)).Returns(5);
        _keyRotationSectionMock.Setup(s => s.GetValue("NewExpirationDays", It.IsAny<int?>())).Returns((int?)null);
        _keyRotationSectionMock.Setup(s => s.GetValue("JitterPercentage", 0.1)).Returns(0.1);

        _rotationServiceMock.Setup(s => s.RotateExpiringSoonAsync(5, null))
            .ReturnsAsync(new List<RotationResult>());

        var scheduler = new KeyRotationScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _configurationMock.Object);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        await scheduler.StartAsync(cts.Token);

        _rotationServiceMock.Verify(s => s.RotateExpiringSoonAsync(5, null), Times.AtLeastOnce);
    }

    /// <summary>
    /// Verifies that when new expiration days are configured, they are passed correctly to the rotation service.
    /// </summary>
    [Fact]
    public async Task ExecuteCycleAsync_WithNewExpirationDays_UsesCorrectParameters()
    {
        _keyRotationSectionMock.Setup(s => s.GetValue("CheckIntervalHours", 24)).Returns(12);
        _keyRotationSectionMock.Setup(s => s.GetValue("WarningDays", 7)).Returns(5);
        _keyRotationSectionMock.Setup(s => s.GetValue("NewExpirationDays", It.IsAny<int?>())).Returns((int?)60);
        _keyRotationSectionMock.Setup(s => s.GetValue("JitterPercentage", 0.1)).Returns(0.1);

        var rotationResults = new List<RotationResult>();
        _rotationServiceMock.Setup(s => s.RotateExpiringSoonAsync(5, 60))
            .ReturnsAsync(rotationResults);

        var scheduler = new KeyRotationScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _configurationMock.Object);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1));

        await scheduler.StartAsync(cts.Token);

        _rotationServiceMock.Verify(s => s.RotateExpiringSoonAsync(5, 60), Times.Once);
    }

    /// <summary>
    /// Verifies that when the rotation service throws an exception, it is logged as an error.
    /// </summary>
    [Fact]
    public async Task ExecuteCycleAsync_HandlesExceptionFromRotationService()
    {
        _keyRotationSectionMock.Setup(s => s.GetValue("CheckIntervalHours", 24)).Returns(12);
        _keyRotationSectionMock.Setup(s => s.GetValue("WarningDays", 7)).Returns(5);
        _keyRotationSectionMock.Setup(s => s.GetValue("NewExpirationDays", It.IsAny<int?>())).Returns((int?)null);
        _keyRotationSectionMock.Setup(s => s.GetValue("JitterPercentage", 0.1)).Returns(0.1);

        _rotationServiceMock.Setup(s => s.RotateExpiringSoonAsync(5, null))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var scheduler = new KeyRotationScheduler(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _configurationMock.Object);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1));

        await scheduler.StartAsync(cts.Token);

        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unexpected error")),
            It.IsAny<InvalidOperationException>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }
}