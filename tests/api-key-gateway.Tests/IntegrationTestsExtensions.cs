// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides extension methods for the <see cref="IntegrationTests"/> class.
/// </summary>
public static class IntegrationTestsExtensions
{
    /// <summary>
    /// Creates a sample <see cref="UsageRecord"/> for testing purposes.
    /// </summary>
    /// <param name="tests">The <see cref="IntegrationTests"/> instance.</param>
    /// <param name="apiKeyId">The API key ID to associate with the usage record.</param>
    /// <returns>A new <see cref="UsageRecord"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    public static UsageRecord CreateSampleUsageRecord(this IntegrationTests tests, string apiKeyId)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);

        return new UsageRecord
        {
            Id = Guid.NewGuid().ToString(),
            ApiKeyId = apiKeyId,
            Endpoint = "/api/test",
            Method = "GET",
            ResponseStatusCode = 200,
            ResponseTimeMs = 50,
            BytesTransferred = 1024
        };
    }

    /// <summary>
    /// Creates a sample <see cref="AuditLog"/> for testing purposes.
    /// </summary>
    /// <param name="tests">The <see cref="IntegrationTests"/> instance.</param>
    /// <param name="resourceId">The ID of the resource associated with the log.</param>
    /// <param name="resourceType">The type of the resource.</param>
    /// <param name="action">The <see cref="AuditAction"/> performed.</param>
    /// <returns>A new <see cref="AuditLog"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="resourceId"/> or <paramref name="resourceType"/> is null or empty.</exception>
    public static AuditLog CreateSampleAuditLog(this IntegrationTests tests, string resourceId, string resourceType, AuditAction action)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrEmpty(resourceId);
        ArgumentException.ThrowIfNullOrEmpty(resourceType);

        return new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = resourceId,
            ResourceType = resourceType,
            Action = action,
            IsSuccess = true
        };
    }

    /// <summary>
    /// Creates a sample <see cref="RateLimit"/> for testing purposes.
    /// </summary>
    /// <param name="tests">The <see cref="IntegrationTests"/> instance.</param>
    /// <param name="apiKeyId">The API key ID.</param>
    /// <param name="requestsPerUnit">The number of requests allowed per unit.</param>
    /// <returns>A new <see cref="RateLimit"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
    public static RateLimit CreateSampleRateLimit(this IntegrationTests tests, string apiKeyId, int requestsPerUnit)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrEmpty(apiKeyId);

        return new RateLimit
        {
            ApiKeyId = apiKeyId,
            RequestsPerUnit = requestsPerUnit,
            Unit = RateLimitUnit.Minute,
            CurrentRequestCount = 0,
            LastResetAt = DateTime.UtcNow
        };
    }
}
