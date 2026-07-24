// =============================================================================
// Author: Automated Test Generation
// =============================================================================

namespace api_key_gateway.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using ApiKeyGateway.Domain.Models;
using Xunit;

public static class ApiEndpointValidationTests
{
    private static ApiEndpoint CreateValidEndpoint()
    {
        return new ApiEndpoint
        {
            Id = "endpoint-1",
            Path = "/api/resource",
            Method = "GET",
            TargetUrl = "https://example.com/api/resource",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            TimeoutMs = 1000,
            MaxPayloadBytes = 1024,
            CacheTtlSeconds = 60,
            Headers = new Dictionary<string, string> { { "X-Custom", "value" } },
            AllowedConsumers = new List<string> { "consumer-1" }
        };
    }

    [Fact]
    public static void Validate_HappyPath_ReturnsEmptyList()
    {
        var endpoint = CreateValidEndpoint();
        var problems = endpoint.Validate();
        Assert.Empty(problems);
    }

    [Fact]
    public static void IsValid_HappyPath_ReturnsTrue()
    {
        var endpoint = CreateValidEndpoint();
        Assert.True(endpoint.IsValid());
    }

    [Fact]
    public static void EnsureValid_HappyPath_DoesNotThrow()
    {
        var endpoint = CreateValidEndpoint();
        var exception = Record.Exception(() => endpoint.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public static void Validate_Null_ThrowsArgumentNullException()
    {
        ApiEndpoint? endpoint = null;
        Assert.Throws<ArgumentNullException>(() => endpoint!.Validate());
    }

    [Fact]
    public static void IsValid_Null_ReturnsFalse()
    {
        ApiEndpoint? endpoint = null;
        Assert.False(endpoint.IsValid());
    }

    [Fact]
    public static void EnsureValid_Null_ThrowsArgumentNullException()
    {
        ApiEndpoint? endpoint = null;
        Assert.Throws<ArgumentNullException>(() => endpoint!.EnsureValid());
    }

    [Fact]
    public static void Validate_InvalidValues_ReturnsAllProblems()
    {
        var endpoint = new ApiEndpoint
        {
            Id = "",
            Path = "api/resource",
            Method = "FETCH",
            TargetUrl = "not-a-url",
            CreatedAt = default,
            TimeoutMs = 0,
            MaxPayloadBytes = -1,
            CacheTtlSeconds = -5,
            Headers = null,
            AllowedConsumers = null
        };

        var problems = endpoint.Validate();

        Assert.Equal(10, problems.Count);
        Assert.Contains("Id cannot be null or whitespace", problems);
        Assert.Contains("Path must start with '/'", problems);
        Assert.Contains("Method 'FETCH' is not a valid HTTP method", problems);
        Assert.Contains("TargetUrl 'not-a-url' is not a valid absolute URI", problems);
        Assert.Contains("CreatedAt cannot be default(DateTime)", problems);
        Assert.Contains("TimeoutMs must be a positive number", problems);
        Assert.Contains("MaxPayloadBytes must be a positive number", problems);
        Assert.Contains("CacheTtlSeconds cannot be negative", problems);
        Assert.Contains("Headers dictionary cannot be null", problems);
        Assert.Contains("AllowedConsumers list cannot be null", problems);
    }

    [Fact]
    public static void EnsureValid_InvalidValues_ThrowsArgumentExceptionWithMessage()
    {
        var endpoint = new ApiEndpoint
        {
            Id = "",
            Path = "api/resource",
            Method = "FETCH",
            TargetUrl = "not-a-url",
            CreatedAt = default,
            TimeoutMs = 0,
            MaxPayloadBytes = -1,
            CacheTtlSeconds = -5,
            Headers = null,
            AllowedConsumers = null
        };

        var ex = Assert.Throws<ArgumentException>(() => endpoint.EnsureValid());
        Assert.Contains("ApiEndpoint is invalid", ex.Message);
        Assert.Contains("Id cannot be null or whitespace", ex.Message);
        Assert.Contains("Path must start with '/'", ex.Message);
    }

    [Fact]
    public static void Validate_BoundaryValues_ReturnsAppropriateProblems()
    {
        var endpoint = new ApiEndpoint
        {
            Id = "id",
            Path = "/path",
            Method = "GET",
            TargetUrl = "https://example.com",
            CreatedAt = DateTime.UtcNow.AddHours(1), // future
            TimeoutMs = 300001, // over max
            MaxPayloadBytes = 104857601, // over max
            CacheTtlSeconds = 86401, // over max
            Headers = new Dictionary<string, string>(),
            AllowedConsumers = new List<string>()
        };

        var problems = endpoint.Validate();

        Assert.Equal(4, problems.Count);
        Assert.Contains("CreatedAt cannot be in the future", problems);
        Assert.Contains("TimeoutMs cannot exceed 300000 (5 minutes)", problems);
        Assert.Contains("MaxPayloadBytes cannot exceed 104857600 (100 MB)", problems);
        Assert.Contains("CacheTtlSeconds cannot exceed 86400 (24 hours)", problems);
    }
}
