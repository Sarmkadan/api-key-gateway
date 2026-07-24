// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using ApiKeyGateway.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="KeyStoreUnavailableException"/> class.
/// </summary>
public class KeyStoreUnavailableExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessageCorrectly()
    {
        // Arrange
        var message = "Key store is unavailable";

        // Act
        var exception = new KeyStoreUnavailableException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.Operation.Should().BeNull();
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndOperation_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Key store is unavailable";
        var operation = "ValidateApiKey";

        // Act
        var exception = new KeyStoreUnavailableException(message, operation);

        // Assert
        exception.Message.Should().Be(message);
        exception.Operation.Should().Be(operation);
        exception.InnerException.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithMessageAndOperation_HandlesNullOrEmptyOperation(string? operation)
    {
        // Arrange
        var message = "Key store is unavailable";

        // Act
        var exception = new KeyStoreUnavailableException(message, operation);

        // Assert
        exception.Message.Should().Be(message);
        exception.Operation.Should().Be(operation);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Key store is unavailable";
        var innerException = new InvalidOperationException("Database connection failed");

        // Act
        var exception = new KeyStoreUnavailableException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.Operation.Should().BeNull();
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [Fact]
    public void Constructor_WithMessageOperationAndInnerException_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Key store is unavailable";
        var operation = "GetKeyFromStore";
        var innerException = new TimeoutException("Operation timed out");

        // Act
        var exception = new KeyStoreUnavailableException(message, operation, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.Operation.Should().Be(operation);
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "ValidateKey")]
    [InlineData("Store unavailable", null)]
    public void Constructor_WithNullParameters_HandlesGracefully(string? message, string? operation)
    {
        // Act
        var exception = new KeyStoreUnavailableException(message, operation);

        // Assert
        exception.Message.Should().NotBeNull();
        exception.Operation.Should().Be(operation);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageOperationAndInnerException_HandlesNullInnerException()
    {
        // Arrange
        var message = "Key store is unavailable";
        var operation = "CheckKeyExistence";

        // Act
        var exception = new KeyStoreUnavailableException(message, operation, null);

        // Assert
        exception.Message.Should().Be(message);
        exception.Operation.Should().Be(operation);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Inheritance_ShouldBeApiKeyGatewayException()
    {
        // Arrange & Act
        var exception = new KeyStoreUnavailableException("Test message");

        // Assert
        exception.Should().BeAssignableTo<ApiKeyGatewayException>();
    }

    [Fact]
    public void Property_Operation_IsInitOnly()
    {
        // Arrange & Act
        var exception = new KeyStoreUnavailableException("Test message", "TestOperation");

        // Assert - Operation should be set during construction and cannot be changed
        exception.Operation.Should().Be("TestOperation");
    }

    [Fact]
    public void Exception_ShouldHaveMeaningfulMessage()
    {
        // Arrange
        var message = "Redis connection pool exhausted";
        var operation = "GetCachedKey";

        // Act
        var exception = new KeyStoreUnavailableException(message, operation);

        // Assert
        exception.Message.Should().Be(message);
        exception.ToString().Should().Contain(message);
    }
}