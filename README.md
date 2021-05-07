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
