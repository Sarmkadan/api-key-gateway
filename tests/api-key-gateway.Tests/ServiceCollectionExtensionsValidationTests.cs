// =============================================================================
// Tests for ServiceCollectionExtensionsValidation
// =============================================================================

using System;
using System.Collections.Generic;
using ApiKeyGateway.Configuration;
using Xunit;

namespace api_key_gateway.Tests;

public class ServiceCollectionExtensionsValidationTests
{
    private static GatewayConfiguration CreateValidConfiguration()
    {
        return new GatewayConfiguration
        {
            MinKeyLength = 10,
            MaxKeyLength = 20,
            DefaultKeyExpirationDays = 30,
            AuditLogRetentionDays = 90,
            EnableRateLimiting = true,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 50,
            ClockSkewToleranceSeconds = 5
        };
    }

    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var config = CreateValidConfiguration();

        // Act
        IReadOnlyList<string> errors = ServiceCollectionExtensionsValidation.Validate(config);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var config = CreateValidConfiguration();

        // Act
        bool isValid = ServiceCollectionExtensionsValidation.IsValid(config);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var config = CreateValidConfiguration();

        // Act / Assert
        var exception = Record.Exception(() => ServiceCollectionExtensionsValidation.EnsureValid(config));
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_Null_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensionsValidation.Validate(null!));
    }

    [Fact]
    public void IsValid_Null_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensionsValidation.IsValid(null!));
    }

    [Fact]
    public void EnsureValid_Null_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensionsValidation.EnsureValid(null!));
    }

    [Fact]
    public void Validate_InvalidValues_ReturnsExpectedErrors()
    {
        // Arrange: create a configuration that violates several rules
        var config = new GatewayConfiguration
        {
            MinKeyLength = 0,                     // invalid
            MaxKeyLength = -5,                    // invalid and less than MinKeyLength
            DefaultKeyExpirationDays = 0,        // invalid
            AuditLogRetentionDays = -1,           // invalid
            EnableRateLimiting = true,
            DefaultRateLimitPerHour = 0,          // invalid when rate limiting enabled
            MaxConcurrentRequests = 0,            // invalid
            ClockSkewToleranceSeconds = -10       // invalid
        };

        // Act
        IReadOnlyList<string> errors = ServiceCollectionExtensionsValidation.Validate(config);

        // Assert
        var expectedMessages = new[]
        {
            "MinKeyLength must be greater than zero.",
            "MaxKeyLength must be greater than zero.",
            "MaxKeyLength must be greater than or equal to MinKeyLength.",
            "DefaultKeyExpirationDays must be greater than zero.",
            "AuditLogRetentionDays must be greater than zero.",
            "DefaultRateLimitPerHour must be greater than zero when rate limiting is enabled.",
            "MaxConcurrentRequests must be greater than zero.",
            "ClockSkewToleranceSeconds must be non-negative."
        };

        Assert.Equal(expectedMessages.Length, errors.Count);
        foreach (var expected in expectedMessages)
        {
            Assert.Contains(expected, errors);
        }
    }

    [Fact]
    public void EnsureValid_InvalidConfiguration_ThrowsArgumentException()
    {
        // Arrange
        var config = new GatewayConfiguration
        {
            MinKeyLength = 0,
            MaxKeyLength = 0,
            DefaultKeyExpirationDays = 0,
            AuditLogRetentionDays = 0,
            EnableRateLimiting = true,
            DefaultRateLimitPerHour = 0,
            MaxConcurrentRequests = 0,
            ClockSkewToleranceSeconds = -1
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() => ServiceCollectionExtensionsValidation.EnsureValid(config));

        // Assert
        Assert.Contains("Gateway configuration is invalid:", ex.Message);
        // Ensure at least one validation error is present in the message
        Assert.Contains("MinKeyLength must be greater than zero.", ex.Message);
    }
}
