// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for IP whitelist functionality in API keys.
/// Tests the <see cref="ApiKey.IsIpAllowed"/> method and related service methods
/// for managing IP whitelist entries.
/// </summary>
public class IpWhitelistTests
{
    private readonly Mock<IApiKeyRepository> _repositoryMock;
    private readonly Mock<ILogger<ApiKeyService>> _loggerMock;
    private readonly ApiKeyService _sut;

    /// <summary>
/// Initializes a new instance of the <see cref="IpWhitelistTests"/> class.
/// Sets up mock dependencies for testing IP whitelist functionality.
/// </summary>
public IpWhitelistTests()
    {
        _repositoryMock = new Mock<IApiKeyRepository>();
        _loggerMock = new Mock<ILogger<ApiKeyService>>();
        _sut = new ApiKeyService(_repositoryMock.Object, _loggerMock.Object);
    }

    // ── ApiKey.IsIpAllowed ────────────────────────────────────────────────

    [Fact]
    public void IsIpAllowed_NoWhitelist_AllowsAnyIp()
    {
        var key = new ApiKey { IpWhitelist = null };
        key.IsIpAllowed("1.2.3.4").Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_EmptyWhitelist_AllowsAnyIp()
    {
        var key = new ApiKey { IpWhitelist = "" };
        key.IsIpAllowed("1.2.3.4").Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_WhitelistedIp_ReturnsTrue()
    {
        var key = new ApiKey { IpWhitelist = "10.0.0.1,192.168.1.1" };
        key.IsIpAllowed("10.0.0.1").Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_NonWhitelistedIp_ReturnsFalse()
    {
        var key = new ApiKey { IpWhitelist = "10.0.0.1,192.168.1.1" };
        key.IsIpAllowed("8.8.8.8").Should().BeFalse();
    }

    [Fact]
    public void IsIpAllowed_WhitelistWithSpaces_TrimsAndMatches()
    {
        var key = new ApiKey { IpWhitelist = " 10.0.0.1 , 192.168.1.1 " };
        key.IsIpAllowed("192.168.1.1").Should().BeTrue();
        key.IsIpAllowed("99.99.99.99").Should().BeFalse();
    }

    // ── Service: GetIpWhitelistAsync ──────────────────────────────────────

    [Fact]
    public async Task GetIpWhitelistAsync_KeyNotFound_ReturnsEmptyList()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((ApiKey?)null);

        var result = await _sut.GetIpWhitelistAsync("missing");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetIpWhitelistAsync_KeyWithWhitelist_ReturnsDistinctList()
    {
        var key = new ApiKey { Id = "k1", IpWhitelist = "10.0.0.1,10.0.0.2,10.0.0.1" };
        _repositoryMock.Setup(r => r.GetByIdAsync("k1")).ReturnsAsync(key);

        var result = await _sut.GetIpWhitelistAsync("k1");

        result.Should().BeEquivalentTo(["10.0.0.1", "10.0.0.2"]);
    }

    // ── Service: SetIpWhitelistAsync ──────────────────────────────────────

    [Fact]
    public async Task SetIpWhitelistAsync_KeyNotFound_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((ApiKey?)null);

        var result = await _sut.SetIpWhitelistAsync("missing", ["1.2.3.4"]);

        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ApiKey>()), Times.Never);
    }

    [Fact]
    public async Task SetIpWhitelistAsync_ValidIps_PersistsWhitelistAsCommaSeparated()
    {
        var key = new ApiKey { Id = "k1", IpWhitelist = null };
        _repositoryMock.Setup(r => r.GetByIdAsync("k1")).ReturnsAsync(key);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ApiKey>())).Returns(Task.CompletedTask);

        var result = await _sut.SetIpWhitelistAsync("k1", ["10.0.0.1", "10.0.0.2"]);

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<ApiKey>(k =>
            k.IpWhitelist == "10.0.0.1,10.0.0.2")), Times.Once);
    }

    [Fact]
    public async Task SetIpWhitelistAsync_EmptyList_ClearsWhitelist()
    {
        var key = new ApiKey { Id = "k1", IpWhitelist = "10.0.0.1" };
        _repositoryMock.Setup(r => r.GetByIdAsync("k1")).ReturnsAsync(key);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ApiKey>())).Returns(Task.CompletedTask);

        var result = await _sut.SetIpWhitelistAsync("k1", []);

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<ApiKey>(k => k.IpWhitelist == null)), Times.Once);
    }

    // ── Service: AddIpToWhitelistAsync ────────────────────────────────────

    [Fact]
    public async Task AddIpToWhitelistAsync_NewIp_AddsToList()
    {
        var key = new ApiKey { Id = "k1", IpWhitelist = "10.0.0.1" };
        _repositoryMock.Setup(r => r.GetByIdAsync("k1")).ReturnsAsync(key);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ApiKey>())).Returns(Task.CompletedTask);

        var result = await _sut.AddIpToWhitelistAsync("k1", "10.0.0.2");

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<ApiKey>(k =>
            k.IpWhitelist!.Contains("10.0.0.1") && k.IpWhitelist.Contains("10.0.0.2"))), Times.Once);
    }

    [Fact]
    public async Task AddIpToWhitelistAsync_DuplicateIp_ReturnsFalse()
    {
        var key = new ApiKey { Id = "k1", IpWhitelist = "10.0.0.1" };
        _repositoryMock.Setup(r => r.GetByIdAsync("k1")).ReturnsAsync(key);

        var result = await _sut.AddIpToWhitelistAsync("k1", "10.0.0.1");

        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ApiKey>()), Times.Never);
    }

    [Fact]
    public async Task AddIpToWhitelistAsync_EmptyIp_ThrowsValidationException()
    {
        var act = async () => await _sut.AddIpToWhitelistAsync("k1", "");
        await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.ValidationException>();
    }

    // ── Service: RemoveIpFromWhitelistAsync ───────────────────────────────

    [Fact]
    public async Task RemoveIpFromWhitelistAsync_ExistingIp_RemovesFromList()
    {
        var key = new ApiKey { Id = "k1", IpWhitelist = "10.0.0.1,10.0.0.2" };
        _repositoryMock.Setup(r => r.GetByIdAsync("k1")).ReturnsAsync(key);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ApiKey>())).Returns(Task.CompletedTask);

        var result = await _sut.RemoveIpFromWhitelistAsync("k1", "10.0.0.1");

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<ApiKey>(k =>
            !k.IpWhitelist!.Contains("10.0.0.1") && k.IpWhitelist.Contains("10.0.0.2"))), Times.Once);
    }

    [Fact]
    public async Task RemoveIpFromWhitelistAsync_NonExistentIp_ReturnsFalse()
    {
        var key = new ApiKey { Id = "k1", IpWhitelist = "10.0.0.1" };
        _repositoryMock.Setup(r => r.GetByIdAsync("k1")).ReturnsAsync(key);

        var result = await _sut.RemoveIpFromWhitelistAsync("k1", "9.9.9.9");

        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ApiKey>()), Times.Never);
    }
}
