// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Configuration;
using ApiKeyGateway.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Unit tests for <see cref="RateLimitingOptionsValidation"/>.
/// </summary>
public class RateLimitingOptionsValidationTests
{
    private static Dictionary<string, RateLimitPolicyOptions> DefaultPolicies() => new(StringComparer.OrdinalIgnoreCase)
    {
        ["free"] = new RateLimitPolicyOptions { RequestsPerUnit = 100, WindowSeconds = 3600 },
        ["pro"] = new RateLimitPolicyOptions { RequestsPerUnit = 10000, WindowSeconds = 60 }
    };

    [Fact]
    public void Validate_ReturnsEmpty_WhenNoPoliciesOrOverridesConfigured()
    {
        // Arrange
        var options = new RateLimitingOptions();

        // Act
        var errors = RateLimitingOptionsValidation.Validate(options);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ReturnsEmpty_ForValidPoliciesAndOverrides()
    {
        // Arrange
        var options = new RateLimitingOptions { Policies = DefaultPolicies() };
        options.KeyOverrides["key-1"] = new RateLimitKeyOverrideOptions { Policy = "pro", RequestsPerUnit = 500 };

        // Act
        var errors = RateLimitingOptionsValidation.Validate(options);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ReturnsError_WhenKeyOverrideReferencesUnknownPolicy_IncludingPolicyName()
    {
        // Arrange
        var options = new RateLimitingOptions { Policies = DefaultPolicies() };
        options.KeyOverrides["key-1"] = new RateLimitKeyOverrideOptions { Policy = "enterprise-gold" };

        // Act
        var errors = RateLimitingOptionsValidation.Validate(options);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Contain("enterprise-gold")
            .And.Contain("key-1");
    }

    [Fact]
    public void Validate_AcceptsPolicyNameThatDiffersOnlyByCase()
    {
        // Arrange
        var options = new RateLimitingOptions { Policies = DefaultPolicies() };
        options.KeyOverrides["key-1"] = new RateLimitKeyOverrideOptions { Policy = "PRO" };

        // Act
        var errors = RateLimitingOptionsValidation.Validate(options);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ValidatePolicy_ReturnsError_WhenRequestsPerUnitNotPositive(int requests)
    {
        // Arrange
        var policy = new RateLimitPolicyOptions { RequestsPerUnit = requests, WindowSeconds = 60 };

        // Act
        var errors = RateLimitingOptionsValidation.ValidatePolicy("free", policy);

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("free").And.Contain("RequestsPerUnit");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(86401)]
    [InlineData(-1)]
    public void ValidatePolicy_ReturnsError_WhenWindowOutsideOneSecondToOneDay(int window)
    {
        // Arrange
        var policy = new RateLimitPolicyOptions { RequestsPerUnit = 10, WindowSeconds = window };

        // Act
        var errors = RateLimitingOptionsValidation.ValidatePolicy("free", policy);

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("WindowSeconds").And.Contain("1").And.Contain("86400");
    }

    [Theory]
    [InlineData(1, RateLimitUnit.Second)]
    [InlineData(60, RateLimitUnit.Minute)]
    [InlineData(3600, RateLimitUnit.Hour)]
    [InlineData(86400, RateLimitUnit.Day)]
    public void ValidatePolicy_ReturnsEmpty_ForBoundaryAndSupportedWindows(int window, RateLimitUnit expectedUnit)
    {
        // Arrange
        var policy = new RateLimitPolicyOptions { RequestsPerUnit = 10, WindowSeconds = window };

        // Act
        var errors = RateLimitingOptionsValidation.ValidatePolicy("free", policy);
        var mapped = RateLimitingOptionsValidation.TryGetUnit(window, out var unit);

        // Assert
        errors.Should().BeEmpty();
        mapped.Should().BeTrue();
        unit.Should().Be(expectedUnit);
    }

    [Fact]
    public void ValidatePolicy_ReturnsError_WhenWindowInRangeButNotASupportedUnit()
    {
        // Arrange
        var policy = new RateLimitPolicyOptions { RequestsPerUnit = 10, WindowSeconds = 90 };

        // Act
        var errors = RateLimitingOptionsValidation.ValidatePolicy("free", policy);

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("supported rate limit unit");
    }

    [Fact]
    public void ValidatePolicy_ReturnsError_WhenPolicyIsNull()
    {
        // Act
        var errors = RateLimitingOptionsValidation.ValidatePolicy("free", null);

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("free");
    }

    [Fact]
    public void ValidateKeyOverride_ReturnsError_WhenPolicyNameMissing()
    {
        // Arrange
        var keyOverride = new RateLimitKeyOverrideOptions { Policy = "  " };

        // Act
        var errors = RateLimitingOptionsValidation.ValidateKeyOverride("key-1", keyOverride, DefaultPolicies());

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("key-1").And.Contain("policy name");
    }

    [Fact]
    public void ValidateKeyOverride_ReturnsError_WhenOverrideIsNull()
    {
        // Act
        var errors = RateLimitingOptionsValidation.ValidateKeyOverride("key-1", null, DefaultPolicies());

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("key-1");
    }

    [Fact]
    public void ValidateKeyOverride_ReturnsError_WhenKeyIdIsBlank()
    {
        // Arrange
        var keyOverride = new RateLimitKeyOverrideOptions { Policy = "free" };

        // Act
        var errors = RateLimitingOptionsValidation.ValidateKeyOverride(" ", keyOverride, DefaultPolicies());

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("key id");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidateKeyOverride_ReturnsError_WhenRequestsPerUnitNotPositive(int requests)
    {
        // Arrange
        var keyOverride = new RateLimitKeyOverrideOptions { Policy = "free", RequestsPerUnit = requests };

        // Act
        var errors = RateLimitingOptionsValidation.ValidateKeyOverride("key-1", keyOverride, DefaultPolicies());

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("RequestsPerUnit").And.Contain("key-1");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(86401)]
    public void ValidateKeyOverride_ReturnsError_WhenWindowOutOfRange(int window)
    {
        // Arrange
        var keyOverride = new RateLimitKeyOverrideOptions { Policy = "free", WindowSeconds = window };

        // Act
        var errors = RateLimitingOptionsValidation.ValidateKeyOverride("key-1", keyOverride, DefaultPolicies());

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("WindowSeconds");
    }

    [Fact]
    public void ValidateKeyOverride_ReturnsError_WhenWindowNotASupportedUnit()
    {
        // Arrange
        var keyOverride = new RateLimitKeyOverrideOptions { Policy = "free", WindowSeconds = 120 };

        // Act
        var errors = RateLimitingOptionsValidation.ValidateKeyOverride("key-1", keyOverride, DefaultPolicies());

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("supported rate limit unit");
    }

    [Fact]
    public void ValidateKeyOverride_ReturnsEmpty_WhenOnlyPolicyIsSet()
    {
        // Arrange
        var keyOverride = new RateLimitKeyOverrideOptions { Policy = "free" };

        // Act
        var errors = RateLimitingOptionsValidation.ValidateKeyOverride("key-1", keyOverride, DefaultPolicies());

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateKeyOverride_ReportsUnknownPolicyAndInvalidValuesTogether()
    {
        // Arrange
        var keyOverride = new RateLimitKeyOverrideOptions { Policy = "nope", RequestsPerUnit = 0, WindowSeconds = 0 };

        // Act
        var errors = RateLimitingOptionsValidation.ValidateKeyOverride("key-1", keyOverride, DefaultPolicies());

        // Assert
        errors.Should().HaveCount(3);
        errors.Should().Contain(e => e.Contains("nope"));
    }

    [Fact]
    public void Validate_ReportsEveryBadPolicyAndOverride()
    {
        // Arrange
        var options = new RateLimitingOptions
        {
            Policies = new Dictionary<string, RateLimitPolicyOptions>
            {
                ["broken"] = new RateLimitPolicyOptions { RequestsPerUnit = 0, WindowSeconds = 60 }
            }
        };
        options.KeyOverrides["key-1"] = new RateLimitKeyOverrideOptions { Policy = "missing" };

        // Act
        var errors = RateLimitingOptionsValidation.Validate(options);

        // Assert
        errors.Should().HaveCount(2);
        errors.Should().Contain(e => e.Contains("broken"));
        errors.Should().Contain(e => e.Contains("missing"));
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenOptionsNull()
    {
        // Act
        Action act = () => RateLimitingOptionsValidation.Validate(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
