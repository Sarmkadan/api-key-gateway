// =============================================================================
// test: add RateLimitExceededException validation tests
// =============================================================================

using System;
using System.Collections.Generic;
using ApiKeyGateway.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class RateLimitExceededExceptionValidationTests
{
    private static RateLimitExceededException CreateValidException()
    {
        return new RateLimitExceededException("Rate limit exceeded")
        {
            ApiKeyId = "valid-key-id",
            Limit = 100,
            WindowInSeconds = 60,
            RetryAfter = DateTime.UtcNow.AddMinutes(5)
        };
    }

    [Fact]
    public void Validate_ValidException_ReturnsEmptyList()
    {
        // Arrange
        var ex = CreateValidException();

        // Act
        var problems = ex.Validate();

        // Assert
        problems.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_ValidException_ReturnsTrue()
    {
        // Arrange
        var ex = CreateValidException();

        // Act
        var result = ex.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EnsureValid_ValidException_DoesNotThrow()
    {
        // Arrange
        var ex = CreateValidException();

        // Act / Assert
        ex.Invoking(e => e.EnsureValid()).Should().NotThrow();
    }

    [Fact]
    public void Validate_Null_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((RateLimitExceededException)null!).Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsValid_Null_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((RateLimitExceededException)null!).IsValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_Null_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((RateLimitExceededException)null!).EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_InvalidFields_ReturnsAllExpectedProblems()
    {
        // Arrange: create an exception with a variety of invalid values
        var ex = new RateLimitExceededException(string.Empty) // Message invalid
        {
            ApiKeyId = "   ",                     // ApiKeyId invalid (whitespace)
            Limit = -5,                           // Limit invalid (<=0)
            WindowInSeconds = 0,                  // Window invalid (<=0)
            RetryAfter = DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(-1), DateTimeKind.Utc) // past
        };

        // Act
        var problems = ex.Validate();

        // Assert
        problems.Should().Contain("Message cannot be null, empty, or whitespace.");
        problems.Should().Contain("ApiKeyId cannot be null, empty, or whitespace.");
        problems.Should().Contain("Limit must be greater than 0.");
        problems.Should().Contain("WindowInSeconds must be greater than 0.");
        problems.Should().Contain("RetryAfter cannot be in the past.");
    }

    [Fact]
    public void EnsureValid_InvalidFields_ThrowsArgumentException_WithProblemDetails()
    {
        // Arrange: create an exception with several distinct problems
        var ex = new RateLimitExceededException(null) // Message null
        {
            ApiKeyId = null,                     // ApiKeyId null
            Limit = 2_000_000,                   // Limit exceeds max
            WindowInSeconds = 400_000_000,       // Window exceeds 1 year
            RetryAfter = default                 // default DateTime
        };

        // Act
        Action act = () => ex.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*RateLimitExceededException is invalid*")
            .Where(e => e.Message.Contains("Message cannot be null, empty, or whitespace.") &&
                        e.Message.Contains("ApiKeyId cannot be null, empty, or whitespace.") &&
                        e.Message.Contains("Limit must be a reasonable value (maximum 1000000).") &&
                        e.Message.Contains("WindowInSeconds must be a reasonable value (maximum 1 year).") &&
                        e.Message.Contains("RetryAfter cannot be the default DateTime value."));
    }
}
