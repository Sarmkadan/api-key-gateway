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

## RequestValidator

The `RequestValidator` class provides reusable validation methods for common request parameters such as email addresses, URLs, IP addresses, string lengths, numeric ranges, and GUIDs. These static methods help prevent code duplication across controllers and keep validation logic centralized for easy maintenance and consistency.

### Example Usage

```csharp
using ApiKeyGateway.Validation;

// Validate an email address format
var emailResult = RequestValidator.ValidateEmail("user@example.com");
if (!emailResult.IsValid)
{
    Console.WriteLine($"Email validation failed: {emailResult.Message}");
}

// Validate a URL with HTTPS requirement
var urlResult = RequestValidator.ValidateUrl("https://api.example.com/v1/users");
if (!urlResult.IsValid)
{
    Console.WriteLine($"URL validation failed: {urlResult.Message}");
}

// Validate an IP address
var ipResult = RequestValidator.ValidateIpAddress("192.168.1.100");
if (!ipResult.IsValid)
{
    Console.WriteLine($"IP address validation failed: {ipResult.Message}");
}

// Validate string length (minimum 3, maximum 100 characters)
var lengthResult = RequestValidator.ValidateLength("Production API Key", minLength: 3, maxLength: 100, fieldName: "API Key Name");
if (!lengthResult.IsValid)
{
    Console.WriteLine($"Length validation failed: {lengthResult.Message}");
}

// Validate a numeric range (between 1 and 1000)
var rangeResult = RequestValidator.ValidateRange(500, minimum: 1, maximum: 1000, fieldName: "Request Limit");
if (!rangeResult.IsValid)
{
    Console.WriteLine($"Range validation failed: {rangeResult.Message}");
}

// Validate a GUID
var guidResult = RequestValidator.ValidateGuid(Guid.NewGuid(), fieldName: "User ID");
if (!guidResult.IsValid)
{
    Console.WriteLine($"GUID validation failed: {guidResult.Message}");
}
```

## ResponseFormatterExtensions

The `ResponseFormatterExtensions` class provides extension methods for formatting HTTP responses with a consistent structure across the API. It includes methods for creating successful responses, error responses, and paginated responses, all following a standard envelope pattern with data, metadata, and error information.

### Example Usage

```csharp
using ApiKeyGateway.Utilities;
using System.Net;

// Create a successful response with data
var userData = new { Id = 1, Name = "John Doe", Email = "john@example.com" };
var successResponse = ResponseFormatterExtensions.Success(userData, "User retrieved successfully");

Console.WriteLine($"Success: {successResponse.Success}");
Console.WriteLine($"Status: {successResponse.StatusCode}");
Console.WriteLine($"Message: {successResponse.Message}");
Console.WriteLine($"Data: {successResponse.Data?.Name}");
Console.WriteLine($"Timestamp: {successResponse.Timestamp}");

// Create an error response for validation failure
var validationError = ResponseFormatterExtensions.Error<ApiResponse<object>>(
    (int)HttpStatusCode.BadRequest,
    "Validation failed: email is required",
    "VALIDATION_ERROR",
    new { Field = "email", Error = "Required field" }
);

Console.WriteLine($"Error Success: {validationError.Success}");
Console.WriteLine($"Error Status: {validationError.StatusCode}");
Console.WriteLine($"Error Message: {validationError.Message}");
Console.WriteLine($"Error Code: {validationError.ErrorCode}");
Console.WriteLine($"Error Details: {validationError.Details}");

// Create a paginated response for list endpoints
var allItems = Enumerable.Range(1, 100).Select(i => new { Id = i, Name = $"Item {i}" });
var paginatedResponse = ResponseFormatterExtensions.Paginated(
    allItems.Skip(20).Take(10),  // Items for page 3
    pageNumber: 3,
    pageSize: 10,
    totalCount: 100
);

Console.WriteLine($"Paginated Items: {paginatedResponse.Items.Count}");
Console.WriteLine($"Page: {paginatedResponse.PageNumber}");
Console.WriteLine($"Page Size: {paginatedResponse.PageSize}");
Console.WriteLine($"Total Count: {paginatedResponse.TotalCount}");
Console.WriteLine($"Total Pages: {paginatedResponse.TotalPages}");
Console.WriteLine($"Has More: {paginatedResponse.HasMore}");
Console.WriteLine($"Timestamp: {paginatedResponse.Timestamp}");
```

## CacheKeyGenerator

The `CacheKeyGenerator` class provides a centralized way to generate consistent cache keys across the entire API Key Gateway application. By using a single source for key generation, the system prevents cache miss issues caused by inconsistent naming conventions and makes it easy to change key structure globally. All cache keys follow a consistent `apigw:<type>:<identifier>` format with appropriate separators for different cache entry types.

### Example Usage

```csharp
using ApiKeyGateway.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

// Create cache provider instance
var memoryCache = new MemoryCache(new MemoryCacheOptions());
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<InMemoryCacheProvider>();
var cacheProvider = new InMemoryCacheProvider(memoryCache, logger);

// Generate and use cache keys for API key operations
string apiKeyId = "key_abc123";
string endpoint = "/api/v1/users";
DateTime today = DateTime.UtcNow.Date;

// Store API key entity
string apiKeyCacheKey = CacheKeyGenerator.GetApiKeyKey(apiKeyId);
await cacheProvider.SetAsync(apiKeyCacheKey, new
{
    Id = apiKeyId,
    ConsumerId = "consumer_xyz789",
    Name = "Production API Key",
    Status = "Active",
    CreatedAt = DateTime.UtcNow
}, expiration: TimeSpan.FromMinutes(5));

// Store API key metadata separately for granular invalidation
string metadataCacheKey = CacheKeyGenerator.GetApiKeyMetadataKey(apiKeyId);
await cacheProvider.SetAsync(metadataCacheKey, new
{
    Permissions = new[] { "read", "write" },
    RateLimit = 1000,
    Quota = 50000
}, expiration: TimeSpan.FromHours(1));

// Store rate limit tracking for a specific endpoint
string rateLimitCacheKey = CacheKeyGenerator.GetRateLimitKey(apiKeyId, endpoint);
await cacheProvider.SetAsync(rateLimitCacheKey, new
{
    CurrentCount = 42,
    WindowStart = DateTime.UtcNow,
    Limit = 1000
}, expiration: TimeSpan.FromMinutes(1));

// Store usage statistics for reporting
string usageStatsCacheKey = CacheKeyGenerator.GetUsageStatsKey(apiKeyId, today);
await cacheProvider.SetAsync(usageStatsCacheKey, new
{
    RequestCount = 1500,
    ResponseBytes = 2_500_000,
    ResponseTimeMs = 12500
}, expiration: TimeSpan.FromDays(1));

// Store quota information
string quotaCacheKey = CacheKeyGenerator.GetQuotaKey(apiKeyId);
await cacheProvider.SetAsync(quotaCacheKey, new
{
    Limit = 50000,
    CurrentUsage = 12500,
    Period = "Daily"
}, expiration: TimeSpan.FromDays(1));

// Store webhook delivery status to prevent duplicates
string webhookCacheKey = CacheKeyGenerator.GetWebhookDeliveryKey(Guid.NewGuid());
await cacheProvider.SetAsync(webhookCacheKey, new
{
    Status = "Delivered",
    Retries = 0,
    LastAttempt = DateTime.UtcNow
}, expiration: TimeSpan.FromDays(7));

// Store external API response cache
var externalParams = new Dictionary<string, string>
{
    ["param1"] = "value1",
    ["param2"] = "value2"
};
string externalApiCacheKey = CacheKeyGenerator.GetExternalApiCacheKey(
    "stripe",
    "/v1/customers",
    externalParams
);
await cacheProvider.SetAsync(externalApiCacheKey, stripeCustomerData, expiration: TimeSpan.FromMinutes(30));

// Invalidate all cache entries for a specific API key
string invalidationPattern = CacheKeyGenerator.GetApiKeyInvalidationPattern(apiKeyId);
await cacheProvider.RemoveByPatternAsync(invalidationPattern);

// Invalidate all rate limit entries across all API keys
string rateLimitInvalidationPattern = CacheKeyGenerator.GetRateLimitInvalidationPattern();
await cacheProvider.RemoveByPatternAsync(rateLimitInvalidationPattern);
```

