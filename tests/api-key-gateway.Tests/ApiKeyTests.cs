using System;
using System.Collections.Generic;
using System.Linq;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using Xunit;

namespace api_key_gateway.Tests;

public class ApiKeyTests
{
    private ApiKey CreateDefaultKey()
    {
        return new ApiKey
        {
            Id = "key-1",
            ConsumerId = "consumer-1",
            Name = "Test Key",
            KeyHash = "hash",
            Prefix = "pre",
            Status = ApiKeyStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, string>()
        };
    }

    [Fact]
    public void IsActive_ReturnsTrue_WhenStatusActiveAndNotExpired()
    {
        var key = CreateDefaultKey();
        Assert.True(key.IsActive);
    }

    [Fact]
    public void IsActive_ReturnsFalse_WhenStatusDisabled()
    {
        var key = CreateDefaultKey();
        key.Disable();
        Assert.False(key.IsActive);
    }

    [Fact]
    public void CanBeUsed_ReturnsFalse_WhenRevoked()
    {
        var key = CreateDefaultKey();
        key.Revoke();
        Assert.False(key.CanBeUsed());
    }

    [Fact]
    public void RecordUsage_UpdatesCountersAndLastUsed()
    {
        var key = CreateDefaultKey();
        var beforeCount = key.RequestCount;
        var beforeBytes = key.BytesTransferred;

        key.RecordUsage(1234);

        Assert.Equal(beforeCount + 1, key.RequestCount);
        Assert.Equal(beforeBytes + 1234, key.BytesTransferred);
        Assert.NotNull(key.LastUsedAt);
        Assert.True((DateTime.UtcNow - key.LastUsedAt!.Value).TotalSeconds < 1);
    }

    [Fact]
    public void Disable_SetsStatusAndDisabledAt()
    {
        var key = CreateDefaultKey();
        key.Disable();

        Assert.Equal(ApiKeyStatus.Disabled, key.Status);
        Assert.NotNull(key.DisabledAt);
    }

    [Fact]
    public void Enable_ResetsStatusAndDisabledAt_WhenPreviouslyDisabled()
    {
        var key = CreateDefaultKey();
        key.Disable();

        key.Enable();

        Assert.Equal(ApiKeyStatus.Active, key.Status);
        Assert.Null(key.DisabledAt);
    }

    [Fact]
    public void Enable_DoesNothing_WhenNotDisabled()
    {
        var key = CreateDefaultKey();
        var originalStatus = key.Status;
        var originalDisabledAt = key.DisabledAt;

        key.Enable();

        Assert.Equal(originalStatus, key.Status);
        Assert.Equal(originalDisabledAt, key.DisabledAt);
    }

    [Fact]
    public void IsIpAllowed_ReturnsTrue_WhenWhitelistIsNull()
    {
        var key = CreateDefaultKey();
        key.IpWhitelist = null;
        Assert.True(key.IsIpAllowed("10.0.0.1"));
    }

    [Fact]
    public void IsIpAllowed_ReturnsTrue_WhenIpInWhitelist()
    {
        var key = CreateDefaultKey();
        key.IpWhitelist = "10.0.0.1, 10.0.0.2";
        Assert.True(key.IsIpAllowed("10.0.0.2"));
    }

    [Fact]
    public void IsIpAllowed_ReturnsFalse_WhenIpNotInWhitelist()
    {
        var key = CreateDefaultKey();
        key.IpWhitelist = "10.0.0.1,10.0.0.2";
        Assert.False(key.IsIpAllowed("10.0.0.3"));
    }

    [Fact]
    public void IsIpAllowed_ReturnsFalse_WhenIpIsNull_AndWhitelistDefined()
    {
        var key = CreateDefaultKey();
        key.IpWhitelist = "10.0.0.1";
        Assert.False(key.IsIpAllowed(null!));
    }

    [Fact]
    public void IsScopeAllowed_ReturnsTrue_WhenAllowedScopesIsNull()
    {
        var key = CreateDefaultKey();
        key.AllowedScopes = null;
        Assert.True(key.IsScopeAllowed("/any/path"));
    }

    [Fact]
    public void IsScopeAllowed_ReturnsTrue_WhenPathMatchesScope()
    {
        var key = CreateDefaultKey();
        key.AllowedScopes = "/api/metrics, /api/stats";
        Assert.True(key.IsScopeAllowed("/api/metrics/summary"));
    }

    [Fact]
    public void IsScopeAllowed_ReturnsFalse_WhenPathDoesNotMatchAnyScope()
    {
        var key = CreateDefaultKey();
        key.AllowedScopes = "/api/metrics, /api/stats";
        Assert.False(key.IsScopeAllowed("/api/unknown"));
    }

    [Fact]
    public void IsScopeAllowed_Throws_WhenRequestPathIsNull()
    {
        var key = CreateDefaultKey();
        key.AllowedScopes = "/api/metrics";

        Assert.Throws<NullReferenceException>(() => key.IsScopeAllowed(null!));
    }
}
