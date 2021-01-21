// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Extension methods for <see cref="ApiKeyRotationServiceTests"/> to simplify test setup and assertions.
/// </summary>
public static class ApiKeyRotationServiceTestsExtensions
{
    /// <summary>
    /// Creates a mock <see cref="ApiKey"/> with common test values.
    /// </summary>
    /// <param name="id">Key ID</param>
    /// <param name="consumerId">Consumer ID</param>
    /// <param name="status">Key status (default: Active)</param>
    /// <param name="expiresInDays">Days until expiration (default: 30)</param>
    /// <param name="ipWhitelist">IP whitelist string (default: null)</param>
    /// <returns>Configured ApiKey instance</returns>
    public static ApiKey WithTestValues(
        this ApiKey apiKey,
        string id,
        string consumerId,
        Domain.Enums.ApiKeyStatus status = Domain.Enums.ApiKeyStatus.Active,
        int expiresInDays = 30,
        string? ipWhitelist = null)
    {
        return new ApiKey
        {
            Id = id,
            ConsumerId = consumerId,
            Name = $"Test Key - {id}",
            Status = status,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            ExpiresAt = status == Domain.Enums.ApiKeyStatus.Active
                ? DateTime.UtcNow.AddDays(expiresInDays)
                : null,
            IpWhitelist = ipWhitelist
        };
    }

    /// <summary>
    /// Sets up the repository mock to return the specified key.
    /// </summary>
    /// <param name="mock">Repository mock</param>
    /// <param name="key">Key to return</param>
    /// <returns>Configured mock for fluent chaining</returns>
    public static Mock<IApiKeyRepository> SetupGetById(
        this Mock<IApiKeyRepository> mock,
        ApiKey key)
    {
        mock.Setup(r => r.GetByIdAsync(key.Id))
            .ReturnsAsync(key);
        return mock;
    }

    /// <summary>
    /// Sets up the repository mock to return multiple keys.
    /// </summary>
    /// <param name="mock">Repository mock</param>
    /// <param name="keys">Keys to return</param>
    /// <returns>Configured mock for fluent chaining</returns>
    public static Mock<IApiKeyRepository> SetupGetKeys(
        this Mock<IApiKeyRepository> mock,
        IEnumerable<ApiKey> keys)
    {
        mock.Setup(r => r.GetKeysExpiringBeforeAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(keys.ToList());

        foreach (var key in keys)
        {
            mock.SetupGetById(key);
        }

        return mock;
    }

    /// <summary>
    /// Sets up the API key service mock to create a replacement key.
    /// </summary>
    /// <param name="mock">API key service mock</param>
    /// <param name="consumerId">Consumer ID for the new key</param>
    /// <param name="newKeyId">ID of the new key to create</param>
    /// <param name="ipWhitelist">IP whitelist to preserve (default: null)</param>
    /// <returns>Configured mock for fluent chaining</returns>
    public static Mock<IApiKeyService> SetupCreateKey(
        this Mock<IApiKeyService> mock,
        string consumerId,
        string newKeyId,
        string? ipWhitelist = null)
    {
        var newKey = new ApiKey()
            .WithTestValues(newKeyId, consumerId, ipWhitelist: ipWhitelist);

        mock.Setup(s => s.CreateKeyAsync(
                consumerId,
                It.IsAny<string>(),
                It.IsAny<int?>()))
            .ReturnsAsync(newKey);

        return mock;
    }

    /// <summary>
    /// Sets up the API key service mock to revoke a key.
    /// </summary>
    /// <param name="mock">API key service mock</param>
    /// <param name="keyId">Key ID to revoke</param>
    /// <param name="success">Whether revocation should succeed (default: true)</param>
    /// <returns>Configured mock for fluent chaining</returns>
    public static Mock<IApiKeyService> SetupRevokeKey(
        this Mock<IApiKeyService> mock,
        string keyId,
        bool success = true)
    {
        mock.Setup(s => s.RevokeKeyAsync(keyId))
            .ReturnsAsync(success);

        return mock;
    }

    /// <summary>
    /// Sets up the repository mock to update keys.
    /// </summary>
    /// <param name="mock">Repository mock</param>
    /// <returns>Configured mock for fluent chaining</returns>
    public static Mock<IApiKeyRepository> SetupUpdate(
        this Mock<IApiKeyRepository> mock)
    {
        mock.Setup(r => r.UpdateAsync(It.IsAny<ApiKey>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    /// <summary>
    /// Verifies that a key rotation occurred successfully.
    /// </summary>
    /// <param name="result">Rotation result</param>
    /// <param name="oldKeyId">Expected old key ID</param>
    /// <param name="newKeyId">Expected new key ID</param>
    /// <param name="consumerId">Expected consumer ID</param>
    public static void ShouldHaveRotated(
        this RotationResult result,
        string oldKeyId,
        string newKeyId,
        string consumerId)
    {
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.OldKeyId.Should().Be(oldKeyId);
        result.NewKeyId.Should().Be(newKeyId);
        result.ConsumerId.Should().Be(consumerId);
        result.NewKeyExpiresAt.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that a key rotation failed with the expected reason.
    /// </summary>
    /// <param name="result">Rotation result</param>
    /// <param name="expectedReason">Expected failure reason</param>
    public static void ShouldHaveFailed(
        this RotationResult result,
        string expectedReason)
    {
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(expectedReason);
    }

    /// <summary>
    /// Creates a test service with mocked dependencies.
    /// </summary>
    /// <param name="apiKeyServiceMock">Configured API key service mock</param>
    /// <param name="repositoryMock">Configured repository mock</param>
    /// <param name="loggerMock">Logger mock</param>
    /// <returns>Configured ApiKeyRotationService instance</returns>
    public static ApiKeyRotationService BuildRotationService(
        this Mock<IApiKeyService> apiKeyServiceMock,
        Mock<IApiKeyRepository> repositoryMock,
        Mock<ILogger<ApiKeyRotationService>> loggerMock)
    {
        return new ApiKeyRotationService(
            apiKeyServiceMock.Object,
            repositoryMock.Object,
            loggerMock.Object);
    }
}