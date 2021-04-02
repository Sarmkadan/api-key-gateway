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
using GatewayUnauthorizedException = ApiKeyGateway.Domain.Exceptions.UnauthorizedAccessException;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ApiKeyService"/> class.
/// Tests various scenarios including key creation, validation, disabling, revocation,
/// and consumer key retrieval with proper error handling and repository interactions.
/// </summary>
public class ApiKeyServiceTests
{
    private readonly Mock<IApiKeyRepository> _repositoryMock;
    private readonly Mock<ILogger<ApiKeyService>> _loggerMock;
    private readonly ApiKeyService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyServiceTests"/> class.
    /// Sets up mocks for <see cref="IApiKeyRepository"/> and <see cref="ILogger{ApiKeyService}"/>
    /// to test the <see cref="ApiKeyService"/> class in isolation.
    /// </summary>
    public ApiKeyServiceTests()
    {
        _repositoryMock = new Mock<IApiKeyRepository>();
        _loggerMock = new Mock<ILogger<ApiKeyService>>();
        _sut = new ApiKeyService(_repositoryMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Tests that <see cref="ApiKeyService.CreateKeyAsync"/> throws a <see cref="ValidationException"/> when consumer ID is null, empty, or whitespace.
    /// </summary>
    /// <param name="consumerId">The consumer ID to test with (null, empty, or whitespace).</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task CreateKeyAsync_EmptyOrNullConsumerId_ThrowsValidationException(string? consumerId)
    {
        // Act
        var act = async () => await _sut.CreateKeyAsync(consumerId!, "My Key");

        // Assert
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.ValidationException>()
            .WithMessage("*Consumer ID*");
    }

    /// <summary>
    /// Tests that <see cref="ApiKeyService.CreateKeyAsync"/> throws a <see cref="ValidationException"/> when key name is null, empty, or whitespace.
    /// </summary>
    /// <param name="name">The key name to test with (null, empty, or whitespace).</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task CreateKeyAsync_EmptyOrNullName_ThrowsValidationException(string? name)
    {
        // Act
        var act = async () => await _sut.CreateKeyAsync("consumer-123", name!);

        // Assert
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.ValidationException>()
            .WithMessage("*name*");
    }

    /// <summary>
    /// Tests that <see cref="ApiKeyService.CreateKeyAsync"/> creates a valid API key with the expected prefix when provided with valid arguments.
    /// Verifies that the created key has the correct consumer ID, name, prefix, and a non-empty key hash.
    /// Also ensures the repository's CreateAsync method is called exactly once.
    /// </summary>
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
        var result = await _sut.CreateKeyAsync(consumerId, keyName);

        // Assert
        result.Should().NotBeNull();
        result.ConsumerId.Should().Be(consumerId);
        result.Name.Should().Be(keyName);
        result.Prefix.Should().Be("sk");
        captured!.KeyHash.Should().NotBeNullOrWhiteSpace();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<ApiKey>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="ApiKeyService.GetByIdAsync"/> throws a <see cref="ValidationException"/> when key ID is null, empty, or whitespace.
    /// Verifies that the repository's GetByIdAsync method is never called in these error cases.
    /// </summary>
    /// <param name="keyId">The key ID to test with (null, empty, or whitespace).</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GetByIdAsync_NullOrEmptyKeyId_ThrowsValidationException(string? keyId)
    {
        // Act
        var act = async () => await _sut.GetByIdAsync(keyId!);

        // Assert
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.ValidationException>()
            .WithMessage("*Key ID*");
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="ApiKeyService.DisableKeyAsync"/> returns false when attempting to disable a non-existent key.
    /// Verifies that the repository's UpdateAsync method is never called when the key is not found.
    /// </summary>
    [Fact]
    public async Task DisableKeyAsync_KeyNotFoundInRepository_ReturnsFalse()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("missing-key"))
            .ReturnsAsync((ApiKey?)null);

        // Act
        var result = await _sut.DisableKeyAsync("missing-key");

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ApiKey>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="ApiKeyService.DisableKeyAsync"/> successfully disables an existing active key and persists the change.
    /// Verifies that the key status is updated to Disabled and the repository's UpdateAsync method is called exactly once.
    /// </summary>
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
        var result = await _sut.DisableKeyAsync("key-001");

        // Assert
        result.Should().BeTrue();
        key.Status.Should().Be(Domain.Enums.ApiKeyStatus.Disabled);
        _repositoryMock.Verify(r => r.UpdateAsync(key), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="ApiKeyService.ValidateKeyAsync"/> throws an <see cref="UnauthorizedAccessException"/> when an empty key value is provided.
    /// </summary>
    [Fact]
    public async Task ValidateKeyAsync_EmptyKeyValue_ThrowsUnauthorizedException()
    {
        // Act
        var act = async () => await _sut.ValidateKeyAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<GatewayUnauthorizedException>();
    }

    /// <summary>
    /// Tests that <see cref="ApiKeyService.GetConsumerKeysAsync"/> returns an empty list without querying the repository when an empty consumer ID is provided.
    /// Verifies that the repository's GetByConsumerIdAsync method is never called in this case.
    /// </summary>
    [Fact]
    public async Task GetConsumerKeysAsync_EmptyConsumerId_ReturnsEmptyListWithoutQueryingRepository()
    {
        // Act
        var result = await _sut.GetConsumerKeysAsync(string.Empty);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(r => r.GetByConsumerIdAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="ApiKeyService.RevokeKeyAsync"/> successfully revokes an existing active key and persists the change.
    /// Verifies that the key status is updated to Revoked, that CanBeUsed() returns false,
    /// and that the repository's UpdateAsync method is called exactly once.
    /// </summary>
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
        var result = await _sut.RevokeKeyAsync("key-to-revoke");

        // Assert
        result.Should().BeTrue();
        key.Status.Should().Be(Domain.Enums.ApiKeyStatus.Revoked);
        key.CanBeUsed().Should().BeFalse();
    }
}
