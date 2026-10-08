// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ApiKeyGateway.Tests;

public class UsageTrackingOptionsValidationTests
{
    private const string ValidConnectionString = "Server=.;Database=ApiKeyGateway;Trusted_Connection=true;";

    private static UsageTrackingOptions CreateValidOptions() => new()
    {
        StoreProvider = "SqlServer",
        FlushInterval = TimeSpan.FromSeconds(2),
        RetentionDays = 90,
        SkipStartupCheck = false
    };

    [Fact]
    public void Validate_ValidOptionsAndConnectionString_ReturnsNoErrors()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act
        var errors = UsageTrackingOptionsValidation.Validate(options, ValidConnectionString);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingConnectionString_ReturnsError(string? connectionString)
    {
        // Arrange
        var options = CreateValidOptions();

        // Act
        var errors = UsageTrackingOptionsValidation.Validate(options, connectionString);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("Connection string 'DefaultConnection' must be configured for usage tracking.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_BlankStoreProvider_ReturnsError(string? storeProvider)
    {
        // Arrange
        var options = CreateValidOptions();
        options.StoreProvider = storeProvider!;

        // Act
        var errors = UsageTrackingOptionsValidation.Validate(options, ValidConnectionString);

        // Assert
        errors.Should().ContainSingle().Which.Should().Be("UsageTracking:StoreProvider must be set.");
    }

    [Fact]
    public void Validate_UnsupportedStoreProvider_ReturnsErrorNamingProviderAndSupportedList()
    {
        // Arrange
        var options = CreateValidOptions();
        options.StoreProvider = "PostgreSql";

        // Act
        var errors = UsageTrackingOptionsValidation.Validate(options, ValidConnectionString);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("UsageTracking:StoreProvider 'PostgreSql' is not supported. Supported providers: SqlServer.");
    }

    [Theory]
    [InlineData("SqlServer")]
    [InlineData("sqlserver")]
    [InlineData("SQLSERVER")]
    public void Validate_SupportedStoreProviderAnyCase_IsAccepted(string storeProvider)
    {
        // Arrange
        var options = CreateValidOptions();
        options.StoreProvider = storeProvider;

        // Act
        var errors = UsageTrackingOptionsValidation.Validate(options, ValidConnectionString);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_NonPositiveFlushInterval_ReturnsError(int seconds)
    {
        // Arrange
        var options = CreateValidOptions();
        options.FlushInterval = TimeSpan.FromSeconds(seconds);

        // Act
        var errors = UsageTrackingOptionsValidation.Validate(options, ValidConnectionString);

        // Assert
        errors.Should().Contain("UsageTracking:FlushInterval must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_NonPositiveRetentionDays_ReturnsError(int retentionDays)
    {
        // Arrange
        var options = CreateValidOptions();
        options.RetentionDays = retentionDays;

        // Act
        var errors = UsageTrackingOptionsValidation.Validate(options, ValidConnectionString);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("UsageTracking:RetentionDays must be greater than zero.");
    }

    [Fact]
    public void Validate_RetentionShorterThanFlushInterval_ReturnsError()
    {
        // Arrange
        var options = CreateValidOptions();
        options.FlushInterval = TimeSpan.FromDays(2);
        options.RetentionDays = 1;

        // Act
        var errors = UsageTrackingOptionsValidation.Validate(options, ValidConnectionString);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("UsageTracking:RetentionDays must cover at least one UsageTracking:FlushInterval.");
    }

    [Fact]
    public void Validate_RetentionEqualToFlushInterval_IsAccepted()
    {
        // Arrange
        var options = CreateValidOptions();
        options.FlushInterval = TimeSpan.FromDays(1);
        options.RetentionDays = 1;

        // Act
        var errors = UsageTrackingOptionsValidation.Validate(options, ValidConnectionString);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_NonPositiveFlushIntervalAndRetention_ReportsEveryErrorOnce()
    {
        // Arrange
        var options = CreateValidOptions();
        options.FlushInterval = TimeSpan.Zero;
        options.RetentionDays = 0;

        // Act
        var errors = UsageTrackingOptionsValidation.Validate(options, null);

        // Assert
        errors.Should().HaveCount(3);
        errors.Should().Contain("UsageTracking:FlushInterval must be greater than zero.");
        errors.Should().Contain("UsageTracking:RetentionDays must be greater than zero.");
        errors.Should().Contain(e => e.StartsWith("Connection string 'DefaultConnection'"));
    }

    [Fact]
    public void Validate_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        UsageTrackingOptions options = null!;

        // Act
        var act = () => UsageTrackingOptionsValidation.Validate(options, ValidConnectionString);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validator_InvalidOptions_ReturnsFailureWithAllMessages()
    {
        // Arrange
        var validator = new UsageTrackingOptionsValidator(ValidConnectionString);
        var options = CreateValidOptions();
        options.StoreProvider = "Mongo";
        options.FlushInterval = TimeSpan.Zero;

        // Act
        var result = validator.Validate(Options.DefaultName, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().HaveCount(2);
    }

    [Fact]
    public void Validator_ValidOptions_ReturnsSuccess()
    {
        // Arrange
        var validator = new UsageTrackingOptionsValidator(ValidConnectionString);

        // Act
        var result = validator.Validate(Options.DefaultName, CreateValidOptions());

        // Assert
        result.Succeeded.Should().BeTrue();
    }
}
