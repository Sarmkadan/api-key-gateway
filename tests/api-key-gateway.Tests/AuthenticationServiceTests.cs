// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Xunit;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UnauthorizedAccessException = ApiKeyGateway.Domain.Exceptions.UnauthorizedAccessException;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Tests for the AuthenticationService class.
/// </summary>
public class AuthenticationServiceTests
{
    private readonly Mock<IApiKeyService> _apiKeyServiceMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly AuthenticationService _sut;

    /// <summary>
    /// Initializes a new instance of the AuthenticationServiceTests class.
    /// </summary>
    public AuthenticationServiceTests()
    {
        _apiKeyServiceMock = new Mock<IApiKeyService>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _loggerMock = new Mock<ILogger<AuthenticationService>>();
        _sut = new AuthenticationService(_apiKeyServiceMock.Object, _auditLogServiceMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Tests that the constructor throws an ArgumentNullException when the apiKeyService parameter is null.
    /// </summary>
    [Fact]
    public void Constructor_NullApiKeyService_ThrowsArgumentNullException()
    {
        var act = () => new AuthenticationService(null!, _auditLogServiceMock.Object, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("apiKeyService");
    }

    /// <summary>
    /// Tests that the constructor throws an ArgumentNullException when the auditLogService parameter is null.
    /// </summary>
    [Fact]
    public void Constructor_NullAuditLogService_ThrowsArgumentNullException()
    {
        var act = () => new AuthenticationService(_apiKeyServiceMock.Object, null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("auditLogService");
    }

    /// <summary>
    /// Tests that the constructor throws an ArgumentNullException when the logger parameter is null.
    /// </summary>
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new AuthenticationService(_apiKeyServiceMock.Object, _auditLogServiceMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method returns a failure result when the apiKey parameter is empty or null.
    /// </summary>
    /// <param name="apiKey">The API key to test.</param>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task AuthenticateAsync_EmptyOrNullApiKey_ReturnsFailureResult(string? apiKey)
    {
        var result = await _sut.AuthenticateAsync(apiKey!, "192.168.1.1");
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(AuthenticationFailureReason.MissingApiKey);
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method returns a failure result when the key is invalid.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_InvalidKey_ReturnsFailureResultWithInvalidFormatReason()
    {
        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_invalidkey"))
            .ReturnsAsync((ApiKey?)null);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_invalidkey", "192.168.1.1");
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(AuthenticationFailureReason.InvalidApiKeyFormat);
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method returns a failure result when the key is expired.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_ExpiredKey_ReturnsFailureResultWithExpiredReason()
    {
        var expiredKey = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddDays(-1) // Expired yesterday
        };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_expiredkey"))
            .ReturnsAsync(expiredKey);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_expiredkey", "192.168.1.1");
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(AuthenticationFailureReason.ApiKeyExpired);
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method returns a failure result when the key is disabled.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_DisabledKey_ReturnsFailureResultWithDisabledReason()
    {
        var disabledKey = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Disabled
        };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_disabledkey"))
            .ReturnsAsync(disabledKey);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_disabledkey", "192.168.1.1");
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(AuthenticationFailureReason.ApiKeyDisabled);
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method returns a success result with the API key when the key is valid.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_ValidKey_ReturnsSuccessResultWithKey()
    {
        var key = new ApiKey { Id = "key-123", ConsumerId = "consumer-abc", Status = ApiKeyStatus.Active };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_validkey123456789"))
            .ReturnsAsync(key);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_validkey123456789", "192.168.1.1");
        result.Success.Should().BeTrue();
        result.ApiKey.Should().NotBeNull();
        result.ApiKey!.Id.Should().Be("key-123");
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method returns a failure result when the IP address is not whitelisted.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_IpNotWhitelisted_ReturnsFailureResultWithIpNotWhitelistedReason()
    {
        var key = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Active,
            IpWhitelist = "192.168.1.1,192.168.1.2"
        };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_whitelistedkey"))
            .ReturnsAsync(key);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_whitelistedkey", "192.168.1.50");
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(AuthenticationFailureReason.IpNotWhitelisted);
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method returns a success result when the IP address is whitelisted.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_IpWhitelisted_ReturnsSuccessResultWithKey()
    {
        var key = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Active,
            IpWhitelist = "192.168.1.1,192.168.1.2"
        };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_whitelistedkey"))
            .ReturnsAsync(key);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_whitelistedkey", "192.168.1.1");
        result.Success.Should().BeTrue();
        result.ApiKey.Should().NotBeNull();
        result.ApiKey!.Id.Should().Be("key-123");
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method records usage when authentication succeeds.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_ValidKey_RecordsUsage()
    {
        var key = new ApiKey { Id = "key-123", ConsumerId = "consumer-abc", Status = ApiKeyStatus.Active };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_validkey123456789"))
            .ReturnsAsync(key);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_validkey123456789", "192.168.1.1");

        result.Success.Should().BeTrue();
        result.ApiKey.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method handles DataAccessException and returns service unavailable result.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_DataAccessException_ReturnsServiceUnavailableResult()
    {
        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync(It.IsAny<string>()))
            .ThrowsAsync(new DataAccessException("Database connection failed"));

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_testkey", "192.168.1.1");
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(AuthenticationFailureReason.ServiceUnavailable);
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method handles generic exceptions and returns service unavailable result.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_GenericException_ReturnsServiceUnavailableResult()
    {
        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_testkey", "192.168.1.1");
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(AuthenticationFailureReason.ServiceUnavailable);
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Tests that the ValidateIpAsync method throws ArgumentNullException when key is null.
    /// </summary>
    [Fact]
    public async Task ValidateIpAsync_NullKey_ThrowsArgumentNullException()
    {
        var act = () => _sut.ValidateIpAsync(null!, "192.168.1.1");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that the ValidateIpAsync method returns true when IP is whitelisted.
    /// </summary>
    [Fact]
    public async Task ValidateIpAsync_IpWhitelisted_ReturnsTrue()
    {
        var key = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Active,
            IpWhitelist = "192.168.1.1,192.168.1.2"
        };

        var result = await _sut.ValidateIpAsync(key, "192.168.1.1");
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that the ValidateIpAsync method returns false when IP is not whitelisted.
    /// </summary>
    [Fact]
    public async Task ValidateIpAsync_IpNotWhitelisted_ReturnsFalse()
    {
        var key = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Active,
            IpWhitelist = "192.168.1.1,192.168.1.2"
        };

        var result = await _sut.ValidateIpAsync(key, "192.168.1.50");
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that the ValidateIpAsync method returns true when IP whitelist is empty.
    /// </summary>
    [Fact]
    public async Task ValidateIpAsync_EmptyWhitelist_ReturnsTrue()
    {
        var key = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Active,
            IpWhitelist = ""
        };

        var result = await _sut.ValidateIpAsync(key, "192.168.1.1");
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that the ValidateIpAsync method returns true when IP whitelist is null.
    /// </summary>
    [Fact]
    public async Task ValidateIpAsync_NullWhitelist_ReturnsTrue()
    {
        var key = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Active,
            IpWhitelist = null
        };

        var result = await _sut.ValidateIpAsync(key, "192.168.1.1");
        result.Should().BeTrue();
    }
}
