// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for AuthenticationServiceTests to provide additional test
// scenarios and helper methods for testing AuthenticationService behavior
// =============================================================================

using Xunit;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

public static class AuthenticationServiceTestsExtensions
{
    /// <summary>
    /// Creates a mock AuthenticationService with default mocks for testing
    /// </summary>
    /// <param name="apiKeyStatus">The status to set for the mock API key</param>
    /// <param name="ipWhitelist">Optional IP whitelist string</param>
    /// <returns>Configured AuthenticationService instance</returns>
    public static AuthenticationService CreateMockAuthenticationService(
        this AuthenticationServiceTests _,
        ApiKeyStatus apiKeyStatus = ApiKeyStatus.Active,
        string? ipWhitelist = null)
    {
        var apiKeyServiceMock = new Mock<IApiKeyService>();
        var auditLogServiceMock = new Mock<IAuditLogService>();
        var loggerMock = new Mock<ILogger<AuthenticationService>>();

        var apiKey = new ApiKey
        {
            Id = "test-key-id",
            ConsumerId = "test-consumer-id",
            Status = apiKeyStatus,
            IpWhitelist = ipWhitelist
        };

        apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(apiKey);

        auditLogServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        return new AuthenticationService(
            apiKeyServiceMock.Object,
            auditLogServiceMock.Object,
            loggerMock.Object);
    }

    /// <summary>
    /// Verifies that the authentication service properly handles API key deactivation scenarios
    /// </summary>
    public static async Task AuthenticateAsync_DeactivatedKey_ThrowsUnauthorizedException(
        this AuthenticationServiceTests _,
        ApiKeyStatus deactivatedStatus)
    {
        // Arrange
        var authService = _.CreateMockAuthenticationService(apiKeyStatus: deactivatedStatus);
        var deactivatedKey = deactivatedStatus switch
        {
            ApiKeyStatus.Revoked => "sk_revokedkey",
            ApiKeyStatus.Expired => "sk_expiredkey",
            ApiKeyStatus.Disabled => "sk_disabledkey",
            _ => throw new ArgumentOutOfRangeException(nameof(deactivatedStatus), deactivatedStatus, null)
        };

        // Act
        var act = async () => await authService.AuthenticateAsync(deactivatedKey, "192.168.1.1");

        // Assert
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("*not active*");
    }

    /// <summary>
    /// Creates a collection of test API keys with different statuses for bulk testing scenarios
    /// </summary>
    /// <returns>Read-only list of test API keys</returns>
    public static IReadOnlyList<ApiKey> CreateTestApiKeys(this AuthenticationServiceTests _)
    {
        return new List<ApiKey>
        {
            new ApiKey
            {
                Id = "active-key-1",
                ConsumerId = "consumer-1",
                Status = ApiKeyStatus.Active,
                IpWhitelist = "192.168.1.1,192.168.1.2"
            },
            new ApiKey
            {
                Id = "active-key-2",
                ConsumerId = "consumer-2",
                Status = ApiKeyStatus.Active,
                IpWhitelist = "10.0.0.1"
            },
            new ApiKey
            {
                Id = "revoked-key-1",
                ConsumerId = "consumer-3",
                Status = ApiKeyStatus.Revoked
            },
            new ApiKey
            {
                Id = "expired-key-1",
                ConsumerId = "consumer-4",
                Status = ApiKeyStatus.Expired
            }
        }.AsReadOnly();
    }

    /// <summary>
    /// Verifies that the authentication service properly validates API key format
    /// </summary>
    public static async Task AuthenticateAsync_InvalidKeyFormat_ThrowsUnauthorizedException(
        this AuthenticationServiceTests _)
    {
        // Arrange
        var authService = _.CreateMockAuthenticationService();
        var invalidKeys = new[] { "invalid", "short", "no-prefix", "sk_", "SK_VALIDKEY123" };

        foreach (var invalidKey in invalidKeys)
        {
            // Act
            var act = async () => await authService.AuthenticateAsync(invalidKey, "192.168.1.1");

            // Assert
            await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.UnauthorizedAccessException>();
        }
    }
}