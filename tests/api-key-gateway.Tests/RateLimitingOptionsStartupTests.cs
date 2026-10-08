// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Verifies that <see cref="ServiceCollectionExtensions.AddRateLimitingOptions"/> fails the host
/// on startup when the rate limit configuration is invalid.
/// </summary>
public class RateLimitingOptionsStartupTests
{
    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static IHost BuildHost(IConfiguration configuration) =>
        new HostBuilder()
            .ConfigureServices(services => services.AddRateLimitingOptions(configuration))
            .Build();

    [Fact]
    public async Task StartAsync_Succeeds_WhenPoliciesAndOverridesAreValid()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["RateLimiting:Policies:free:RequestsPerUnit"] = "100",
            ["RateLimiting:Policies:free:WindowSeconds"] = "3600",
            ["RateLimiting:Policies:pro:RequestsPerUnit"] = "10000",
            ["RateLimiting:Policies:pro:WindowSeconds"] = "60",
            ["RateLimiting:KeyOverrides:key-1:Policy"] = "pro"
        });
        using var host = BuildHost(configuration);

        // Act
        var act = () => host.StartAsync();

        // Assert
        await act.Should().NotThrowAsync();
        await host.StopAsync();
    }

    [Fact]
    public async Task StartAsync_Succeeds_WhenNoRateLimitingSectionConfigured()
    {
        // Arrange
        using var host = BuildHost(BuildConfiguration(new Dictionary<string, string?>()));

        // Act
        var act = () => host.StartAsync();

        // Assert
        await act.Should().NotThrowAsync();
        await host.StopAsync();
    }

    [Fact]
    public async Task StartAsync_Fails_WithPolicyNameInMessage_WhenKeyReferencesUnknownPolicy()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["RateLimiting:Policies:free:RequestsPerUnit"] = "100",
            ["RateLimiting:Policies:free:WindowSeconds"] = "3600",
            ["RateLimiting:KeyOverrides:key-1:Policy"] = "enterprise-gold"
        });
        using var host = BuildHost(configuration);

        // Act
        var act = () => host.StartAsync();

        // Assert
        var exception = await act.Should().ThrowAsync<OptionsValidationException>();
        exception.Which.Message.Should().Contain("enterprise-gold");
        exception.Which.Message.Should().Contain("key-1");
    }

    [Fact]
    public async Task StartAsync_Fails_WhenPolicyLimitIsNotPositive()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["RateLimiting:Policies:free:RequestsPerUnit"] = "0",
            ["RateLimiting:Policies:free:WindowSeconds"] = "3600"
        });
        using var host = BuildHost(configuration);

        // Act
        var act = () => host.StartAsync();

        // Assert
        var exception = await act.Should().ThrowAsync<OptionsValidationException>();
        exception.Which.Message.Should().Contain("free").And.Contain("RequestsPerUnit");
    }

    [Fact]
    public async Task StartAsync_Fails_WhenPolicyWindowExceedsOneDay()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["RateLimiting:Policies:free:RequestsPerUnit"] = "100",
            ["RateLimiting:Policies:free:WindowSeconds"] = "86401"
        });
        using var host = BuildHost(configuration);

        // Act
        var act = () => host.StartAsync();

        // Assert
        var exception = await act.Should().ThrowAsync<OptionsValidationException>();
        exception.Which.Message.Should().Contain("free").And.Contain("WindowSeconds");
    }
}
