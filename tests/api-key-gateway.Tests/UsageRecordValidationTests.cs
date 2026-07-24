using System;
using System.Collections.Generic;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class UsageRecordValidationTests
{
    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var usageRecord = new UsageRecord
        {
            Id = "test-id",
            ApiKeyId = "test-api-key",
            ConsumerId = "test-consumer",
            RecordedAt = DateTime.UtcNow,
            Endpoint = "test-endpoint",
            Method = "GET",
            ResponseStatusCode = 200,
            RequestBytes = 100,
            ResponseBytes = 200,
            ResponseTimeMs = 50,
            ErrorCode = null,
            SourceIp = null,
            UserAgent = null,
            Tags = new Dictionary<string, string>
            {
                {"key", "value"}
            }
        };

        // Act
        var errors = UsageRecordValidation.Validate(usageRecord);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => UsageRecordValidation.Validate(null));
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var usageRecord = new UsageRecord
        {
            Id = "test-id",
            ApiKeyId = "test-api-key",
            ConsumerId = "test-consumer",
            RecordedAt = DateTime.UtcNow,
            Endpoint = "test-endpoint",
            Method = "GET",
            ResponseStatusCode = 200,
            RequestBytes = 100,
            ResponseBytes = 200,
            ResponseTimeMs = 50,
            ErrorCode = null,
            SourceIp = null,
            UserAgent = null,
            Tags = new Dictionary<string, string>
            {
                {"key", "value"}
            }
        };

        // Act
        var result = UsageRecordValidation.IsValid(usageRecord);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => UsageRecordValidation.IsValid(null));
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var usageRecord = new UsageRecord
        {
            Id = "test-id",
            ApiKeyId = "test-api-key",
            ConsumerId = "test-consumer",
            RecordedAt = DateTime.UtcNow,
            Endpoint = "test-endpoint",
            Method = "GET",
            ResponseStatusCode = 200,
            RequestBytes = 100,
            ResponseBytes = 200,
            ResponseTimeMs = 50,
            ErrorCode = null,
            SourceIp = null,
            UserAgent = null,
            Tags = new Dictionary<string, string>
            {
                {"key", "value"}
            }
        };

        // Act & Assert
        UsageRecordValidation.EnsureValid(usageRecord);
    }

    [Fact]
    public void EnsureValid_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => UsageRecordValidation.EnsureValid(null));
    }

    [Fact]
    public void EnsureValid_InvalidInput_ThrowsArgumentException()
    {
        // Arrange
        var usageRecord = new UsageRecord
        {
            Id = "",
            ApiKeyId = "test-api-key",
            ConsumerId = "test-consumer",
            RecordedAt = DateTime.UtcNow,
            Endpoint = "test-endpoint",
            Method = "GET",
            ResponseStatusCode = 200,
            RequestBytes = 100,
            ResponseBytes = 200,
            ResponseTimeMs = 50,
            ErrorCode = null,
            SourceIp = null,
            UserAgent = null,
            Tags = new Dictionary<string, string>
            {
                {"key", "value"}
            }
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => UsageRecordValidation.EnsureValid(usageRecord));
    }
}
