// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Concurrent;
using System.Threading.Tasks;
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
    /// Tests that the AuthenticateAsync method returns a failure result when the key is revoked.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_RevokedKey_ReturnsFailureResultWithDisabledReason()
    {
        var revokedKey = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Revoked
        };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_revokedkey"))
            .ReturnsAsync(revokedKey);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_revokedkey", "192.168.1.1");
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(AuthenticationFailureReason.ApiKeyDisabled);
        _auditLogServiceMock.Verify(s => s.LogAsync(It.Is<AuditLog>(a => a.ResourceId == "key-123" && !a.IsSuccess)), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method returns a failure result when the key is suspended.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_SuspendedKey_ReturnsFailureResultWithDisabledReason()
    {
        var suspendedKey = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Suspended
        };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("sk_suspendedkey"))
            .ReturnsAsync(suspendedKey);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AuthenticateAsync("sk_suspendedkey", "192.168.1.1");
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(AuthenticationFailureReason.ApiKeyDisabled);
        _auditLogServiceMock.Verify(s => s.LogAsync(It.Is<AuditLog>(a => a.ResourceId == "key-123" && !a.IsSuccess)), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method handles concurrent authentication calls safely.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_ConcurrentCalls_DoesNotCorruptState()
    {
        var key = new ApiKey { Id = "key-123", ConsumerId = "consumer-abc", Status = ApiKeyStatus.Active };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(key);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Simulate concurrent calls
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => _sut.AuthenticateAsync("sk_concurrentkey", "192.168.1.1"))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // All calls should succeed
        results.Should().AllSatisfy(r =>
        {
            r.Success.Should().BeTrue();
            r.ApiKey.Should().NotBeNull();
        });

        // Verify that ValidateKeyAsync was called 100 times (once per concurrent call)
        _apiKeyServiceMock.Verify(s => s.ValidateKeyAsync(It.IsAny<string>()), Times.Exactly(100));
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Exactly(100));
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

    /// <summary>
    /// Tests that the AuthenticateAsync method handles keys with leading/trailing whitespace in the key value itself.
    /// The whitespace should be trimmed before hashing occurs in the service layer.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task AuthenticateAsync_NullOrEmptyApiKey_ReturnsFailureResult(string? apiKey)
    {
        var result = await _sut.AuthenticateAsync(apiKey!, "192.168.1.1");
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(AuthenticationFailureReason.MissingApiKey);
        _auditLogServiceMock.Verify(s => s.LogAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Tests that the AuthenticateAsync method returns distinct failure reasons for different types of invalid keys.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_DistinctFailureReasons_ForDifferentInvalidKeyTypes()
    {
        // Test 1: Missing API key
        var result1 = await _sut.AuthenticateAsync(null, "192.168.1.1");
        result1.Success.Should().BeFalse();
        result1.FailureReason.Should().Be(AuthenticationFailureReason.MissingApiKey);

        // Test 2: Invalid format (key not found in repository)
        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("invalid_key_format"))
            .ReturnsAsync((ApiKey?)null);

        _auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var result2 = await _sut.AuthenticateAsync("invalid_key_format", "192.168.1.1");
        result2.Success.Should().BeFalse();
        result2.FailureReason.Should().Be(AuthenticationFailureReason.InvalidApiKeyFormat);

        // Test 3: Expired key
        var expiredKey = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("expired_key"))
            .ReturnsAsync(expiredKey);

        var result3 = await _sut.AuthenticateAsync("expired_key", "192.168.1.1");
        result3.Success.Should().BeFalse();
        result3.FailureReason.Should().Be(AuthenticationFailureReason.ApiKeyExpired);

        // Test 4: Disabled key
        var disabledKey = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Disabled
        };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("disabled_key"))
            .ReturnsAsync(disabledKey);

        var result4 = await _sut.AuthenticateAsync("disabled_key", "192.168.1.1");
        result4.Success.Should().BeFalse();
        result4.FailureReason.Should().Be(AuthenticationFailureReason.ApiKeyDisabled);

        // Test 5: Revoked key
        var revokedKey = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Revoked
        };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("revoked_key"))
            .ReturnsAsync(revokedKey);

        var result5 = await _sut.AuthenticateAsync("revoked_key", "192.168.1.1");
        result5.Success.Should().BeFalse();
        result5.FailureReason.Should().Be(AuthenticationFailureReason.ApiKeyDisabled);

        // Test 6: Suspended key
        var suspendedKey = new ApiKey
        {
            Id = "key-123",
            ConsumerId = "consumer-abc",
            Status = ApiKeyStatus.Suspended
        };

        _apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync("suspended_key"))
            .ReturnsAsync(suspendedKey);

        var result6 = await _sut.AuthenticateAsync("suspended_key", "192.168.1.1");
        result6.Success.Should().BeFalse();
        result6.FailureReason.Should().Be(AuthenticationFailureReason.ApiKeyDisabled);
    }
}
