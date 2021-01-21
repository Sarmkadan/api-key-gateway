// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Concurrent;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Extension methods for <see cref="RateLimitingServiceTests"/> to provide reusable test utilities
/// for rate limiting service scenarios.
/// </summary>
public static class RateLimitingServiceTestsExtensions
{
    /// <summary>
    /// Creates a configured <see cref="RateLimitingService"/> instance with default mocks.
    /// </summary>
    /// <param name="tests">The test instance</param>
    /// <returns>Configured rate limiting service</returns>
    /// <exception cref="ArgumentNullException">When tests is null</exception>
    public static RateLimitingService CreateService(this RateLimitingServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var repositoryMock = new Mock<IRateLimitRepository>();
        var loggerMock = new Mock<ILogger<RateLimitingService>>();
        return new RateLimitingService(repositoryMock.Object, loggerMock.Object);
    }

    /// <summary>
    /// Creates a rate limit configuration for testing purposes.
    /// </summary>
    /// <param name="apiKeyId">The API key identifier</param>
    /// <param name="requestsPerUnit">Maximum requests allowed</param>
    /// <param name="unit">Time unit for rate limiting</param>
    /// <param name="currentCount">Current request count (default: 0)</param>
    /// <returns>Configured rate limit model</returns>
    /// <exception cref="ArgumentException">When apiKeyId is null or empty</exception>
    public static RateLimit CreateRateLimit(
        this RateLimitingServiceTests tests,
        string apiKeyId,
        int requestsPerUnit,
        RateLimitUnit unit,
        int currentCount = 0)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);

        return new RateLimit
        {
            ApiKeyId = apiKeyId,
            RequestsPerUnit = requestsPerUnit,
            Unit = unit,
            CurrentRequestCount = currentCount,
            LastResetAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Executes multiple concurrent requests against the rate limiting service and returns
    /// the results with exception tracking.
    /// </summary>
    /// <param name="service">The rate limiting service</param>
    /// <param name="keyId">The API key identifier</param>
    /// <param name="requestCount">Number of concurrent requests to make</param>
    /// <returns>Collection of results and exceptions</returns>
    /// <exception cref="ArgumentException">When keyId is null or empty</exception>
    public static async Task<ConcurrentBag<RateLimitResult>> ExecuteConcurrentRequestsAsync(
        this RateLimitingService service,
        string keyId,
        int requestCount)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyId);

        var results = new ConcurrentBag<RateLimitResult>();
        var tasks = Enumerable.Range(0, requestCount).Select(async _ =>
        {
            try
            {
                var result = await service.CheckLimitAsync(keyId);
                results.Add(new RateLimitResult(result, null));
            }
            catch (Exception ex)
            {
                results.Add(new RateLimitResult(false, ex));
            }
        });

        await Task.WhenAll(tasks);
        return results;
    }

    /// <summary>
    /// Verifies that all requests in a collection resulted in rate limit exceptions.
    /// </summary>
    /// <param name="results">Collection of rate limit results</param>
    /// <param name="expectedCount">Expected number of exceptions</param>
    /// <returns>Task for async operation</returns>
    /// <exception cref="ArgumentNullException">When results is null</exception>
    public static async Task ShouldAllThrowRateLimitExceededAsync(
        this Task<ConcurrentBag<RateLimitResult>> resultsTask,
        int expectedCount)
    {
        var results = await resultsTask;
        ArgumentNullException.ThrowIfNull(results);

        results.Should().HaveCount(expectedCount);
        results.Should().AllSatisfy(result =>
        {
            result.Success.Should().BeFalse();
            result.Exception.Should().NotBeNull();
            result.Exception.Should().BeOfType<RateLimitExceededException>();
        });
    }

    /// <summary>
    /// Verifies that all requests in a collection succeeded.
    /// </summary>
    /// <param name="resultsTask">Task returning collection of rate limit results</param>
    /// <param name="expectedCount">Expected number of successful requests</param>
    /// <returns>Task for async operation</returns>
    /// <exception cref="ArgumentNullException">When resultsTask is null</exception>
    public static async Task ShouldAllSucceedAsync(
        this Task<ConcurrentBag<RateLimitResult>> resultsTask,
        int expectedCount)
    {
        var results = await resultsTask;
        ArgumentNullException.ThrowIfNull(results);

        results.Should().HaveCount(expectedCount);
        results.Should().AllSatisfy(result =>
        {
            result.Success.Should().BeTrue();
            result.Exception.Should().BeNull();
        });
    }

    /// <summary>
    /// Helper record to track rate limit test results.
    /// </summary>
    /// <param name="Success">Whether the request succeeded</param>
    /// <param name="Exception">Exception thrown, if any</param>
    public record RateLimitResult(bool Success, Exception? Exception);
}