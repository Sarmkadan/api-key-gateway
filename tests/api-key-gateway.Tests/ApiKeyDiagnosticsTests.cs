// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Reflection;
using ApiKeyGateway.Configuration;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Verifies the diagnostic output (<see cref="object.ToString"/> and <see cref="DebuggerDisplayAttribute"/>)
/// of key, usage and policy models. Key material must never appear in either.
/// </summary>
public class ApiKeyDiagnosticsTests
{
    private const string FakeHash = "3f9a1c_SECRET_HASH_VALUE_do_not_log";

    /// <summary>
    /// Verifies that <see cref="ApiKey.ToString"/> shows the prefix and mask but not the stored hash.
    /// </summary>
    [Fact]
    public void ApiKeyToString_WithHash_ShowsPrefixAndMaskOnly()
    {
        // Arrange
        var key = new ApiKey { Id = "key-1", KeyHash = FakeHash, Prefix = "sk_live", Description = "billing" };

        // Act
        var text = key.ToString();

        // Assert
        text.Should().Contain("sk_live****");
        text.Should().NotContain(FakeHash);
        text.Should().NotContain("3f9a1c");
    }

    /// <summary>
    /// Verifies that <see cref="ApiKey.MaskedKey"/> falls back to a bare mask when no prefix is stored.
    /// </summary>
    [Fact]
    public void ApiKeyMaskedKey_WithoutPrefix_ReturnsBareMask()
    {
        // Arrange
        var key = new ApiKey { KeyHash = FakeHash, Prefix = string.Empty };

        // Act
        var masked = key.MaskedKey;

        // Assert
        masked.Should().Be("****");
    }

    /// <summary>
    /// Verifies that the <see cref="ApiKey"/> debugger view uses the masked key and not the hash.
    /// </summary>
    [Fact]
    public void ApiKey_DebuggerDisplay_UsesMaskedKey()
    {
        // Arrange
        var attribute = typeof(ApiKey).GetCustomAttribute<DebuggerDisplayAttribute>();

        // Act
        var display = attribute?.Value;

        // Assert
        display.Should().NotBeNull();
        display.Should().Contain("MaskedKey");
        display.Should().NotContain("KeyHash");
    }

    /// <summary>
    /// Verifies that <see cref="UsageRecord.ToString"/> omits client IP and user agent.
    /// </summary>
    [Fact]
    public void UsageRecordToString_WithSourceIpAndUserAgent_OmitsThem()
    {
        // Arrange
        var record = new UsageRecord
        {
            Id = "rec-1",
            ApiKeyId = "key-1",
            Endpoint = "/api/items",
            Method = "GET",
            ResponseStatusCode = 200,
            SourceIp = "10.1.2.3",
            UserAgent = "curl/8.0"
        };

        // Act
        var text = record.ToString();

        // Assert
        text.Should().Contain("Endpoint = /api/items");
        text.Should().NotContain("10.1.2.3");
        text.Should().NotContain("curl/8.0");
    }

    /// <summary>
    /// Verifies that <see cref="UsageRecord"/> has a debugger view naming method, endpoint and status.
    /// </summary>
    [Fact]
    public void UsageRecord_DebuggerDisplay_NamesMethodEndpointAndStatus()
    {
        // Arrange
        var attribute = typeof(UsageRecord).GetCustomAttribute<DebuggerDisplayAttribute>();

        // Act
        var display = attribute?.Value;

        // Assert
        display.Should().Contain("Method").And.Contain("Endpoint").And.Contain("ResponseStatusCode");
    }

    /// <summary>
    /// Verifies that <see cref="UsageSnapshot.ToString"/> reports the in-memory counters.
    /// </summary>
    [Fact]
    public void UsageSnapshotToString_WithCounters_ReportsRequestCount()
    {
        // Arrange
        var snapshot = new UsageSnapshot { ApiKeyId = "key-1", RequestCount = 42, BytesTransferred = 1024 };

        // Act
        var text = snapshot.ToString();

        // Assert
        text.Should().Contain("ApiKeyId = key-1").And.Contain("RequestCount = 42").And.Contain("BytesTransferred = 1024");
    }

    /// <summary>
    /// Verifies that <see cref="RateLimitDecision.ToString"/> reports the outcome and remaining permits.
    /// </summary>
    [Fact]
    public void RateLimitDecisionToString_Denied_ReportsAllowedFalseAndRemaining()
    {
        // Arrange
        var decision = new RateLimitDecision
        {
            ApiKeyId = "key-1",
            Allowed = false,
            Limit = 100,
            Remaining = 0,
            ResetAtUtc = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var text = decision.ToString();

        // Assert
        text.Should().Contain("Allowed = False").And.Contain("Limit = 100").And.Contain("Remaining = 0");
    }

    /// <summary>
    /// Verifies that the rate limit policy and override options describe their settings without a body dump.
    /// </summary>
    [Fact]
    public void RateLimitOptionsToString_Policy_ReportsRequestsAndWindow()
    {
        // Arrange
        var policy = new RateLimitPolicyOptions { RequestsPerUnit = 500, WindowSeconds = 60 };
        var overrideOptions = new RateLimitKeyOverrideOptions { Policy = "pro", RequestsPerUnit = 1000 };
        var rateLimiting = new RateLimitingOptions();
        rateLimiting.Policies["pro"] = policy;

        // Act
        var policyText = policy.ToString();
        var overrideText = overrideOptions.ToString();
        var optionsText = rateLimiting.ToString();

        // Assert
        policyText.Should().Contain("RequestsPerUnit = 500").And.Contain("WindowSeconds = 60");
        overrideText.Should().Contain("Policy = pro").And.Contain("RequestsPerUnit = 1000");
        optionsText.Should().Contain("Policies = 1").And.Contain("FailurePolicy = FailClosed");
    }

    /// <summary>
    /// Verifies that the policy debugger view shows the request count and window.
    /// </summary>
    [Fact]
    public void RateLimitPolicyOptions_DebuggerDisplay_ShowsRequestsAndWindow()
    {
        // Arrange
        var attribute = typeof(RateLimitPolicyOptions).GetCustomAttribute<DebuggerDisplayAttribute>();

        // Act
        var display = attribute?.Value;

        // Assert
        display.Should().Contain("RequestsPerUnit").And.Contain("WindowSeconds");
    }
}
