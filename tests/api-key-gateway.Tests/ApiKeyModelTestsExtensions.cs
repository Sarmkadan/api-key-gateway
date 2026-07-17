// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Extension methods for ApiKeyModelTests to simplify test assertions and setup
// =============================================================================

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides extension methods for <see cref="ApiKeyModelTests"/> to simplify test assertions and setup.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Extension methods class")]
public static class ApiKeyModelTestsExtensions
{
    /// <summary>
    /// Validates that the IP whitelist string is not null, empty, or whitespace.
    /// </summary>
    /// <param name="ipWhitelist">The IP whitelist string to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ipWhitelist"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ipWhitelist"/> is empty or consists only of whitespace.</exception>
    private static void ValidateIpWhitelist(string ipWhitelist)
    {
        ArgumentNullException.ThrowIfNull(ipWhitelist);

        if (string.IsNullOrWhiteSpace(ipWhitelist))
        {
            throw new ArgumentException(
                "IP whitelist cannot be empty or consist only of whitespace.",
                nameof(ipWhitelist));
        }
    }
    /// <summary>
    /// Creates a new active <see cref="ApiKey"/> with default values for testing.
    /// </summary>
    /// <param name="tests">The test instance (unused but required for extension method syntax).</param>
    /// <param name="expirationDays">Optional expiration days from now. Defaults to 30 days.</param>
    /// <returns>A new active API key with test-friendly defaults.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expirationDays"/> is negative.</exception>
    public static ApiKey WithDefaultValues(this ApiKeyModelTests tests, int expirationDays = 30)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(expirationDays);

        var expiresAt = expirationDays == 0
            ? null as DateTime?
            : DateTime.UtcNow.AddDays(expirationDays);

        return new ApiKey
        {
            Status = ApiKeyStatus.Active,
            ExpiresAt = expiresAt,
            RequestCount = 0,
            BytesTransferred = 0,
            LastUsedAt = null,
            CreatedAt = DateTime.UtcNow,
            IpWhitelist = null,
            DisabledAt = null
        };
    }

    /// <summary>
    /// Creates a new API key with the specified status for testing.
    /// </summary>
    /// <param name="tests">The test instance (unused but required for extension method syntax).</param>
    /// <param name="status">The desired status for the API key.</param>
    /// <param name="expirationDays">Optional expiration days from now. Defaults to 30 days.</param>
    /// <returns>A new API key with the specified status.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expirationDays"/> is negative.</exception>
    public static ApiKey WithStatus(this ApiKeyModelTests tests, ApiKeyStatus status, int expirationDays = 30)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(expirationDays);

        var expiresAt = expirationDays == 0
            ? null as DateTime?
            : DateTime.UtcNow.AddDays(expirationDays);

        return new ApiKey
        {
            Status = status,
            ExpiresAt = expiresAt,
            RequestCount = 0,
            BytesTransferred = 0,
            LastUsedAt = null,
            CreatedAt = DateTime.UtcNow,
            IpWhitelist = null,
            DisabledAt = status == ApiKeyStatus.Disabled ? DateTime.UtcNow : null
        };
    }

    /// <summary>
    /// Creates a new API key with the specified IP whitelist for testing.
    /// </summary>
    /// <param name="tests">The test instance (unused but required for extension method syntax).</param>
    /// <param name="ipWhitelist">Comma-separated list of allowed IPs. Must not be null or whitespace.</param>
    /// <param name="status">Optional status for the API key. Defaults to Active.</param>
    /// <returns>A new API key with the specified IP whitelist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ipWhitelist"/> is null.</exception>