## RetryPolicyBuilder

The `RetryPolicyBuilder` class provides a fluent interface for creating retry policies with exponential backoff. This is useful for making operations resilient to transient failures such as network issues, temporary service unavailability, or rate limiting. The builder allows configuration of maximum retry attempts, initial delay, backoff multiplier, maximum delay, and specific exception types that should trigger retries.

### Example Usage

```csharp
using ApiKeyGateway.Utilities;
using System;
using System.Net;
using System.Net.Http;

// Create a retry policy for transient HTTP errors
var retryPolicy = new RetryPolicyBuilder()
    .WithMaxRetries(5)
    .WithInitialDelay(200)
    .WithBackoffMultiplier(2.5)
    .WithMaxDelay(30000)
    .RetryOn<HttpRequestException>()
    .RetryOn<TimeoutException>();

// Use the retry policy to execute an HTTP request with resilience
var httpClient = new HttpClient();
var apiUrl = "https://api.example.com/data";

var result = await retryPolicy.Build<HttpResponseMessage>()(async () =>
{
    var response = await httpClient.GetAsync(apiUrl);
    response.EnsureSuccessStatusCode();
    return response;
});

Console.WriteLine($"Successfully retrieved data: {result.StatusCode}");

// Create a retry policy for database operations
var dbRetryPolicy = new RetryPolicyBuilder()
    .WithMaxRetries(3)
    .WithInitialDelay(100)
    .WithBackoffMultiplier(2.0)
    .RetryOn<InvalidOperationException>();

// Use the retry policy for database operations
var userData = await dbRetryPolicy.Build<ApiKey>()(async () =>
{
    // Database operation that might fail transiently
    return await apiKeyRepository.GetByIdAsync("key_001");
});

Console.WriteLine($"Retrieved user data: {userData?.Name}");

// Create a simple retry policy with default settings
var simpleRetry = new RetryPolicyBuilder()
    .WithMaxRetries(2)
    .WithInitialDelay(500);

// Use the simple retry policy
var success = await simpleRetry.Build<bool>()(async () =>
{
    // Operation that might fail
    return true;
});

Console.WriteLine($"Operation completed successfully: {success}");
```

## ApiResponseBuilder

The `ApiResponseBuilder<T>` class provides a fluent interface for constructing consistent API responses across all endpoints. It allows you to build responses with data, status codes, success/failure states, metadata, and error collections using a clean builder pattern. This ensures uniform response structure throughout the application while maintaining flexibility for different scenarios.

### Example Usage

```csharp
using ApiKeyGateway.Utilities;
using System.Net;

// Create a successful response with data
var userData = new { Id = 1, Name = "John Doe", Email = "john@example.com" };
var successResponse = ApiResponseBuilderFactory.Success(userData, "User retrieved successfully");

Console.WriteLine($"Success: {successResponse.success}");
Console.WriteLine($"Status: {successResponse.statusCode}");
Console.WriteLine($"Message: {successResponse.message}");
Console.WriteLine($"Data: {successResponse.data?.Name}");
Console.WriteLine($"Timestamp: {successResponse.timestamp}");

// Create a response using the builder pattern
var builder = new ApiResponseBuilder<object>()
    .WithData(userData)
    .Success("User data retrieved");

// Add metadata to the response
builder.WithMetadata("pagination", new { Page = 1, PageSize = 10, Total = 100 });
builder.WithMetadata("cache", new { Hit = true, DurationMs = 42 });

var builtResponse = builder.Build();
Console.WriteLine($"Metadata: {builtResponse.metadata}");

// Create an error response for validation failure
var validationError = ApiResponseBuilderFactory.BadRequest(
    "Validation failed",
    "Email is required",
    "Name must be at least 3 characters"
);

Console.WriteLine($"Error Success: {validationError.success}");
Console.WriteLine($"Error Status: {validationError.statusCode}");
Console.WriteLine($"Error Message: {validationError.message}");
Console.WriteLine($"Error Code: {validationError.errorCode}");
Console.WriteLine($"Error Details: {validationError.errors}");

// Create a 404 Not Found response
var notFoundResponse = ApiResponseBuilderFactory.NotFound("User");
Console.WriteLine($"Not Found: {notFoundResponse.statusCode} - {notFoundResponse.message}");

// Create a 401 Unauthorized response
var unauthorizedResponse = ApiResponseBuilderFactory.Unauthorized("Invalid API key");
Console.WriteLine($"Unauthorized: {unauthorizedResponse.statusCode} - {unauthorizedResponse.message}");

// Create a 429 Too Many Requests response
var rateLimitResponse = ApiResponseBuilderFactory.TooManyRequests("API key rate limit exceeded");
Console.WriteLine($"Rate Limit: {rateLimitResponse.statusCode} - {rateLimitResponse.message}");

// Create a 500 Internal Server Error response
var serverErrorResponse = ApiResponseBuilderFactory.InternalServerError("Database connection failed");
Console.WriteLine($"Server Error: {serverErrorResponse.statusCode} - {serverErrorResponse.message}");
```

## AdminController

The `AdminController` provides administrative endpoints for managing the API Key Gateway. It offers system statistics, configuration management, usage data export, system diagnostics, and emergency operations like rate limit resets. These endpoints are protected and intended for administrative use only.

### Example Usage

