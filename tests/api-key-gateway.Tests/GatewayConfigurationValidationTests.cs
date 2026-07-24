using System;
using System.Collections.Generic;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class GatewayConfigurationValidationTests
{
    [Fact]
    public void Validate_ValidConfiguration_ReturnsEmptyList()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "test-id",
            JwtSecret = "valid-jwt-secret-key-123",
            DatabaseConnectionString = "Server=localhost;Database=test;User Id=sa;Password=Strong!Pass123;",
            MinKeyLength = 16,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var errors = GatewayConfigurationValidation.Validate(configuration);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        GatewayConfiguration? configuration = null;

        // Act
        Action act = () => GatewayConfigurationValidation.Validate(configuration!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_EmptyId_ReturnsErrorMessage()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "",
            JwtSecret = "valid-secret",
            DatabaseConnectionString = "valid-connection",
            MinKeyLength = 16,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var errors = GatewayConfigurationValidation.Validate(configuration);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("Id must not be empty or whitespace.");
    }

    [Fact]
    public void Validate_WhitespaceId_ReturnsErrorMessage()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "   ",
            JwtSecret = "valid-secret",
            DatabaseConnectionString = "valid-connection",
            MinKeyLength = 16,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var errors = GatewayConfigurationValidation.Validate(configuration);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("Id must not be empty or whitespace.");
    }

    [Fact]
    public void Validate_EmptyJwtSecret_ReturnsErrorMessage()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "test-id",
            JwtSecret = "",
            DatabaseConnectionString = "valid-connection",
            MinKeyLength = 16,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var errors = GatewayConfigurationValidation.Validate(configuration);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("JwtSecret must not be empty or whitespace.");
    }

    [Fact]
    public void Validate_ZeroMinKeyLength_ReturnsErrorMessage()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "test-id",
            JwtSecret = "valid-secret",
            DatabaseConnectionString = "valid-connection",
            MinKeyLength = 0,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var errors = GatewayConfigurationValidation.Validate(configuration);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("MinKeyLength must be greater than zero.");
    }

    [Fact]
    public void Validate_NegativeMaxKeyLength_ReturnsErrorMessage()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "test-id",
            JwtSecret = "valid-secret",
            DatabaseConnectionString = "valid-connection",
            MinKeyLength = 16,
            MaxKeyLength = -5,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var errors = GatewayConfigurationValidation.Validate(configuration);

        // Assert
        errors.Should().Contain("MaxKeyLength must be greater than zero.");
    }

    [Fact]
    public void Validate_MaxKeyLengthLessThanMinKeyLength_ReturnsErrorMessage()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "test-id",
            JwtSecret = "valid-secret",
            DatabaseConnectionString = "valid-connection",
            MinKeyLength = 32,
            MaxKeyLength = 16,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var errors = GatewayConfigurationValidation.Validate(configuration);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("MaxKeyLength must be greater than or equal to MinKeyLength.");
    }

    [Fact]
    public void Validate_DefaultKeyExpirationDaysZero_ReturnsErrorMessage()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "test-id",
            JwtSecret = "valid-secret",
            DatabaseConnectionString = "valid-connection",
            MinKeyLength = 16,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 0,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var errors = GatewayConfigurationValidation.Validate(configuration);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("DefaultKeyExpirationDays must be greater than zero.");
    }

    [Fact]
    public void Validate_AuditLogRetentionDaysZero_ReturnsErrorMessage()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "test-id",
            JwtSecret = "valid-secret",
            DatabaseConnectionString = "valid-connection",
            MinKeyLength = 16,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 0,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var errors = GatewayConfigurationValidation.Validate(configuration);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("AuditLogRetentionDays must be greater than zero.");
    }

    [Fact]
    public void Validate_DefaultRateLimitPerHourZero_ReturnsErrorMessage()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "test-id",
            JwtSecret = "valid-secret",
            DatabaseConnectionString = "valid-connection",
            MinKeyLength = 16,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 0,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var errors = GatewayConfigurationValidation.Validate(configuration);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("DefaultRateLimitPerHour must be greater than zero.");
    }

    [Fact]
    public void Validate_MaxConcurrentRequestsZero_ReturnsErrorMessage()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "test-id",
            JwtSecret = "valid-secret",
            DatabaseConnectionString = "valid-connection",
            MinKeyLength = 16,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 0,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var errors = GatewayConfigurationValidation.Validate(configuration);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("MaxConcurrentRequests must be greater than zero.");
    }

    [Fact]
    public void Validate_UpdatedAtIsMinValue_ReturnsErrorMessage()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "test-id",
            JwtSecret = "valid-secret",
            DatabaseConnectionString = "valid-connection",
            MinKeyLength = 16,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.MinValue
        };

        // Act
        var errors = GatewayConfigurationValidation.Validate(configuration);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("UpdatedAt must not be the default value.");
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ReturnsMultipleErrors()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "",
            JwtSecret = "",
            DatabaseConnectionString = "",
            MinKeyLength = 0,
            MaxKeyLength = 0,
            DefaultKeyExpirationDays = 0,
            AuditLogRetentionDays = 0,
            DefaultRateLimitPerHour = 0,
            MaxConcurrentRequests = 0,
            UpdatedAt = DateTime.MinValue
        };

        // Act
        var errors = GatewayConfigurationValidation.Validate(configuration);

        // Assert - check that we get multiple errors (at least 7, up to 11)
        errors.Should().HaveCountGreaterThanOrEqualTo(7);
        errors.Should().Contain("Id must not be empty or whitespace.");
        errors.Should().Contain("JwtSecret must not be empty or whitespace.");
        errors.Should().Contain("DatabaseConnectionString must not be empty or whitespace.");
        errors.Should().Contain("MinKeyLength must be greater than zero.");
        errors.Should().Contain("MaxKeyLength must be greater than zero.");
        errors.Should().Contain("DefaultKeyExpirationDays must be greater than zero.");
        errors.Should().Contain("AuditLogRetentionDays must be greater than zero.");
        errors.Should().Contain("DefaultRateLimitPerHour must be greater than zero.");
        errors.Should().Contain("MaxConcurrentRequests must be greater than zero.");
        errors.Should().Contain("UpdatedAt must not be the default value.");
    }

    [Fact]
    public void IsValid_ValidConfiguration_ReturnsTrue()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "test-id",
            JwtSecret = "valid-jwt-secret-key-123",
            DatabaseConnectionString = "Server=localhost;Database=test;User Id=sa;Password=Strong!Pass123;",
            MinKeyLength = 16,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = GatewayConfigurationValidation.IsValid(configuration);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_InvalidConfiguration_ReturnsFalse()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "", // Invalid
            JwtSecret = "valid-secret",
            DatabaseConnectionString = "valid-connection",
            MinKeyLength = 16,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = GatewayConfigurationValidation.IsValid(configuration);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        GatewayConfiguration? configuration = null;

        // Act
        Action act = () => GatewayConfigurationValidation.IsValid(configuration!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_ValidConfiguration_DoesNotThrow()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "test-id",
            JwtSecret = "valid-jwt-secret-key-123",
            DatabaseConnectionString = "Server=localhost;Database=test;User Id=sa;Password=Strong!Pass123;",
            MinKeyLength = 16,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        Action act = () => GatewayConfigurationValidation.EnsureValid(configuration);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_InvalidConfiguration_ThrowsArgumentException()
    {
        // Arrange
        var configuration = new GatewayConfiguration
        {
            Id = "", // Invalid
            JwtSecret = "valid-secret",
            DatabaseConnectionString = "valid-connection",
            MinKeyLength = 16,
            MaxKeyLength = 256,
            DefaultKeyExpirationDays = 365,
            AuditLogRetentionDays = 90,
            DefaultRateLimitPerHour = 1000,
            MaxConcurrentRequests = 100,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        Action act = () => GatewayConfigurationValidation.EnsureValid(configuration);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Id must not be empty or whitespace.*");
    }

    [Fact]
    public void EnsureValid_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        GatewayConfiguration? configuration = null;

        // Act
        Action act = () => GatewayConfigurationValidation.EnsureValid(configuration!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}