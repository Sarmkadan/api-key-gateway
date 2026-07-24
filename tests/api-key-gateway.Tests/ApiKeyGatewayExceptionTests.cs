// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using ApiKeyGateway.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ApiKeyGatewayException"/> class.
/// </summary>
public class ApiKeyGatewayExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Test message";

        // Act
        var exception = new ApiKeyGatewayException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().BeNull();
        exception.InnerException.Should().BeNull();
        exception.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public void Constructor_WithMessageAndErrorCode_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Test message";
        var errorCode = "ERR001";

        // Act
        var exception = new ApiKeyGatewayException(message, errorCode);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be(errorCode);
        exception.InnerException.Should().BeNull();
        exception.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Test message";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new ApiKeyGatewayException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().BeNull();
        exception.InnerException.Should().Be(innerException);
        exception.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public void Constructor_WithMessageErrorCodeAndInnerException_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Test message";
        var errorCode = "ERR002";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new ApiKeyGatewayException(message, errorCode, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be(errorCode);
        exception.InnerException.Should().Be(innerException);
        exception.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(500));
    }
}