```csharp
using ApiKeyGateway.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Set up services
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddSingleton<IMetricsCollectionService, MetricsCollectionService>();
services.AddSingleton<IDataExportService, DataExportService>();

var serviceProvider = services.BuildServiceProvider();
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
var metricsService = serviceProvider.GetRequiredService<IMetricsCollectionService>();
var dataExportService = serviceProvider.GetRequiredService<IDataExportService>();

// Create controller instance
var controller = new AdminController(
    loggerFactory.CreateLogger<AdminController>(),
    metricsService,
    dataExportService
);

// Get system statistics
var statsResult = controller.GetStats();
if (statsResult is OkObjectResult okResult)
{
    var stats = okResult.Value as dynamic;
    Console.WriteLine($"Total requests: {stats.totalRequests}");
    Console.WriteLine($"Active keys: {stats.activeApiKeys}");
}

// Export usage data (last 7 days)
var exportResult = await controller.ExportUsageData(format: "csv");
if (exportResult is FileResult fileResult)
{
    Console.WriteLine($"Exported file: {fileResult.FileDownloadName}");
}

// Get current configuration
var configResult = controller.GetConfiguration();
if (configResult is OkObjectResult configOkResult)
{
    var config = configOkResult.Value as dynamic;
    Console.WriteLine($"Max API keys: {config.maxApiKeys}");
    Console.WriteLine($"Cache enabled: {config.cacheEnabled}");
}

// Run system diagnostics
var diagnosticsResult = await controller.RunDiagnostics();
if (diagnosticsResult is OkObjectResult diagnosticsOkResult)
{
    var diagnostics = diagnosticsOkResult.Value as dynamic;
    Console.WriteLine($"Overall status: {diagnostics.overallStatus}");
}

// Reset rate limits (emergency operation)
var resetResult = await controller.ResetRateLimits();
if (resetResult is OkObjectResult resetOkResult)
{
    Console.WriteLine($"Rate limits reset: {resetOkResult.Value}");
}
```

## UsageController

The `UsageController` provides endpoints for retrieving API usage statistics and tracking records. It allows consumers to monitor their API consumption patterns by retrieving aggregated statistics for API keys, detailed usage records, and total usage for consumers. The controller supports date range filtering and provides comprehensive metrics including request counts, success rates, data transfer volumes, and response time statistics.

### Example Usage

```csharp
using ApiKeyGateway.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Set up services
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddSingleton<IUsageTrackingService, UsageTrackingService>();

var serviceProvider = services.BuildServiceProvider();
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
var usageService = serviceProvider.GetRequiredService<IUsageTrackingService>();

// Create controller instance
var controller = new UsageController(
  usageService,
  loggerFactory.CreateLogger<UsageController>()
);

// Get usage statistics for an API key (last 30 days)
var statsResult = await controller.GetKeyStatistics(
  apiKeyId: "sk_prod_abc123xyz",
  startDate: DateTime.UtcNow.AddDays(-30),
  endDate: DateTime.UtcNow
);
if (statsResult is OkObjectResult statsOkResult)
{
  var stats = statsOkResult.Value as UsageStatisticsResponse;
  Console.WriteLine($"API Key: {stats.ApiKeyId}");
  Console.WriteLine($"Period: {stats.StartDate:yyyy-MM-dd} to {stats.EndDate:yyyy-MM-dd}");
  Console.WriteLine($"Total requests: {stats.TotalRequests}");
  Console.WriteLine($"Successful: {stats.SuccessfulRequests}");
  Console.WriteLine($"Failed: {stats.FailedRequests}");
  Console.WriteLine($"Success rate: {stats.SuccessRate:P}");
  Console.WriteLine($"Total bytes: {stats.TotalBytesTransferred}");
  Console.WriteLine($"Average response time: {stats.AverageResponseTimeMs}ms");
  Console.WriteLine($"Unique endpoints: {stats.UniqueEndpoints}");
}

// Get detailed usage records for an API key (last 7 days, max 50 records)
var recordsResult = await controller.GetKeyRecords(
  apiKeyId: "sk_prod_abc123xyz",
  startDate: DateTime.UtcNow.AddDays(-7),
  endDate: DateTime.UtcNow,
  limit: 50
);
if (recordsResult is OkObjectResult recordsOkResult)
{
  var records = recordsOkResult.Value as List<UsageRecordResponse>;
  Console.WriteLine($"Found {records.Count} usage records");
  foreach (var record in records.Take(5))
  {
    Console.WriteLine($" - {record.RecordedAt:yyyy-MM-dd HH:mm:ss}: {record.Method} {record.Endpoint} ({record.StatusCode})");
    Console.WriteLine($"   Request: {record.RequestBytes} bytes, Response: {record.ResponseBytes} bytes, {record.ResponseTimeMs}ms");
  }
}

// Get total usage for a consumer (last 30 days)
var consumerResult = await controller.GetConsumerUsage(
  consumerId: "consumer_prod_xyz789",
  startDate: DateTime.UtcNow.AddDays(-30),
  endDate: DateTime.UtcNow
);
if (consumerResult is OkObjectResult consumerOkResult)
{
  var consumerUsage = consumerOkResult.Value as ConsumerUsageResponse;
  Console.WriteLine($"Consumer: {consumerUsage.ConsumerId}");
  Console.WriteLine($"Period: {consumerUsage.StartDate:yyyy-MM-dd} to {consumerUsage.EndDate:yyyy-MM-dd}");
  Console.WriteLine($"Total bytes: {consumerUsage.TotalBytesTransferred} bytes");
  Console.WriteLine($"Total GB: {consumerUsage.TotalGBTransferred} GB");
}
```

## AnalyticsController

The `AnalyticsController` provides aggregated usage analytics for API keys, allowing consumers to monitor their API consumption patterns. It offers endpoints for retrieving high-level summaries, top endpoints by usage, and time-series trends (hourly and daily). All date parameters are treated as UTC and sensible defaults are applied when not provided.

### Example Usage

