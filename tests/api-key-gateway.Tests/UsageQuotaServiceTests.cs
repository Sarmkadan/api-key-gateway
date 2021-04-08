// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Unit tests for <see cref="UsageQuotaService"/> that verify quota checking, recording, and management functionality.
/// </summary>
public class UsageQuotaServiceTests
{
	private readonly Mock<IUsageQuotaRepository> _repositoryMock;
	private readonly Mock<ILogger<UsageQuotaService>> _loggerMock;
	private readonly UsageQuotaService _sut;

	/// <summary>
	/// Initializes a new instance of the <see cref="UsageQuotaServiceTests"/> class.
	/// </summary>
	public UsageQuotaServiceTests()
	{
		_repositoryMock = new Mock<IUsageQuotaRepository>();
		_loggerMock = new Mock<ILogger<UsageQuotaService>>();
		_sut = new UsageQuotaService(_repositoryMock.Object, _loggerMock.Object);
	}

	[Fact]
	/// <summary>
	/// Verifies that the constructor throws ArgumentNullException when repository is null.
	/// </summary>
	public void Constructor_NullRepository_ThrowsArgumentNullException()
	{
		var act = () => new UsageQuotaService(null!, _loggerMock.Object);
		act.Should().Throw<ArgumentNullException>().WithParameterName("repository");
	}

	[Fact]
	/// <summary>
	/// Verifies that the constructor throws ArgumentNullException when logger is null.
	/// </summary>
	public void Constructor_NullLogger_ThrowsArgumentNullException()
	{
		var act = () => new UsageQuotaService(_repositoryMock.Object, null!);
		act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
	}

	[Theory]
	[InlineData("")]
	[InlineData(null)]
	[InlineData(" ")]
	/// <summary>
	/// Tests that empty, null, or whitespace API key IDs return unlimited quota without repository interaction.
	/// </summary>
	/// <param name="apiKeyId">The API key ID to test with (empty, null, or whitespace).</param>
	public async Task CheckAndRecordAsync_EmptyOrNullKeyId_ReturnsUnlimitedQuota(string? apiKeyId)
	{
		var result = await _sut.CheckAndRecordAsync(apiKeyId!);

		result.IsExceeded.Should().BeFalse();
		result.Remaining.Should().Be(long.MaxValue);
		result.Limit.Should().Be(long.MaxValue);
		_repositoryMock.Verify(r => r.GetByApiKeyIdAsync(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	/// <summary>
	/// Confirms that when no quota exists for an API key, unlimited quota is returned.
	/// </summary>
	public async Task CheckAndRecordAsync_NoQuotaConfigured_ReturnsUnlimitedQuota()
	{
		_repositoryMock
			.Setup(r => r.GetByApiKeyIdAsync("key-no-quota"))
			.ReturnsAsync((UsageQuota?)null);

		var result = await _sut.CheckAndRecordAsync("key-no-quota");

		result.IsExceeded.Should().BeFalse();
		result.Remaining.Should().Be(long.MaxValue);
		result.Limit.Should().Be(long.MaxValue);
	}

	[Fact]
	/// <summary>
	/// Ensures disabled quotas (IsEnabled=false) return unlimited quota regardless of current usage.
	/// </summary>
	public async Task CheckAndRecordAsync_QuotaDisabled_ReturnsUnlimitedQuota()
	{
		var quota = new UsageQuota
		{
			ApiKeyId = "key-disabled",
			QuotaLimit = 1000,
			IsEnabled = false,
			CurrentUsage = 500
		};

		_repositoryMock
			.Setup(r => r.GetByApiKeyIdAsync("key-disabled"))
			.ReturnsAsync(quota);

		var result = await _sut.CheckAndRecordAsync("key-disabled");

		result.IsExceeded.Should().BeFalse();
		result.Remaining.Should().Be(long.MaxValue);
	}

	[Fact]
	/// <summary>
	/// Validates that below-quota usage increments the counter and returns correct remaining quota.
	/// </summary>
	public async Task CheckAndRecordAsync_BelowQuota_IncrementsUsageAndReturnsRemaining()
	{
		var quota = new UsageQuota
		{
			ApiKeyId = "key-below",
			QuotaLimit = 1000,
			Period = QuotaPeriod.Day,
			CurrentUsage = 500,
			IsEnabled = true,
			PeriodStartAt = DateTime.UtcNow.Date
		};

		_repositoryMock
			.Setup(r => r.GetByApiKeyIdAsync("key-below"))
			.ReturnsAsync(quota);
		_repositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
			.Returns(Task.CompletedTask);

		var result = await _sut.CheckAndRecordAsync("key-below");

		result.IsExceeded.Should().BeFalse();
		result.Limit.Should().Be(1000);
		result.Remaining.Should().Be(499);
		quota.CurrentUsage.Should().Be(501);
		_repositoryMock.Verify(r => r.UpdateAsync(quota), Times.Once);
	}

	[Fact]
	/// <summary>
	/// Tests that at-quota usage returns IsExceeded=true with zero remaining quota.
	/// </summary>
	public async Task CheckAndRecordAsync_AtQuota_ReturnsExceededWithNoRemaining()
	{
		var quota = new UsageQuota
		{
			ApiKeyId = "key-at-limit",
			QuotaLimit = 100,
			Period = QuotaPeriod.Hour,
			CurrentUsage = 100,
			IsEnabled = true,
			// Start of the current calendar hour: still inside the active window.
			PeriodStartAt = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Hour)
		};

		_repositoryMock
			.Setup(r => r.GetByApiKeyIdAsync("key-at-limit"))
			.ReturnsAsync(quota);

		var result = await _sut.CheckAndRecordAsync("key-at-limit");

		result.IsExceeded.Should().BeTrue();
		result.Remaining.Should().Be(0);
		result.Limit.Should().Be(100);
		_repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UsageQuota>()), Times.Never);
	}

