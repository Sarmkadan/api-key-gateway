// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

## ApiKeyRepository

The `ApiKeyRepository` class is a concrete implementation of the `IApiKeyRepository` interface, providing data access and persistence for API keys using an ADO.NET connection with an in-memory write-through cache. It supports CRUD operations on API keys, including creating, retrieving, updating, and deleting keys.

### Example Usage

```csharp
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Data;
using ApiKeyGateway.Domain.Models;

// Create a new API key repository instance
var connection = new SqlServerConnection("Server=localhost;Database=api_key_gateway;User Id=sa;Password=your_password;");
var logger = new Logger<ApiKeyRepository>(new LoggerFactory());
var repository = new ApiKeyRepository(connection, logger);

// Create a new API key
var newApiKey = new ApiKey
{
    Id = "key_001",
    ConsumerId = "consumer_001",
    Name = "Test API Key",
    KeyHash = "hashed_value_here",
    Prefix = "test_abc",
    Status = ApiKeyStatus.Active,
    CreatedAt = DateTime.UtcNow,
    ExpiresAt = DateTime.UtcNow.AddYears(1),
    Description = "Test API key for development"
};

var createdKey = await repository.CreateAsync(newApiKey);
Console.WriteLine($"Created API key: {createdKey.Id}");

// Retrieve an API key by ID
var retrievedKey = await repository.GetByIdAsync("key_001");
if (retrievedKey != null)
{
    Console.WriteLine($"Retrieved key: {retrievedKey.Name}");
}

// Update the API key
retrievedKey.Name = "Updated Test API Key";
await repository.UpdateAsync(retrievedKey);

// Delete the API key
await repository.DeleteAsync("key_001");
```

## IAuditLogRepository

The `IAuditLogRepository` interface defines methods for creating, querying, and managing audit log entries in the system. It supports creating logs for resource actions, retrieving logs by resource ID or date range, and purging old logs.

### Example Usage

```csharp
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Data;
using ApiKeyGateway.Domain.Models;

// Create an audit log repository instance
var connection = new SqlServerConnection("Server=localhost;Database=api_key_gateway;User Id=sa;Password=your_password;");
var logger = new Logger<AuditLogRepository>(new LoggerFactory());
var auditRepo = new AuditLogRepository(connection, logger);

// Create a new audit log entry
var auditLog = new AuditLog
{
    Id = "audit_001",
    ResourceId = "key_001",
    ResourceType = "ApiKey",
    Action = Domain.Enums.AuditAction.KeyCreated,
    PerformedBy = "admin_user",
    PerformedAt = DateTime.UtcNow,
    HttpStatusCode = 201,
    SourceIp = "192.168.1.1",
    Reason = "Initial creation",
    IsSuccess = true
};

await auditRepo.CreateAsync(auditLog);
Console.WriteLine("Audit log created for API key creation");

// Retrieve logs for a specific resource
var logs = await auditRepo.GetByResourceIdAsync("key_001", limit: 50);
foreach (var log in logs)
{
    Console.WriteLine($"Action: {log.GetActionDescription()} at {log.PerformedAt}");
}

// Retrieve logs for a date range
var recentLogs = await auditRepo.GetByDateRangeAsync(
    startDate: DateTime.UtcNow.AddDays(-7),
    endDate: DateTime.UtcNow);

// Delete logs older than 30 days
var deletedCount = await auditRepo.DeleteOlderThanAsync(DateTime.UtcNow.AddDays(-30));
Console.WriteLine($"Deleted {deletedCount} old audit logs");
```

## RateLimitRepository

The `RateLimitRepository` class provides data access and persistence for rate limit configurations. It supports CRUD operations on rate limits.

### Example Usage

```csharp
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Data;
using ApiKeyGateway.Domain.Models;

// Create a rate limit repository instance
var connection = new SqlServerConnection("Server=localhost;Database=api_key_gateway;User Id=sa;Password=your_password;");
var logger = new Logger<RateLimitRepository>(new LoggerFactory());
var rateLimitRepo = new RateLimitRepository(connection, logger);

// Create a new rate limit
var newRateLimit = new RateLimit
{
    Id = "rl_001",
    ApiKeyId = "key_001",
    RequestsPerUnit = 100,
    Unit = Domain.Enums.RateLimitUnit.Minute,
    IsEnabled = true,
    CreatedAt = DateTime.UtcNow,
    LastResetAt = DateTime.UtcNow,
    CurrentRequestCount = 0
};

var createdRateLimit = await rateLimitRepo.CreateAsync(newRateLimit);
Console.WriteLine($"Created rate limit: {createdRateLimit.Id}");

// Retrieve a rate limit by API key ID
var retrievedRateLimit = await rateLimitRepo.GetByApiKeyIdAsync("key_001");
if (retrievedRateLimit != null)
{
    Console.WriteLine($"Retrieved rate limit: {retrievedRateLimit.RequestsPerUnit} requests per {retrievedRateLimit.Unit}");
}

// Update the rate limit
retrievedRateLimit.RequestsPerUnit = 200;
await rateLimitRepo.UpdateAsync(retrievedRateLimit);

// Delete the rate limit
await rateLimitRepo.DeleteAsync("rl_001");
```