```csharp
using ApiKeyGateway.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Set up services
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddSingleton<IUsageAnalyticsService, UsageAnalyticsService>();

var serviceProvider = services.BuildServiceProvider();
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
var analyticsService = serviceProvider.GetRequiredService<IUsageAnalyticsService>();

// Create controller instance
var controller = new AnalyticsController(
  analyticsService,
  loggerFactory.CreateLogger<AnalyticsController>()
);

// Get high-level usage summary for an API key (last 30 days)
var summaryResult = await controller.GetSummary(
  keyId: "sk_prod_abc123xyz",
  from: DateTime.UtcNow.AddDays(-30),
  to: DateTime.UtcNow
);
if (summaryResult is OkObjectResult summaryOkResult)
{
  var summary = summaryOkResult.Value as dynamic;
  Console.WriteLine($"Total requests: {summary.totalRequests}");
  Console.WriteLine($"Total errors: {summary.totalErrors}");
  Console.WriteLine($"Average response time: {summary.avgResponseTime}ms");
  Console.WriteLine($"Total bytes transferred: {summary.totalBytesTransferred} bytes");
}

// Get top 10 most-called endpoints for an API key (last 7 days)
var topEndpointsResult = await controller.GetTopEndpoints(
  keyId: "sk_prod_abc123xyz",
  limit: 10,
  from: DateTime.UtcNow.AddDays(-7),
  to: DateTime.UtcNow
);
if (topEndpointsResult is OkObjectResult endpointsOkResult)
{
  var endpoints = endpointsOkResult.Value as dynamic;
  Console.WriteLine($"Top endpoints:");
  foreach (var endpoint in endpoints)
  {
    Console.WriteLine($" - {endpoint.path}: {endpoint.requestCount} requests");
  }
}

// Get hourly trends for an API key (last 24 hours)
var hourlyTrendResult = await controller.GetHourlyTrend(
  keyId: "sk_prod_abc123xyz",
  from: DateTime.UtcNow.AddHours(-24),
  to: DateTime.UtcNow
);
if (hourlyTrendResult is OkObjectResult hourlyOkResult)
{
  var hourlyData = hourlyOkResult.Value as dynamic;
  Console.WriteLine($"Hourly trend data points: {hourlyData.Count}");
  foreach (var bucket in hourlyData)
  {
    Console.WriteLine($" - {bucket.hour}: {bucket.requestCount} requests, {bucket.avgResponseTime}ms avg");
  }
}

// Get daily trends for an API key (last 30 days)
var dailyTrendResult = await controller.GetDailyTrend(
  keyId: "sk_prod_abc123xyz",
  from: DateTime.UtcNow.AddDays(-30),
  to: DateTime.UtcNow
);
if (dailyTrendResult is OkObjectResult dailyOkResult)
{
  var dailyData = dailyOkResult.Value as dynamic;
  Console.WriteLine($"Daily trend data points: {dailyData.Count}");
  foreach (var bucket in dailyData)
  {
    Console.WriteLine($" - {bucket.date:yyyy-MM-dd}: {bucket.requestCount} requests, {bucket.errorCount} errors");
  }
}
```

## DateTimeExtensions

The `DateTimeExtensions` class provides a set of utility extension methods for working with `DateTime` values. It includes methods for finding the start/end of days, weeks, and months, checking if dates are in the past or future, calculating days until a date, and formatting dates as human-readable time strings. These extensions simplify common date manipulation tasks throughout the application.

### Example Usage

```csharp
using ApiKeyGateway.Utilities;
using System;

// Get the start and end of the current day
DateTime now = DateTime.UtcNow;
DateTime startOfDay = now.StartOfDay();
DateTime endOfDay = now.EndOfDay();
Console.WriteLine($"Today runs from {startOfDay:yyyy-MM-dd HH:mm:ss} to {endOfDay:yyyy-MM-dd HH:mm:ss}");

// Get the start of the current week (Monday)
DateTime startOfWeek = now.StartOfWeek();
Console.WriteLine($"Week started on: {startOfWeek:yyyy-MM-dd}");

// Get the start and end of the current month
DateTime startOfMonth = now.StartOfMonth();
DateTime endOfMonth = now.EndOfMonth();
Console.WriteLine($"Month runs from {startOfMonth:yyyy-MM-dd} to {endOfMonth:yyyy-MM-dd}");

// Check if a date is in the past or future
DateTime yesterday = DateTime.UtcNow.AddDays(-1);
DateTime tomorrow = DateTime.UtcNow.AddDays(1);
Console.WriteLine($"Yesterday is in past: {yesterday.IsInPast()}");
Console.WriteLine($"Tomorrow is in future: {tomorrow.IsInFuture()}");

// Calculate days until a future date
DateTime nextMonth = DateTime.UtcNow.AddMonths(1);
int daysUntil = nextMonth.DaysUntil();
Console.WriteLine($"Days until next month: {daysUntil}");

// Format a date as human-readable time
DateTime oneHourAgo = DateTime.UtcNow.AddHours(-1);
string humanTime = oneHourAgo.ToHumanReadableTime();
Console.WriteLine($"Time ago: {humanTime}");
```

## ApiKeysController

The `ApiKeysController` provides endpoints for managing API keys throughout their lifecycle. It allows consumers to create, retrieve, update, and delete API keys, as well as manage IP whitelists and rotate keys for security. The controller supports enabling/disabling keys, revoking keys, and managing IP restrictions to control where API keys can be used from.

### Example Usage

```csharp
using ApiKeyGateway.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Set up services
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddSingleton<IApiKeyService, ApiKeyService>();
services.AddSingleton<IApiKeyRotationService, ApiKeyRotationService>();

var serviceProvider = services.BuildServiceProvider();
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
var apiKeyService = serviceProvider.GetRequiredService<IApiKeyService>();
var rotationService = serviceProvider.GetRequiredService<IApiKeyRotationService>();

// Create controller instance
var controller = new ApiKeysController(
    apiKeyService,
    rotationService,
    loggerFactory.CreateLogger<ApiKeysController>()
);

// Create a new API key
var createResult = await controller.CreateKey(new CreateKeyRequest
{
    ConsumerId = "consumer_prod_xyz789",
    Name = "Production API Key",
    ExpirationDays = 90
});

if (createResult is CreatedAtActionResult createdResult)
{
    var response = createdResult.Value as CreateKeyResponse;
    Console.WriteLine($"Created API key: {response?.KeyId}");
    Console.WriteLine($"Consumer: {response?.ConsumerId}");
    Console.WriteLine($"Expires at: {response?.ExpiresAt}");
}

// Retrieve an API key by ID
var getResult = await controller.GetKeyById("sk_prod_abc123xyz");
if (getResult is OkObjectResult okResult)
{
    var key = okResult.Value as GetKeyResponse;
    Console.WriteLine($"Key: {key?.Name}, Status: {key?.Status}");
    Console.WriteLine($"Created: {key?.CreatedAt}, Expires: {key?.ExpiresAt}");
}

// List all API keys for a consumer
var listResult = await controller.GetConsumerKeys("consumer_prod_xyz789");
if (listResult is OkObjectResult listOkResult)
{
    var keys = listOkResult.Value as List<GetKeyResponse>;
    Console.WriteLine($"Found {keys?.Count} keys for consumer");
}

// Disable an API key
await controller.DisableKey("sk_prod_abc123xyz");
Console.WriteLine("API key disabled");

// Enable an API key
await controller.EnableKey("sk_prod_abc123xyz");
Console.WriteLine("API key enabled");

// Revoke an API key permanently
await controller.RevokeKey("sk_prod_abc123xyz");
Console.WriteLine("API key revoked");

// Get IP whitelist for an API key
var whitelistResult = await controller.GetIpWhitelist("sk_prod_abc123xyz");
if (whitelistResult is OkObjectResult whitelistOkResult)
{
    var whitelist = whitelistResult.Value as IpWhitelistResponse;
    Console.WriteLine($"Whitelist unrestricted: {whitelist?.IsUnrestricted}");
    if (!whitelist?.IsUnrestricted == true)
    {
        Console.WriteLine("Allowed IPs:");
        foreach (var ip in whitelist?.AllowedIps ?? new List<string>())
        {
            Console.WriteLine($" - {ip}");
        }
    }
}

// Set IP whitelist (replace all entries)
var setWhitelistResult = await controller.SetIpWhitelist("sk_prod_abc123xyz", new SetIpWhitelistRequest
{
    AllowedIps = new List<string> { "192.168.1.100", "10.0.0.5" }
});
if (setWhitelistResult is OkObjectResult setOkResult)
{
    var response = setOkResult.Value as IpWhitelistResponse;
    Console.WriteLine($"Set whitelist with {response?.AllowedIps?.Count} entries");
}

// Add IP to whitelist
var addIpResult = await controller.AddIpToWhitelist("sk_prod_abc123xyz", new IpAddressRequest
{
    IpAddress = "192.168.1.200"
});
if (addIpResult is OkObjectResult addOkResult)
{
    var response = addOkResult.Value as IpWhitelistResponse;
    Console.WriteLine($"Added IP, total whitelist entries: {response?.AllowedIps?.Count}");
}

// Remove IP from whitelist
var removeIpResult = await controller.RemoveIpFromWhitelist("sk_prod_abc123xyz", "192.168.1.100");
if (removeIpResult is OkObjectResult removeOkResult)
{
    var response = removeIpResult.Value as IpWhitelistResponse;
    Console.WriteLine($"Removed IP, whitelist unrestricted: {response?.IsUnrestricted}");
}

// Rotate an API key (create new key, revoke old one)
var rotateResult = await controller.RotateKey("sk_prod_abc123xyz", new RotateKeyRequest
{
    NewExpirationDays = 30
});
if (rotateResult is OkObjectResult rotateOkResult)
{
    var response = rotateOkResult.Value as RotateKeyResponse;
    Console.WriteLine($"Rotated key from {response?.OldKeyId} to {response?.NewKeyId}");
    Console.WriteLine($"New key expires: {response?.NewKeyExpiresAt}");
}

// Delete an API key
await controller.DeleteKey("sk_prod_abc123xyz");
Console.WriteLine("API key deleted");
```

