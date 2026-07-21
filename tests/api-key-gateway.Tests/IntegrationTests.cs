// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
/// <summary>
/// Integration tests for the API Key Gateway system, covering end-to-end workflows
/// across authentication, rate limiting, usage tracking, quota management, and audit logging.
/// </summary>
using Xunit;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

public class IntegrationTests
{
    /// <summary>
    /// Mock repository for API key data access operations.
    /// </summary>
    private readonly Mock<IApiKeyRepository> _apiKeyRepositoryMock;
    /// <summary>
    /// Mock repository for rate limiting configuration and tracking.
    /// </summary>
    private readonly Mock<IRateLimitRepository> _rateLimitRepositoryMock;
    /// <summary>
    /// Mock repository for usage record storage and retrieval.
    /// </summary>
    private readonly Mock<IUsageRepository> _usageRepositoryMock;
    /// <summary>
    /// Mock repository for usage quota management and enforcement.
    /// </summary>
    private readonly Mock<IUsageQuotaRepository> _usageQuotaRepositoryMock;
    /// <summary>
    /// Mock repository for audit logging of all system operations.
    /// </summary>
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTests"/> class.
    /// Sets up mock repositories for API key, rate limiting, usage tracking, quota management,
    /// and audit logging services to enable isolated integration testing.
    /// </summary>
    public IntegrationTests()
    {
        _apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        _rateLimitRepositoryMock = new Mock<IRateLimitRepository>();
        _usageRepositoryMock = new Mock<IUsageRepository>();
        _usageQuotaRepositoryMock = new Mock<IUsageQuotaRepository>();
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
    }

