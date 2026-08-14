// SPDX-License-Identifier: MIT
// Copyright © 2024

using System;
using System.Collections.Generic;
using ApiKeyGateway.Domain.Models;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Unit tests for <see cref="UsageQuotaValidation"/>.
/// </summary>
public sealed class UsageQuotaValidationTests
{
    private static UsageQuota CreateValidQuota()
    {
        var now = DateTime.UtcNow;
        return new UsageQuota
        {
            Id = Guid.NewGuid().ToString(),
            ApiKeyId = Guid.NewGuid().ToString(),
            QuotaLimit = 100,
            IsEnabled = true,
            Period = Enums.QuotaPeriod.Daily,
            CreatedAt = now,
            PeriodStartAt = UsageQuota.GetPeriodStart(now, Enums.QuotaPeriod.Daily),
            CurrentUsage = 0
        };
    }

    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var quota = CreateValidQuota();

        // Act
        IReadOnlyList<string> errors = quota.Validate();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var quota = CreateValidQuota();

        // Act
        bool isValid = quota.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var quota = CreateValidQuota();

        // Act / Assert
        var exception = Record.Exception(() => quota.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_Null_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((UsageQuota)null!).Validate());
    }

    [Fact]
    public void Validate_InvalidId_ReturnsError()
    {
        // Arrange
        var quota = CreateValidQuota();
        quota = quota with { Id = string.Empty };

        // Act
        var errors = quota.Validate();

        // Assert
        Assert.Contains("Id must not be null or empty.", errors);
    }

    [Fact]
    public void Validate_NegativeQuotaLimit_ExceptUnlimited_ReturnsError()
    {
        // Arrange
        var quota = CreateValidQuota();
        quota = quota with { QuotaLimit = -5 };

        // Act
        var errors = quota.Validate();

        // Assert
        Assert.Contains(
            $"QuotaLimit must be non-negative or {Models.QuotaLimit.Unlimited} for unlimited (found {quota.QuotaLimit}).",
            errors);
    }

    [Fact]
    public void Validate_CurrentUsageExceedsLimit_ReturnsError()
    {
        // Arrange
        var quota = CreateValidQuota();
        quota = quota with { QuotaLimit = 10, CurrentUsage = 15 };

        // Act
        var errors = quota.Validate();

        // Assert
        Assert.Contains(
            $"CurrentUsage ({quota.CurrentUsage}) exceeds QuotaLimit ({quota.QuotaLimit}).",
            errors);
    }

    [Fact]
    public void EnsureValid_Invalid_ThrowsArgumentException()
    {
        // Arrange
        var quota = CreateValidQuota();
        quota = quota with { Id = string.Empty, QuotaLimit = -1 };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => quota.EnsureValid());
        Assert.Contains("Id must not be null or empty.", ex.Message);
        // The second error may be omitted because QuotaLimit = -1 is the unlimited sentinel,
        // but we still verify that an ArgumentException is thrown.
    }
}