	[Fact]
	/// <summary>
	/// Verifies period rollover resets the usage counter before recording new usage.
	/// </summary>
	public async Task CheckAndRecordAsync_PeriodRolledOver_ResetsCounterBeforeRecording()
	{
		var quota = new UsageQuota
		{
			ApiKeyId = "key-rollover",
			QuotaLimit = 500,
			Period = QuotaPeriod.Day,
			CurrentUsage = 450,
			IsEnabled = true,
			PeriodStartAt = DateTime.UtcNow.AddDays(-2)
		};

		_repositoryMock
			.Setup(r => r.GetByApiKeyIdAsync("key-rollover"))
			.ReturnsAsync(quota);
		_repositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
			.Returns(Task.CompletedTask);

		var result = await _sut.CheckAndRecordAsync("key-rollover");

		result.IsExceeded.Should().BeFalse();
		result.Remaining.Should().Be(499);
		quota.CurrentUsage.Should().Be(1);
		_repositoryMock.Verify(r => r.UpdateAsync(quota), Times.Once);
	}

	[Fact]
	/// <summary>
	/// Confirms that existing quotas are returned by GetQuotaAsync.
	/// </summary>
	public async Task GetQuotaAsync_KeyExists_ReturnsQuota()
	{
		var quota = new UsageQuota
		{
			ApiKeyId = "key-get",
			QuotaLimit = 1000,
			Period = QuotaPeriod.Month
		};

		_repositoryMock
			.Setup(r => r.GetByApiKeyIdAsync("key-get"))
			.ReturnsAsync(quota);

		var result = await _sut.GetQuotaAsync("key-get");

		result.Should().Be(quota);
	}

	[Fact]
	/// <summary>
	/// Ensures GetQuotaAsync returns null for non-existent API keys.
	/// </summary>
	public async Task GetQuotaAsync_KeyDoesNotExist_ReturnsNull()
	{
		_repositoryMock
			.Setup(r => r.GetByApiKeyIdAsync("key-missing"))
			.ReturnsAsync((UsageQuota?)null);

		var result = await _sut.GetQuotaAsync("key-missing");

		result.Should().BeNull();
	}

	[Theory]
	[InlineData("")]
	[InlineData(null)]
	/// <summary>
	/// Tests that empty/null API key IDs cause SetQuotaAsync to return false without creating quota.
	/// </summary>
	/// <param name="apiKeyId">The API key ID to test with (empty or null).</param>
	public async Task SetQuotaAsync_EmptyOrNullKeyId_ReturnsFalse(string? apiKeyId)
	{
		var result = await _sut.SetQuotaAsync(apiKeyId!, 1000, QuotaPeriod.Day);
		result.Should().BeFalse();
		_repositoryMock.Verify(r => r.CreateAsync(It.IsAny<UsageQuota>()), Times.Never);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(-100)]
	/// <summary>
	/// Validates that invalid (zero/negative) quota limits cause SetQuotaAsync to return false.
	/// </summary>
	/// <param name="quotaLimit">The invalid quota limit to test.</param>
	public async Task SetQuotaAsync_InvalidQuotaLimit_ReturnsFalse(long quotaLimit)
	{
		var result = await _sut.SetQuotaAsync("key-set", quotaLimit, QuotaPeriod.Day);
		result.Should().BeFalse();
	}

