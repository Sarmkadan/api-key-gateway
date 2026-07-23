namespace ApiKeyGateway.Tests;

using System;
using Xunit;
using ApiKeyGateway.Domain.Exceptions;
using DomainException = ApiKeyGateway.Domain.Exceptions.UnauthorizedAccessException;

public class UnauthorizedAccessExceptionValidationTests
{
    [Fact]
    public void Validate_ValidExceptionWithMessage_ReturnsEmptyList()
    {
        // Arrange
        var exception = new DomainException("Test message");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ValidExceptionWithMessageAndReason_ReturnsEmptyList()
    {
        // Arrange
        var exception = new DomainException("Test message", "Invalid API key");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ValidExceptionWithMessageReasonAndSourceIp_ReturnsEmptyList()
    {
        // Arrange
        var exception = new DomainException("Test message", "Invalid API key", "192.168.1.1");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ExceptionWithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        DomainException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.Validate());
    }

    [Fact]
    public void Validate_ExceptionWithEmptyMessage_ReturnsValidationError()
    {
        // Arrange
        var exception = new DomainException(string.Empty);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Single(result);
        Assert.Contains("Message cannot be null, empty, or whitespace.", result);
    }

    [Fact]
    public void Validate_ExceptionWithWhitespaceMessage_ReturnsValidationError()
    {
        // Arrange
        var exception = new DomainException("   ");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Single(result);
        Assert.Contains("Message cannot be null, empty, or whitespace.", result);
    }

    [Fact]
    public void Validate_ExceptionWithWhitespaceReason_ReturnsValidationError()
    {
        // Arrange
        var exception = new DomainException("Test message", "   ");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Single(result);
        Assert.Contains("Reason cannot be whitespace if specified.", result);
    }

    [Fact]
    public void Validate_ExceptionWithWhitespaceSourceIp_ReturnsValidationError()
    {
        // Arrange
        var exception = new DomainException("Test message", "Invalid API key", "   ");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Single(result);
        Assert.Contains("SourceIp cannot be whitespace if specified.", result);
    }

    [Fact]
    public void IsValid_ValidException_ReturnsTrue()
    {
        // Arrange
        var exception = new DomainException("Test message");

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_NullException_ReturnsFalse()
    {
        // Arrange
        DomainException exception = null!;

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_InvalidException_ReturnsFalse()
    {
        // Arrange
        var exception = new DomainException(string.Empty);

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EnsureValid_ValidException_DoesNotThrow()
    {
        // Arrange
        var exception = new DomainException("Test message");

        // Act & Assert
        var exceptionThrown = Record.Exception(() => exception.EnsureValid());
        Assert.Null(exceptionThrown);
    }

    [Fact]
    public void EnsureValid_NullException_ThrowsArgumentNullException()
    {
        // Arrange
        DomainException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.EnsureValid());
    }

    [Fact]
    public void EnsureValid_InvalidException_ThrowsArgumentException()
    {
        // Arrange
        var exception = new DomainException(string.Empty);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => exception.EnsureValid());
        Assert.StartsWith("UnauthorizedAccessException is invalid:", ex.Message);
        Assert.Contains("Message cannot be null, empty, or whitespace.", ex.Message);
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void EnsureValid_InvalidExceptionWithMultipleProblems_ThrowsArgumentExceptionWithAllProblems()
    {
        // Arrange
        var exception = new DomainException("   ", "   ", "   ");

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => exception.EnsureValid());
        Assert.Contains("Message cannot be null, empty, or whitespace.", ex.Message);
        Assert.Contains("Reason cannot be whitespace if specified.", ex.Message);
        Assert.Contains("SourceIp cannot be whitespace if specified.", ex.Message);
        Assert.Equal("value", ex.ParamName);
    }
}