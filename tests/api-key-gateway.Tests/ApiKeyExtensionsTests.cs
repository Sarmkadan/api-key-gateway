using System;
using System.Collections.Generic;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class ApiKeyExtensionsTests
{
    [Fact]
    public void IsExpiringWithin_HappyPath_ReturnsTrue()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var result = apiKey.IsExpiringWithin(TimeSpan.FromHours(2));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpiringWithin_ExpiresInPast_ReturnsFalse()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        // Act
        var result = apiKey.IsExpiringWithin(TimeSpan.FromHours(2));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpiringWithin_ExpiresAtNull_ReturnsFalse()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            ExpiresAt = null
        };

        // Act
        var result = apiKey.IsExpiringWithin(TimeSpan.FromHours(2));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpiringWithin_NegativeDuration_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Act
        Action act = () => apiKey.IsExpiringWithin(TimeSpan.FromHours(-1));

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void IsExpiringWithin_NullApiKey_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ApiKeyExtensions.IsExpiringWithin(null!, TimeSpan.FromHours(1));

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetMetadataValue_HappyPath_ReturnsValue()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            Metadata = new Dictionary<string, string> { { "role", "admin" } }
        };

        // Act
        var value = apiKey.GetMetadataValue("role");

        // Assert
        value.Should().Be("admin");
    }

    [Fact]
    public void GetMetadataValue_KeyNotFound_ReturnsNull()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            Metadata = new Dictionary<string, string>()
        };

        // Act
        var value = apiKey.GetMetadataValue("missing");

        // Assert
        value.Should().BeNull();
    }

    [Fact]
    public void GetMetadataValue_EmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            Metadata = new Dictionary<string, string>()
        };

        // Act
        Action act = () => apiKey.GetMetadataValue("");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetMetadataValue_NullApiKey_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ApiKeyExtensions.GetMetadataValue(null!, "key");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetScopes_HappyPath_ReturnsScopes()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            AllowedScopes = "read,write, delete "
        };

        // Act
        var scopes = apiKey.GetScopes();

        // Assert
        scopes.Should().BeEquivalentTo(new[] { "read", "write", "delete" });
    }

    [Fact]
    public void GetScopes_EmptyOrWhitespace_ReturnsEmptyList()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            AllowedScopes = "   "
        };

        // Act
        var scopes = apiKey.GetScopes();

        // Assert
        scopes.Should().BeEmpty();
    }

    [Fact]
    public void GetScopes_NullAllowedScopes_ReturnsEmptyList()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            AllowedScopes = null
        };

        // Act
        var scopes = apiKey.GetScopes();

        // Assert
        scopes.Should().BeEmpty();
    }

    [Fact]
    public void GetScopes_NullApiKey_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ApiKeyExtensions.GetScopes(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