## UsageQuotaRepository

The `UsageQuotaRepository` class provides data access and persistence for usage quotas. It supports CRUD operations on usage quotas.

### Example Usage

```csharp
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Data;
using ApiKeyGateway.Domain.Models;

// Create a usage quota repository instance
var connection = new SqlServerConnection("Server=localhost;Database=api_key_gateway;User Id=sa;Password=your_password;");
var logger = new Logger<UsageQuotaRepository>(new LoggerFactory());
var usageQuotaRepo = new UsageQuotaRepository(connection, logger);

// Create a new usage quota
var newUsageQuota = new UsageQuota
{
    Id = "quota_001",
    ApiKeyId = "key_001",
    QuotaLimit = 1000,
    IsEnabled = true,
    Period = Domain.Enums.QuotaPeriod.Day,
    CreatedAt = DateTime.UtcNow,
    PeriodStartAt = DateTime.UtcNow,
    CurrentUsage = 0
};

var createdUsageQuota = await usageQuotaRepo.CreateAsync(newUsageQuota);
Console.WriteLine($"Created usage quota: {createdUsageQuota.Id}");

// Retrieve a usage quota by API key ID
var retrievedUsageQuota = await usageQuotaRepo.GetByApiKeyIdAsync("key_001");
if (retrievedUsageQuota != null)
{
    Console.WriteLine($"Retrieved usage quota: {retrievedUsageQuota.QuotaLimit} for {retrievedUsageQuota.Period}");
}

// Update the usage quota
retrievedUsageQuota.CurrentUsage = 500;
await usageQuotaRepo.UpdateAsync(retrievedUsageQuota);

// Delete the usage quota
await usageQuotaRepo.DeleteAsync("quota_001");
```

## DatabaseTransformationRuleRepository

`DatabaseTransformationRuleRepository` is a concrete repository that stores and manages `TransformationRule` entities in a relational database. It provides methods to query rules by API key, consumer, or globally, as well as to create, update, and soft‑delete rules.

### Example Usage

```csharp
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Data;
using ApiKeyGateway.Domain.Models;
using Microsoft.Extensions.Logging;

// Set up dependencies
var connection = new SqlServerConnection("Server=localhost;Database=api_key_gateway;User Id=sa;Password=your_password;");
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<DatabaseTransformationRuleRepository>();

// Create the repository instance
var ruleRepo = new DatabaseTransformationRuleRepository(connection, logger);

// Retrieve rules for a specific API key
var apiKeyRules = await ruleRepo.GetByApiKeyAsync("key_001");

// Retrieve rules for a specific consumer
var consumerRules = await ruleRepo.GetByConsumerAsync("consumer_001");

// Retrieve all global rules
var globalRules = await ruleRepo.GetGlobalRulesAsync();

// Create a new transformation rule
var newRule = new TransformationRule
{
    Name = "AddCustomHeader",
    Description = "Adds a custom header to responses",
    Scope = TransformationScope.ApiKey,
    ApiKeyId = "key_001",
    ConsumerId = null,
    Type = TransformationRuleType.BuiltIn,
    Action = BuiltInAction.AddHeader,
    LuaScript = null,
    Parameters = new Dictionary<string, string>
    {
        ["HeaderName"] = "X-Custom-Header",
        ["HeaderValue"] = "MyValue"
    },
    Priority = 10,
    CreatedBy = "admin"
};

var newRuleId = await ruleRepo.CreateAsync(newRule);
Console.WriteLine($"Created rule with Id: {newRuleId}");

// Update the rule
newRule.Description = "Updated description";
var updated = await ruleRepo.UpdateAsync(newRule);
Console.WriteLine($"Rule updated: {updated}");

// Delete (soft‑delete) the rule
var deleted = await ruleRepo.DeleteAsync(newRuleId);
Console.WriteLine($"Rule deleted: {deleted}");
```

