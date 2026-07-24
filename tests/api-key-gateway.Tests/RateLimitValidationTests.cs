// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using Xunit;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="RateLimitValidation"/> class.
/// Tests the validation methods for <see cref="RateLimit"/> instances including
/// validation error detection, validity checks, and exception throwing.
/// </summary>
public class RateLimitValidationTests
{
    /// <summary>
    /// Tests that Validate returns empty list for valid RateLimit
    /// </summary>
    [Fact]
    public void Validate_ValidRateLimit_ReturnsEmptyList()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Validate returns error for null Id
    /// </summary>
    [Fact]
    public void Validate_NullId_ReturnsError()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "Id cannot be null or whitespace.");
    }

    /// <summary>
    /// Tests that Validate returns error for whitespace Id
    /// </summary>
    [Fact]
    public void Validate_WhitespaceId_ReturnsError()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "   ",
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "Id cannot be null or whitespace.");
    }

    /// <summary>
    /// Tests that Validate returns error for null ApiKeyId
    /// </summary>
    [Fact]
    public void Validate_NullApiKeyId_ReturnsError()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "ApiKeyId cannot be null or whitespace.");
    }

    /// <summary>
    /// Tests that Validate returns error for whitespace ApiKeyId
    /// </summary>
    [Fact]
    public void Validate_WhitespaceApiKeyId_ReturnsError()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            ApiKeyId = "\t",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "ApiKeyId cannot be null or whitespace.");
    }

    /// <summary>
    /// Tests that Validate returns error for zero RequestsPerUnit
    /// </summary>
    [Fact]
    public void Validate_ZeroRequestsPerUnit_ReturnsError()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 0,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "RequestsPerUnit must be a positive integer greater than zero.");
    }

    /// <summary>
    /// Tests that Validate returns error for negative RequestsPerUnit
    /// </summary>
    [Fact]
    public void Validate_NegativeRequestsPerUnit_ReturnsError()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = -10,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "RequestsPerUnit must be a positive integer greater than zero.");
    }

    /// <summary>
    /// Tests that Validate returns error for excessive RequestsPerUnit
    /// </summary>
    [Fact]
    public void Validate_ExcessiveRequestsPerUnit_ReturnsError()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 1_000_001,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "RequestsPerUnit cannot exceed 1,000,000.");
    }

    /// <summary>
    /// Tests that Validate returns error for invalid Unit enum value
    /// </summary>
    [Fact]
    public void Validate_InvalidUnit_ReturnsError()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 100,
            Unit = (RateLimitUnit)999, // Invalid enum value
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "Unit contains an invalid value.");
    }

    /// <summary>
    /// Tests that Validate returns error for default CreatedAt
    /// </summary>
    [Fact]
    public void Validate_DefaultCreatedAt_ReturnsError()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = default,
            CurrentRequestCount = 50
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "CreatedAt cannot be the default DateTime value.");
    }

    /// <summary>
    /// Tests that Validate returns error for negative CurrentRequestCount
    /// </summary>
    [Fact]
    public void Validate_NegativeCurrentRequestCount_ReturnsError()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = -5
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "CurrentRequestCount cannot be negative.");
    }

    /// <summary>
    /// Tests that Validate returns error when CurrentRequestCount exceeds RequestsPerUnit
    /// </summary>
    [Fact]
    public void Validate_CurrentRequestCountExceedsRequestsPerUnit_ReturnsError()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 150
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "CurrentRequestCount cannot exceed RequestsPerUnit.");
    }

    /// <summary>
    /// Tests that Validate returns error when CurrentRequestCount exceeds RequestsPerUnit for enabled rate limit
    /// </summary>
    [Fact]
    public void Validate_CurrentRequestCountExceedsRequestsPerUnitForEnabledRateLimit_ReturnsError()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 150
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "CurrentRequestCount cannot exceed RequestsPerUnit when IsEnabled is true.");
    }

    /// <summary>
    /// Tests that Validate returns error for default LastResetAt
    /// </summary>
    [Fact]
    public void Validate_DefaultLastResetAt_ReturnsError()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50,
            LastResetAt = DateTime.MinValue
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "LastResetAt cannot be the default DateTime value.");
    }

    /// <summary>
    /// Tests that Validate returns multiple errors when multiple properties are invalid
    /// </summary>
    [Fact]
    public void Validate_MultipleInvalidProperties_ReturnsAllErrors()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "   ",
            ApiKeyId = "",
            RequestsPerUnit = -10,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = default,
            CurrentRequestCount = 50
        };

        // Act
        var errors = rateLimit.Validate();

        // Assert
        errors.Should().HaveCount(6);
        errors.Should().Contain(e => e == "Id cannot be null or whitespace.");
        errors.Should().Contain(e => e == "ApiKeyId cannot be null or whitespace.");
        errors.Should().Contain(e => e == "RequestsPerUnit must be a positive integer greater than zero.");
        errors.Should().Contain(e => e == "CreatedAt cannot be the default DateTime value.");
    }

    /// <summary>
    /// Tests that Validate throws ArgumentNullException when rateLimit is null
    /// </summary>
    [Fact]
    public void Validate_NullRateLimit_ThrowsArgumentNullException()
    {
        // Arrange
        RateLimit? nullRateLimit = null;

        // Act
        Action act = () => nullRateLimit!.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that IsValid returns true for valid RateLimit
    /// </summary>
    [Fact]
    public void IsValid_ValidRateLimit_ReturnsTrue()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        var isValid = rateLimit.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsValid returns false for invalid RateLimit
    /// </summary>
    [Fact]
    public void IsValid_InvalidRateLimit_ReturnsFalse()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        var isValid = rateLimit.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsValid throws ArgumentNullException when rateLimit is null
    /// </summary>
    [Fact]
    public void IsValid_NullRateLimit_ThrowsArgumentNullException()
    {
        // Arrange
        RateLimit? nullRateLimit = null;

        // Act
        Action act = () => nullRateLimit!.IsValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that EnsureValid does not throw for valid RateLimit
    /// </summary>
    [Fact]
    public void EnsureValid_ValidRateLimit_DoesNotThrow()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "test-id",
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        Action act = () => rateLimit.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that EnsureValid throws ArgumentException for invalid RateLimit
    /// </summary>
    [Fact]
    public void EnsureValid_InvalidRateLimit_ThrowsArgumentException()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            ApiKeyId = "test-api-key-id",
            RequestsPerUnit = 100,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        Action act = () => rateLimit.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*RateLimit validation failed:*");
    }

    /// <summary>
    /// Tests that EnsureValid throws ArgumentNullException when rateLimit is null
    /// </summary>
    [Fact]
    public void EnsureValid_NullRateLimit_ThrowsArgumentNullException()
    {
        // Arrange
        RateLimit? nullRateLimit = null;

        // Act
        Action act = () => nullRateLimit!.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that EnsureValid throws ArgumentException with specific error message for invalid RateLimit
    /// </summary>
    [Fact]
    public void EnsureValid_InvalidRateLimit_ThrowsArgumentExceptionWithDetails()
    {
        // Arrange
        var rateLimit = new RateLimit
        {
            Id = "",
            ApiKeyId = "",
            RequestsPerUnit = -5,
            Unit = RateLimitUnit.Hour,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CurrentRequestCount = 50
        };

        // Act
        Action act = () => rateLimit.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .Where(e => e.Message.Contains("Id cannot be null or whitespace"))
            .Where(e => e.Message.Contains("ApiKeyId cannot be null or whitespace"))
            .Where(e => e.Message.Contains("RequestsPerUnit must be a positive integer"));
    }
}