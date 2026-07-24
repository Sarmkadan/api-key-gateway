using Xunit;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Tests;

public class ApiKeyValidationTests
{
    private static ApiKey CreateValidApiKey()
    {
        return new ApiKey
        {
            Id = "test-id",
            ConsumerId = "test-consumer",
            Name = "test-name",
            KeyHash = "12345678",
            Prefix = "12345678",
            Status = ApiKeyStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, string>()
        };
    }

    [Fact]
    public void Validate_ValidKey_ReturnsEmptyList()
    {
        // Arrange
        var key = CreateValidApiKey();

        // Act
        var errors = key.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InvalidKey_ReturnsErrors()
    {
        // Arrange
        var key = new ApiKey
        {
            Id = "", // Invalid: empty
            ConsumerId = "", // Invalid: empty
            Name = new string('a', 101), // Invalid: too long
            KeyHash = "123", // Invalid: too short
            Prefix = "123", // Invalid: wrong length
            Status = (ApiKeyStatus)999, // Invalid: undefined
            CreatedAt = default, // Invalid: default
            Metadata = null // Invalid: null
        };

        // Act
        var errors = key.Validate();

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain("Id must not be empty.");
        errors.Should().Contain("ConsumerId must not be empty.");
        errors.Should().Contain("Name must not exceed 100 characters.");
        errors.Should().Contain("KeyHash must be at least 8 characters long.");
        errors.Should().Contain("Prefix must be exactly 8 characters long.");
        errors.Should().Contain("Status must be a valid ApiKeyStatus value.");
        errors.Should().Contain("CreatedAt must be set to a valid date.");
        errors.Should().Contain("Metadata must not be null.");
    }

    [Fact]
    public void IsValid_ValidKey_ReturnsTrue()
    {
        // Arrange
        var key = CreateValidApiKey();

        // Act & Assert
        key.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_InvalidKey_ReturnsFalse()
    {
        // Arrange
        var key = new ApiKey(); // Completely empty, highly invalid

        // Act & Assert
        key.IsValid().Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_ValidKey_DoesNotThrow()
    {
        // Arrange
        var key = CreateValidApiKey();

        // Act & Assert
        key.Invoking(k => k.EnsureValid()).Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_InvalidKey_ThrowsArgumentException()
    {
        // Arrange
        var key = new ApiKey(); // Completely empty, highly invalid

        // Act & Assert
        key.Invoking(k => k.EnsureValid()).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Validate_NullKey_ThrowsArgumentNullException()
    {
        // Arrange
        ApiKey key = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => key.Validate());
    }
}