## StatsController

The `StatsController` provides endpoints for retrieving usage statistics, rate limit status, endpoint-specific metrics, recent activity, and quota utilization for authenticated API keys. These endpoints are designed to help API key owners monitor their usage patterns and stay within configured limits.

### Example Usage

```csharp
using ApiKeyGateway.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Set up services
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

// Create controller instance
var controller = new StatsController(
    loggerFactory.CreateLogger<StatsController>()
);

// Get usage statistics for the last 24 hours
var usageResult = controller.GetUsageStatistics("day");
if (usageResult is OkObjectResult okResult)
{
    var stats = okResult.Value as dynamic;
    Console.WriteLine($"Period: {stats.period}");
    Console.WriteLine($"Requests: {stats.requests}");
    Console.WriteLine($"Errors: {stats.errors}");
}

// Get current rate limit status
var rateLimitResult = controller.GetRateLimitStatus();
if (rateLimitResult is OkObjectResult rateLimitOkResult)
{
    var rateLimit = rateLimitOkResult.Value as dynamic;
    Console.WriteLine($"Rate limit status: {rateLimit.status}");
    Console.WriteLine($"Hourly limit: {rateLimit.rateLimits.hourly.limit}");
    Console.WriteLine($"Daily limit: {rateLimit.rateLimits.daily.limit}");
}

// Get endpoint-specific statistics
var endpointResult = controller.GetEndpointStatistics();
if (endpointResult is OkObjectResult endpointOkResult)
{
    var endpoints = endpointOkResult.Value as dynamic;
    Console.WriteLine($"Endpoints monitored: {endpoints.endpoints.Length}");
    foreach (var endpoint in endpoints.endpoints)
    {
        Console.WriteLine($"  {endpoint.path}: {endpoint.requests} requests, {endpoint.avgResponseTime}ms avg");
    }
}

// Get recent activity (last 50 requests)
var activityResult = controller.GetRecentActivity(limit: 50);
if (activityResult is OkObjectResult activityOkResult)
{
    var activity = activityOkResult.Value as dynamic;
    Console.WriteLine($"Recent activity for API key: {activity.apiKeyId}");
    Console.WriteLine($"Showing {activity.recentRequests.Length} requests");
}

// Get quota status
var quotaResult = controller.GetQuotaStatus();
if (quotaResult is OkObjectResult quotaOkResult)
{
    var quota = quotaOkResult.Value as dynamic;
    Console.WriteLine($"Quota type: {quota.quotaType}");
    Console.WriteLine($"Requests today: {quota.usage.requestsToday} / {quota.limits.requestsPerDay}");
    Console.WriteLine($"Data transfer: {quota.usage.dataTransferGbThisMonth} GB / {quota.limits.dataTransferGbMonth} GB");
    if (quota.warnings.Length > 0)
    {
        Console.WriteLine("Warnings:");
        foreach (var warning in quota.warnings)
        {
            Console.WriteLine($"  - {warning}");
        }
    }
}
```

## RateLimitCalculationHelper

The `RateLimitCalculationHelper` class provides utility methods for rate limit calculations and window management. It encapsulates the logic for determining if requests are within quota, calculating reset times, and providing human-readable information about rate limit status. This helper is separated from business logic to allow easy testing and reuse across different components.

### Example Usage

```csharp
using ApiKeyGateway.Utilities;
using ApiKeyGateway.Domain.Enums;
using System;

// Calculate rate limit window boundaries
DateTime now = DateTime.UtcNow;
DateTime windowStart = RateLimitCalculationHelper.GetWindowStart(now, RateLimitUnit.Minute);
DateTime windowEnd = RateLimitCalculationHelper.GetWindowEnd(now, RateLimitUnit.Minute);
Console.WriteLine($"Current window: {windowStart:yyyy-MM-dd HH:mm:ss} to {windowEnd:yyyy-MM-dd HH:mm:ss}");

// Check if a request is allowed based on current usage
int currentUsage = 45;
int limit = 100;
int secondsUntilAllowed = RateLimitCalculationHelper.GetSecondsUntilAllowed(currentUsage, limit, windowStart, RateLimitUnit.Minute);
if (secondsUntilAllowed > 0)
{
    Console.WriteLine($"Rate limit exceeded. Try again in {secondsUntilAllowed} seconds.");
}
else
{
    Console.WriteLine("Request allowed - within rate limit.");
}

// Calculate quota percentage for monitoring
int percentage = RateLimitCalculationHelper.CalculateQuotagePercentage(currentUsage, limit);
Console.WriteLine($"Quota usage: {percentage}%");

// Check if warning should be shown to user
bool shouldWarn = RateLimitCalculationHelper.ShouldWarnAboutLimit(percentage);
if (shouldWarn)
{
    Console.WriteLine("Warning: Approaching rate limit!");
}

// Get human-readable reset time for API responses
string resetTime = RateLimitCalculationHelper.GetReadableResetTime(windowEnd);
Console.WriteLine($"Time until reset: {resetTime}");

// Example with different rate limit units
foreach (RateLimitUnit unit in Enum.GetValues<RateLimitUnit>())
{
    DateTime unitStart = RateLimitCalculationHelper.GetWindowStart(now, unit);
    DateTime unitEnd = RateLimitCalculationHelper.GetWindowEnd(now, unit);
    Console.WriteLine($"{unit}: {unitStart:yyyy-MM-dd HH:mm:ss} to {unitEnd:yyyy-MM-dd HH:mm:ss}");
}
```

