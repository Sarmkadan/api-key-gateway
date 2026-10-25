// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Configuration;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class TransformationPipelineOptionsValidationTests
{
    public static TheoryData<Action<TransformationPipelineOptions>, string> InvalidOptionCases => new()
    {
        { o => o.MaxRulesPerRequest = 0, "MaxRulesPerRequest must be a positive integer greater than zero." },
        { o => o.MaxBodySizeBytes = -1, "MaxBodySizeBytes must be a positive integer greater than zero." },
        { o => o.RuleCacheTtl = TimeSpan.FromSeconds(-1), "RuleCacheTtl cannot be negative." },
        { o => o.StaticRules = null!, "StaticRules cannot be null." },
        { o => { o.MaxRulesPerRequest = 1; o.StaticRules = [new TransformationRule(), new TransformationRule()]; }, "StaticRules count cannot exceed MaxRulesPerRequest." },
        { o => o.Lua = null!, "Lua execution options cannot be null." },
        { o => o.Lua.MaxExecutionMs = 0, "Lua.MaxExecutionMs must be a positive integer greater than zero." },
        { o => o.Lua.MaxScriptSizeBytes = 0, "Lua.MaxScriptSizeBytes must be a positive integer greater than zero." },
        { o => o.Lua.MaxMemoryBytes = -1, "Lua.MaxMemoryBytes cannot be negative." },
        { o => o.Lua.DisallowedGlobals = null!, "Lua.DisallowedGlobals cannot be null." },
        { o => o.Lua.DisallowedGlobals = new[] { "eval", "   " }, "Lua.DisallowedGlobals cannot contain empty or whitespace-only entries." }
    };

    [Fact]
    public void NullOptions_ValidateAndEnsureValidThrow_IsValidReturnsFalse()
    {
        // Arrange
        TransformationPipelineOptions? options = null;

        // Act
        var validate = () => TransformationPipelineOptionsValidation.Validate(options!);
        var ensureValid = () => TransformationPipelineOptionsValidation.EnsureValid(options!);
        var isValid = TransformationPipelineOptionsValidation.IsValid(options);

        // Assert
        validate.Should().Throw<ArgumentNullException>().WithParameterName("value");
        ensureValid.Should().Throw<ArgumentNullException>().WithParameterName("value");
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_DefaultOptions_ReturnsNoErrorsAndEnsureValidDoesNotThrow()
    {
        // Arrange
        var options = new TransformationPipelineOptions();

        // Act
        var errors = TransformationPipelineOptionsValidation.Validate(options);
        var act = () => TransformationPipelineOptionsValidation.EnsureValid(options);

        // Assert
        errors.Should().BeEmpty();
        TransformationPipelineOptionsValidation.IsValid(options).Should().BeTrue();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_BoundaryValues_AreAccepted()
    {
        // Arrange: zero TTL disables caching, zero memory disables Lua memory enforcement,
        // and StaticRules may equal MaxRulesPerRequest exactly.
        var options = new TransformationPipelineOptions
        {
            RuleCacheTtl = TimeSpan.Zero,
            MaxRulesPerRequest = 2,
            StaticRules = [new TransformationRule(), new TransformationRule()]
        };
        options.Lua.MaxMemoryBytes = 0;

        // Act
        var errors = TransformationPipelineOptionsValidation.Validate(options);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(InvalidOptionCases))]
    public void Validate_InvalidOption_ReturnsSingleExpectedError(
        Action<TransformationPipelineOptions> mutate,
        string expectedError)
    {
        // Arrange
        var options = new TransformationPipelineOptions();
        mutate(options);

        // Act
        var errors = TransformationPipelineOptionsValidation.Validate(options);

        // Assert
        errors.Should().ContainSingle().Which.Should().Be(expectedError);
        TransformationPipelineOptionsValidation.IsValid(options).Should().BeFalse();
    }

    [Fact]
    public void Validate_MultipleProblems_ReturnsAllErrorsInDeclarationOrder()
    {
        // Arrange
        var options = new TransformationPipelineOptions
        {
            MaxRulesPerRequest = 0,
            RuleCacheTtl = TimeSpan.FromSeconds(-5),
            StaticRules = null!
        };
        options.Lua.MaxMemoryBytes = -1;

        // Act
        var errors = TransformationPipelineOptionsValidation.Validate(options);

        // Assert
        errors.Should().Equal(
            "MaxRulesPerRequest must be a positive integer greater than zero.",
            "RuleCacheTtl cannot be negative.",
            "StaticRules cannot be null.",
            "Lua.MaxMemoryBytes cannot be negative.");
    }

    [Fact]
    public void EnsureValid_InvalidOptions_ThrowsArgumentExceptionListingEveryError()
    {
        // Arrange
        var options = new TransformationPipelineOptions { MaxBodySizeBytes = 0 };
        options.Lua.MaxExecutionMs = 0;

        // Act
        var act = () => TransformationPipelineOptionsValidation.EnsureValid(options);

        // Assert
        var exception = act.Should().Throw<ArgumentException>().Which;
        exception.ParamName.Should().Be("value");
        exception.Message.Should().StartWith("TransformationPipelineOptions validation failed:");
        exception.Message.Should().Contain("MaxBodySizeBytes must be a positive integer greater than zero.");
        exception.Message.Should().Contain("Lua.MaxExecutionMs must be a positive integer greater than zero.");
    }
}