## UsageRepository

The `UsageRepository` class provides data access and persistence for API usage tracking records. It supports creating usage records, querying usage data by API key, consumer, or date range, and cleaning up old records based on retention policies.

### Example Usage

```csharp
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Data;
using ApiKeyGateway.Domain.Models;

// Create a usage repository instance
var connection = new SqlServerConnection("Server=localhost;Database=api_key_gateway;User Id=sa;Password=your_password;");
var logger = new Logger<UsageRepository>(new LoggerFactory());
var usageRepo = new UsageRepository(connection, logger);

// Create a new usage record
var newUsageRecord = new UsageRecord
{
    Id = Guid.NewGuid().ToString(),
    ApiKeyId = "key_001",
    ConsumerId = "consumer_001",
    RecordedAt = DateTime.UtcNow,
    Endpoint = "/api/users",
    Method = "GET",
    ResponseStatusCode = 200,
    RequestBytes = 1250,
    ResponseBytes = 4096,
    ResponseTimeMs = 42,
    SourceIp = "192.168.1.100"
};

await usageRepo.CreateAsync(newUsageRecord);
Console.WriteLine("Usage record created successfully");

// Retrieve usage records for an API key within a date range
var apiKeyUsage = await usageRepo.GetByApiKeyAndDateRangeAsync(
    apiKeyId: "key_001",
    startDate: DateTime.UtcNow.AddDays(-7),
    endDate: DateTime.UtcNow
);
Console.WriteLine($"Found {apiKeyUsage.Count} usage records for API key");

// Retrieve usage records for a consumer within a date range
var consumerUsage = await usageRepo.GetByConsumerAndDateRangeAsync(
    consumerId: "consumer_001",
    startDate: DateTime.UtcNow.AddDays(-7),
    endDate: DateTime.UtcNow
);
Console.WriteLine($"Found {consumerUsage.Count} usage records for consumer");

// Retrieve all usage records within a date range
var allUsage = await usageRepo.GetUsageAsync(
    startDate: DateTime.UtcNow.AddDays(-7),
    endDate: DateTime.UtcNow
);
Console.WriteLine($"Found {allUsage.Count} total usage records");

// Delete usage records older than 30 days
var deletedCount = await usageRepo.DeleteOldRecordsAsync(30);
Console.WriteLine($"Deleted {deletedCount} old usage records");
```

## ICacheProvider

The `ICacheProvider` interface defines an abstraction for cache operations, enabling different caching backends (in-memory, Redis, Memcached) to be used interchangeably. It provides asynchronous methods for common cache operations including get, set, remove, existence checks, atomic increments, and pattern-based removal. This abstraction is critical for supporting both single-instance and distributed deployments without changing calling code.

### Example Usage

```csharp
using ApiKeyGateway.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

// Set up dependencies
var memoryCache = new MemoryCache(new MemoryCacheOptions());
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<InMemoryCacheProvider>();

// Create the cache provider instance
var cacheProvider = new InMemoryCacheProvider(memoryCache, logger);

// Store a value in cache with 5-minute expiration
await cacheProvider.SetAsync("api_key:key_001:metadata", new
{
    Id = "key_001",
    ConsumerId = "consumer_001",
    Name = "Production Key",
    Status = "Active",
    CreatedAt = DateTime.UtcNow
}, expiration: TimeSpan.FromMinutes(5));

// Retrieve a value from cache
var cachedValue = await cacheProvider.GetAsync<ApiKeyMetadata>("api_key:key_001:metadata");
if (cachedValue != null)
{
    Console.WriteLine($"Retrieved cached metadata for key: {cachedValue.Name}");
}

// Check if a key exists in cache
var exists = await cacheProvider.ExistsAsync("api_key:key_001:metadata");
Console.WriteLine($"Cache contains key: {exists}");

// Atomically increment a counter for rate limiting
var requestCount = await cacheProvider.IncrementAsync("rate_limit:key_001:192.168.1.100", increment: 1);
Console.WriteLine($"Current request count: {requestCount}");

// Remove a specific key from cache
await cacheProvider.RemoveAsync("api_key:key_001:metadata");

// Remove all keys matching a pattern (e.g., all keys for a specific consumer)
var removedCount = await cacheProvider.RemoveByPatternAsync("api_key:consumer_001:*");
Console.WriteLine($"Removed {removedCount} matching cache entries");
```
