using System;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class TransformationRuleValidationTests
{
    private static TransformationRule CreateValidRule()
    {
        return new TransformationRule
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Valid Rule",
            Priority = 50,
            CreatedAt = DateTime.UtcNow.AddMinutes(-1),
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void Validate_ValidRule_ReturnsEmptyList()
    {
        // Arrange
        var rule = CreateValidRule();

        // Act
        var errors = rule.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_NullRule_ThrowsArgumentNullException()
    {
        // Arrange
        TransformationRule? rule = null;

        // Act
        Action act = () => rule!.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_EmptyId_ReturnsError()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.Id = string.Empty;

        // Act
        var errors = rule.Validate();

        // Assert
        errors.Should().ContainSingle().Which.Should().Be("Id must not be empty or whitespace.");
    }

    [Fact]
    public void Validate_WhitespaceId_ReturnsError()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.Id = "   ";

        // Act
        var errors = rule.Validate();

        // Assert
        errors.Should().ContainSingle().Which.Should().Be("Id must not be empty or whitespace.");
    }

    [Fact]
    public void Validate_NullName_ReturnsError()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.Name = null!;

        // Act
        var errors = rule.Validate();

        // Assert
        errors.Should().ContainSingle().Which.Should().Be("Name must not be empty or whitespace.");
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.Name = string.Empty;

        // Act
        var errors = rule.Validate();

        // Assert
        errors.Should().ContainSingle().Which.Should().Be("Name must not be empty or whitespace.");
    }

    [Fact]
    public void Validate_WhitespaceName_ReturnsError()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.Name = "   ";

        // Act
        var errors = rule.Validate();

        // Assert
        errors.Should().ContainSingle().Which.Should().Be("Name must not be empty or whitespace.");
    }

    [Fact]
    public void Validate_NegativePriority_ReturnsError()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.Priority = -1;

        // Act
        var errors = rule.Validate();

        // Assert
        errors.Should().ContainSingle().Which.Should().Be("Priority must be between 0 and 1000 inclusive.");
    }

    [Fact]
    public void Validate_PriorityAbove1000_ReturnsError()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.Priority = 1001;

        // Act
        var errors = rule.Validate();

        // Assert
        errors.Should().ContainSingle().Which.Should().Be("Priority must be between 0 and 1000 inclusive.");
    }

    [Fact]
    public void Validate_DefaultCreatedAt_ReturnsError()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.CreatedAt = default;

        // Act
        var errors = rule.Validate();

        // Assert
        errors.Should().ContainSingle().Which.Should().Be("CreatedAt must not be default.");
    }

    [Fact]
    public void Validate_DefaultUpdatedAt_ReturnsError()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.UpdatedAt = default;

        // Act
        var errors = rule.Validate();

        // Assert
        errors.Should().ContainSingle().Which.Should().Be("UpdatedAt must not be default.");
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.Id = "   ";
        rule.Name = string.Empty;
        rule.Priority = -5;
        rule.CreatedAt = default;
        rule.UpdatedAt = default;

        // Act
        var errors = rule.Validate();

        // Assert
        errors.Should().HaveCount(5);
        errors.Should().Contain("Id must not be empty or whitespace.");
        errors.Should().Contain("Name must not be empty or whitespace.");
        errors.Should().Contain("Priority must be between 0 and 1000 inclusive.");
        errors.Should().Contain("CreatedAt must not be default.");
        errors.Should().Contain("UpdatedAt must not be default.");
    }

    [Fact]
    public void Validate_ReturnsReadOnlyList()
    {
        // Arrange
        var rule = CreateValidRule();

        // Act
        var errors = rule.Validate();

        // Assert
        errors.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    [Fact]
    public void IsValid_ValidRule_ReturnsTrue()
    {
        // Arrange
        var rule = CreateValidRule();

        // Act
        var isValid = rule.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_InvalidRule_ReturnsFalse()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.Id = string.Empty;

        // Act
        var isValid = rule.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_NullRule_ThrowsArgumentNullException()
    {
        // Arrange
        TransformationRule? rule = null;

        // Act
        Action act = () => rule!.IsValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_ValidRule_DoesNotThrow()
    {
        // Arrange
        var rule = CreateValidRule();

        // Act
        Action act = () => rule.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_InvalidRule_ThrowsArgumentException()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.Id = string.Empty;
        rule.Name = "   ";
        rule.Priority = 1001;

        // Act
        Action act = () => rule.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Id must not be empty or whitespace.*Name must not be empty or whitespace.*Priority must be between 0 and 1000 inclusive.*");
    }

    [Fact]
    public void EnsureValid_NullRule_ThrowsArgumentNullException()
    {
        // Arrange
        TransformationRule? rule = null;

        // Act
        Action act = () => rule!.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

}