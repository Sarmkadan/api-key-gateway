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
	/// <param name="consumerId">The consumer ID associated with the usage record. Defaults to "test-consumer".</param>
	/// <returns>A new <see cref="UsageRecord"/> instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> or <paramref name="consumerId"/> is null or empty.</exception>
	public static UsageRecord CreateSampleUsageRecord(this IntegrationTests tests, string apiKeyId, string consumerId = "test-consumer")
	{
		ArgumentNullException.ThrowIfNull(tests);
		ArgumentException.ThrowIfNullOrEmpty(apiKeyId);
		ArgumentException.ThrowIfNullOrEmpty(consumerId);

		return new UsageRecord
		{
			Id = Guid.NewGuid().ToString(),
			ApiKeyId = apiKeyId,
			ConsumerId = consumerId,
			Endpoint = "/api/test",
			Method = "GET",
			ResponseStatusCode = 200,
			ResponseTimeMs = 50,
			RequestBytes = 512,
			ResponseBytes = 1024,
			Tags = []
		};
	}

	/// <summary>
	/// Creates a sample <see cref="AuditLog"/> for testing purposes.
	/// </summary>
	/// <param name="tests">The <see cref="IntegrationTests"/> instance.</param>
	/// <param name="resourceId">The ID of the resource associated with the log.</param>
	/// <param name="resourceType">The type of the resource.</param>
	/// <param name="action">The <see cref="AuditAction"/> performed.</param>
	/// <param name="performedBy">The user or system that performed the action. Defaults to "system".</param>
	/// <returns>A new <see cref="AuditLog"/> instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="resourceId"/> or <paramref name="resourceType"/> is null or empty.</exception>
	public static AuditLog CreateSampleAuditLog(
		this IntegrationTests tests,
		string resourceId,
		string resourceType,
		AuditAction action,
		string performedBy = "system")
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
			PerformedBy = performedBy,
			IsSuccess = true
		};
	}

	/// <summary>
	/// Creates a sample <see cref="RateLimit"/> for testing purposes.
	/// </summary>
	/// <param name="tests">The <see cref="IntegrationTests"/> instance.</param>
	/// <param name="apiKeyId">The API key ID.</param>
	/// <param name="requestsPerUnit">The number of requests allowed per unit.</param>
	/// <param name="isEnabled">Whether the rate limit is enabled. Defaults to true.</param>
	/// <returns>A new <see cref="RateLimit"/> instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="apiKeyId"/> is null or empty.</exception>
	public static RateLimit CreateSampleRateLimit(
		this IntegrationTests tests,
		string apiKeyId,
		int requestsPerUnit,
		bool isEnabled = true)
	{
		ArgumentNullException.ThrowIfNull(tests);
		ArgumentException.ThrowIfNullOrEmpty(apiKeyId);

		return new RateLimit
		{
			Id = Guid.NewGuid().ToString(),
			ApiKeyId = apiKeyId,
			RequestsPerUnit = requestsPerUnit,
			Unit = RateLimitUnit.Minute,
			IsEnabled = isEnabled,
			CurrentRequestCount = 0,
			LastResetAt = DateTime.UtcNow,
			CreatedAt = DateTime.UtcNow
		};
	}
}