## ICircuitBreaker

The `ICircuitBreaker` interface provides a simple circuit breaker pattern implementation for fault tolerance. It prevents cascading failures by monitoring operation failures and stopping requests to failing services when a configurable failure threshold is exceeded. The circuit breaker automatically transitions through states (Closed → Open → Half-Open → Closed) and recovers when the service becomes healthy again. This pattern is particularly useful for external API calls, database operations, or any remote service integration where transient failures should not cascade through the entire system.


### Example Usage

```csharp
using ApiKeyGateway.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

// Create a circuit breaker with 5 failures threshold and 30-second timeout
var circuitBreaker = new CircuitBreaker(
    failureThreshold: 5,
    timeout: TimeSpan.FromSeconds(30),
    logger: new Logger<CircuitBreaker>(new LoggerFactory())
);

// Execute an operation that might fail
try
{
    var result = await circuitBreaker.ExecuteAsync(async () =>
    {
        // Simulate an external API call or database operation
        var response = await externalService.GetDataAsync("some_endpoint");
        return response;
    });
    
    Console.WriteLine("Operation succeeded!");
    circuitBreaker.RecordSuccess();
}
catch (InvalidOperationException ex) when (ex.Message.Contains("Circuit breaker is open"))
{
    Console.WriteLine("Circuit breaker is open - request blocked");
    // Return cached response or degraded functionality
}

// Track operation failures
try
{
    await circuitBreaker.ExecuteAsync(async () => await failingOperation());
}
catch
{
    circuitBreaker.RecordFailure();
    Console.WriteLine("Operation failed, failure recorded");
}

// Check circuit breaker state
var state = circuitBreaker.GetState();
Console.WriteLine($"Current state: {state}");

// When service recovers, failures reset automatically
// After failure threshold reached, circuit opens and blocks requests for timeout period
for (int i = 0; i < 5; i++)
{
    try
    {
        await circuitBreaker.ExecuteAsync(async () => await failingOperation());
    }
    catch { circuitBreaker.RecordFailure(); }
}

// Circuit opens after 5 failures
Console.WriteLine($"Circuit breaker state after failures: {circuitBreaker.GetState()}");

// After timeout period, circuit transitions to Half-Open and allows one test request

## IntegrationTests

The `IntegrationTests` class provides comprehensive integration tests that validate the complete workflow of the API Key Gateway system. It tests end-to-end scenarios including API key creation, authentication, rate limiting, usage tracking, quota enforcement, IP whitelisting, and audit logging. These tests ensure all components work together correctly in realistic scenarios.

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using Xunit;
using System.Threading.Tasks;

// Integration test suite for API Key Gateway functionality
public class ApiKeyGatewayIntegrationTests : IntegrationTests
{
    [Fact]
    public async Task FullWorkflow_CreateKeyToUsageTracking_CompletesSuccessfully()
    {
        // Create a new API key
        var createResponse = await CreateKeyAsync("consumer_001", "Integration Test Key", 30);
        Assert.NotNull(createResponse?.KeyId);
        
        // Authenticate with the new key
        var authResult = await AuthenticateAsync(createResponse.KeyId);
        Assert.True(authResult.IsAuthenticated);
        
        // Track API usage
        var usageRecord = await RecordUsageAsync(createResponse.KeyId, "/api/users", "GET", 200, 1250, 4096, 42);
        Assert.NotNull(usageRecord);
        
        // Verify usage was tracked
        var stats = await GetKeyStatisticsAsync(createResponse.KeyId, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
        Assert.True(stats.TotalRequests > 0);
    }
    
    [Fact]
    public async Task RateLimiting_FullWorkflow_EnforcesAndRecords()
    {
        // Create API key with rate limit
        var createResponse = await CreateKeyAsync("consumer_002", "Rate Limited Key", 30);
        
        // Exceed rate limit
        for (int i = 0; i < 150; i++)
        {
            var result = await MakeAuthenticatedRequestAsync(createResponse.KeyId, "/api/test", "GET");
            if (i < 100)
            {
                Assert.True(result.IsSuccess);
            }
            else
            {
                Assert.Equal(429, result.StatusCode); // Too Many Requests
            }
        }
        
        // Verify rate limit was enforced
        var rateLimitStatus = await GetRateLimitStatusAsync(createResponse.KeyId);
        Assert.Equal(429, rateLimitStatus.StatusCode);
    }
    
    [Fact]
    public async Task UsageTracking_RecordMultipleAndGetStatistics()
    {
        // Create API key
        var createResponse = await CreateKeyAsync("consumer_003", "Usage Tracking Key", 30);
        
        // Record multiple usage events
        await RecordUsageAsync(createResponse.KeyId, "/api/users", "GET", 200, 1000, 2000, 35);
        await RecordUsageAsync(createResponse.KeyId, "/api/products", "POST", 201, 1500, 3000, 85);
        await RecordUsageAsync(createResponse.KeyId, "/api/orders", "GET", 200, 800, 1500, 28);
        
        // Retrieve statistics
        var stats = await GetKeyStatisticsAsync(
            createResponse.KeyId, 
            DateTime.UtcNow.AddDays(-7), 
            DateTime.UtcNow
        );
        
        Assert.Equal(3, stats.TotalRequests);
        Assert.Equal(3, stats.SuccessfulRequests);
        Assert.Equal(0, stats.FailedRequests);
        Assert.True(stats.TotalBytesTransferred > 0);
    }
    
    [Fact]
    public async Task UsageQuota_EnforceAndReset_WorksCorrectly()
    {
        // Create API key with usage quota
        var createResponse = await CreateKeyAsync("consumer_004", "Quota Limited Key", 30);
        
        // Exceed quota
        for (int i = 0; i < 1001; i++) // Exceeds default quota of 1000
        {
            var result = await MakeAuthenticatedRequestAsync(createResponse.KeyId, "/api/test", "GET");
            if (i < 1000)
            {
                Assert.True(result.IsSuccess);
            }
            else
            {
                Assert.Equal(429, result.StatusCode); // Quota Exceeded
            }
        }
        
        // Reset quota
        var resetResult = await ResetUsageQuotaAsync(createResponse.KeyId);
        Assert.True(resetResult);
        
        // Verify quota was reset
        var stats = await GetKeyStatisticsAsync(createResponse.KeyId, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
        Assert.Equal(0, stats.TotalRequests);
    }
    
    [Fact]
    public async Task Authentication_IpWhitelist_ValidatesCorrectly()
    {
        // Create API key
        var createResponse = await CreateKeyAsync("consumer_005", "IP Restricted Key", 30);
        
        // Set IP whitelist
        var whitelistResult = await SetIpWhitelistAsync(createResponse.KeyId, new[] { "192.168.1.100", "10.0.0.5" });
        Assert.NotNull(whitelistResult);
        
        // Request from allowed IP should succeed
        var allowedResult = await MakeAuthenticatedRequestFromIpAsync(createResponse.KeyId, "192.168.1.100", "/api/test", "GET");
        Assert.True(allowedResult.IsSuccess);
        
        // Request from blocked IP should fail
        var blockedResult = await MakeAuthenticatedRequestFromIpAsync(createResponse.KeyId, "192.168.1.200", "/api/test", "GET");
        Assert.Equal(403, blockedResult.StatusCode); // Forbidden
    }
    
    [Fact]
    public async Task AuditLogging_ConcurrentOperations_AllLogged()
    {
        // Create API key
        var createResponse = await CreateKeyAsync("consumer_006", "Audit Logged Key", 30);
        
        // Perform multiple operations
        await CreateKeyAsync("consumer_006", "Additional Key", 30);
        await DisableKeyAsync(createResponse.KeyId);
        await EnableKeyAsync(createResponse.KeyId);
        await RecordUsageAsync(createResponse.KeyId, "/api/test", "GET", 200, 500, 1000, 25);
        
        // Retrieve audit logs
        var auditLogs = await GetAuditLogsByResourceAsync(createResponse.KeyId, limit: 50);
        Assert.True(auditLogs.Count >= 4); // At least 4 operations logged
        
        foreach (var log in auditLogs)
        {
            Assert.Equal(createResponse.KeyId, log.ResourceId);
            Assert.Equal("ApiKey", log.ResourceType);
            Assert.True(log.IsSuccess);
        }
    }
    
    [Fact]
    public async Task CompleteFlow_CreateKeyAuthenticateAndTrack_Works()
    {
        // Complete end-to-end workflow
        var createResponse = await CreateKeyAsync("consumer_007", "Complete Flow Key", 30);
        Assert.NotNull(createResponse?.KeyId);
        
        // Authenticate
        var authResult = await AuthenticateAsync(createResponse.KeyId);
        Assert.True(authResult.IsAuthenticated);
        Assert.Equal(createResponse.KeyId, authResult.ApiKeyId);
        
        // Make authenticated requests
        var request1 = await MakeAuthenticatedRequestAsync(createResponse.KeyId, "/api/users", "GET");
        Assert.True(request1.IsSuccess);
        
        var request2 = await MakeAuthenticatedRequestAsync(createResponse.KeyId, "/api/products", "POST");
        Assert.True(request2.IsSuccess);
        
        // Verify usage tracking
        var stats = await GetKeyStatisticsAsync(createResponse.KeyId, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
        Assert.Equal(2, stats.TotalRequests);
        Assert.Equal(2, stats.SuccessfulRequests);
        
        // Clean up
        await DeleteKeyAsync(createResponse.KeyId);
        var deletedKey = await GetKeyByIdAsync(createResponse.KeyId);
        Assert.Null(deletedKey);
    }
}
```

