// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    private readonly Mock<IApiKeyRepository> _apiKeyRepositoryMock;
    private readonly Mock<IRateLimitRepository> _rateLimitRepositoryMock;
    private readonly Mock<IUsageRepository> _usageRepositoryMock;
    private readonly Mock<IUsageQuotaRepository> _usageQuotaRepositoryMock;
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;

    public IntegrationTests()
    {
        _apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        _rateLimitRepositoryMock = new Mock<IRateLimitRepository>();
        _usageRepositoryMock = new Mock<IUsageRepository>();
        _usageQuotaRepositoryMock = new Mock<IUsageQuotaRepository>();
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
    }

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
        result1.Should().NotBeNull();
        result1.Id.Should().Be("key-auth");

        var act = async () => await authService.AuthenticateAsync("sk_testkey", "203.0.113.50");
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

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

        var tasks = Enumerable.Range(0, 50).Select(i =>
        {
            var log = new AuditLog
            {
                Id = $"log-{i}",
                ResourceId = $"key-{i}",
                ResourceType = "ApiKey",
                Action = AuditAction.KeyUsed,
                IsSuccess = true
            };
            return auditService.LogAsync(log);
        });

        await Task.WhenAll(tasks);

        capturedLogs.Should().HaveCount(50);
        _auditLogRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<AuditLog>()), Times.Exactly(50));
    }

    [Fact]
    public async Task CompleteFlow_CreateKeyAuthenticateAndTrack_Works()
    {
        var consumerId = "consumer-complete";
        var keyName = "Complete Flow Key";
        ApiKey? createdKey = null;

        _apiKeyRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<ApiKey>()))
            .Callback<ApiKey>(k => createdKey = k)
            .ReturnsAsync((ApiKey k) => k);

        _apiKeyRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => createdKey != null && createdKey.Id == id ? createdKey : null);

        var apiKeyLoggerMock = new Mock<ILogger<ApiKeyService>>();
        var apiKeyService = new ApiKeyService(_apiKeyRepositoryMock.Object, apiKeyLoggerMock.Object);

        var apiKey = await apiKeyService.CreateKeyAsync(consumerId, keyName);
        apiKey.Should().NotBeNull();
        apiKey.Status.Should().Be(ApiKeyStatus.Active);

        _usageRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<UsageRecord>()))
            .Returns(Task.CompletedTask);

        var usageLoggerMock = new Mock<ILogger<UsageTrackingService>>();
        var usageService = new UsageTrackingService(_usageRepositoryMock.Object, usageLoggerMock.Object);

        var usageRecord = new UsageRecord
        {
            Id = Guid.NewGuid().ToString(),
            ApiKeyId = apiKey.Id,
            Endpoint = "/api/test",
            Method = "GET",
            ResponseStatusCode = 200,
            ResponseTimeMs = 45,
            BytesTransferred = 1024
        };

        await usageService.RecordUsageAsync(usageRecord);

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
            Action = AuditAction.KeyUsed,
            IsSuccess = true
        };

        await auditService.LogAsync(auditLog);

        _usageRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<UsageRecord>()), Times.Once);
        _auditLogRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<AuditLog>()), Times.Once);
    }
}
