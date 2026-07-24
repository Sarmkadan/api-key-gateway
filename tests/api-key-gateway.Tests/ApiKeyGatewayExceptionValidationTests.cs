using System;
using System.Collections.Generic;
using System.Reflection;
using ApiKeyGateway.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class ApiKeyGatewayExceptionValidationTests
{
    private static void SetProperty(object target, string propertyName, object value)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        prop?.SetValue(target, value);
    }

    [Fact]
    public void Validate_ValidException_ReturnsEmptyList()
    {
        // Arrange
        var exception = new ApiKeyGatewayException("Valid message");

        // Act
        var problems = exception.Validate();

        // Assert
        problems.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InvalidException_ReturnsAllProblems()
    {
        // Arrange
        var exception = new ApiKeyGatewayException("   "); // whitespace message
        SetProperty(exception, nameof(ApiKeyGatewayException.ErrorCode), "   "); // whitespace error code
        SetProperty(exception, nameof(ApiKeyGatewayException.OccurredAt), default(DateTime)); // default date

        // Act
        var problems = exception.Validate();

        // Assert
        problems.Should().Contain("Message is null, empty, or whitespace.");
        problems.Should().Contain("OccurredAt is default DateTime (Unix epoch).");
        problems.Should().Contain("ErrorCode is whitespace.");
    }

    [Fact]
    public void Validate_OccurredAtKindNotUtc_ReturnsProblem()
    {
        // Arrange
        var exception = new ApiKeyGatewayException("Valid message");
        var localTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Local);
        SetProperty(exception, nameof(ApiKeyGatewayException.OccurredAt), localTime);

        // Act
        var problems = exception.Validate();

        // Assert
        problems.Should().Contain("OccurredAt must be in UTC kind.");
    }

    [Fact]
    public void Validate_NullException_ThrowsArgumentNullException()
    {
        // Arrange
        ApiKeyGatewayException? exception = null;

        // Act
        Action act = () => exception!.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsValid_ValidException_ReturnsTrue()
    {
        // Arrange
        var exception = new ApiKeyGatewayException("Valid message");

        // Act
        var result = exception.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_InvalidException_ReturnsFalse()
    {
        // Arrange
        var exception = new ApiKeyGatewayException("   ");
        SetProperty(exception, nameof(ApiKeyGatewayException.ErrorCode), "   ");
        SetProperty(exception, nameof(ApiKeyGatewayException.OccurredAt), default(DateTime));

        // Act
        var result = exception.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_NullException_ThrowsArgumentNullException()
    {
        // Arrange
        ApiKeyGatewayException? exception = null;

        // Act
        Action act = () => exception!.IsValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_ValidException_DoesNotThrow()
    {
        // Arrange
        var exception = new ApiKeyGatewayException("Valid message");

        // Act
        Action act = () => exception.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_InvalidException_ThrowsArgumentException()
    {
        // Arrange
        var exception = new ApiKeyGatewayException("   ");
        SetProperty(exception, nameof(ApiKeyGatewayException.ErrorCode), "   ");
        SetProperty(exception, nameof(ApiKeyGatewayException.OccurredAt), default(DateTime));

        // Act
        Action act = () => exception.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ApiKeyGatewayException is invalid:*");
    }

    [Fact]
    public void EnsureValid_NullException_ThrowsArgumentNullException()
    {
        // Arrange
        ApiKeyGatewayException? exception = null;

        // Act
        Action act = () => exception!.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
