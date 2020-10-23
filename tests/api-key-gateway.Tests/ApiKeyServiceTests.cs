// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using GatewayUnauthorizedException = ApiKeyGateway.Domain.Exceptions.UnauthorizedAccessException;

namespace ApiKeyGateway.Tests;

public class ApiKeyServiceTests
{
    private readonly Mock<IApiKeyRepository> _repositoryMock;
    private readonly Mock<ILogger<ApiKeyService>> _loggerMock;
    private readonly ApiKeyService _sut;

    public ApiKeyServiceTests()
    {
        _repositoryMock = new Mock<IApiKeyRepository>();
        _loggerMock = new Mock<ILogger<ApiKeyService>>();
        _sut = new ApiKeyService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateKeyAsync_EmptyOrNullConsumerId_ThrowsArgumentException(string? consumerId)
    {
        // Act
        var act = async () => await _sut.CreateKeyAsync(consumerId!, "My Key").ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Consumer ID*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateKeyAsync_EmptyOrNullName_ThrowsArgumentException(string? name)
    {
        // Act
        var act = async () => await _sut.CreateKeyAsync("consumer-123", name!).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public async Task CreateKeyAsync_ValidArguments_CreatesKeyWithExpectedPrefix()
    {
        // Arrange
        const string consumerId = "consumer-abc";
        const string keyName = "Integration Key";
        ApiKey? captured = null;

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<ApiKey>()))
            .Callback<ApiKey>(k => captured = k)
            .ReturnsAsync((ApiKey k) => k);

        // Act
        var result = await _sut.CreateKeyAsync(consumerId, keyName).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.ConsumerId.Should().Be(consumerId);
        result.Name.Should().Be(keyName);
        result.Prefix.Should().Be("sk");
        captured!.KeyHash.Should().NotBeNullOrWhiteSpace();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<ApiKey>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NullOrEmptyKeyId_ReturnsNull()
    {
        // Act
        var resultNull = await _sut.GetByIdAsync(null!).ConfigureAwait(false);
        var resultEmpty = await _sut.GetByIdAsync(string.Empty).ConfigureAwait(false);
        var resultWhitespace = await _sut.GetByIdAsync("   ").ConfigureAwait(false);

        // Assert
        resultNull.Should().BeNull();
        resultEmpty.Should().BeNull();
        resultWhitespace.Should().BeNull();
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DisableKeyAsync_KeyNotFoundInRepository_ReturnsFalse()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("missing-key"))
            .ReturnsAsync((ApiKey?)null);

        // Act
        var result = await _sut.DisableKeyAsync("missing-key").ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ApiKey>()), Times.Never);
    }

    [Fact]
    public async Task DisableKeyAsync_ExistingKey_DisablesAndPersists()
    {
        // Arrange
        var key = new ApiKey { Status = Domain.Enums.ApiKeyStatus.Active };
        _repositoryMock
            .Setup(r => r.GetByIdAsync("key-001"))
            .ReturnsAsync(key);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<ApiKey>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DisableKeyAsync("key-001").ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        key.Status.Should().Be(Domain.Enums.ApiKeyStatus.Disabled);
        _repositoryMock.Verify(r => r.UpdateAsync(key), Times.Once);
    }

    [Fact]
    public async Task ValidateKeyAsync_EmptyKeyValue_ThrowsUnauthorizedException()
    {
        // Act
        var act = async () => await _sut.ValidateKeyAsync(string.Empty).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<GatewayUnauthorizedException>().ConfigureAwait(false);
    }

    [Fact]
    public async Task GetConsumerKeysAsync_EmptyConsumerId_ReturnsEmptyListWithoutQueryingRepository()
    {
        // Act
        var result = await _sut.GetConsumerKeysAsync(string.Empty).ConfigureAwait(false);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(r => r.GetByConsumerIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RevokeKeyAsync_ExistingKey_SetsRevokedStatusAndPersists()
    {
        // Arrange
        var key = new ApiKey { Status = Domain.Enums.ApiKeyStatus.Active };
        _repositoryMock
            .Setup(r => r.GetByIdAsync("key-to-revoke"))
            .ReturnsAsync(key);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<ApiKey>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RevokeKeyAsync("key-to-revoke").ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        key.Status.Should().Be(Domain.Enums.ApiKeyStatus.Revoked);
        key.CanBeUsed().Should().BeFalse();
    }
}
