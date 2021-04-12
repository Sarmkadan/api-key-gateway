// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    /// Tests that the AuthenticateAsync method throws an UnauthorizedAccessException when the apiKey parameter is empty or null.
    /// </summary>
    /// <param name="apiKey">The API key to test.</param>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task AuthenticateAsync_EmptyOrNullApiKey_ThrowsUnauthorizedException(string? apiKey)
    {
        var act = async () => await _sut.AuthenticateAsync(apiKey!, "192.168.1.1");
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method returns the API key and logs a success when the key is valid.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_ValidKey_ReturnsKeyAndLogsSuccess()
    {
        var key = new ApiKey { Id = "key-123", ConsumerId = "consumer-abc", Status = ApiKeyStatus.Active };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_validkey123456789"))
            .ReturnsAsync(key);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_validkey123456789", "192.168.1.1");

        result.Should().Be(key);
        _auditLogServiceMock.Verify(s => s.LogAsync(It.Is<AuditLog>(
            log => log.IsSuccess && log.Action == AuditAction.KeyUsed
        )), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method throws an UnauthorizedAccessException when the key is invalid.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_InvalidKey_ThrowsUnauthorizedExceptionAndLogsFailure()
    {
        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_invalidkey"))
            .ReturnsAsync((ApiKey?)null);

        var act = async () => await _sut.AuthenticateAsync("sk_invalidkey", "192.168.1.1");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _auditLogServiceMock.Verify(s => s.LogAsync(It.Is<AuditLog>(
            log => !log.IsSuccess && log.Action == AuditAction.UnauthorizedAttempt
        )), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method throws an UnauthorizedAccessException when the IP address is not whitelisted.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_IpNotWhitelisted_ThrowsUnauthorizedException()
    {
        var key = new ApiKey
        {
            Id = "key-456",
            ConsumerId = "consumer-xyz",
            Status = ApiKeyStatus.Active,
            IpWhitelist = "10.0.0.1, 10.0.0.2"
        };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_whitelistedkey"))
            .ReturnsAsync(key);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var act = async () => await _sut.AuthenticateAsync("sk_whitelistedkey", "192.168.1.50");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not whitelisted*");
        _auditLogServiceMock.Verify(s => s.LogAsync(It.Is<AuditLog>(
            log => !log.IsSuccess && log.Action == AuditAction.UnauthorizedAttempt
        )), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method allows authentication when the IP address is whitelisted.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_IpWhitelisted_AllowsAuthentication()
    {
        var key = new ApiKey
        {
            Id = "key-789",
            ConsumerId = "consumer-pqr",
            Status = ApiKeyStatus.Active,
            IpWhitelist = "192.168.1.50, 192.168.1.51"
        };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_whitelistedkey"))
            .ReturnsAsync(key);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_whitelistedkey", "192.168.1.50");

        result.Should().Be(key);
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method allows authentication without IP check when no IP address is provided.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_NoIpAddressProvided_AllowsAuthenticationWithoutIpCheck()
    {
        var key = new ApiKey
        {
            Id = "key-999",
            ConsumerId = "consumer-stu",
            Status = ApiKeyStatus.Active,
            IpWhitelist = "10.0.0.1"
        };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_testkey"))
            .ReturnsAsync(key);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_testkey");

        result.Should().Be(key);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method throws a KeyStoreUnavailableException when the API key service throws a DataAccessException.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_ApiKeyServiceThrowsDataAccessException_ThrowsKeyStoreUnavailable()
    {
        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync(It.IsAny<string>()))
            .ThrowsAsync(new DataAccessException("Connection failed", "ValidateKey", "ApiKey", new Exception()));

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var act = async () => await _sut.AuthenticateAsync("sk_anykey", "192.168.1.1");

        await act.Should().ThrowAsync<KeyStoreUnavailableException>();
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method throws an UnauthorizedAccessException when an unexpected exception occurs.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_UnexpectedException_ThrowsUnauthorizedException()
    {
        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var act = async () => await _sut.AuthenticateAsync("sk_anykey");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    /// <summary>
    /// Tests that the ValidateIpAsync method throws an ArgumentNullException when the key parameter is null.
    /// </summary>
    [Fact]
    public async Task ValidateIpAsync_NullKey_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.ValidateIpAsync(null!, "192.168.1.1");
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("key");
    }

    /// <summary>
    /// Tests that the ValidateIpAsync method returns true when the IP address is empty.
    /// </summary>
    [Fact]
    public async Task ValidateIpAsync_EmptyIpAddress_ReturnsTrue()
    {
        var key = new ApiKey { Id = "key-111", IpWhitelist = "10.0.0.1" };

        var result = await _sut.ValidateIpAsync(key, "");

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that the ValidateIpAsync method returns true when the IP address is in the whitelist.
    /// </summary>
    [Fact]
    public async Task ValidateIpAsync_IpInWhitelist_ReturnsTrue()
    {
        var key = new ApiKey { Id = "key-222", IpWhitelist = "10.0.0.1, 10.0.0.2" };

        var result = await _sut.ValidateIpAsync(key, "10.0.0.2");

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that the ValidateIpAsync method returns false and logs a failure when the IP address is not in the whitelist.
    /// </summary>
    [Fact]
    public async Task ValidateIpAsync_IpNotInWhitelist_ReturnsFalseAndLogsFailure()
    {
        var key = new ApiKey { Id = "key-333", IpWhitelist = "10.0.0.1" };

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ValidateIpAsync(key, "192.168.1.100");

        result.Should().BeFalse();
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Tests that the LogAuthenticationAttemptAsync method logs a successful authentication attempt.
    /// </summary>
    /// <param name="keyId">The ID of the API key.</param>
    /// <param name="isSuccess">Whether the authentication attempt was successful.</param>
    /// <param name="message">The message to log.</param>
    [Fact]
    public async Task LogAuthenticationAttemptAsync_SuccessfulAttempt_LogsKeyUsedAction()
    {
        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        await _sut.LogAuthenticationAttemptAsync("key-444", true, "Test success");

        _auditLogServiceMock.Verify(s => s.LogAsync(It.Is<AuditLog>(
            log => log.IsSuccess && log.Action == AuditAction.KeyUsed && log.ResourceId == "key-444"
        )), Times.Once);
    }

    /// <summary>
    /// Tests that the LogAuthenticationAttemptAsync method logs a failed authentication attempt.
    /// </summary>
    /// <param name="keyId">The ID of the API key.</param>
    /// <param name="isSuccess">Whether the authentication attempt was successful.</param>
    /// <param name="message">The message to log.</param>
    [Fact]
    public async Task LogAuthenticationAttemptAsync_FailedAttempt_LogsUnauthorizedAttemptAction()
    {
        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        await _sut.LogAuthenticationAttemptAsync("key-555", false, "Test failure");

        _auditLogServiceMock.Verify(s => s.LogAsync(It.Is<AuditLog>(
            log => !log.IsSuccess && log.Action == AuditAction.UnauthorizedAttempt && log.ResourceId == "key-555"
        )), Times.Once);
    }

    /// <summary>
    /// Tests that the LogAuthenticationAttemptAsync method does not propagate exceptions from the audit log service.
    /// </summary>
    [Fact]
    public async Task LogAuthenticationAttemptAsync_AuditServiceThrows_DoesNotPropagate()
    {
        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .ThrowsAsync(new Exception("Audit failed"));

        var act = async () => await _sut.LogAuthenticationAttemptAsync("key-666", true);

        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method allows concurrent authentication attempts.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_ConcurrentCalls_AllAuthenticate()
    {
        var key = new ApiKey { Id = "key-concurrent", Status = ApiKeyStatus.Active };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(key);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var tasks = Enumerable.Range(0, 10).Select(_ =>
            _sut.AuthenticateAsync("sk_testkey")
        );

        var results = await Task.WhenAll(tasks);

        results.Should().HaveCount(10);
        results.Should().AllSatisfy(r => r.Should().Be(key));
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.AtLeastOnce);
    }
}
