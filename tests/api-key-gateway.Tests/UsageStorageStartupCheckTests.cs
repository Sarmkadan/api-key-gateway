// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Configuration;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ApiKeyGateway.Tests;

public class UsageStorageStartupCheckTests
{
    private static IOptions<UsageTrackingOptions> CreateOptions(bool skipStartupCheck = false) =>
        Options.Create(new UsageTrackingOptions { SkipStartupCheck = skipStartupCheck });

    private static UsageStorageStartupCheck CreateSut(IUsageStorageProbe probe, bool skipStartupCheck = false) =>
        new(probe, CreateOptions(skipStartupCheck), NullLogger<UsageStorageStartupCheck>.Instance);

    [Fact]
    public async Task StartAsync_StoreReachable_PingsOnceAndCompletes()
    {
        // Arrange
        var store = new FakeUsageStorageProbe();
        var sut = CreateSut(store);

        // Act
        var act = () => sut.StartAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        store.PingCount.Should().Be(1);
    }

    [Fact]
    public async Task StartAsync_StoreUnreachable_ThrowsStorageUnavailableExceptionWithProviderName()
    {
        // Arrange
        var store = new FakeUsageStorageProbe(_ => throw new InvalidOperationException("connection refused"));
        var sut = CreateSut(store);

        // Act
        var act = () => sut.StartAsync(CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<StorageUnavailableException>();
        exception.Which.Provider.Should().Be("SqlServer");
        exception.Which.Message.Should().Contain("'SqlServer'").And.Contain("connection refused");
        exception.Which.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task StartAsync_StoreUnreachable_PingsExactlyOnce()
    {
        // Arrange
        var store = new FakeUsageStorageProbe(_ => throw new InvalidOperationException("connection refused"));
        var sut = CreateSut(store);

        // Act
        var act = () => sut.StartAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<StorageUnavailableException>();
        store.PingCount.Should().Be(1);
    }

    [Fact]
    public async Task StartAsync_SkipStartupCheckEnabled_DoesNotPingAndCompletes()
    {
        // Arrange
        var store = new FakeUsageStorageProbe(_ => throw new InvalidOperationException("connection refused"));
        var sut = CreateSut(store, skipStartupCheck: true);

        // Act
        var act = () => sut.StartAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        store.PingCount.Should().Be(0);
    }

    [Fact]
    public async Task StartAsync_HostShutdownDuringPing_PropagatesCancellationUnwrapped()
    {
        // Arrange
        using var hostShutdown = new CancellationTokenSource();
        var store = new FakeUsageStorageProbe(async ct =>
        {
            hostShutdown.Cancel();
            await Task.Delay(Timeout.Infinite, ct);
        });
        var sut = CreateSut(store);

        // Act
        var act = () => sut.StartAsync(hostShutdown.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task StopAsync_AnyState_CompletesWithoutPinging()
    {
        // Arrange
        var store = new FakeUsageStorageProbe();
        var sut = CreateSut(store);

        // Act
        await sut.StopAsync(CancellationToken.None);

        // Assert
        store.PingCount.Should().Be(0);
    }
}

/// <summary>
/// Runs <see cref="ServiceCollectionExtensions.AddUsageTrackingStorage"/> inside a real generic host to
/// verify options validation and the startup ping happen in the order the host starts them.
/// </summary>
public class UsageStorageStartupHostTests
{
    private static IHost BuildHost(IDictionary<string, string?> settings, FakeUsageStorageProbe probe) =>
        new HostBuilder()
            .ConfigureAppConfiguration(config => config.AddInMemoryCollection(settings))
            .ConfigureServices((context, services) =>
            {
                services.AddUsageTrackingStorage(context.Configuration, context.Configuration.GetConnectionString("DefaultConnection"));
                // Registered after the SQL Server probe, so this fake is the one resolved.
                services.AddSingleton<IUsageStorageProbe>(probe);
            })
            .Build();

    [Fact]
    public async Task StartHost_StoreUnreachable_FailsWithStorageUnavailableException()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=db;Database=ApiKeyGateway;",
        };
        var probe = new FakeUsageStorageProbe(_ => throw new InvalidOperationException("login failed"));
        using var host = BuildHost(settings, probe);

        // Act
        var act = () => host.StartAsync();

        // Assert
        var exception = await act.Should().ThrowAsync<StorageUnavailableException>();
        exception.Which.Provider.Should().Be("SqlServer");
        probe.PingCount.Should().Be(1);
    }

    [Fact]
    public async Task StartHost_StoreReachable_StartsAndPingsOnce()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=db;Database=ApiKeyGateway;",
        };
        var probe = new FakeUsageStorageProbe();
        using var host = BuildHost(settings, probe);

        // Act
        await host.StartAsync();

        // Assert
        probe.PingCount.Should().Be(1);
        await host.StopAsync();
    }

    [Fact]
    public async Task StartHost_BlankConnectionString_FailsOptionsValidationBeforePing()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "   ",
        };
        var probe = new FakeUsageStorageProbe();
        using var host = BuildHost(settings, probe);

        // Act
        var act = () => host.StartAsync();

        // Assert
        await act.Should().ThrowAsync<Microsoft.Extensions.Options.OptionsValidationException>()
            .WithMessage("*Connection string 'DefaultConnection'*");
        probe.PingCount.Should().Be(0);
    }

    [Fact]
    public async Task StartHost_SkipStartupCheckEnabled_StartsWithoutPinging()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=db;Database=ApiKeyGateway;",
            ["UsageTracking:SkipStartupCheck"] = "true",
        };
        var probe = new FakeUsageStorageProbe(_ => throw new InvalidOperationException("login failed"));
        using var host = BuildHost(settings, probe);

        // Act
        var act = () => host.StartAsync();

        // Assert
        await act.Should().NotThrowAsync();
        probe.PingCount.Should().Be(0);
        await host.StopAsync();
    }
}