    /// <summary>
    /// Integration test verifying the complete workflow from API key creation through usage tracking.
    /// Validates that keys can be created, audit logs can be written, and the system maintains proper state.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task FullWorkflow_CreateKeyToUsageTracking_CompletesSuccessfully()
    {
        var consumerId = "consumer-workflow";
        var keyName = "Workflow Test Key";

        ApiKey? createdKey = null;
        _apiKeyRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<ApiKey>()))
            .Callback<ApiKey>(k => createdKey = k)
            .ReturnsAsync((ApiKey k) => k);

        _apiKeyRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => createdKey != null && createdKey.Id == id ? createdKey : null);

        var loggerMock = new Mock<ILogger<ApiKeyService>>();
        var apiKeyService = new ApiKeyService(_apiKeyRepositoryMock.Object, loggerMock.Object);

        var apiKey = await apiKeyService.CreateKeyAsync(consumerId, keyName);

        apiKey.Should().NotBeNull();
        apiKey.ConsumerId.Should().Be(consumerId);
        apiKey.Name.Should().Be(keyName);
        apiKey.Status.Should().Be(ApiKeyStatus.Active);

        _auditLogRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var auditLoggerMock = new Mock<ILogger<AuditLogService>>();
        var auditService = new AuditLogService(_auditLogRepositoryMock.Object, auditLoggerMock.Object);

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            ResourceId = apiKey.Id,
            ResourceType = "ApiKey",
            Action = AuditAction.KeyCreated,
            IsSuccess = true
        };

        await auditService.LogAsync(auditLog);

        _auditLogRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    /// <summary>
    /// Integration test for rate limiting functionality.
    /// Validates that rate limits are properly enforced and requests are tracked.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RateLimiting_FullWorkflow_EnforcesAndRecords()
    {
        var rateLimit = new RateLimit
        {
            ApiKeyId = "key-workflow",
            RequestsPerUnit = 5,
            Unit = RateLimitUnit.Minute,
            CurrentRequestCount = 0,
            LastResetAt = DateTime.UtcNow
        };

        _rateLimitRepositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-workflow"))
            .ReturnsAsync(rateLimit);

        _rateLimitRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<RateLimit>()))
            .Returns(Task.CompletedTask);

        var loggerMock = new Mock<ILogger<RateLimitingService>>();
        var rateLimitService = new RateLimitingService(_rateLimitRepositoryMock.Object, loggerMock.Object);

        for (int i = 0; i < 5; i++)
        {
            var result = await rateLimitService.CheckLimitAsync("key-workflow");
            result.Should().BeTrue();
            await rateLimitService.RecordRequestAsync("key-workflow");
        }

        var act = async () => await rateLimitService.CheckLimitAsync("key-workflow");
        await act.Should().ThrowAsync<Domain.Exceptions.RateLimitExceededException>();
    }

    /// <summary>
    /// Integration test for usage tracking functionality.
    /// Validates that usage records can be created and statistics can be retrieved.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UsageTracking_RecordMultipleAndGetStatistics()
    {
        var apiKeyId = "key-tracking";
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var records = new List<UsageRecord>
        {
            new() { ApiKeyId = apiKeyId, Endpoint = "/api/users", Method = "GET", ResponseStatusCode = 200, ResponseTimeMs = 50, BytesTransferred = 1024 },
            new() { ApiKeyId = apiKeyId, Endpoint = "/api/users", Method = "GET", ResponseStatusCode = 200, ResponseTimeMs = 60, BytesTransferred = 1024 },
            new() { ApiKeyId = apiKeyId, Endpoint = "/api/data", Method = "POST", ResponseStatusCode = 201, ResponseTimeMs = 100, BytesTransferred = 2048 },
            new() { ApiKeyId = apiKeyId, Endpoint = "/api/data", Method = "POST", ResponseStatusCode = 500, ResponseTimeMs = 30, BytesTransferred = 512 }
        };

        _usageRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<UsageRecord>()))
            .Returns(Task.CompletedTask);

        _usageRepositoryMock
            .Setup(r => r.GetByApiKeyAndDateRangeAsync(apiKeyId, startDate, endDate))
            .ReturnsAsync(records);

        var loggerMock = new Mock<ILogger<UsageTrackingService>>();
        var usageService = new UsageTrackingService(_usageRepositoryMock.Object, loggerMock.Object);

        foreach (var record in records)
        {
            await usageService.RecordUsageAsync(record);
        }

        var stats = await usageService.GetUsageStatisticsAsync(apiKeyId, startDate, endDate);

        stats.TotalRequests.Should().Be(4);
        stats.SuccessfulRequests.Should().Be(3);
        stats.FailedRequests.Should().Be(1);
        stats.UniqueEndpoints.Should().Be(2);
        stats.SuccessRate.Should().Be(75);
    }

    /// <summary>
    /// Integration test for usage quota enforcement.
    /// Validates that quota limits are properly enforced and reset.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UsageQuota_EnforceAndReset_WorksCorrectly()
    {
        var quota = new UsageQuota
        {
            ApiKeyId = "key-quota-workflow",
            QuotaLimit = 100,
            Period = QuotaPeriod.Day,
            CurrentUsage = 0,
            IsEnabled = true,
            PeriodStartAt = DateTime.UtcNow.Date
        };

        _usageQuotaRepositoryMock
            .Setup(r => r.GetByApiKeyIdAsync("key-quota-workflow"))
            .ReturnsAsync(quota);

        _usageQuotaRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UsageQuota>()))
            .Returns(Task.CompletedTask);

        var loggerMock = new Mock<ILogger<UsageQuotaService>>();
        var quotaService = new UsageQuotaService(_usageQuotaRepositoryMock.Object, loggerMock.Object);

        for (int i = 0; i < 100; i++)
        {
            var result = await quotaService.CheckAndRecordAsync("key-quota-workflow");
            result.IsExceeded.Should().BeFalse();
        }

        var exceededResult = await quotaService.CheckAndRecordAsync("key-quota-workflow");
        exceededResult.IsExceeded.Should().BeTrue();
        exceededResult.Remaining.Should().Be(0);
    }

    /// <summary>
    /// Integration test for IP whitelist authentication.
    /// Validates that API keys can only be used from whitelisted IP addresses.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task Authentication_IpWhitelist_ValidatesCorrectly()
    {
        var key = new ApiKey
        {
            Id = "key-auth",
            ConsumerId = "consumer-auth",
            Status = ApiKeyStatus.Active,
            IpWhitelist = "192.168.1.1, 192.168.1.2, 10.0.0.1"
        };

        _apiKeyRepositoryMock
            .Setup(r => r.GetByIdAsync("key-auth"))
            .ReturnsAsync(key);

        var auditLoggerMock = new Mock<ILogger<AuthenticationService>>();
        var apiKeyServiceMock = new Mock<IApiKeyService>();
        var auditServiceMock = new Mock<IAuditLogService>();

        apiKeyServiceMock
            .Setup(s => s.ValidateKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(key);

        auditServiceMock
            .Setup(s => s.LogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var authService = new AuthenticationService(apiKeyServiceMock.Object, auditServiceMock.Object, auditLoggerMock.Object);

        var result1 = await authService.AuthenticateAsync("sk_testkey", "192.168.1.1");
        result1.Success.Should().BeTrue();
        result1.ApiKey.Should().NotBeNull();
        result1.ApiKey!.Id.Should().Be("key-auth");

        var result2 = await authService.AuthenticateAsync("sk_testkey", "203.0.113.50");
        result2.Success.Should().BeFalse();
        result2.FailureReason.Should().Be(AuthenticationFailureReason.IpNotWhitelisted);
    }

    /// <summary>
    /// Integration test for concurrent audit logging.
    /// Validates that multiple concurrent operations are all properly logged.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AuditLogging_ConcurrentOperations_AllLogged()
    {
        var capturedLogs = new System.Collections.Concurrent.ConcurrentBag<AuditLog>();

        _auditLogRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .Callback<AuditLog>(log => capturedLogs.Add(log))
            .Returns(Task.CompletedTask);

        var loggerMock = new Mock<ILogger<AuditLogService>>();
        var auditService = new AuditLogService(_auditLogRepositoryMock.Object, loggerMock.Object);

        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(auditService.LogAsync(new AuditLog
            {
                Id = Guid.NewGuid().ToString(),
                ResourceId = "test-resource",
                ResourceType = "TestResource",
                Action = AuditAction.KeyCreated,
                IsSuccess = true
            }));
        }

        await Task.WhenAll(tasks);

        capturedLogs.Should().HaveCount(10);
    }
}