// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for AuditLogValidation extension methods
// =====================================================================

using Xunit;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Unit tests for <see cref="AuditLogValidation"/> extension methods.
/// Tests the Validate, IsValid, and EnsureValid methods.
/// </summary>
public class AuditLogValidationTests
{
    [Fact]
    public void Validate_ValidAuditLog_ReturnsEmptyList()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = Guid.NewGuid().ToString(),
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            PerformedBy = "test-user",
            PerformedAt = DateTime.UtcNow,
            Changes = new Dictionary<string, object>()
        };

        // Act
        var errors = log.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyId_ReturnsError()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = string.Empty,
            ResourceId = Guid.NewGuid().ToString(),
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            PerformedBy = "test-user",
            PerformedAt = DateTime.UtcNow,
            Changes = new Dictionary<string, object>()
        };

        // Act
        var errors = log.Validate();

        // Assert
        errors.Should().ContainSingle(e => e.Contains("Id is required"));
    }

    [Fact]
    public void Validate_EmptyResourceId_ReturnsError()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = string.Empty,
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            PerformedBy = "test-user",
            PerformedAt = DateTime.UtcNow,
            Changes = new Dictionary<string, object>()
        };

        // Act
        var errors = log.Validate();

        // Assert
        errors.Should().ContainSingle(e => e.Contains("ResourceId is required"));
    }

    [Fact]
    public void Validate_EmptyResourceType_ReturnsError()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = Guid.NewGuid().ToString(),
            ResourceType = string.Empty,
            Action = AuditAction.KeyCreated,
            PerformedBy = "test-user",
            PerformedAt = DateTime.UtcNow,
            Changes = new Dictionary<string, object>()
        };

        // Act
        var errors = log.Validate();

        // Assert
        errors.Should().ContainSingle(e => e.Contains("ResourceType is required"));
    }

    [Fact]
    public void Validate_EmptyPerformedBy_ReturnsError()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = Guid.NewGuid().ToString(),
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            PerformedBy = string.Empty,
            PerformedAt = DateTime.UtcNow,
            Changes = new Dictionary<string, object>()
        };

        // Act
        var errors = log.Validate();

        // Assert
        errors.Should().ContainSingle(e => e.Contains("PerformedBy is required"));
    }

    [Fact]
    public void Validate_DefaultAction_ReturnsError()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = Guid.NewGuid().ToString(),
            ResourceType = "ApiKey",
            Action = default,
            PerformedBy = "test-user",
            PerformedAt = DateTime.UtcNow,
            Changes = new Dictionary<string, object>()
        };

        // Act
        var errors = log.Validate();

        // Assert
        errors.Should().ContainSingle(e => e.Contains("Action is required"));
    }

    [Fact]
    public void Validate_InvalidHttpStatusCode_ReturnsError()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = Guid.NewGuid().ToString(),
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            PerformedBy = "test-user",
            PerformedAt = DateTime.UtcNow,
            HttpStatusCode = 99,
            Changes = new Dictionary<string, object>()
        };

        // Act
        var errors = log.Validate();

        // Assert
        errors.Should().ContainSingle(e => e.Contains("HttpStatusCode"));
    }

    [Fact]
    public void Validate_NullChanges_ReturnsError()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = Guid.NewGuid().ToString(),
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            PerformedBy = "test-user",
            PerformedAt = DateTime.UtcNow
        };

        // Act
        var errors = log.Validate();

        // Assert
        errors.Should().ContainSingle(e => e.Contains("Changes dictionary cannot be null"));
    }

    [Fact]
    public void Validate_EmptyReason_WhenProvided_ReturnsError()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = Guid.NewGuid().ToString(),
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            PerformedBy = "test-user",
            PerformedAt = DateTime.UtcNow,
            Reason = "   ",
            Changes = new Dictionary<string, object>()
        };

        // Act
        var errors = log.Validate();

        // Assert
        errors.Should().ContainSingle(e => e.Contains("Reason cannot be empty"));
    }

    [Fact]
    public void Validate_NullAuditLog_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullLog!.Validate());
    }

    [Fact]
    public void IsValid_ValidAuditLog_ReturnsTrue()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = Guid.NewGuid().ToString(),
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            PerformedBy = "test-user",
            PerformedAt = DateTime.UtcNow,
            Changes = new Dictionary<string, object>()
        };

        // Act
        var isValid = log.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_InvalidAuditLog_ReturnsFalse()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = string.Empty,
            ResourceId = Guid.NewGuid().ToString(),
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            PerformedBy = "test-user",
            PerformedAt = DateTime.UtcNow,
            Changes = new Dictionary<string, object>()
        };

        // Act
        var isValid = log.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_NullAuditLog_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullLog!.IsValid());
    }

    [Fact]
    public void EnsureValid_ValidAuditLog_DoesNotThrow()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = Guid.NewGuid().ToString(),
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            PerformedBy = "test-user",
            PerformedAt = DateTime.UtcNow,
            Changes = new Dictionary<string, object>()
        };

        // Act
        var act = () => log.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_InvalidAuditLog_ThrowsArgumentException()
    {
        // Arrange
        var log = new AuditLog
        {
            Id = string.Empty,
            ResourceId = Guid.NewGuid().ToString(),
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            PerformedBy = "test-user",
            PerformedAt = DateTime.UtcNow,
            Changes = new Dictionary<string, object>()
        };

        // Act
        var act = () => log.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EnsureValid_NullAuditLog_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullLog!.EnsureValid());
    }
}