## ApiKeyValidator

The `ApiKeyValidator` class provides validation methods for API key format, strength, and metadata. It ensures API keys meet security and format requirements before creation, helping to prevent weak or predictable keys that could compromise security. The validator separates validation logic from business logic for reusability across the application.

### Example Usage

```csharp
using ApiKeyGateway.Validation;

// Validate API key format (length, character diversity)
var keyFormatResult = ApiKeyValidator.ValidateKeyFormat("sk_ABC123def456GHI789jkl012MNO345pqr678");
if (!keyFormatResult.IsValid)
{
    Console.WriteLine($"Key format validation failed: {keyFormatResult.Message}");
}

// Validate API key name/description
var nameResult = ApiKeyValidator.ValidateKeyName("Production API Key - Web Service");
if (!nameResult.IsValid)
{
    Console.WriteLine($"Name validation failed: {nameResult.Message}");
}

// Validate quota limits
var quotaResult = ApiKeyValidator.ValidateQuotaLimit(10000);
if (!quotaResult.IsValid)
{
    Console.WriteLine($"Quota validation failed: {quotaResult.Message}");
}

// Check if validation was successful
bool isKeyValid = keyFormatResult.IsValid && nameResult.IsValid && quotaResult.IsValid;
Console.WriteLine($"Overall validation result: {(isKeyValid ? "PASSED" : "FAILED")}");

// Access error messages and errors collection
if (!keyFormatResult.IsValid)
{
    Console.WriteLine($"Error: {keyFormatResult.Message}");
    if (keyFormatResult.Errors.Any())
    {
        foreach (var error in keyFormatResult.Errors)
        {
            Console.WriteLine($" - {error}");
        }
    }
}
```

## ApiKeyServiceTests

The `ApiKeyServiceTests` class provides unit tests for the `ApiKeyService` class, covering all major functionality including key creation, retrieval, validation, status management, and consumer-specific operations. It tests both success and failure scenarios to ensure the service behaves correctly under various conditions, including invalid inputs, missing keys, and repository errors.

### Example Usage

```csharp
using ApiKeyGateway.Services;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;
using Moq;
using Xunit;

// Create a mock repository
var mockRepository = new Mock<IApiKeyRepository>();

// Set up test data
var testKey = new ApiKey
{
    Id = "test_key_123",
    ConsumerId = "consumer_123",
    Name = "Test API Key",
    KeyHash = "hashed_value_here",
    Prefix = "test_",
    Status = ApiKeyStatus.Active,
    CreatedAt = DateTime.UtcNow,
    ExpiresAt = DateTime.UtcNow.AddYears(1)
};

// Test CreateKeyAsync with valid arguments
mockRepository.Setup(r => r.CreateAsync(It.IsAny<ApiKey>()))
    .ReturnsAsync(testKey);

var service = new ApiKeyService(mockRepository.Object, Mock.Of<ILogger<ApiKeyService>>());

// Create a valid API key
var createdKey = await service.CreateKeyAsync("consumer_123", "Production API Key", 90);
Assert.Equal("test_key_123", createdKey.Id);
Assert.Equal("test_", createdKey.Prefix);

// Test GetByIdAsync with existing key
mockRepository.Setup(r => r.GetByIdAsync("test_key_123"))
    .ReturnsAsync(testKey);

var retrievedKey = await service.GetByIdAsync("test_key_123");
Assert.NotNull(retrievedKey);
Assert.Equal("Test API Key", retrievedKey.Name);

// Test DisableKeyAsync
mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ApiKey>()))
    .ReturnsAsync((ApiKey k) => k);

var disableResult = await service.DisableKeyAsync("test_key_123");
Assert.True(disableResult);

// Test ValidateKeyAsync with valid key
mockRepository.Setup(r => r.GetByIdAsync("test_key_123"))
    .ReturnsAsync(testKey);

var isValid = await service.ValidateKeyAsync("test_key_123");
Assert.True(isValid);

// Test GetConsumerKeysAsync
mockRepository.Setup(r => r.GetByConsumerAsync("consumer_123"))
    .ReturnsAsync(new List<ApiKey> { testKey });

var consumerKeys = await service.GetConsumerKeysAsync("consumer_123");
Assert.Single(consumerKeys);

// Test RevokeKeyAsync
var revokeResult = await service.RevokeKeyAsync("test_key_123");
Assert.True(revokeResult);
```