	[Fact]
	/// <summary>
	/// Confirms SetQuotaAsync creates new quota and returns true for valid parameters.
	/// </summary>
	public async Task SetQuotaAsync_NewQuota_CreatesAndReturnsTrue()
	{
		_repositoryMock
			.Setup(r => r.GetByApiKeyIdAsync("key-new-quota"))
			.ReturnsAsync((UsageQuota?)null);
		_repositoryMock
			.Setup(r => r.CreateAsync(It.IsAny<UsageQuota>()))
			.ReturnsAsync((UsageQuota q) => q);

		var result = await _sut.SetQuotaAsync("key-new-quota", 5000, QuotaPeriod.Month);

		result.Should().BeTrue();
		_repositoryMock.Verify(r => r.CreateAsync(It.Is<UsageQuota>(
			q => q.ApiKeyId == "key-new-quota" && q.QuotaLimit == 5000 && q.Period == QuotaPeriod.Month
		)), Times.Once);
	}

	[Fact]
	/// <summary>
	/// Tests that SetQuotaAsync updates existing quota while preserving Id, CurrentUsage, and CreatedAt.
	/// </summary>
	public async Task SetQuotaAsync_ExistingQuota_UpdatesAndReturnsTrue()
	{
		var existingQuota = new UsageQuota
		{
			Id = "quota-123",
			ApiKeyId = "key-existing",
			QuotaLimit = 1000,
			Period = QuotaPeriod.Day,
			CurrentUsage = 250,
			IsEnabled = true,
			CreatedAt = DateTime.UtcNow.AddDays(-10)
		};

		_repositoryMock
			.Setup(r => r.GetByApiKeyIdAsync("key-existing"))
			.ReturnsAsync(existingQuota);
		_repositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
			.Returns(Task.CompletedTask);

		var result = await _sut.SetQuotaAsync("key-existing", 2000, QuotaPeriod.Week);

		result.Should().BeTrue();
		_repositoryMock.Verify(r => r.UpdateAsync(It.Is<UsageQuota>(
			q => q.Id == "quota-123" &&
			q.QuotaLimit == 2000 &&
			q.Period == QuotaPeriod.Week &&
			q.CurrentUsage == 250 &&
			q.CreatedAt == existingQuota.CreatedAt
		)), Times.Once);
	}

	[Fact]
	/// <summary>
	/// Validates concurrent CheckAndRecordAsync calls correctly increment usage without race conditions.
	/// </summary>
	public async Task CheckAndRecordAsync_ConcurrentCalls_AllIncrementCountCorrectly()
	{
		var quota = new UsageQuota
		{
			ApiKeyId = "key-concurrent",
			QuotaLimit = 10000,
			Period = QuotaPeriod.Day,
			CurrentUsage = 0,
			IsEnabled = true,
			PeriodStartAt = DateTime.UtcNow.Date
		};

		_repositoryMock
			.Setup(r => r.GetByApiKeyIdAsync("key-concurrent"))
			.ReturnsAsync(quota);
		_repositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
			.Returns(Task.CompletedTask);

		var tasks = Enumerable.Range(0, 100).Select(_ =>
			_sut.CheckAndRecordAsync("key-concurrent")
		);

		var results = await Task.WhenAll(tasks);

		results.Should().AllSatisfy(r => r.IsExceeded.Should().BeFalse());
		_repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UsageQuota>()), Times.Exactly(100));
	}

	[Fact]
	/// <summary>
	/// Ensures exceeded quotas do not trigger repository updates.
	/// </summary>
	public async Task CheckAndRecordAsync_ExceededQuota_DoesNotUpdate()
	{
		var quota = new UsageQuota
		{
			ApiKeyId = "key-exceeded",
			QuotaLimit = 100,
			Period = QuotaPeriod.Hour,
			CurrentUsage = 100,
			IsEnabled = true,
			// Start of the current calendar hour: still inside the active window.
			PeriodStartAt = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Hour)
		};

		_repositoryMock
			.Setup(r => r.GetByApiKeyIdAsync("key-exceeded"))
			.ReturnsAsync(quota);

		var result = await _sut.CheckAndRecordAsync("key-exceeded");

		result.IsExceeded.Should().BeTrue();
		_repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UsageQuota>()), Times.Never);
	}
}