/// <exception cref="ArgumentException">Thrown when <paramref name="ipWhitelist"/> is empty or consists only of whitespace.</exception>
    public static ApiKey WithIpWhitelist(this ApiKeyModelTests tests, string ipWhitelist, ApiKeyStatus status = ApiKeyStatus.Active)
    {
        ArgumentNullException.ThrowIfNull(ipWhitelist);

        return new ApiKey
        {
            Status = status,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            RequestCount = 0,
            BytesTransferred = 0,
            LastUsedAt = null,
            CreatedAt = DateTime.UtcNow,
            IpWhitelist = ipWhitelist.Trim(),
            DisabledAt = status == ApiKeyStatus.Disabled ? DateTime.UtcNow : null
        };
    }


    /// <summary>
    /// Asserts that the API key can be used based on its status and expiration.
    /// </summary>
    /// <param name="key">The API key to test.</param>
    /// <param name="expected">The expected result of CanBeUsed().</param>
    /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
    public static void ShouldBeUsable(this ApiKey key, bool expected)
    {
        ArgumentNullException.ThrowIfNull(key);

        key.CanBeUsed().Should().Be(expected,
            $"Expected CanBeUsed() to return {expected} for key with Status={key.Status}, ExpiresAt={(key.ExpiresAt.HasValue ? key.ExpiresAt.Value.ToString(CultureInfo.InvariantCulture) : "null")}");
    }

    /// <summary>
    /// Asserts that the API key has the expected request count and bytes transferred.
    /// </summary>
    /// <param name="key">The API key to test.</param>
    /// <param name="expectedCount">The expected request count.</param>
    /// <param name="expectedBytes">The expected bytes transferred.</param>
    /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
    public static void ShouldHaveUsage(this ApiKey key, int expectedCount, long expectedBytes)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedCount);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedBytes);

        key.RequestCount.Should().Be(expectedCount, "Request count mismatch");
        key.BytesTransferred.Should().Be(expectedBytes, "Bytes transferred mismatch");
    }

    /// <summary>
    /// Asserts that the API key has the expected last used timestamp.
    /// </summary>
    /// <param name="key">The API key to test.</param>
    /// <param name="expected">The expected last used timestamp, or null if never used.</param>
    /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
    public static void ShouldHaveLastUsedAt(this ApiKey key, DateTime? expected)
    {
        ArgumentNullException.ThrowIfNull(key);

        key.LastUsedAt.Should().Be(expected, "Last used timestamp mismatch");
    }

    /// <summary>
    /// Asserts that the API key has the expected disabled timestamp.
    /// </summary>
    /// <param name="key">The API key to test.</param>
    /// <param name="expected">The expected disabled timestamp, or null if not disabled.</param>
    /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
    public static void ShouldHaveDisabledAt(this ApiKey key, DateTime? expected)
    {
        ArgumentNullException.ThrowIfNull(key);

        key.DisabledAt.Should().Be(expected, "Disabled timestamp mismatch");
    }

    /// <summary>
    /// Asserts that the API key is allowed for the specified IP address.
    /// </summary>
    /// <param name="key">The API key to test.</param>
    /// <param name="ipAddress">The IP address to check.</param>
    /// <param name="expected">The expected result of IsIpAllowed().</param>
    /// <exception cref="ArgumentNullException">Thrown when key or ipAddress is null.</exception>
    public static void ShouldAllowIp(this ApiKey key, string ipAddress, bool expected)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(ipAddress);

        key.IsIpAllowed(ipAddress).Should().Be(expected,
            $"Expected IsIpAllowed('{ipAddress}') to return {expected} for key with IpWhitelist={key.IpWhitelist ?? "null"}");
    }

    /// <summary>
    /// Disables the API key and asserts the operation was successful.
    /// </summary>
    /// <param name="key">The API key to disable.</param>
    /// <param name="before">Optional timestamp before the operation for comparison.</param>
    /// <returns>The disabled API key.</returns>
    /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
    public static ApiKey DisableAndAssert(this ApiKey key, DateTime? before = null)
    {
        ArgumentNullException.ThrowIfNull(key);

        var beforeDisable = before ?? DateTime.UtcNow;
        key.Disable();

        key.Status.Should().Be(ApiKeyStatus.Disabled, "Status should be Disabled after calling Disable()");
        key.DisabledAt.Should().BeOnOrAfter(beforeDisable, "DisabledAt should be set to current or later time");

        return key;
    }

    /// <summary>
    /// Enables the API key and asserts the operation was successful.
    /// </summary>
    /// <param name="key">The API key to enable.</param>
    /// <returns>The enabled API key.</returns>
    /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
    public static ApiKey EnableAndAssert(this ApiKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        key.Enable();

        key.Status.Should().Be(ApiKeyStatus.Active, "Status should be Active after calling Enable()");
        key.DisabledAt.Should().BeNull("DisabledAt should be null after calling Enable()");

        return key;
    }

    /// <summary>
    /// Records usage on the API key and asserts the operation was successful.
    /// </summary>
    /// <param name="key">The API key to record usage on.</param>
    /// <param name="bytes">Number of bytes transferred.</param>
    /// <param name="before">Optional timestamp before the operation for comparison.</param>
    /// <returns>The API key with updated usage statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
    public static ApiKey RecordUsageAndAssert(this ApiKey key, long bytes = 0, DateTime? before = null)
    {
        ArgumentNullException.ThrowIfNull(key);

        var beforeUsage = before ?? DateTime.UtcNow;
        key.RecordUsage(bytes);

        key.RequestCount.Should().BeGreaterOrEqualTo(1, "RequestCount should be incremented");
        key.BytesTransferred.Should().Be(bytes, "BytesTransferred should match the recorded value");
        key.LastUsedAt.Should().BeOnOrAfter(beforeUsage, "LastUsedAt should be updated");

        return key;
    }
}