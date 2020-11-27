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

public class ApiKeyRotationServiceTests
{
    private readonly Mock<IApiKeyService> _apiKeyServiceMock;
    private readonly Mock<IApiKeyRepository> _repositoryMock;
    private readonly Mock<ILogger<ApiKeyRotationService>> _loggerMock;
    private readonly ApiKeyRotationService _sut;

    public ApiKeyRotationServiceTests()
    {
        _apiKeyServiceMock = new Mock<IApiKeyService>();
        _repositoryMock = new Mock<IApiKeyRepository>();
        _loggerMock = new Mock<ILogger<ApiKeyRotationService>>();
        _sut = new ApiKeyRotationService(
            _apiKeyServiceMock.Object,
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RotateKeyAsync_KeyNotFound_ReturnsFailureResult()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("missing"))
            .ReturnsAsync((ApiKey?)null);

        // Act
        var result = await _sut.RotateKeyAsync("missing");

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be("Key not found");
        _apiKeyServiceMock.Verify(s => s.CreateKeyAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task RotateKeyAsync_InactiveKey_ReturnsFailureResult()
    {
        // Arrange
        var key = new ApiKey
        {
            Id = "key-001",
            ConsumerId = "consumer-1",
            Name = "Test Key",
            Status = Domain.Enums.ApiKeyStatus.Revoked
        };
        _repositoryMock.Setup(r => r.GetByIdAsync("key-001")).ReturnsAsync(key);

        // Act
        var result = await _sut.RotateKeyAsync("key-001");

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("not active");
        _apiKeyServiceMock.Verify(s => s.CreateKeyAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task RotateKeyAsync_ActiveKey_CreatesNewKeyAndRevokesOld()
    {
        // Arrange
        var oldKey = new ApiKey
        {
            Id = "old-key",
            ConsumerId = "consumer-1",
            Name = "My Key",
            Status = Domain.Enums.ApiKeyStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            CreatedAt = DateTime.UtcNow.AddDays(-27)
        };

        var newKey = new ApiKey
        {
            Id = "new-key",
            ConsumerId = "consumer-1",
            Name = "My Key (rotated)",
            Status = Domain.Enums.ApiKeyStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        _repositoryMock.Setup(r => r.GetByIdAsync("old-key")).ReturnsAsync(oldKey);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ApiKey>())).Returns(Task.CompletedTask);
        _apiKeyServiceMock
            .Setup(s => s.CreateKeyAsync("consumer-1", It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(newKey);
        _apiKeyServiceMock
            .Setup(s => s.UpdateKeyMetadataAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(true);
        _apiKeyServiceMock.Setup(s => s.RevokeKeyAsync("old-key")).ReturnsAsync(true);

        // Act
        var result = await _sut.RotateKeyAsync("old-key");

        // Assert
        result.Success.Should().BeTrue();
        result.OldKeyId.Should().Be("old-key");
        result.NewKeyId.Should().Be("new-key");
        result.ConsumerId.Should().Be("consumer-1");
        _apiKeyServiceMock.Verify(s => s.RevokeKeyAsync("old-key"), Times.Once);
        _apiKeyServiceMock.Verify(s => s.CreateKeyAsync("consumer-1", It.IsAny<string>(), It.IsAny<int?>()), Times.Once);
    }

    [Fact]
    public async Task RotateKeyAsync_PreservesIpWhitelistOnNewKey()
    {
        // Arrange
        var oldKey = new ApiKey
        {
            Id = "key-whitelist",
            ConsumerId = "consumer-2",
            Name = "Whitelisted Key",
            Status = Domain.Enums.ApiKeyStatus.Active,
            IpWhitelist = "10.0.0.1,192.168.1.100"
        };

        var newKey = new ApiKey
        {
            Id = "new-key-2",
            ConsumerId = "consumer-2",
            Name = "Whitelisted Key (rotated)"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync("key-whitelist")).ReturnsAsync(oldKey);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ApiKey>())).Returns(Task.CompletedTask);
        _apiKeyServiceMock
            .Setup(s => s.CreateKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(newKey);
        _apiKeyServiceMock.Setup(s => s.RevokeKeyAsync(It.IsAny<string>())).ReturnsAsync(true);

        // Act
        var result = await _sut.RotateKeyAsync("key-whitelist");

        // Assert
        result.Success.Should().BeTrue();
        newKey.IpWhitelist.Should().Be("10.0.0.1,192.168.1.100");
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<ApiKey>(k => k.IpWhitelist == "10.0.0.1,192.168.1.100")), Times.Once);
    }

    [Fact]
    public async Task RotateExpiringSoonAsync_NoExpiringKeys_ReturnsEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetKeysExpiringBeforeAsync(It.IsAny<DateTime>()))
            .ReturnsAsync([]);

        // Act
        var results = await _sut.RotateExpiringSoonAsync(7);

        // Assert
        results.Should().BeEmpty();
        _apiKeyServiceMock.Verify(s => s.CreateKeyAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task RotateExpiringSoonAsync_WithExpiringKeys_RotatesEachKey()
    {
        // Arrange
        var keys = new List<ApiKey>
        {
            new() { Id = "key-a", ConsumerId = "c1", Name = "Key A", Status = Domain.Enums.ApiKeyStatus.Active },
            new() { Id = "key-b", ConsumerId = "c2", Name = "Key B", Status = Domain.Enums.ApiKeyStatus.Active }
        };

        _repositoryMock
            .Setup(r => r.GetKeysExpiringBeforeAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(keys);

        foreach (var key in keys)
        {
            var localKey = key;
            _repositoryMock.Setup(r => r.GetByIdAsync(localKey.Id)).ReturnsAsync(localKey);
            var replacement = new ApiKey { Id = $"new-{localKey.Id}", ConsumerId = localKey.ConsumerId };
            _apiKeyServiceMock
                .Setup(s => s.CreateKeyAsync(localKey.ConsumerId, It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(replacement);
            _apiKeyServiceMock.Setup(s => s.RevokeKeyAsync(localKey.Id)).ReturnsAsync(true);
        }

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ApiKey>())).Returns(Task.CompletedTask);

        // Act
        var results = await _sut.RotateExpiringSoonAsync(7);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RotateKeyAsync_EmptyId_ThrowsArgumentException(string? keyId)
    {
        var act = async () => await _sut.RotateKeyAsync(keyId!);
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