## ValidationHelpers

The `ValidationHelpers` class provides a collection of static utility methods for validating common input patterns such as email addresses, API keys, IP addresses, GUIDs, and URLs. These validation methods use regular expressions and .NET's built-in parsing capabilities to ensure data integrity before processing. The `SanitizeInput` method helps prevent injection attacks and enforces length limits on user-provided strings.

### Example Usage

```csharp
using ApiKeyGateway.Utilities;
using System;

// Validate an email address
string email = "user@example.com";
bool isValidEmail = ValidationHelpers.IsValidEmail(email);
Console.WriteLine($"Email '{email}' is valid: {isValidEmail}");

// Validate an API key format (sk_ prefix followed by 32+ alphanumeric characters)
string apiKey = "sk_ABC123def456GHI789jkl012MNO345pqr678";
bool isValidApiKey = ValidationHelpers.IsValidApiKeyFormat(apiKey);
Console.WriteLine($"API key format is valid: {isValidApiKey}");

// Validate an IP address
string ipAddress = "192.168.1.100";
bool isValidIp = ValidationHelpers.IsValidIpAddress(ipAddress);
Console.WriteLine($"IP address '{ipAddress}' is valid: {isValidIp}");

// Validate a GUID
string guidValue = "550e8400-e29b-41d4-a716-446655440000";
bool isValidGuid = ValidationHelpers.IsValidGuid(guidValue);
Console.WriteLine($"GUID '{guidValue}' is valid: {isValidGuid}");

// Validate a URL
string url = "https://api.example.com/v1/users";
bool isValidUrl = ValidationHelpers.IsValidUrl(url);
Console.WriteLine($"URL '{url}' is valid: {isValidUrl}");

// Sanitize user input to prevent injection attacks
string maliciousInput = "  <script>alert('xss')</script>  ";
string sanitized = ValidationHelpers.SanitizeInput(maliciousInput, maxLength: 200);
Console.WriteLine($"Sanitized input: '{sanitized}'");
```

## RequestContextHelper

The `RequestContextHelper` class provides utility methods for extracting and validating information from HTTP requests. It centralizes request parsing logic to ensure consistency across the application, handling API key extraction, correlation ID generation, pagination parameters, client IP address resolution, request scope identification, and content negotiation checks.

### Example Usage

```csharp
using ApiKeyGateway.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

// Example ASP.NET Core minimal API endpoint using RequestContextHelper
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/protected", (HttpRequest request) => {
    // Extract API key from either X-API-Key header or Authorization: Bearer header
    string? apiKey = RequestContextHelper.ExtractApiKey(request);
    if (apiKey == null)
    {
        return Results.Unauthorized();
    }
    
    // Get or create correlation ID for request tracing
    string correlationId = RequestContextHelper.GetOrCreateCorrelationId(request);
    Console.WriteLine($"Processing request with correlation ID: {correlationId}");
    
    // Extract pagination parameters from query string
    var (pageNumber, pageSize) = RequestContextHelper.ExtractPaginationParams(request);
    Console.WriteLine($"Pagination: page {pageNumber}, size {pageSize}");
    
    // Get client IP address (handles X-Forwarded-For and X-Real-IP headers)
    string clientIp = RequestContextHelper.GetClientIpAddress(request);
    Console.WriteLine($"Request from IP: {clientIp}");
    
    // Get request scope (API key or "anonymous")
    string scope = RequestContextHelper.GetRequestScope(request);
    Console.WriteLine($"Request scope: {scope}");
    
    // Check if client accepts JSON responses
    bool acceptsJson = RequestContextHelper.AcceptsJson(request);
    Console.WriteLine($"Client accepts JSON: {acceptsJson}");
    
    return Results.Ok(new {
        Message = "API key validated successfully",
        ApiKey = apiKey,
        CorrelationId = correlationId,
        PageNumber = pageNumber,
        PageSize = pageSize,
        ClientIp = clientIp,
        Scope = scope
    });
});

app.Run();
```

## JsonSerializationHelper

The `JsonSerializationHelper` class provides centralized JSON serialization and deserialization utilities that enforce consistent formatting and naming conventions across the application. It handles conversion between C# PascalCase properties and API camelCase responses, manages null value handling, and includes safe deserialization methods that prevent exceptions on invalid JSON input. This helper ensures that all API responses follow the same serialization pattern and provides both compact and formatted output options.

### Example Usage

```csharp
using ApiKeyGateway.Utilities;
using System.Text.Json;

// Serialize an object to compact JSON (camelCase, no whitespace)
var user = new { Id = 1, UserName = "john_doe", EmailAddress = "john@example.com" };
string compactJson = JsonSerializationHelper.SerializeCompact(user);
Console.WriteLine(compactJson);
// Output: {"userName":"john_doe","emailAddress":"john@example.com"}

// Serialize an object to formatted JSON (pretty-printed)
string formattedJson = JsonSerializationHelper.SerializeFormatted(user);
Console.WriteLine(formattedJson);
/* Output:
{
  "userName": "john_doe",
  "emailAddress": "john@example.com"
}
*/

// Deserialize JSON back to an object
string jsonInput = "{\"userName\":\"jane_doe\",\"emailAddress\":\"jane@example.com\"}";
var deserializedUser = JsonSerializationHelper.Deserialize<Dictionary<string, object>>(jsonInput);
Console.WriteLine(deserializedUser["userName"]); // Output: jane_doe

// Safely deserialize with error handling (returns null on failure)
string invalidJson = "{ invalid json }";
var safeResult = JsonSerializationHelper.SafeDeserialize<Dictionary<string, object>>(invalidJson);
Console.WriteLine(safeResult == null ? "Deserialization failed safely" : "Success"); // Output: Deserialization failed safely

// Validate JSON without full deserialization
bool isValid = JsonSerializationHelper.IsValidJson("{\"key\":\"value\"}");
Console.WriteLine(isValid); // Output: True
bool isInvalid = JsonSerializationHelper.IsValidJson("not valid json");
Console.WriteLine(isInvalid); // Output: False
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
